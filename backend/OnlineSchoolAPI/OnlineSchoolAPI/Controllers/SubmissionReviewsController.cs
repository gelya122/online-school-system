using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionReviewsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public SubmissionReviewsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubmissionReviewDto>>> GetSubmissionReviews()
    {
        var reviews = await _context.SubmissionReviews
            .Select(r => new SubmissionReviewDto
            {
                ReviewId = r.ReviewId,
                SubmissionId = r.SubmissionId,
                QuestionNumber = r.QuestionNumber,
                StudentVariantId = r.StudentVariantId,
                IsCorrect = r.IsCorrect,
                TeacherComment = r.TeacherComment,
                PointsAwarded = r.PointsAwarded,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubmissionReviewDto>> GetSubmissionReview(int id)
    {
        var review = await _context.SubmissionReviews.FindAsync(id);
        if (review == null) return NotFound();

        return Ok(new SubmissionReviewDto
        {
            ReviewId = review.ReviewId,
            SubmissionId = review.SubmissionId,
            QuestionNumber = review.QuestionNumber,
            StudentVariantId = review.StudentVariantId,
            IsCorrect = review.IsCorrect,
            TeacherComment = review.TeacherComment,
            PointsAwarded = review.PointsAwarded,
            CreatedAt = review.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionReviewDto>> CreateSubmissionReview(CreateSubmissionReviewDto dto)
    {
        var review = new SubmissionReview
        {
            SubmissionId = dto.SubmissionId,
            QuestionNumber = dto.QuestionNumber,
            StudentVariantId = dto.StudentVariantId,
            IsCorrect = dto.IsCorrect,
            TeacherComment = dto.TeacherComment,
            PointsAwarded = dto.PointsAwarded
        };

        _context.SubmissionReviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubmissionReview), new { id = review.ReviewId }, new SubmissionReviewDto
        {
            ReviewId = review.ReviewId,
            SubmissionId = review.SubmissionId,
            QuestionNumber = review.QuestionNumber,
            StudentVariantId = review.StudentVariantId,
            IsCorrect = review.IsCorrect,
            TeacherComment = review.TeacherComment,
            PointsAwarded = review.PointsAwarded,
            CreatedAt = review.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubmissionReview(int id, UpdateSubmissionReviewDto dto)
    {
        var review = await _context.SubmissionReviews.FindAsync(id);
        if (review == null) return NotFound();

        if (dto.QuestionNumber.HasValue) review.QuestionNumber = dto.QuestionNumber;
        if (dto.StudentVariantId.HasValue) review.StudentVariantId = dto.StudentVariantId;
        if (dto.IsCorrect.HasValue) review.IsCorrect = dto.IsCorrect;
        if (dto.TeacherComment != null) review.TeacherComment = dto.TeacherComment;
        if (dto.PointsAwarded.HasValue) review.PointsAwarded = dto.PointsAwarded;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubmissionReview(int id)
    {
        var review = await _context.SubmissionReviews.FindAsync(id);
        if (review == null) return NotFound();

        _context.SubmissionReviews.Remove(review);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

