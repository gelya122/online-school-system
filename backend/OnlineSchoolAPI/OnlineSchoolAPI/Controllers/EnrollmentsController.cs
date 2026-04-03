using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public EnrollmentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
    {
        var enrollments = await _context.Enrollments
            .Select(e => new EnrollmentDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                InstanceId = e.InstanceId,
                AssignedTeacherId = e.AssignedTeacherId,
                EnrolledAt = e.EnrolledAt,
                EnrollmentStatusId = e.EnrollmentStatusId,
                CompletedAt = e.CompletedAt,
                FinalScore = e.FinalScore
            })
            .ToListAsync();
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound();

        return Ok(new EnrollmentDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            InstanceId = enrollment.InstanceId,
            AssignedTeacherId = enrollment.AssignedTeacherId,
            EnrolledAt = enrollment.EnrolledAt,
            EnrollmentStatusId = enrollment.EnrollmentStatusId,
            CompletedAt = enrollment.CompletedAt,
            FinalScore = enrollment.FinalScore
        });
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentDto>> CreateEnrollment(CreateEnrollmentDto dto)
    {
        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            InstanceId = dto.InstanceId,
            AssignedTeacherId = dto.AssignedTeacherId,
            EnrollmentStatusId = dto.EnrollmentStatusId
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.EnrollmentId }, new EnrollmentDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            InstanceId = enrollment.InstanceId,
            AssignedTeacherId = enrollment.AssignedTeacherId,
            EnrolledAt = enrollment.EnrolledAt,
            EnrollmentStatusId = enrollment.EnrollmentStatusId,
            CompletedAt = enrollment.CompletedAt,
            FinalScore = enrollment.FinalScore
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEnrollment(int id, UpdateEnrollmentDto dto)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound();

        if (dto.AssignedTeacherId.HasValue) enrollment.AssignedTeacherId = dto.AssignedTeacherId;
        if (dto.EnrollmentStatusId.HasValue) enrollment.EnrollmentStatusId = dto.EnrollmentStatusId;
        if (dto.CompletedAt.HasValue) enrollment.CompletedAt = dto.CompletedAt;
        if (dto.FinalScore.HasValue) enrollment.FinalScore = dto.FinalScore;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound();

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

