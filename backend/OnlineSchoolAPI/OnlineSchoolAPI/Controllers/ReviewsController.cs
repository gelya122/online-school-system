using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public ReviewsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    private static bool IsRatingOutOfRange(int? rating) =>
        rating is { } r && (r < 1 || r > 5);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
    {
        var reviews = await _context.Reviews
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                StudentId = r.StudentId,
                CourseId = r.CourseId,
                Rating = r.Rating,
                Comment = r.Comment,
                IsPublished = r.IsPublished,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        return Ok(new ReviewDto
        {
            ReviewId = review.ReviewId,
            StudentId = review.StudentId,
            CourseId = review.CourseId,
            Rating = review.Rating,
            Comment = review.Comment,
            IsPublished = review.IsPublished,
            CreatedAt = review.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto dto)
    {
        if (IsRatingOutOfRange(dto.Rating))
            return BadRequest("Оценка должна быть в диапазоне от 1 до 5.");

        var review = new Review
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsPublished = dto.IsPublished
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, new ReviewDto
        {
            ReviewId = review.ReviewId,
            StudentId = review.StudentId,
            CourseId = review.CourseId,
            Rating = review.Rating,
            Comment = review.Comment,
            IsPublished = review.IsPublished,
            CreatedAt = review.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto dto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        if (dto.Rating.HasValue)
        {
            if (IsRatingOutOfRange(dto.Rating))
                return BadRequest("Оценка должна быть в диапазоне от 1 до 5.");
            review.Rating = dto.Rating;
        }
        if (dto.Comment != null) review.Comment = dto.Comment;
        if (dto.IsPublished.HasValue) review.IsPublished = dto.IsPublished;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

