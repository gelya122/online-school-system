using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentVariantsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AssignmentVariantsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentVariantDto>>> GetAssignmentVariants()
    {
        var variants = await _context.AssignmentVariants
            .Select(v => new AssignmentVariantDto
            {
                VariantId = v.VariantId,
                AssignmentId = v.AssignmentId,
                VariantText = v.VariantText,
                IsCorrect = v.IsCorrect,
                VariantOrder = v.VariantOrder
            })
            .ToListAsync();
        return Ok(variants);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssignmentVariantDto>> GetAssignmentVariant(int id)
    {
        var variant = await _context.AssignmentVariants.FindAsync(id);
        if (variant == null) return NotFound();

        return Ok(new AssignmentVariantDto
        {
            VariantId = variant.VariantId,
            AssignmentId = variant.AssignmentId,
            VariantText = variant.VariantText,
            IsCorrect = variant.IsCorrect,
            VariantOrder = variant.VariantOrder
        });
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentVariantDto>> CreateAssignmentVariant(CreateAssignmentVariantDto dto)
    {
        var variant = new AssignmentVariant
        {
            AssignmentId = dto.AssignmentId,
            VariantText = dto.VariantText,
            IsCorrect = dto.IsCorrect,
            VariantOrder = dto.VariantOrder
        };

        _context.AssignmentVariants.Add(variant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignmentVariant), new { id = variant.VariantId }, new AssignmentVariantDto
        {
            VariantId = variant.VariantId,
            AssignmentId = variant.AssignmentId,
            VariantText = variant.VariantText,
            IsCorrect = variant.IsCorrect,
            VariantOrder = variant.VariantOrder
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignmentVariant(int id, UpdateAssignmentVariantDto dto)
    {
        var variant = await _context.AssignmentVariants.FindAsync(id);
        if (variant == null) return NotFound();

        if (dto.VariantText != null) variant.VariantText = dto.VariantText;
        if (dto.IsCorrect.HasValue) variant.IsCorrect = dto.IsCorrect;
        if (dto.VariantOrder.HasValue) variant.VariantOrder = dto.VariantOrder;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignmentVariant(int id)
    {
        var variant = await _context.AssignmentVariants.FindAsync(id);
        if (variant == null) return NotFound();

        _context.AssignmentVariants.Remove(variant);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

