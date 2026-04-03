using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderItemsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public OrderItemsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetOrderItems()
    {
        var items = await _context.OrderItems
            .Select(i => new OrderItemDto
            {
                OrderItemId = i.OrderItemId,
                OrderId = i.OrderId,
                CourseId = i.CourseId,
                InstanceId = i.InstanceId,
                Price = i.Price,
                Quantity = i.Quantity,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderItemDto>> GetOrderItem(int id)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item == null) return NotFound();

        return Ok(new OrderItemDto
        {
            OrderItemId = item.OrderItemId,
            OrderId = item.OrderId,
            CourseId = item.CourseId,
            InstanceId = item.InstanceId,
            Price = item.Price,
            Quantity = item.Quantity,
            CreatedAt = item.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrderItemDto>> CreateOrderItem(CreateOrderItemDto dto)
    {
        var item = new OrderItem
        {
            OrderId = dto.OrderId,
            CourseId = dto.CourseId,
            InstanceId = dto.InstanceId,
            Price = dto.Price,
            Quantity = dto.Quantity
        };

        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderItem), new { id = item.OrderItemId }, new OrderItemDto
        {
            OrderItemId = item.OrderItemId,
            OrderId = item.OrderId,
            CourseId = item.CourseId,
            InstanceId = item.InstanceId,
            Price = item.Price,
            Quantity = item.Quantity,
            CreatedAt = item.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderItem(int id, UpdateOrderItemDto dto)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item == null) return NotFound();

        if (dto.InstanceId.HasValue) item.InstanceId = dto.InstanceId;
        if (dto.Price.HasValue) item.Price = dto.Price.Value;
        if (dto.Quantity.HasValue) item.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderItem(int id)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item == null) return NotFound();

        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

