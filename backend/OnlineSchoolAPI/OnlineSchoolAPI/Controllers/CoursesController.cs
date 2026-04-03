using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public CoursesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses([FromQuery] int? subjectId, [FromQuery] int? examId)
    {
        var query = _context.Courses.AsQueryable();

        if (subjectId.HasValue)
        {
            query = query.Where(c => c.Category.SubjectId == subjectId.Value);
        }

        if (examId.HasValue)
        {
            query = query.Where(c => c.Category.ExamId == examId.Value);
        }

        var courses = await query
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                ShortDescription = c.ShortDescription,
                CategoryId = c.CategoryId,
                SubjectId = c.Category.SubjectId,
                ExamId = c.Category.ExamId,
                CoverImgUrl = c.CoverImgUrl,
                Price = c.Price,
                DiscountPrice = c.DiscountPrice,
                TotalHours = c.TotalHours,
                WhatYouGet = c.WhatYouGet,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        var course = await _context.Courses
            .Where(c => c.CourseId == id)
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                ShortDescription = c.ShortDescription,
                CategoryId = c.CategoryId,
                SubjectId = c.Category.SubjectId,
                ExamId = c.Category.ExamId,
                CoverImgUrl = c.CoverImgUrl,
                Price = c.Price,
                DiscountPrice = c.DiscountPrice,
                TotalHours = c.TotalHours,
                WhatYouGet = c.WhatYouGet,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto dto)
    {
        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            CategoryId = dto.CategoryId,
            CoverImgUrl = dto.CoverImgUrl,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            TotalHours = dto.TotalHours,
            WhatYouGet = dto.WhatYouGet,
            IsActive = dto.IsActive
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var categoryMeta = await _context.CourseCategories
            .Where(c => c.CategoryId == course.CategoryId)
            .Select(c => new { c.SubjectId, c.ExamId })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, new CourseDto
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            ShortDescription = course.ShortDescription,
            CategoryId = course.CategoryId,
            SubjectId = categoryMeta?.SubjectId,
            ExamId = categoryMeta?.ExamId,
            CoverImgUrl = course.CoverImgUrl,
            Price = course.Price,
            DiscountPrice = course.DiscountPrice,
            TotalHours = course.TotalHours,
            WhatYouGet = course.WhatYouGet,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        if (dto.Title != null) course.Title = dto.Title;
        if (dto.Description != null) course.Description = dto.Description;
        if (dto.ShortDescription != null) course.ShortDescription = dto.ShortDescription;
        if (dto.CategoryId.HasValue) course.CategoryId = dto.CategoryId.Value;
        if (dto.CoverImgUrl != null) course.CoverImgUrl = dto.CoverImgUrl;
        if (dto.Price.HasValue) course.Price = dto.Price.Value;
        if (dto.DiscountPrice.HasValue) course.DiscountPrice = dto.DiscountPrice;
        if (dto.TotalHours.HasValue) course.TotalHours = dto.TotalHours;
        if (dto.WhatYouGet != null) course.WhatYouGet = dto.WhatYouGet;
        if (dto.IsActive.HasValue) course.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

