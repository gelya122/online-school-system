using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AssignmentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAssignments()
    {
        var assignments = await _context.Assignments
            .Select(a => new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                LessonId = a.LessonId,
                Title = a.Title,
                Description = a.Description,
                MaxScore = a.MaxScore,
                DueDaysAfterLesson = a.DueDaysAfterLesson,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
        return Ok(assignments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssignmentDto>> GetAssignment(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) return NotFound();

        return Ok(new AssignmentDto
        {
            AssignmentId = assignment.AssignmentId,
            LessonId = assignment.LessonId,
            Title = assignment.Title,
            Description = assignment.Description,
            MaxScore = assignment.MaxScore,
            DueDaysAfterLesson = assignment.DueDaysAfterLesson,
            CreatedAt = assignment.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentDto>> CreateAssignment(CreateAssignmentDto dto)
    {
        if (await _context.Assignments.AnyAsync(a => a.LessonId == dto.LessonId))
            return BadRequest("К одному уроку можно добавить только одно домашнее задание.");

        var assignment = new Assignment
        {
            LessonId = dto.LessonId,
            Title = dto.Title,
            Description = dto.Description,
            MaxScore = dto.MaxScore,
            DueDaysAfterLesson = dto.DueDaysAfterLesson
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignment), new { id = assignment.AssignmentId }, new AssignmentDto
        {
            AssignmentId = assignment.AssignmentId,
            LessonId = assignment.LessonId,
            Title = assignment.Title,
            Description = assignment.Description,
            MaxScore = assignment.MaxScore,
            DueDaysAfterLesson = assignment.DueDaysAfterLesson,
            CreatedAt = assignment.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignment(int id, UpdateAssignmentDto dto)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) return NotFound();

        if (dto.Title != null) assignment.Title = dto.Title;
        if (dto.Description != null) assignment.Description = dto.Description;
        if (dto.MaxScore.HasValue) assignment.MaxScore = dto.MaxScore.Value;
        if (dto.DueDaysAfterLesson.HasValue) assignment.DueDaysAfterLesson = dto.DueDaysAfterLesson;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) return NotFound();

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
