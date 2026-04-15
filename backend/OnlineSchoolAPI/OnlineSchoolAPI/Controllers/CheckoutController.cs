using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;
    private readonly IOrderReceiptEmailService _receiptEmail;

    public CheckoutController(OnlineSchoolDbContext context, IOrderReceiptEmailService receiptEmail)
    {
        _context = context;
        _receiptEmail = receiptEmail;
    }

    /// <summary>
    /// Оформление заказа: цены из БД, промокод на сервере; заказ, оплата, зачисление (enrollment),
    /// student_lesson_access, student_progress; при рассрочке — installment_plan и installment_payment.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CheckoutResponseDto>> Post([FromBody] CheckoutRequestDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest("Корзина пуста");

        if (dto.UseInstallment && (dto.InstallmentCount is null or < 2))
            return BadRequest("Для рассрочки укажите InstallmentCount не менее 2.");

        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == dto.UserId);
        if (student == null)
            return BadRequest("Профиль ученика не найден. Убедитесь, что аккаунт привязан к записи ученика.");

        await using var tx = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        decimal subtotal = 0;
        var preparedLines = new List<(CheckoutLineRequestDto Line, Course Course, decimal UnitPrice, CourseInstance Instance)>();

        foreach (var rawLine in dto.Items)
        {
            var line = rawLine;
            var qty = line.Quantity < 1 ? 1 : line.Quantity;
            line.Quantity = qty;

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == line.CourseId);
            if (course == null)
            {
                await tx.RollbackAsync();
                return BadRequest($"Курс не найден: {line.CourseId}");
            }

            if (course.IsActive == false)
            {
                await tx.RollbackAsync();
                return BadRequest($"Курс недоступен: {course.Title}");
            }

            var instance = await ResolveInstanceAsync(course.CourseId, line.InstanceId);
            if (instance == null)
            {
                await tx.RollbackAsync();
                return BadRequest(
                    line.InstanceId.HasValue
                        ? "Указанный поток курса не найден или не относится к курсу."
                        : $"Для курса «{course.Title}» нет доступных потоков. Укажите поток (instanceId) вручную.");
            }

            if (instance.IsActive == false)
            {
                await tx.RollbackAsync();
                return BadRequest($"Поток «{instance.InstanceName}» недоступен для записи.");
            }

            var unit = EffectivePrice(course);
            subtotal += unit * qty;
            preparedLines.Add((line, course, unit, instance));
        }

        decimal discountAmount = 0;
        int? promoCodeId = null;
        string? promoMessage = null;
        PromoCode? promoEntity = null;

        if (!string.IsNullOrWhiteSpace(dto.PromoCode))
        {
            var codeNorm = dto.PromoCode.Trim();
            promoEntity = await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code != null && p.Code.ToLower() == codeNorm.ToLower());

            if (promoEntity == null)
            {
                await tx.RollbackAsync();
                return BadRequest("Промокод не найден");
            }

            string? typeName = null;
            if (promoEntity.TypeId.HasValue)
            {
                typeName = await _context.DiscountTypes.AsNoTracking()
                    .Where(t => t.TypeId == promoEntity.TypeId.Value)
                    .Select(t => t.TypeName)
                    .FirstOrDefaultAsync();
            }

            var (ok, message, discount) = PromoCodeDiscountCalculator.Compute(promoEntity, typeName, subtotal);
            if (!ok)
            {
                await tx.RollbackAsync();
                return BadRequest(message);
            }

            discountAmount = discount;
            promoCodeId = promoEntity.PromoCodeId;
            promoMessage = message;
            promoEntity.CurrentUses = (promoEntity.CurrentUses ?? 0) + 1;
        }

        var finalAmount = Math.Max(0, subtotal - discountAmount);

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var order = new AppOrder
        {
            StudentId = student.StudentId,
            OrderNumber = orderNumber,
            TotalAmount = subtotal,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            OrderStatusId = 1,
            MethodId = dto.MethodId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AppOrders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var (line, course, unitPrice, instance) in preparedLines)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.OrderId,
                CourseId = course.CourseId,
                InstanceId = instance.InstanceId,
                Price = unitPrice,
                Quantity = line.Quantity,
                CreatedAt = DateTime.UtcNow
            });

            var alreadyEnrolled = await _context.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId && e.InstanceId == instance.InstanceId);
            if (alreadyEnrolled)
            {
                await tx.RollbackAsync();
                return BadRequest($"Вы уже записаны на поток «{instance.InstanceName}» ({course.Title}).");
            }

            var enrollment = new Enrollment
            {
                StudentId = student.StudentId,
                InstanceId = instance.InstanceId,
                EnrolledAt = DateTime.UtcNow,
                EnrollmentStatusId = 1
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            await CreateLessonAccessAndProgressAsync(enrollment, instance, course.CourseId);
        }

        var paidStatusId = await ResolvePaymentStatusIdAsync("completed", "paid", "success");
        var pendingStatusId = await ResolvePaymentStatusIdAsync("pending", "ожид");

        var useInstallment = dto.UseInstallment
            && dto.InstallmentCount is >= 2
            && finalAmount > 0;

        var payment = new Payment
        {
            OrderId = order.OrderId,
            Amount = finalAmount,
            MethodId = dto.MethodId,
            CreatedAt = DateTime.UtcNow,
            PaymentStatusId = useInstallment ? pendingStatusId : paidStatusId,
            PaidAt = useInstallment ? null : DateTime.UtcNow
        };
        _context.Payments.Add(payment);

        if (!useInstallment && finalAmount > 0)
        {
            order.PaidAt = DateTime.UtcNow;
        }

        if (useInstallment)
        {
            var count = dto.InstallmentCount!.Value;
            var total = finalAmount;
            var basePart = Math.Round(total / count, 2, MidpointRounding.AwayFromZero);
            var plan = new InstallmentPlan
            {
                OrderId = order.OrderId,
                TotalAmount = total,
                InstallmentCount = count,
                MonthlyPayment = basePart,
                NextPaymentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                PlanStatus = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.InstallmentPlans.Add(plan);
            await _context.SaveChangesAsync();

            decimal sum = 0;
            var start = DateOnly.FromDateTime(DateTime.UtcNow);
            for (var i = 1; i <= count; i++)
            {
                var amount = i < count ? basePart : total - sum;
                sum += amount;
                _context.InstallmentPayments.Add(new InstallmentPayment
                {
                    PlanId = plan.PlanId,
                    InstallmentNumber = i,
                    DueDate = start.AddMonths(i),
                    Amount = amount,
                    PaymentStatus = "pending",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        var receiptLines = preparedLines
            .Select(x => (x.Course.Title, x.UnitPrice, x.Line.Quantity))
            .ToList();

        var recipientEmail = await _context.Users.AsNoTracking()
            .Where(u => u.UserId == dto.UserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(recipientEmail))
        {
            await _receiptEmail.SendCheckoutReceiptAsync(
                recipientEmail,
                orderNumber,
                receiptLines,
                subtotal,
                discountAmount,
                finalAmount,
                promoMessage,
                useInstallment,
                useInstallment ? dto.InstallmentCount : null);
        }

        return Ok(new CheckoutResponseDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            TotalAmount = subtotal,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PromoCodeId = promoCodeId,
            PromoMessage = promoMessage
        });
    }

    private async Task CreateLessonAccessAndProgressAsync(
        Enrollment enrollment,
        CourseInstance instance,
        int courseId)
    {
        var now = DateTime.UtcNow;
        var plans = await _context.CourseSchedulePlans
            .Where(p => p.InstanceId == instance.InstanceId)
            .OrderBy(p => p.ReleaseDayOffset)
            .ThenBy(p => p.LessonId)
            .ToListAsync();

        var accessRows = new List<StudentLessonAccess>();

        if (plans.Count > 0)
        {
            foreach (var plan in plans)
            {
                var plannedDate = instance.StartDate.AddDays(plan.ReleaseDayOffset);
                var access = new StudentLessonAccess
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    LessonId = plan.LessonId,
                    PlanId = plan.PlanId,
                    PlannedAccessDate = plannedDate,
                    PlannedAccessTime = plan.ReleaseTime,
                    IsAvailable = true,
                    CreatedAt = now
                };
                accessRows.Add(access);
                _context.StudentLessonAccesses.Add(access);
            }
        }
        else
        {
            var lessonIds = await _context.Lessons
                .Where(l => l.Module.CourseId == courseId)
                .OrderBy(l => l.Module.ModuleOrder)
                .ThenBy(l => l.LessonOrder)
                .Select(l => l.LessonId)
                .ToListAsync();

            foreach (var lessonId in lessonIds)
            {
                var access = new StudentLessonAccess
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    LessonId = lessonId,
                    PlanId = null,
                    PlannedAccessDate = instance.StartDate,
                    IsAvailable = true,
                    CreatedAt = now
                };
                accessRows.Add(access);
                _context.StudentLessonAccesses.Add(access);
            }
        }

        await _context.SaveChangesAsync();

        foreach (var access in accessRows)
        {
            _context.StudentProgresses.Add(new StudentProgress
            {
                EnrollmentId = enrollment.EnrollmentId,
                LessonId = access.LessonId,
                AccessId = access.AccessId,
                IsCompleted = false,
                CreatedAt = now
            });
        }
    }

    private async Task<CourseInstance?> ResolveInstanceAsync(int courseId, int? requestedInstanceId)
    {
        if (requestedInstanceId.HasValue)
        {
            return await _context.CourseInstances
                .FirstOrDefaultAsync(i =>
                    i.InstanceId == requestedInstanceId.Value && i.CourseId == courseId);
        }

        return await _context.CourseInstances
            .Where(i => i.CourseId == courseId && (i.IsActive == null || i.IsActive == true))
            .OrderBy(i => i.StartDate)
            .FirstOrDefaultAsync();
    }

    private async Task<int?> ResolvePaymentStatusIdAsync(params string[] nameHints)
    {
        var statuses = await _context.PaymentStatuses.AsNoTracking().ToListAsync();
        foreach (var hint in nameHints)
        {
            var h = hint.ToLowerInvariant();
            var match = statuses.FirstOrDefault(s =>
                s.StatusName != null && s.StatusName.ToLowerInvariant().Contains(h));
            if (match != null)
                return match.StatusId;
        }

        return statuses.FirstOrDefault()?.StatusId;
    }

    private static decimal EffectivePrice(Course c)
    {
        if (c.DiscountPrice.HasValue && c.DiscountPrice.Value > 0 && c.DiscountPrice.Value < c.Price)
            return c.DiscountPrice.Value;
        return c.Price;
    }
}
