using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentProgressesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public StudentProgressesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentProgressDto>>> GetStudentProgresses()
    {
        var progresses = await _context.StudentProgresses
            .Select(p => new StudentProgressDto
            {
                ProgressId = p.ProgressId,
                EnrollmentId = p.EnrollmentId,
                LessonId = p.LessonId,
                AccessId = p.AccessId,
                IsCompleted = p.IsCompleted,
                CompletedAt = p.CompletedAt,
                WatchTimeSeconds = p.WatchTimeSeconds,
                LastAccessed = p.LastAccessed,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(progresses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentProgressDto>> GetStudentProgress(int id)
    {
        var progress = await _context.StudentProgresses.FindAsync(id);
        if (progress == null) return NotFound();

        return Ok(new StudentProgressDto
        {
            ProgressId = progress.ProgressId,
            EnrollmentId = progress.EnrollmentId,
            LessonId = progress.LessonId,
            AccessId = progress.AccessId,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt,
            WatchTimeSeconds = progress.WatchTimeSeconds,
            LastAccessed = progress.LastAccessed,
            CreatedAt = progress.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<StudentProgressDto>> CreateStudentProgress(CreateStudentProgressDto dto)
    {
        var progress = new StudentProgress
        {
            EnrollmentId = dto.EnrollmentId,
            LessonId = dto.LessonId,
            AccessId = dto.AccessId,
            IsCompleted = dto.IsCompleted,
            WatchTimeSeconds = dto.WatchTimeSeconds
        };

        _context.StudentProgresses.Add(progress);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudentProgress), new { id = progress.ProgressId }, new StudentProgressDto
        {
            ProgressId = progress.ProgressId,
            EnrollmentId = progress.EnrollmentId,
            LessonId = progress.LessonId,
            AccessId = progress.AccessId,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt,
            WatchTimeSeconds = progress.WatchTimeSeconds,
            LastAccessed = progress.LastAccessed,
            CreatedAt = progress.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudentProgress(int id, UpdateStudentProgressDto dto)
    {
        var progress = await _context.StudentProgresses.FindAsync(id);
        if (progress == null) return NotFound();

        if (dto.IsCompleted.HasValue) progress.IsCompleted = dto.IsCompleted;
        if (dto.CompletedAt.HasValue) progress.CompletedAt = dto.CompletedAt;
        if (dto.WatchTimeSeconds.HasValue) progress.WatchTimeSeconds = dto.WatchTimeSeconds;
        if (dto.LastAccessed.HasValue) progress.LastAccessed = dto.LastAccessed;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudentProgress(int id)
    {
        var progress = await _context.StudentProgresses.FindAsync(id);
        if (progress == null) return NotFound();

        _context.StudentProgresses.Remove(progress);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

