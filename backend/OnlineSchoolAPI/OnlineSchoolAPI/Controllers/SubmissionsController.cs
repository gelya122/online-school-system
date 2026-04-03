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
            .Select(s => new SubmissionDto
            {
                SubmissionId = s.SubmissionId,
                ProgressId = s.ProgressId,
                AssignmentId = s.AssignmentId,
                StudentAnswerText = s.StudentAnswerText,
                AttachedFileUrl = s.AttachedFileUrl,
                AttachedFileName = s.AttachedFileName,
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
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null) return NotFound();

        return Ok(new SubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            ProgressId = submission.ProgressId,
            AssignmentId = submission.AssignmentId,
            StudentAnswerText = submission.StudentAnswerText,
            AttachedFileUrl = submission.AttachedFileUrl,
            AttachedFileName = submission.AttachedFileName,
            SubmittedAt = submission.SubmittedAt,
            SubmissionStatusId = submission.SubmissionStatusId,
            Score = submission.Score,
            TeacherComment = submission.TeacherComment,
            GradedAt = submission.GradedAt,
            GradedByEmployeeId = submission.GradedByEmployeeId,
            CreatedAt = submission.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionDto>> CreateSubmission(CreateSubmissionDto dto)
    {
        var submission = new Submission
        {
            ProgressId = dto.ProgressId,
            AssignmentId = dto.AssignmentId,
            StudentAnswerText = dto.StudentAnswerText,
            AttachedFileUrl = dto.AttachedFileUrl,
            AttachedFileName = dto.AttachedFileName,
            SubmissionStatusId = dto.SubmissionStatusId
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubmission), new { id = submission.SubmissionId }, new SubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            ProgressId = submission.ProgressId,
            AssignmentId = submission.AssignmentId,
            StudentAnswerText = submission.StudentAnswerText,
            AttachedFileUrl = submission.AttachedFileUrl,
            AttachedFileName = submission.AttachedFileName,
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

        if (dto.StudentAnswerText != null) submission.StudentAnswerText = dto.StudentAnswerText;
        if (dto.AttachedFileUrl != null) submission.AttachedFileUrl = dto.AttachedFileUrl;
        if (dto.AttachedFileName != null) submission.AttachedFileName = dto.AttachedFileName;
        if (dto.SubmissionStatusId.HasValue) submission.SubmissionStatusId = dto.SubmissionStatusId;
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

