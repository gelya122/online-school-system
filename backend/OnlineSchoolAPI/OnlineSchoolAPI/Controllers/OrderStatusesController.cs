using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderStatusesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public OrderStatusesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderStatusDto>>> GetOrderStatuses()
    {
        var statuses = await _context.OrderStatuses
            .Select(s => new OrderStatusDto
            {
                StatusId = s.StatusId,
                StatusName = s.StatusName,
                Description = s.Description
            })
            .ToListAsync();
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderStatusDto>> GetOrderStatus(int id)
    {
        var status = await _context.OrderStatuses.FindAsync(id);
        if (status == null) return NotFound();

        return Ok(new OrderStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrderStatusDto>> CreateOrderStatus(CreateOrderStatusDto dto)
    {
        var status = new OrderStatus
        {
            StatusName = dto.StatusName,
            Description = dto.Description
        };

        _context.OrderStatuses.Add(status);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderStatus), new { id = status.StatusId }, new OrderStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
    {
        var status = await _context.OrderStatuses.FindAsync(id);
        if (status == null) return NotFound();

        if (dto.StatusName != null) status.StatusName = dto.StatusName;
        if (dto.Description != null) status.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderStatus(int id)
    {
        var status = await _context.OrderStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _context.OrderStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

