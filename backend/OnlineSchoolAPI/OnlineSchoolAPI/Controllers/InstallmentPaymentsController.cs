using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstallmentPaymentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public InstallmentPaymentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstallmentPaymentDto>>> GetInstallmentPayments()
    {
        var payments = await _context.InstallmentPayments
            .Select(p => new InstallmentPaymentDto
            {
                InstallmentPaymentId = p.InstallmentPaymentId,
                PlanId = p.PlanId,
                InstallmentNumber = p.InstallmentNumber,
                DueDate = p.DueDate,
                Amount = p.Amount,
                PaymentStatus = p.PaymentStatus,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InstallmentPaymentDto>> GetInstallmentPayment(int id)
    {
        var payment = await _context.InstallmentPayments.FindAsync(id);
        if (payment == null) return NotFound();

        return Ok(new InstallmentPaymentDto
        {
            InstallmentPaymentId = payment.InstallmentPaymentId,
            PlanId = payment.PlanId,
            InstallmentNumber = payment.InstallmentNumber,
            DueDate = payment.DueDate,
            Amount = payment.Amount,
            PaymentStatus = payment.PaymentStatus,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<InstallmentPaymentDto>> CreateInstallmentPayment(CreateInstallmentPaymentDto dto)
    {
        var payment = new InstallmentPayment
        {
            PlanId = dto.PlanId,
            InstallmentNumber = dto.InstallmentNumber,
            DueDate = dto.DueDate,
            Amount = dto.Amount,
            PaymentStatus = dto.PaymentStatus
        };

        _context.InstallmentPayments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInstallmentPayment), new { id = payment.InstallmentPaymentId }, new InstallmentPaymentDto
        {
            InstallmentPaymentId = payment.InstallmentPaymentId,
            PlanId = payment.PlanId,
            InstallmentNumber = payment.InstallmentNumber,
            DueDate = payment.DueDate,
            Amount = payment.Amount,
            PaymentStatus = payment.PaymentStatus,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInstallmentPayment(int id, UpdateInstallmentPaymentDto dto)
    {
        var payment = await _context.InstallmentPayments.FindAsync(id);
        if (payment == null) return NotFound();

        if (dto.PaymentStatus != null) payment.PaymentStatus = dto.PaymentStatus;
        if (dto.PaidAt.HasValue) payment.PaidAt = dto.PaidAt;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInstallmentPayment(int id)
    {
        var payment = await _context.InstallmentPayments.FindAsync(id);
        if (payment == null) return NotFound();

        _context.InstallmentPayments.Remove(payment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

