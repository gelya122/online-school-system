using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentTypesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AssignmentTypesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentTypeDto>>> GetAssignmentTypes()
    {
        var types = await _context.AssignmentTypes
            .Select(t => new AssignmentTypeDto
            {
                TypeId = t.TypeId,
                TypeName = t.TypeName,
                Description = t.Description
            })
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssignmentTypeDto>> GetAssignmentType(int id)
    {
        var type = await _context.AssignmentTypes.FindAsync(id);
        if (type == null) return NotFound();

        return Ok(new AssignmentTypeDto
        {
            TypeId = type.TypeId,
            TypeName = type.TypeName,
            Description = type.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentTypeDto>> CreateAssignmentType(CreateAssignmentTypeDto dto)
    {
        var type = new AssignmentType
        {
            TypeName = dto.TypeName,
            Description = dto.Description
        };

        _context.AssignmentTypes.Add(type);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignmentType), new { id = type.TypeId }, new AssignmentTypeDto
        {
            TypeId = type.TypeId,
            TypeName = type.TypeName,
            Description = type.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignmentType(int id, UpdateAssignmentTypeDto dto)
    {
        var type = await _context.AssignmentTypes.FindAsync(id);
        if (type == null) return NotFound();

        if (dto.TypeName != null) type.TypeName = dto.TypeName;
        if (dto.Description != null) type.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignmentType(int id)
    {
        var type = await _context.AssignmentTypes.FindAsync(id);
        if (type == null) return NotFound();

        _context.AssignmentTypes.Remove(type);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

