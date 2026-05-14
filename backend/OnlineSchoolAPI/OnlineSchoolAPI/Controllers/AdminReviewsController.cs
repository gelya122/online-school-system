using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminReviewsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminReviewsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/reviews")]
    public async Task<ActionResult<IReadOnlyList<AdminReviewListRowDto>>> GetList(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var list = await _context.Reviews.AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.Student)
            .OrderByDescending(r => r.CreatedAt ?? DateTime.MinValue)
            .Take(500)
            .Select(r => new AdminReviewListRowDto
            {
                ReviewId = r.ReviewId,
                StudentId = r.StudentId,
                StudentName = ((r.Student.LastName ?? "") + " " + (r.Student.FirstName ?? "")).Trim(),
                CourseId = r.CourseId,
                CourseTitle = r.Course.Title,
                Rating = r.Rating,
                Comment = r.Comment,
                IsPublished = r.IsPublished,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPatch("api/admin/reviews/{id:int}/published")]
    public async Task<IActionResult> SetPublished(int id, [FromBody] AdminReviewPublishedDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var r = await _context.Reviews.FirstOrDefaultAsync(x => x.ReviewId == id, cancellationToken);
        if (r == null) return NotFound();

        r.IsPublished = dto.IsPublished;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
