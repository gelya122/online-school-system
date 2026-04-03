using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentLessonAccessesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public StudentLessonAccessesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentLessonAccessDto>>> GetStudentLessonAccesses()
    {
        var accesses = await _context.StudentLessonAccesses
            .Select(a => new StudentLessonAccessDto
            {
                AccessId = a.AccessId,
                EnrollmentId = a.EnrollmentId,
                LessonId = a.LessonId,
                PlanId = a.PlanId,
                PlannedAccessDate = a.PlannedAccessDate,
                PlannedAccessTime = a.PlannedAccessTime,
                ActualOpenDatetime = a.ActualOpenDatetime,
                IsAvailable = a.IsAvailable,
                OpenedByEmployeeId = a.OpenedByEmployeeId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
        return Ok(accesses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentLessonAccessDto>> GetStudentLessonAccess(int id)
    {
        var access = await _context.StudentLessonAccesses.FindAsync(id);
        if (access == null) return NotFound();

        return Ok(new StudentLessonAccessDto
        {
            AccessId = access.AccessId,
            EnrollmentId = access.EnrollmentId,
            LessonId = access.LessonId,
            PlanId = access.PlanId,
            PlannedAccessDate = access.PlannedAccessDate,
            PlannedAccessTime = access.PlannedAccessTime,
            ActualOpenDatetime = access.ActualOpenDatetime,
            IsAvailable = access.IsAvailable,
            OpenedByEmployeeId = access.OpenedByEmployeeId,
            CreatedAt = access.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<StudentLessonAccessDto>> CreateStudentLessonAccess(CreateStudentLessonAccessDto dto)
    {
        var access = new StudentLessonAccess
        {
            EnrollmentId = dto.EnrollmentId,
            LessonId = dto.LessonId,
            PlanId = dto.PlanId,
            PlannedAccessDate = dto.PlannedAccessDate,
            PlannedAccessTime = dto.PlannedAccessTime,
            IsAvailable = dto.IsAvailable
        };

        _context.StudentLessonAccesses.Add(access);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudentLessonAccess), new { id = access.AccessId }, new StudentLessonAccessDto
        {
            AccessId = access.AccessId,
            EnrollmentId = access.EnrollmentId,
            LessonId = access.LessonId,
            PlanId = access.PlanId,
            PlannedAccessDate = access.PlannedAccessDate,
            PlannedAccessTime = access.PlannedAccessTime,
            ActualOpenDatetime = access.ActualOpenDatetime,
            IsAvailable = access.IsAvailable,
            OpenedByEmployeeId = access.OpenedByEmployeeId,
            CreatedAt = access.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudentLessonAccess(int id, UpdateStudentLessonAccessDto dto)
    {
        var access = await _context.StudentLessonAccesses.FindAsync(id);
        if (access == null) return NotFound();

        if (dto.PlannedAccessDate.HasValue) access.PlannedAccessDate = dto.PlannedAccessDate.Value;
        if (dto.PlannedAccessTime.HasValue) access.PlannedAccessTime = dto.PlannedAccessTime;
        if (dto.ActualOpenDatetime.HasValue) access.ActualOpenDatetime = dto.ActualOpenDatetime;
        if (dto.IsAvailable.HasValue) access.IsAvailable = dto.IsAvailable;
        if (dto.OpenedByEmployeeId.HasValue) access.OpenedByEmployeeId = dto.OpenedByEmployeeId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudentLessonAccess(int id)
    {
        var access = await _context.StudentLessonAccesses.FindAsync(id);
        if (access == null) return NotFound();

        _context.StudentLessonAccesses.Remove(access);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

