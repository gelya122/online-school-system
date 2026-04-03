using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonTypesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public LessonTypesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LessonTypeDto>>> GetLessonTypes()
    {
        var types = await _context.LessonTypes
            .Select(t => new LessonTypeDto
            {
                TypeId = t.TypeId,
                TypeName = t.TypeName,
                Description = t.Description
            })
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LessonTypeDto>> GetLessonType(int id)
    {
        var type = await _context.LessonTypes.FindAsync(id);
        if (type == null) return NotFound();

        return Ok(new LessonTypeDto
        {
            TypeId = type.TypeId,
            TypeName = type.TypeName,
            Description = type.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<LessonTypeDto>> CreateLessonType(CreateLessonTypeDto dto)
    {
        var type = new LessonType
        {
            TypeName = dto.TypeName,
            Description = dto.Description
        };

        _context.LessonTypes.Add(type);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLessonType), new { id = type.TypeId }, new LessonTypeDto
        {
            TypeId = type.TypeId,
            TypeName = type.TypeName,
            Description = type.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLessonType(int id, UpdateLessonTypeDto dto)
    {
        var type = await _context.LessonTypes.FindAsync(id);
        if (type == null) return NotFound();

        if (dto.TypeName != null) type.TypeName = dto.TypeName;
        if (dto.Description != null) type.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLessonType(int id)
    {
        var type = await _context.LessonTypes.FindAsync(id);
        if (type == null) return NotFound();

        _context.LessonTypes.Remove(type);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

