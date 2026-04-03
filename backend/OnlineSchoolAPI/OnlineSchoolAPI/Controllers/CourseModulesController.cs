using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseModulesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public CourseModulesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseModuleDto>>> GetCourseModules()
    {
        var modules = await _context.CourseModules
            .Select(m => new CourseModuleDto
            {
                ModuleId = m.ModuleId,
                CourseId = m.CourseId,
                Title = m.Title,
                Description = m.Description,
                ModuleOrder = m.ModuleOrder,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
        return Ok(modules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseModuleDto>> GetCourseModule(int id)
    {
        var module = await _context.CourseModules.FindAsync(id);
        if (module == null) return NotFound();

        return Ok(new CourseModuleDto
        {
            ModuleId = module.ModuleId,
            CourseId = module.CourseId,
            Title = module.Title,
            Description = module.Description,
            ModuleOrder = module.ModuleOrder,
            CreatedAt = module.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<CourseModuleDto>> CreateCourseModule(CreateCourseModuleDto dto)
    {
        var module = new CourseModule
        {
            CourseId = dto.CourseId,
            Title = dto.Title,
            Description = dto.Description,
            ModuleOrder = dto.ModuleOrder
        };

        _context.CourseModules.Add(module);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourseModule), new { id = module.ModuleId }, new CourseModuleDto
        {
            ModuleId = module.ModuleId,
            CourseId = module.CourseId,
            Title = module.Title,
            Description = module.Description,
            ModuleOrder = module.ModuleOrder,
            CreatedAt = module.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourseModule(int id, UpdateCourseModuleDto dto)
    {
        var module = await _context.CourseModules.FindAsync(id);
        if (module == null) return NotFound();

        if (dto.Title != null) module.Title = dto.Title;
        if (dto.Description != null) module.Description = dto.Description;
        if (dto.ModuleOrder.HasValue) module.ModuleOrder = dto.ModuleOrder.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseModule(int id)
    {
        var module = await _context.CourseModules.FindAsync(id);
        if (module == null) return NotFound();

        _context.CourseModules.Remove(module);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

