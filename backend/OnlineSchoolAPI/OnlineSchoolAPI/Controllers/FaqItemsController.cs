using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqItemsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public FaqItemsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaqItemDto>>> GetFaqItems()
    {
        var items = await _context.FaqItems
            .Select(i => new FaqItemDto
            {
                FaqId = i.FaqId,
                CategoryId = i.CategoryId,
                Question = i.Question,
                Answer = i.Answer,
                IsActive = i.IsActive,
                ItemOrder = i.ItemOrder,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FaqItemDto>> GetFaqItem(int id)
    {
        var item = await _context.FaqItems.FindAsync(id);
        if (item == null) return NotFound();

        return Ok(new FaqItemDto
        {
            FaqId = item.FaqId,
            CategoryId = item.CategoryId,
            Question = item.Question,
            Answer = item.Answer,
            IsActive = item.IsActive,
            ItemOrder = item.ItemOrder,
            CreatedAt = item.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<FaqItemDto>> CreateFaqItem(CreateFaqItemDto dto)
    {
        var item = new FaqItem
        {
            CategoryId = dto.CategoryId,
            Question = dto.Question,
            Answer = dto.Answer,
            IsActive = dto.IsActive,
            ItemOrder = dto.ItemOrder
        };

        _context.FaqItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFaqItem), new { id = item.FaqId }, new FaqItemDto
        {
            FaqId = item.FaqId,
            CategoryId = item.CategoryId,
            Question = item.Question,
            Answer = item.Answer,
            IsActive = item.IsActive,
            ItemOrder = item.ItemOrder,
            CreatedAt = item.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFaqItem(int id, UpdateFaqItemDto dto)
    {
        var item = await _context.FaqItems.FindAsync(id);
        if (item == null) return NotFound();

        if (dto.CategoryId.HasValue) item.CategoryId = dto.CategoryId;
        if (dto.Question != null) item.Question = dto.Question;
        if (dto.Answer != null) item.Answer = dto.Answer;
        if (dto.IsActive.HasValue) item.IsActive = dto.IsActive;
        if (dto.ItemOrder.HasValue) item.ItemOrder = dto.ItemOrder;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaqItem(int id)
    {
        var item = await _context.FaqItems.FindAsync(id);
        if (item == null) return NotFound();

        _context.FaqItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

