using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentStatusesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public PaymentStatusesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentStatusDto>>> GetPaymentStatuses()
    {
        var statuses = await _context.PaymentStatuses
            .Select(s => new PaymentStatusDto
            {
                StatusId = s.StatusId,
                StatusName = s.StatusName,
                Description = s.Description
            })
            .ToListAsync();
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentStatusDto>> GetPaymentStatus(int id)
    {
        var status = await _context.PaymentStatuses.FindAsync(id);
        if (status == null) return NotFound();

        return Ok(new PaymentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<PaymentStatusDto>> CreatePaymentStatus(CreatePaymentStatusDto dto)
    {
        var status = new PaymentStatus
        {
            StatusName = dto.StatusName,
            Description = dto.Description
        };

        _context.PaymentStatuses.Add(status);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPaymentStatus), new { id = status.StatusId }, new PaymentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, UpdatePaymentStatusDto dto)
    {
        var status = await _context.PaymentStatuses.FindAsync(id);
        if (status == null) return NotFound();

        if (dto.StatusName != null) status.StatusName = dto.StatusName;
        if (dto.Description != null) status.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePaymentStatus(int id)
    {
        var status = await _context.PaymentStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _context.PaymentStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

