using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqCategoriesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public FaqCategoriesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaqCategoryDto>>> GetFaqCategories()
    {
        var categories = await _context.FaqCategories
            .Select(c => new FaqCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryOrder = c.CategoryOrder
            })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FaqCategoryDto>> GetFaqCategory(int id)
    {
        var category = await _context.FaqCategories.FindAsync(id);
        if (category == null) return NotFound();

        return Ok(new FaqCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryOrder = category.CategoryOrder
        });
    }

    [HttpPost]
    public async Task<ActionResult<FaqCategoryDto>> CreateFaqCategory(CreateFaqCategoryDto dto)
    {
        var category = new FaqCategory
        {
            CategoryName = dto.CategoryName,
            CategoryOrder = dto.CategoryOrder
        };

        _context.FaqCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFaqCategory), new { id = category.CategoryId }, new FaqCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryOrder = category.CategoryOrder
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFaqCategory(int id, UpdateFaqCategoryDto dto)
    {
        var category = await _context.FaqCategories.FindAsync(id);
        if (category == null) return NotFound();

        if (dto.CategoryName != null) category.CategoryName = dto.CategoryName;
        if (dto.CategoryOrder.HasValue) category.CategoryOrder = dto.CategoryOrder;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaqCategory(int id)
    {
        var category = await _context.FaqCategories.FindAsync(id);
        if (category == null) return NotFound();

        _context.FaqCategories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

