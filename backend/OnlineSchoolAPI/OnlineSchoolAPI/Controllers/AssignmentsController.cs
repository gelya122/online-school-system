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
                AssignmentTypeId = a.AssignmentTypeId,
                MaxScore = a.MaxScore,
                DueDaysAfterLesson = a.DueDaysAfterLesson,
                CorrectAnswer = a.CorrectAnswer,
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
            AssignmentTypeId = assignment.AssignmentTypeId,
            MaxScore = assignment.MaxScore,
            DueDaysAfterLesson = assignment.DueDaysAfterLesson,
            CorrectAnswer = assignment.CorrectAnswer,
            CreatedAt = assignment.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentDto>> CreateAssignment(CreateAssignmentDto dto)
    {
        var assignment = new Assignment
        {
            LessonId = dto.LessonId,
            Title = dto.Title,
            Description = dto.Description,
            AssignmentTypeId = dto.AssignmentTypeId,
            MaxScore = dto.MaxScore,
            DueDaysAfterLesson = dto.DueDaysAfterLesson,
            CorrectAnswer = dto.CorrectAnswer
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignment), new { id = assignment.AssignmentId }, new AssignmentDto
        {
            AssignmentId = assignment.AssignmentId,
            LessonId = assignment.LessonId,
            Title = assignment.Title,
            Description = assignment.Description,
            AssignmentTypeId = assignment.AssignmentTypeId,
            MaxScore = assignment.MaxScore,
            DueDaysAfterLesson = assignment.DueDaysAfterLesson,
            CorrectAnswer = assignment.CorrectAnswer,
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
        if (dto.AssignmentTypeId.HasValue) assignment.AssignmentTypeId = dto.AssignmentTypeId.Value;
        if (dto.MaxScore.HasValue) assignment.MaxScore = dto.MaxScore.Value;
        if (dto.DueDaysAfterLesson.HasValue) assignment.DueDaysAfterLesson = dto.DueDaysAfterLesson;
        if (dto.CorrectAnswer != null) assignment.CorrectAnswer = dto.CorrectAnswer;

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

