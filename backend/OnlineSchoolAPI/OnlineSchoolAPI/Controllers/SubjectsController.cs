using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public SubjectsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubjectDto>>> GetSubjects()
    {
        var subjects = await _context.Subjects
            .Select(s => new SubjectDto
            {
                SubjectId = s.SubjectId,
                SubjectName = s.SubjectName,
                Description = s.Description,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(subjects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubjectDto>> GetSubject(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        return Ok(new SubjectDto
        {
            SubjectId = subject.SubjectId,
            SubjectName = subject.SubjectName,
            Description = subject.Description,
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<SubjectDto>> CreateSubject(CreateSubjectDto dto)
    {
        var subject = new Subject
        {
            SubjectName = dto.SubjectName,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubject), new { id = subject.SubjectId }, new SubjectDto
        {
            SubjectId = subject.SubjectId,
            SubjectName = subject.SubjectName,
            Description = subject.Description,
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubject(int id, UpdateSubjectDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        if (dto.SubjectName != null) subject.SubjectName = dto.SubjectName;
        if (dto.Description != null) subject.Description = dto.Description;
        if (dto.IsActive.HasValue) subject.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

