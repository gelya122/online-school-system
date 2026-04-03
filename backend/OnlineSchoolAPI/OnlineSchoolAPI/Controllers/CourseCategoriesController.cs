using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseCategoriesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public CourseCategoriesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseCategoryDto>>> GetCourseCategories()
    {
        var categories = await _context.CourseCategories
            .Select(c => new CourseCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                SubjectId = c.SubjectId,
                ExamId = c.ExamId
            })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseCategoryDto>> GetCourseCategory(int id)
    {
        var category = await _context.CourseCategories.FindAsync(id);
        if (category == null) return NotFound();

        return Ok(new CourseCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            SubjectId = category.SubjectId,
            ExamId = category.ExamId
        });
    }

    [HttpPost]
    public async Task<ActionResult<CourseCategoryDto>> CreateCourseCategory(CreateCourseCategoryDto dto)
    {
        var category = new CourseCategory
        {
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            SubjectId = dto.SubjectId,
            ExamId = dto.ExamId
        };

        _context.CourseCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourseCategory), new { id = category.CategoryId }, new CourseCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            SubjectId = category.SubjectId,
            ExamId = category.ExamId
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourseCategory(int id, UpdateCourseCategoryDto dto)
    {
        var category = await _context.CourseCategories.FindAsync(id);
        if (category == null) return NotFound();

        if (dto.CategoryName != null) category.CategoryName = dto.CategoryName;
        if (dto.Description != null) category.Description = dto.Description;
        if (dto.SubjectId.HasValue) category.SubjectId = dto.SubjectId;
        if (dto.ExamId.HasValue) category.ExamId = dto.ExamId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseCategory(int id)
    {
        var category = await _context.CourseCategories.FindAsync(id);
        if (category == null) return NotFound();

        _context.CourseCategories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

