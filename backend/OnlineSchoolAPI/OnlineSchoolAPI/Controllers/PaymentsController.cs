using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public PaymentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
    {
        var payments = await _context.Payments
            .Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                ExternalPaymentId = p.ExternalPaymentId,
                Amount = p.Amount,
                PaymentStatusId = p.PaymentStatusId,
                PaymentMethod = p.PaymentMethod,
                CardLastFour = p.CardLastFour,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        return Ok(new PaymentDto
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            ExternalPaymentId = payment.ExternalPaymentId,
            Amount = payment.Amount,
            PaymentStatusId = payment.PaymentStatusId,
            PaymentMethod = payment.PaymentMethod,
            CardLastFour = payment.CardLastFour,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentDto dto)
    {
        var payment = new Payment
        {
            OrderId = dto.OrderId,
            ExternalPaymentId = dto.ExternalPaymentId,
            Amount = dto.Amount,
            PaymentStatusId = dto.PaymentStatusId,
            PaymentMethod = dto.PaymentMethod,
            CardLastFour = dto.CardLastFour
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, new PaymentDto
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            ExternalPaymentId = payment.ExternalPaymentId,
            Amount = payment.Amount,
            PaymentStatusId = payment.PaymentStatusId,
            PaymentMethod = payment.PaymentMethod,
            CardLastFour = payment.CardLastFour,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, UpdatePaymentDto dto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        if (dto.ExternalPaymentId != null) payment.ExternalPaymentId = dto.ExternalPaymentId;
        if (dto.PaymentStatusId.HasValue) payment.PaymentStatusId = dto.PaymentStatusId;
        if (dto.PaymentMethod != null) payment.PaymentMethod = dto.PaymentMethod;
        if (dto.CardLastFour != null) payment.CardLastFour = dto.CardLastFour;
        if (dto.PaidAt.HasValue) payment.PaidAt = dto.PaidAt;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

