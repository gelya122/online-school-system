using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminPaymentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminPaymentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/payments")]
    public async Task<ActionResult<IReadOnlyList<AdminPaymentListRowDto>>> GetList(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var list = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.PaymentStatus)
            .Include(p => p.Method)
            .OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue)
            .Take(500)
            .Select(p => new AdminPaymentListRowDto
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                OrderNumber = p.Order.OrderNumber,
                Amount = p.Amount,
                Status = p.PaymentStatus != null ? p.PaymentStatus.StatusName : null,
                Method = p.Method != null ? p.Method.MethodName : null,
                ExternalPaymentId = p.ExternalPaymentId,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("api/admin/payments/{id:int}")]
    public async Task<ActionResult<AdminPaymentDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var p = await _context.Payments
            .AsNoTracking()
            .Include(x => x.PaymentStatus)
            .Include(x => x.Method)
            .FirstOrDefaultAsync(x => x.PaymentId == id, cancellationToken);
        if (p == null) return NotFound();

        return Ok(new AdminPaymentDetailsDto
        {
            PaymentId = p.PaymentId,
            OrderId = p.OrderId,
            Amount = p.Amount,
            PaymentStatusId = p.PaymentStatusId,
            Status = p.PaymentStatus != null ? p.PaymentStatus.StatusName : null,
            MethodId = p.MethodId,
            Method = p.Method != null ? p.Method.MethodName : null,
            ExternalPaymentId = p.ExternalPaymentId,
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt
        });
    }

    [HttpPost("api/admin/payments")]
    public async Task<ActionResult<AdminPaymentDetailsDto>> Create([FromBody] AdminPaymentCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var order = await _context.AppOrders
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId, cancellationToken);
        if (order == null) return BadRequest("Заказ не найден");

        var statusId = string.IsNullOrWhiteSpace(dto.Status)
            ? null
            : await ResolvePaymentStatusIdAsync(dto.Status, cancellationToken);

        var p = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            MethodId = dto.MethodId,
            ExternalPaymentId = dto.ExternalPaymentId,
            PaidAt = dto.PaidAt,
            PaymentStatusId = statusId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(p);
        await _context.SaveChangesAsync(cancellationToken);

        return await Get(p.PaymentId, cancellationToken);
    }

    [HttpPatch("api/admin/payments/{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] AdminPaymentStatusPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Status)) return BadRequest("Укажите status");

        var p = await _context.Payments.FirstOrDefaultAsync(x => x.PaymentId == id, cancellationToken);
        if (p == null) return NotFound();

        p.PaymentStatusId = await ResolvePaymentStatusIdAsync(dto.Status, cancellationToken);
        if (dto.Status.Trim().ToLowerInvariant().Contains("paid") || dto.Status.Trim().ToLowerInvariant().Contains("success") ||
            dto.Status.Trim().ToLowerInvariant().Contains("оплач") || dto.Status.Trim().ToLowerInvariant().Contains("completed"))
        {
            p.PaidAt ??= DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/payments/{id:int}/order")]
    public async Task<IActionResult> PatchOrderLink(int id, [FromBody] AdminPaymentOrderPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var orderExists = await _context.AppOrders.AsNoTracking()
            .AnyAsync(o => o.OrderId == dto.OrderId, cancellationToken);
        if (!orderExists) return BadRequest("Заказ не найден");

        var p = await _context.Payments.FirstOrDefaultAsync(x => x.PaymentId == id, cancellationToken);
        if (p == null) return NotFound();

        p.OrderId = dto.OrderId;
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

