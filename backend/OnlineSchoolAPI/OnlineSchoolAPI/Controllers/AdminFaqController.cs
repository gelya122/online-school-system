using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminFaqController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminFaqController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/faq")]
    public async Task<ActionResult<IReadOnlyList<AdminFaqCategoryDto>>> GetTree(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var cats = await _context.FaqCategories.AsNoTracking()
            .Include(c => c.FaqItems)
            .OrderBy(c => c.CategoryOrder ?? int.MaxValue).ThenBy(c => c.CategoryId)
            .ToListAsync(cancellationToken);

        var dto = cats.Select(c => new AdminFaqCategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            CategoryOrder = c.CategoryOrder,
            Items = c.FaqItems
                .OrderBy(i => i.ItemOrder ?? int.MaxValue).ThenBy(i => i.FaqId)
                .Select(i => new AdminFaqItemDto
                {
                    FaqId = i.FaqId,
                    CategoryId = i.CategoryId,
                    Question = i.Question,
                    Answer = i.Answer,
                    ItemOrder = i.ItemOrder,
                    IsActive = i.IsActive
                })
                .ToList()
        }).ToList();

        return Ok(dto);
    }

    [HttpPost("api/admin/faq/categories")]
    public async Task<ActionResult<AdminFaqCategoryDto>> CreateCategory([FromBody] AdminFaqCategoryUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) return BadRequest("Укажите categoryName");

        var e = new FaqCategory
        {
            CategoryName = dto.CategoryName.Trim(),
            CategoryOrder = dto.CategoryOrder
        };
        _context.FaqCategories.Add(e);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminFaqCategoryDto
        {
            CategoryId = e.CategoryId,
            CategoryName = e.CategoryName,
            CategoryOrder = e.CategoryOrder,
            Items = []
        });
    }

    [HttpPut("api/admin/faq/categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminFaqCategoryUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) return BadRequest("Укажите categoryName");

        var e = await _context.FaqCategories.FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (e == null) return NotFound();

        e.CategoryName = dto.CategoryName.Trim();
        e.CategoryOrder = dto.CategoryOrder;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/faq/categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var e = await _context.FaqCategories.Include(c => c.FaqItems).FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (e == null) return NotFound();

        _context.FaqItems.RemoveRange(e.FaqItems);
        _context.FaqCategories.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/faq/items")]
    public async Task<ActionResult<AdminFaqItemDto>> CreateItem([FromBody] AdminFaqItemUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Question)) return BadRequest("Укажите question");
        if (string.IsNullOrWhiteSpace(dto.Answer)) return BadRequest("Укажите answer");

        if (dto.CategoryId is > 0)
        {
            var exists = await _context.FaqCategories.AsNoTracking().AnyAsync(c => c.CategoryId == dto.CategoryId.Value, cancellationToken);
            if (!exists) return BadRequest("Категория не найдена");
        }

        var e = new FaqItem
        {
            CategoryId = dto.CategoryId > 0 ? dto.CategoryId : null,
            Question = dto.Question.Trim(),
            Answer = dto.Answer.Trim(),
            ItemOrder = dto.ItemOrder,
            IsActive = dto.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        };
        _context.FaqItems.Add(e);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminFaqItemDto
        {
            FaqId = e.FaqId,
            CategoryId = e.CategoryId,
            Question = e.Question,
            Answer = e.Answer,
            ItemOrder = e.ItemOrder,
            IsActive = e.IsActive
        });
    }

    [HttpPut("api/admin/faq/items/{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] AdminFaqItemUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Question)) return BadRequest("Укажите question");
        if (string.IsNullOrWhiteSpace(dto.Answer)) return BadRequest("Укажите answer");

        if (dto.CategoryId is > 0)
        {
            var exists = await _context.FaqCategories.AsNoTracking().AnyAsync(c => c.CategoryId == dto.CategoryId.Value, cancellationToken);
            if (!exists) return BadRequest("Категория не найдена");
        }

        var e = await _context.FaqItems.FirstOrDefaultAsync(x => x.FaqId == id, cancellationToken);
        if (e == null) return NotFound();

        e.CategoryId = dto.CategoryId > 0 ? dto.CategoryId : null;
        e.Question = dto.Question.Trim();
        e.Answer = dto.Answer.Trim();
        e.ItemOrder = dto.ItemOrder;
        if (dto.IsActive.HasValue) e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/faq/items/{id:int}")]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var e = await _context.FaqItems.FirstOrDefaultAsync(x => x.FaqId == id, cancellationToken);
        if (e == null) return NotFound();

        _context.FaqItems.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
