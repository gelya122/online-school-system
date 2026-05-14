using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminInstallmentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminInstallmentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/installments")]
    public async Task<ActionResult<IReadOnlyList<AdminInstallmentListRowDto>>> GetList(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var list = await _context.InstallmentPlans
            .AsNoTracking()
            .Include(p => p.Order)
            .OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue)
            .Take(500)
            .Select(p => new AdminInstallmentListRowDto
            {
                PlanId = p.PlanId,
                OrderId = p.OrderId,
                OrderNumber = p.Order.OrderNumber,
                TotalAmount = p.TotalAmount,
                InstallmentCount = p.InstallmentCount,
                MonthlyPayment = p.MonthlyPayment,
                NextPaymentDate = p.NextPaymentDate,
                Status = p.PlanStatus
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("api/admin/installments/{id:int}")]
    public async Task<ActionResult<AdminInstallmentDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var p = await _context.InstallmentPlans
            .AsNoTracking()
            .Include(x => x.InstallmentPayments).ThenInclude(ip => ip.PaymentStatusNavigation)
            .FirstOrDefaultAsync(x => x.PlanId == id, cancellationToken);
        if (p == null) return NotFound();

        var payments = p.InstallmentPayments
            .OrderBy(x => x.InstallmentNumber)
            .Select(ip => new AdminInstallmentPaymentRowDto
            {
                InstallmentPaymentId = ip.InstallmentPaymentId,
                InstallmentNumber = ip.InstallmentNumber,
                DueDate = ip.DueDate,
                Amount = ip.Amount,
                PaymentStatusId = ip.PaymentStatusId,
                StatusName = ip.PaymentStatusNavigation != null ? ip.PaymentStatusNavigation.StatusName : null,
                PaymentStatus = ip.PaymentStatus,
                PaidAt = ip.PaidAt
            })
            .ToList();

        return Ok(new AdminInstallmentDetailsDto
        {
            PlanId = p.PlanId,
            OrderId = p.OrderId,
            TotalAmount = p.TotalAmount,
            InstallmentCount = p.InstallmentCount,
            MonthlyPayment = p.MonthlyPayment,
            NextPaymentDate = p.NextPaymentDate,
            Status = p.PlanStatus,
            Payments = payments
        });
    }

    [HttpPost("api/admin/installments")]
    public async Task<ActionResult<AdminInstallmentDetailsDto>> Create([FromBody] AdminInstallmentCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (dto.InstallmentCount < 2) return BadRequest("installmentCount должен быть >= 2");

        var order = await _context.AppOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId, cancellationToken);
        if (order == null) return BadRequest("Заказ не найден");

        var total = order.FinalAmount;
        var monthly = dto.MonthlyPayment ?? Math.Round(total / dto.InstallmentCount, 2, MidpointRounding.AwayFromZero);
        var next = dto.NextPaymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

        var plan = new InstallmentPlan
        {
            OrderId = dto.OrderId,
            TotalAmount = total,
            InstallmentCount = dto.InstallmentCount,
            MonthlyPayment = monthly,
            NextPaymentDate = next,
            PlanStatus = string.IsNullOrWhiteSpace(dto.Status) ? "active" : dto.Status!.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.InstallmentPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        decimal sum = 0;
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        for (var i = 1; i <= dto.InstallmentCount; i++)
        {
            var amount = i < dto.InstallmentCount ? monthly : total - sum;
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
        await _context.SaveChangesAsync(cancellationToken);

        return await Get(plan.PlanId, cancellationToken);
    }

    [HttpPatch("api/admin/installments/{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] AdminInstallmentStatusPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Status)) return BadRequest("Укажите status");

        var p = await _context.InstallmentPlans.FirstOrDefaultAsync(x => x.PlanId == id, cancellationToken);
        if (p == null) return NotFound();

        p.PlanStatus = dto.Status.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/installment-payments/{id:int}/status")]
    public async Task<IActionResult> PatchInstallmentPaymentStatus(int id, [FromBody] AdminInstallmentPaymentStatusPatchDto dto,
        CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Status)) return BadRequest("Укажите status");

        var ip = await _context.InstallmentPayments.FirstOrDefaultAsync(x => x.InstallmentPaymentId == id, cancellationToken);
        if (ip == null) return NotFound();

        var statusId = await ResolvePaymentStatusIdAsync(dto.Status, cancellationToken);
        ip.PaymentStatusId = statusId;
        var statusName = statusId == null
            ? null
            : await _context.PaymentStatuses.AsNoTracking()
                .Where(s => s.StatusId == statusId.Value)
                .Select(s => s.StatusName)
                .FirstOrDefaultAsync(cancellationToken);
        ip.PaymentStatus = statusName ?? dto.Status.Trim();

        var s = dto.Status.Trim().ToLowerInvariant();
        if (s.Contains("paid") || s.Contains("success") || s.Contains("оплач") || s.Contains("completed"))
            ip.PaidAt ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<int?> ResolvePaymentStatusIdAsync(string wanted, CancellationToken cancellationToken)
    {
        var w = wanted.Trim().ToLowerInvariant();
        return await _context.PaymentStatuses.AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Where(s => s.StatusName.ToLower().Contains(w))
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

