using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public SubmissionsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetSubmissions()
    {
        var submissions = await _context.Submissions
            .AsNoTracking()
            .Select(s => new SubmissionDto
            {
                SubmissionId = s.SubmissionId,
                EnrollmentId = s.EnrollmentId,
                AssignmentId = s.AssignmentId,
                StudentAnswerText = _context.TestStudentAnswers
                    .Where(a => a.SubmissionId == s.SubmissionId)
                    .OrderBy(a => a.QuestionId)
                    .Select(a => a.ResponseText)
                    .FirstOrDefault(),
                AttachedFileUrl = null,
                AttachedFileName = null,
                SubmittedAt = s.SubmittedAt,
                SubmissionStatusId = s.SubmissionStatusId,
                Score = s.Score,
                TeacherComment = s.TeacherComment,
                GradedAt = s.GradedAt,
                GradedByEmployeeId = s.GradedByEmployeeId,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
        return Ok(submissions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(int id)
    {
        var row = await _context.Submissions.AsNoTracking()
            .Where(s => s.SubmissionId == id)
            .Select(s => new SubmissionDto
            {
                SubmissionId = s.SubmissionId,
                EnrollmentId = s.EnrollmentId,
                AssignmentId = s.AssignmentId,
                StudentAnswerText = _context.TestStudentAnswers
                    .Where(a => a.SubmissionId == s.SubmissionId)
                    .OrderBy(a => a.QuestionId)
                    .Select(a => a.ResponseText)
                    .FirstOrDefault(),
                AttachedFileUrl = null,
                AttachedFileName = null,
                SubmittedAt = s.SubmittedAt,
                SubmissionStatusId = s.SubmissionStatusId,
                Score = s.Score,
                TeacherComment = s.TeacherComment,
                GradedAt = s.GradedAt,
                GradedByEmployeeId = s.GradedByEmployeeId,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (row == null) return NotFound();
        return Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionDto>> CreateSubmission(CreateSubmissionDto dto)
    {
        var submission = new Submission
        {
            EnrollmentId = dto.EnrollmentId,
            AssignmentId = dto.AssignmentId,
            SubmissionStatusId = dto.SubmissionStatusId ?? 1
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubmission), new { id = submission.SubmissionId }, new SubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            EnrollmentId = submission.EnrollmentId,
            AssignmentId = submission.AssignmentId,
            StudentAnswerText = dto.StudentAnswerText,
            AttachedFileUrl = null,
            AttachedFileName = null,
            SubmittedAt = submission.SubmittedAt,
            SubmissionStatusId = submission.SubmissionStatusId,
            Score = submission.Score,
            TeacherComment = submission.TeacherComment,
            GradedAt = submission.GradedAt,
            GradedByEmployeeId = submission.GradedByEmployeeId,
            CreatedAt = submission.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubmission(int id, UpdateSubmissionDto dto)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null) return NotFound();

        if (dto.SubmissionStatusId.HasValue) submission.SubmissionStatusId = dto.SubmissionStatusId.Value;
        if (dto.Score.HasValue) submission.Score = dto.Score;
        if (dto.TeacherComment != null) submission.TeacherComment = dto.TeacherComment;
        if (dto.GradedByEmployeeId.HasValue) submission.GradedByEmployeeId = dto.GradedByEmployeeId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubmission(int id)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null) return NotFound();

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
