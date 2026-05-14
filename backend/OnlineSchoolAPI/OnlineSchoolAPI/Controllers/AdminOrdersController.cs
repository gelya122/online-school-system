using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminOrdersController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/orders")]
    public async Task<ActionResult<IReadOnlyList<AdminOrderListRowDto>>> GetList(
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var q = _context.AppOrders
            .AsNoTracking()
            .Include(o => o.Student).ThenInclude(s => s.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.Method)
            .Include(o => o.PromoCode)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Instance)
            .Where(o => o.Student.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(o =>
                (o.OrderNumber ?? "").ToLower().Contains(s) ||
                ((o.Student.FirstName ?? "") + " " + (o.Student.LastName ?? "")).ToLower().Contains(s) ||
                (o.Student.User.Email ?? "").ToLower().Contains(s));
        }

        var list = await q
            .OrderByDescending(o => o.CreatedAt ?? DateTime.MinValue)
            .Take(500)
            .Select(o => new AdminOrderListRowDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                StudentId = o.StudentId,
                StudentName = ((o.Student.FirstName ?? "") + " " + (o.Student.LastName ?? "")).Trim(),
                CourseTitle = o.OrderItems.OrderBy(x => x.OrderItemId).Select(x => x.Course.Title).FirstOrDefault() ?? "",
                InstanceId = o.OrderItems.OrderByDescending(x => x.OrderItemId).Select(x => x.InstanceId).FirstOrDefault(),
                InstanceTitle = o.OrderItems.OrderByDescending(x => x.OrderItemId).Select(x => x.Instance != null ? x.Instance.InstanceName : null).FirstOrDefault(),
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount ?? 0m,
                FinalAmount = o.FinalAmount,
                PromoCode = o.PromoCode != null ? o.PromoCode.Code : null,
                Status = o.OrderStatus != null ? o.OrderStatus.StatusName : null,
                Method = o.Method != null ? o.Method.MethodName : null,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("api/admin/orders/{id:int}")]
    public async Task<ActionResult<AdminOrderDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var o = await _context.AppOrders
            .AsNoTracking()
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.OrderStatus)
            .Include(x => x.Method)
            .Include(x => x.PromoCode)
            .Include(x => x.OrderItems).ThenInclude(oi => oi.Course)
            .Include(x => x.OrderItems).ThenInclude(oi => oi.Instance)
            .FirstOrDefaultAsync(x => x.OrderId == id, cancellationToken);
        if (o == null) return NotFound();

        return Ok(new AdminOrderDetailsDto
        {
            OrderId = o.OrderId,
            OrderNumber = o.OrderNumber,
            StudentId = o.StudentId,
            StudentName = ((o.Student.FirstName ?? "") + " " + (o.Student.LastName ?? "")).Trim(),
            TotalAmount = o.TotalAmount,
            DiscountAmount = o.DiscountAmount ?? 0m,
            FinalAmount = o.FinalAmount,
            PromoCodeId = o.PromoCodeId,
            PromoCode = o.PromoCode != null ? o.PromoCode.Code : null,
            OrderStatusId = o.OrderStatusId,
            Status = o.OrderStatus != null ? o.OrderStatus.StatusName : null,
            MethodId = o.MethodId,
            Method = o.Method != null ? o.Method.MethodName : null,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt,
            Items = o.OrderItems
                .OrderBy(i => i.OrderItemId)
                .Select(i => new AdminOrderItemRowDto
                {
                    OrderItemId = i.OrderItemId,
                    CourseId = i.CourseId,
                    CourseTitle = i.Course.Title,
                    InstanceId = i.InstanceId,
                    InstanceTitle = i.Instance != null ? i.Instance.InstanceName : null,
                    Price = i.Price,
                    Quantity = i.Quantity ?? 1
                })
                .ToList()
        });
    }

    [HttpPost("api/admin/orders")]
    public async Task<ActionResult<AdminOrderDetailsDto>> Create([FromBody] AdminOrderCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == dto.StudentId && s.DeletedAt == null, cancellationToken);
        if (student == null) return BadRequest("Студент не найден");

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == dto.CourseId, cancellationToken);
        if (course == null) return BadRequest("Курс не найден");

        CourseInstance? instance = null;
        if (dto.InstanceId.HasValue)
        {
            instance = await _context.CourseInstances.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InstanceId == dto.InstanceId.Value && i.CourseId == course.CourseId, cancellationToken);
            if (instance == null) return BadRequest("Поток не найден или не относится к курсу");
        }

        var qty = dto.Quantity < 1 ? 1 : dto.Quantity;
        var unit = course.DiscountPrice.HasValue && course.DiscountPrice.Value > 0 ? course.DiscountPrice.Value : course.Price;
        var subtotal = unit * qty;

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var order = new AppOrder
        {
            StudentId = dto.StudentId,
            OrderNumber = orderNumber,
            TotalAmount = subtotal,
            DiscountAmount = 0m,
            FinalAmount = subtotal,
            OrderStatusId = await ResolveOrderStatusIdAsync("pending", "new", "создан", cancellationToken),
            MethodId = dto.MethodId,
            CreatedAt = DateTime.UtcNow
        };
        _context.AppOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _context.OrderItems.Add(new OrderItem
        {
            OrderId = order.OrderId,
            CourseId = course.CourseId,
            InstanceId = instance?.InstanceId,
            Price = unit,
            Quantity = qty,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return await Get(order.OrderId, cancellationToken);
    }

    [HttpPut("api/admin/orders/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminOrderUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var order = await _context.AppOrders.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);
        if (order == null) return NotFound();

        if (dto.MethodId.HasValue) order.MethodId = dto.MethodId.Value;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/orders/{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] AdminOrderStatusPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Status)) return BadRequest("Укажите status");

        var order = await _context.AppOrders.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);
        if (order == null) return NotFound();

        order.OrderStatusId = await ResolveOrderStatusIdAsync(dto.Status, cancellationToken: cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/orders/{id:int}/apply-promo-code")]
    public async Task<IActionResult> ApplyPromoCode(int id, [FromBody] AdminApplyPromoCodeDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var order = await _context.AppOrders
            .Include(o => o.Student)
            .FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);
        if (order == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.PromoCode)) return BadRequest("Укажите promoCode");

        var promo = await _context.PromoCodes
            .FirstOrDefaultAsync(p => p.DeletedAt == null && p.Code.ToLower() == dto.PromoCode.Trim().ToLower(), cancellationToken);
        if (promo == null) return BadRequest("Промокод не найден");

        var typeName = promo.TypeId.HasValue
            ? await _context.DiscountTypes.AsNoTracking()
                .Where(t => t.TypeId == promo.TypeId.Value)
                .Select(t => t.TypeName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var (ok, message, discount) = PromoCodeDiscountCalculator.Compute(promo, typeName, order.TotalAmount);
        if (!ok) return BadRequest(message);

        order.PromoCodeId = promo.PromoCodeId;
        order.DiscountAmount = discount;
        order.FinalAmount = Math.Max(0, order.TotalAmount - discount);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/orders/{id:int}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var order = await _context.AppOrders.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);
        if (order == null) return NotFound();

        order.PaidAt = order.PaidAt ?? DateTime.UtcNow;
        order.OrderStatusId = await ResolveOrderStatusIdAsync("paid", "completed", "оплачен", cancellationToken);

        var paidStatusId = await ResolvePaymentStatusIdAsync("completed", "paid", "success", cancellationToken);
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == id, cancellationToken);
        if (payment == null)
        {
            _context.Payments.Add(new Payment
            {
                OrderId = order.OrderId,
                Amount = order.FinalAmount,
                MethodId = order.MethodId,
                PaymentStatusId = paidStatusId,
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            payment.PaymentStatusId = paidStatusId;
            payment.PaidAt = payment.PaidAt ?? DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/orders/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var order = await _context.AppOrders.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);
        if (order == null) return NotFound();

        order.OrderStatusId = await ResolveOrderStatusIdAsync("cancel", "canceled", "отмен", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<int?> ResolveOrderStatusIdAsync(string wanted, string? alt = null, string? alt2 = null, CancellationToken cancellationToken = default)
    {
        var w = wanted.Trim().ToLowerInvariant();
        var a1 = alt?.Trim().ToLowerInvariant();
        var a2 = alt2?.Trim().ToLowerInvariant();

        return await _context.OrderStatuses.AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Where(s =>
                s.StatusName.ToLower().Contains(w) ||
                (a1 != null && s.StatusName.ToLower().Contains(a1)) ||
                (a2 != null && s.StatusName.ToLower().Contains(a2)))
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<int?> ResolvePaymentStatusIdAsync(string wanted, string? alt = null, string? alt2 = null, CancellationToken cancellationToken = default)
    {
        var w = wanted.Trim().ToLowerInvariant();
        var a1 = alt?.Trim().ToLowerInvariant();
        var a2 = alt2?.Trim().ToLowerInvariant();

        return await _context.PaymentStatuses.AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Where(s =>
                s.StatusName.ToLower().Contains(w) ||
                (a1 != null && s.StatusName.ToLower().Contains(a1)) ||
                (a2 != null && s.StatusName.ToLower().Contains(a2)))
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

