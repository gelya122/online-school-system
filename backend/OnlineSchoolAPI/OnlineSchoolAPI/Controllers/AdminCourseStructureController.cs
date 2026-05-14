using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminCourseStructureController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminCourseStructureController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/courses/{courseId:int}/modules")]
    public async Task<ActionResult<IReadOnlyList<AdminCourseModuleRowDto>>> GetModules(int courseId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var exists = await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var list = await _context.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.ModuleOrder)
            .ThenBy(m => m.ModuleId)
            .Select(m => new AdminCourseModuleRowDto
            {
                ModuleId = m.ModuleId,
                CourseId = m.CourseId,
                Title = m.Title,
                Description = m.Description,
                ModuleOrder = m.ModuleOrder,
                IsActive = m.DeletedAt == null
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/courses/{courseId:int}/modules")]
    public async Task<ActionResult<AdminCourseModuleRowDto>> CreateModule(int courseId, [FromBody] AdminCourseModuleUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название блока.");

        var exists = await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var now = DateTime.UtcNow;
        var module = new CourseModule
        {
            CourseId = courseId,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            ModuleOrder = dto.ModuleOrder,
            CreatedAt = now,
            DeletedAt = dto.IsActive ? null : now
        };
        _context.CourseModules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminCourseModuleRowDto
        {
            ModuleId = module.ModuleId,
            CourseId = module.CourseId,
            Title = module.Title,
            Description = module.Description,
            ModuleOrder = module.ModuleOrder,
            IsActive = module.DeletedAt == null
        });
    }

    [HttpPut("api/admin/course-modules/{moduleId:int}")]
    public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] AdminCourseModuleUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var module = await _context.CourseModules.FirstOrDefaultAsync(m => m.ModuleId == moduleId, cancellationToken);
        if (module == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название блока.");

        module.Title = dto.Title.Trim();
        module.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        module.ModuleOrder = dto.ModuleOrder;
        module.DeletedAt = dto.IsActive ? null : (module.DeletedAt ?? DateTime.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/course-modules/{moduleId:int}")]
    public async Task<IActionResult> DeleteModule(int moduleId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var module = await _context.CourseModules.FirstOrDefaultAsync(m => m.ModuleId == moduleId, cancellationToken);
        if (module == null) return NotFound();

        // soft delete
        module.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/course-modules/reorder")]
    public async Task<IActionResult> ReorderModules([FromBody] AdminReorderRequestDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (dto.Items.Count == 0) return BadRequest("Пустой список.");

        var ids = dto.Items.Select(x => x.Id).ToList();
        var modules = await _context.CourseModules.Where(m => ids.Contains(m.ModuleId)).ToListAsync(cancellationToken);
        if (modules.Count != ids.Count) return BadRequest("Некоторые блоки не найдены.");

        foreach (var item in dto.Items)
        {
            var m = modules.First(x => x.ModuleId == item.Id);
            m.ModuleOrder = item.Order;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/course-modules/{moduleId:int}/lessons")]
    public async Task<ActionResult<IReadOnlyList<AdminLessonRowDto>>> GetLessons(int moduleId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var module = await _context.CourseModules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == moduleId, cancellationToken);
        if (module == null) return NotFound();

        var list = await _context.Lessons
            .AsNoTracking()
            .Include(l => l.LessonType)
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.LessonOrder)
            .ThenBy(l => l.LessonId)
            .Select(l => new AdminLessonRowDto
            {
                LessonId = l.LessonId,
                ModuleId = l.ModuleId,
                Title = l.Title,
                LessonTypeId = l.LessonTypeId,
                LessonTypeName = l.LessonType.TypeName,
                Content = l.Content,
                VideoUrl = l.VideoUrl,
                DurationMinutes = l.DurationMinutes,
                LessonOrder = l.LessonOrder,
                IsActive = l.DeletedAt == null
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/course-modules/{moduleId:int}/lessons")]
    public async Task<ActionResult<AdminLessonRowDto>> CreateLesson(int moduleId, [FromBody] AdminLessonCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var module = await _context.CourseModules.FirstOrDefaultAsync(m => m.ModuleId == moduleId, cancellationToken);
        if (module == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название урока.");

        var now = DateTime.UtcNow;
        var lesson = new Lesson
        {
            ModuleId = moduleId,
            Title = dto.Title.Trim(),
            LessonTypeId = dto.LessonTypeId,
            Content = string.IsNullOrWhiteSpace(dto.Content) ? null : dto.Content,
            VideoUrl = string.IsNullOrWhiteSpace(dto.VideoUrl) ? null : dto.VideoUrl.Trim(),
            DurationMinutes = dto.DurationMinutes,
            LessonOrder = dto.LessonOrder,
            CreatedAt = now,
            DeletedAt = dto.IsActive ? null : now
        };
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminLessonRowDto
        {
            LessonId = lesson.LessonId,
            ModuleId = lesson.ModuleId,
            Title = lesson.Title,
            LessonTypeId = lesson.LessonTypeId,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DurationMinutes = lesson.DurationMinutes,
            LessonOrder = lesson.LessonOrder,
            IsActive = lesson.DeletedAt == null
        });
    }

    [HttpPut("api/admin/lessons/{lessonId:int}")]
    public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] AdminLessonUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == lessonId, cancellationToken);
        if (lesson == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название урока.");

        lesson.Title = dto.Title.Trim();
        lesson.LessonTypeId = dto.LessonTypeId;
        lesson.Content = string.IsNullOrWhiteSpace(dto.Content) ? null : dto.Content;
        lesson.VideoUrl = string.IsNullOrWhiteSpace(dto.VideoUrl) ? null : dto.VideoUrl.Trim();
        lesson.DurationMinutes = dto.DurationMinutes;
        lesson.LessonOrder = dto.LessonOrder;
        lesson.DeletedAt = dto.IsActive ? null : (lesson.DeletedAt ?? DateTime.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/lessons/{lessonId:int}")]
    public async Task<IActionResult> DeleteLesson(int lessonId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == lessonId, cancellationToken);
        if (lesson == null) return NotFound();

        lesson.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/lessons/reorder")]
    public async Task<IActionResult> ReorderLessons([FromBody] AdminReorderRequestDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (dto.Items.Count == 0) return BadRequest("Пустой список.");

        var ids = dto.Items.Select(x => x.Id).ToList();
        var lessons = await _context.Lessons.Where(l => ids.Contains(l.LessonId)).ToListAsync(cancellationToken);
        if (lessons.Count != ids.Count) return BadRequest("Некоторые уроки не найдены.");

        foreach (var item in dto.Items)
        {
            var l = lessons.First(x => x.LessonId == item.Id);
            l.LessonOrder = item.Order;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/lessons/{lessonId:int}/materials")]
    public async Task<ActionResult<IReadOnlyList<AdminLessonMaterialRowDto>>> GetMaterials(int lessonId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var exists = await _context.Lessons.AnyAsync(l => l.LessonId == lessonId, cancellationToken);
        if (!exists) return NotFound();

        var list = await _context.LessonMaterials
            .AsNoTracking()
            .Where(m => m.LessonId == lessonId)
            .OrderByDescending(m => m.UploadedAt)
            .ThenBy(m => m.MaterialId)
            .Select(m => new AdminLessonMaterialRowDto
            {
                MaterialId = m.MaterialId,
                LessonId = m.LessonId,
                FileName = m.FileName,
                FileUrl = m.FileUrl,
                FileType = m.FileType,
                FileSizeKb = m.FileSizeKb,
                UploadedAt = m.UploadedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/lessons/{lessonId:int}/materials")]
    public async Task<ActionResult<AdminLessonMaterialRowDto>> CreateMaterial(int lessonId, [FromBody] AdminLessonMaterialCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var exists = await _context.Lessons.AnyAsync(l => l.LessonId == lessonId, cancellationToken);
        if (!exists) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.FileName) || string.IsNullOrWhiteSpace(dto.FileUrl))
            return BadRequest("Укажите название и ссылку на материал.");

        var material = new LessonMaterial
        {
            LessonId = lessonId,
            FileName = dto.FileName.Trim(),
            FileUrl = dto.FileUrl.Trim(),
            FileType = string.IsNullOrWhiteSpace(dto.FileType) ? null : dto.FileType.Trim(),
            FileSizeKb = dto.FileSizeKb,
            DownloadCount = 0,
            UploadedAt = DateTime.UtcNow
        };
        _context.LessonMaterials.Add(material);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminLessonMaterialRowDto
        {
            MaterialId = material.MaterialId,
            LessonId = material.LessonId,
            FileName = material.FileName,
            FileUrl = material.FileUrl,
            FileType = material.FileType,
            FileSizeKb = material.FileSizeKb,
            UploadedAt = material.UploadedAt
        });
    }

    [HttpDelete("api/admin/lesson-materials/{materialId:int}")]
    public async Task<IActionResult> DeleteMaterial(int materialId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var material = await _context.LessonMaterials.FirstOrDefaultAsync(m => m.MaterialId == materialId, cancellationToken);
        if (material == null) return NotFound();

        _context.LessonMaterials.Remove(material);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("api/admin/lesson-materials/{materialId:int}")]
    public async Task<IActionResult> UpdateMaterial(int materialId, [FromBody] AdminLessonMaterialUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var material = await _context.LessonMaterials.FirstOrDefaultAsync(m => m.MaterialId == materialId, cancellationToken);
        if (material == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.FileName) || string.IsNullOrWhiteSpace(dto.FileUrl))
            return BadRequest("Укажите название и ссылку на материал.");

        material.FileName = dto.FileName.Trim();
        material.FileUrl = dto.FileUrl.Trim();
        material.FileType = string.IsNullOrWhiteSpace(dto.FileType) ? null : dto.FileType.Trim();
        material.FileSizeKb = dto.FileSizeKb;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

