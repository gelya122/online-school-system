using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppOrdersController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AppOrdersController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppOrderDto>>> GetAppOrders()
    {
        var orders = await _context.AppOrders
            .Select(o => new AppOrderDto
            {
                OrderId = o.OrderId,
                StudentId = o.StudentId,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.FinalAmount,
                OrderStatusId = o.OrderStatusId,
                MethodId = o.MethodId,
                MethodName = o.Method != null ? o.Method.MethodName : null,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            })
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppOrderDto>> GetAppOrder(int id)
    {
        var order = await _context.AppOrders
            .Include(o => o.Method)
            .FirstOrDefaultAsync(o => o.OrderId == id);
        if (order == null) return NotFound();

        return Ok(new AppOrderDto
        {
            OrderId = order.OrderId,
            StudentId = order.StudentId,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            OrderStatusId = order.OrderStatusId,
            MethodId = order.MethodId,
            MethodName = order.Method?.MethodName,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<AppOrderDto>> CreateAppOrder(CreateAppOrderDto dto)
    {
        var order = new AppOrder
        {
            StudentId = dto.StudentId,
            OrderNumber = dto.OrderNumber,
            TotalAmount = dto.TotalAmount,
            DiscountAmount = dto.DiscountAmount,
            FinalAmount = dto.FinalAmount,
            OrderStatusId = dto.OrderStatusId,
            MethodId = dto.MethodId
        };

        _context.AppOrders.Add(order);
        await _context.SaveChangesAsync();

        await _context.Entry(order).Reference(o => o.Method).LoadAsync();

        return CreatedAtAction(nameof(GetAppOrder), new { id = order.OrderId }, new AppOrderDto
        {
            OrderId = order.OrderId,
            StudentId = order.StudentId,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            OrderStatusId = order.OrderStatusId,
            MethodId = order.MethodId,
            MethodName = order.Method?.MethodName,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppOrder(int id, UpdateAppOrderDto dto)
    {
        var order = await _context.AppOrders.FindAsync(id);
        if (order == null) return NotFound();

        if (dto.DiscountAmount.HasValue) order.DiscountAmount = dto.DiscountAmount;
        if (dto.FinalAmount.HasValue) order.FinalAmount = dto.FinalAmount.Value;
        if (dto.OrderStatusId.HasValue) order.OrderStatusId = dto.OrderStatusId;
        if (dto.MethodId.HasValue) order.MethodId = dto.MethodId;
        if (dto.PaidAt.HasValue) order.PaidAt = dto.PaidAt;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppOrder(int id)
    {
        var order = await _context.AppOrders.FindAsync(id);
        if (order == null) return NotFound();

        _context.AppOrders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
