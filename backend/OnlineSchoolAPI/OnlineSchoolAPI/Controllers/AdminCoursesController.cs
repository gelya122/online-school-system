using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/admin/courses")]
[Authorize]
public sealed class AdminCoursesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminCoursesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminCourseListRowDto>>> GetList(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? subjectId,
        [FromQuery] int? examId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var q = _context.Courses
            .AsNoTracking()
            .Include(c => c.Category).ThenInclude(cat => cat.Subject)
            .Include(c => c.Category).ThenInclude(cat => cat.Exam)
            .Where(c => c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(c => (c.Title ?? "").ToLower().Contains(s));
        }

        if (categoryId.HasValue)
            q = q.Where(c => c.CategoryId == categoryId.Value);

        if (subjectId.HasValue)
            q = q.Where(c => c.Category.SubjectId == subjectId.Value);

        if (examId.HasValue)
            q = q.Where(c => c.Category.ExamId == examId.Value);

        if (isActive.HasValue)
        {
            if (isActive.Value)
                q = q.Where(c => c.IsActive == null || c.IsActive == true);
            else
                q = q.Where(c => c.IsActive == false);
        }

        // counts
        var rows = await q
            .OrderByDescending(c => c.CreatedAt)
            .Take(500)
            .Select(c => new
            {
                Course = c,
                ModulesCount = c.CourseModules.Count(m => m.DeletedAt == null),
                LessonsCount = c.CourseModules
                    .Where(m => m.DeletedAt == null)
                    .SelectMany(m => m.Lessons)
                    .Count(l => l.DeletedAt == null)
            })
            .Select(x => new AdminCourseListRowDto
            {
                CourseId = x.Course.CourseId,
                Title = x.Course.Title,
                CategoryId = x.Course.CategoryId,
                CategoryName = x.Course.Category.CategoryName,
                SubjectId = x.Course.Category.SubjectId,
                SubjectName = x.Course.Category.Subject != null ? x.Course.Category.Subject.SubjectName : null,
                ExamId = x.Course.Category.ExamId,
                ExamName = x.Course.Category.Exam != null ? x.Course.Category.Exam.ExamName : null,
                Price = x.Course.Price,
                DiscountPrice = x.Course.DiscountPrice,
                IsActive = x.Course.IsActive != false,
                ModulesCount = x.ModulesCount,
                LessonsCount = x.LessonsCount,
                CreatedAt = x.Course.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminCourseDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category).ThenInclude(cat => cat.Subject)
            .Include(c => c.Category).ThenInclude(cat => cat.Exam)
            .FirstOrDefaultAsync(c => c.CourseId == id && c.DeletedAt == null, cancellationToken);

        if (course == null) return NotFound();

        var homeworkSummaries = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.Lesson.Module.CourseId == course.CourseId
                        && a.Lesson.DeletedAt == null
                        && a.Lesson.Module.DeletedAt == null)
            .OrderBy(a => a.Lesson.Module.ModuleOrder)
            .ThenBy(a => a.Lesson.LessonOrder)
            .ThenBy(a => a.Title)
            .Select(a => new AdminCourseHomeworkSummaryDto
            {
                AssignmentId = a.AssignmentId,
                AssignmentTitle = a.Title,
                LessonId = a.LessonId,
                LessonTitle = a.Lesson.Title,
                ModuleId = a.Lesson.ModuleId,
                ModuleTitle = a.Lesson.Module.Title,
                QuestionsCount = a.TestQuestions.Count
            })
            .ToListAsync(cancellationToken);

        var modules = await _context.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == course.CourseId && m.DeletedAt == null)
            .OrderBy(m => m.ModuleOrder)
            .Select(m => new AdminCourseModuleDto
            {
                ModuleId = m.ModuleId,
                Title = m.Title,
                ModuleOrder = m.ModuleOrder,
                LessonsCount = m.Lessons.Count(l => l.DeletedAt == null)
            })
            .ToListAsync(cancellationToken);

        var lessons = await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Module)
            .Include(l => l.LessonType)
            .Where(l => l.DeletedAt == null && l.Module.CourseId == course.CourseId && l.Module.DeletedAt == null)
            .OrderBy(l => l.Module.ModuleOrder).ThenBy(l => l.LessonOrder)
            .Select(l => new AdminCourseLessonDto
            {
                LessonId = l.LessonId,
                ModuleId = l.ModuleId,
                ModuleTitle = l.Module.Title,
                Title = l.Title,
                LessonOrder = l.LessonOrder,
                LessonTypeId = l.LessonTypeId,
                LessonTypeName = l.LessonType != null ? l.LessonType.TypeName : ""
            })
            .ToListAsync(cancellationToken);

        return Ok(new AdminCourseDetailsDto
        {
            CourseId = course.CourseId,
            Title = course.Title,
            ShortDescription = course.ShortDescription,
            Description = course.Description,
            CategoryId = course.CategoryId,
            CategoryName = course.Category.CategoryName,
            SubjectId = course.Category.SubjectId,
            SubjectName = course.Category.Subject != null ? course.Category.Subject.SubjectName : null,
            ExamId = course.Category.ExamId,
            ExamName = course.Category.Exam != null ? course.Category.Exam.ExamName : null,
            CoverImgUrl = course.CoverImgUrl,
            Price = course.Price,
            DiscountPrice = course.DiscountPrice,
            TotalHours = course.TotalHours,
            WhatYouGet = course.WhatYouGet,
            IsActive = course.IsActive != false,
            Modules = modules,
            Lessons = lessons,
            HomeworkSummaries = homeworkSummaries
        });
    }

    [HttpPost]
    public async Task<ActionResult<AdminCourseDetailsDto>> Create([FromBody] AdminCourseUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название курса.");
        if (dto.Price < 0)
            return BadRequest("Цена не может быть отрицательной.");
        if (dto.DiscountPrice is < 0)
            return BadRequest("Скидочная цена не может быть отрицательной.");

        var cat = await _context.CourseCategories.FirstOrDefaultAsync(x => x.CategoryId == dto.CategoryId, cancellationToken);
        if (cat == null)
            return BadRequest("Неизвестная категория.");

        var now = DateTime.UtcNow;
        var course = new Course
        {
            Title = dto.Title.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(dto.ShortDescription) ? null : dto.ShortDescription.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            CoverImgUrl = string.IsNullOrWhiteSpace(dto.CoverImgUrl) ? null : dto.CoverImgUrl.Trim(),
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            TotalHours = dto.TotalHours,
            WhatYouGet = string.IsNullOrWhiteSpace(dto.WhatYouGet) ? null : dto.WhatYouGet.Trim(),
            IsActive = false,
            CreatedAt = now
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);
        return await GetById(course.CourseId, cancellationToken);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminCourseUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.DeletedAt == null, cancellationToken);
        if (course == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название курса.");
        if (dto.Price < 0)
            return BadRequest("Цена не может быть отрицательной.");
        if (dto.DiscountPrice is < 0)
            return BadRequest("Скидочная цена не может быть отрицательной.");

        var cat = await _context.CourseCategories.FirstOrDefaultAsync(x => x.CategoryId == dto.CategoryId, cancellationToken);
        if (cat == null)
            return BadRequest("Неизвестная категория.");

        course.Title = dto.Title.Trim();
        course.ShortDescription = string.IsNullOrWhiteSpace(dto.ShortDescription) ? null : dto.ShortDescription.Trim();
        course.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        course.CategoryId = dto.CategoryId;
        course.CoverImgUrl = string.IsNullOrWhiteSpace(dto.CoverImgUrl) ? null : dto.CoverImgUrl.Trim();
        course.Price = dto.Price;
        course.DiscountPrice = dto.DiscountPrice;
        course.TotalHours = dto.TotalHours;
        course.WhatYouGet = string.IsNullOrWhiteSpace(dto.WhatYouGet) ? null : dto.WhatYouGet.Trim();
        course.IsActive = dto.IsActive;
        course.CreatedAt ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.DeletedAt == null, cancellationToken);
        if (course == null) return NotFound();
        course.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/hide")]
    public async Task<IActionResult> Hide(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.DeletedAt == null, cancellationToken);
        if (course == null) return NotFound();
        course.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.DeletedAt == null, cancellationToken);
        if (course == null) return NotFound();

        course.DeletedAt = DateTime.UtcNow;
        course.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

