using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;
    private readonly IWebHostEnvironment _env;

    public LessonsController(OnlineSchoolDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("{id:int}/video")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PublicUploadStorage.MaxLessonVideoBytes + 65536)]
    public async Task<ActionResult<AvatarUploadResponseDto>> UploadLessonVideo(
        int id,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Выберите видеофайл.");

        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        await using var ms = new MemoryStream((int)file.Length);
        await file.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        string url;
        try
        {
            url = await PublicUploadStorage.SaveLessonVideoAsync(_env, bytes, file.FileName, id, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        lesson.VideoUrl = url;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AvatarUploadResponseDto { AvatarUrl = url });
    }

    [HttpPost("{id:int}/material")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PublicUploadStorage.MaxLessonMaterialBytes + 65536)]
    public async Task<ActionResult<LessonMaterialDto>> UploadLessonMaterial(
        int id,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Выберите файл.");

        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        await using var ms = new MemoryStream((int)file.Length);
        await file.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        (string url, string fileName, string? contentType, int sizeKb) meta;
        try
        {
            meta = await PublicUploadStorage.SaveLessonMaterialAsync(_env, bytes, file.FileName, id, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var material = new LessonMaterial
        {
            LessonId = id,
            FileName = meta.fileName,
            FileUrl = meta.url,
            FileType = meta.contentType,
            FileSizeKb = meta.sizeKb,
            UploadedAt = DateTime.UtcNow
        };
        _context.LessonMaterials.Add(material);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new LessonMaterialDto
        {
            MaterialId = material.MaterialId,
            LessonId = material.LessonId,
            FileName = material.FileName,
            FileUrl = material.FileUrl,
            FileType = material.FileType,
            FileSizeKb = material.FileSizeKb,
            DownloadCount = material.DownloadCount,
            UploadedAt = material.UploadedAt
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessons()
    {
        var lessons = await _context.Lessons
            .Select(l => new LessonDto
            {
                LessonId = l.LessonId,
                ModuleId = l.ModuleId,
                Title = l.Title,
                LessonTypeId = l.LessonTypeId,
                Content = l.Content,
                VideoUrl = l.VideoUrl,
                DurationMinutes = l.DurationMinutes,
                LessonOrder = l.LessonOrder,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LessonDto>> GetLesson(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        return Ok(new LessonDto
        {
            LessonId = lesson.LessonId,
            ModuleId = lesson.ModuleId,
            Title = lesson.Title,
            LessonTypeId = lesson.LessonTypeId,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DurationMinutes = lesson.DurationMinutes,
            LessonOrder = lesson.LessonOrder,
            CreatedAt = lesson.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<LessonDto>> CreateLesson(CreateLessonDto dto)
    {
        var lesson = new Lesson
        {
            ModuleId = dto.ModuleId,
            Title = dto.Title,
            LessonTypeId = dto.LessonTypeId,
            Content = dto.Content,
            VideoUrl = dto.VideoUrl,
            DurationMinutes = dto.DurationMinutes,
            LessonOrder = dto.LessonOrder
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLesson), new { id = lesson.LessonId }, new LessonDto
        {
            LessonId = lesson.LessonId,
            ModuleId = lesson.ModuleId,
            Title = lesson.Title,
            LessonTypeId = lesson.LessonTypeId,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DurationMinutes = lesson.DurationMinutes,
            LessonOrder = lesson.LessonOrder,
            CreatedAt = lesson.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLesson(int id, UpdateLessonDto dto)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        if (dto.Title != null) lesson.Title = dto.Title;
        if (dto.LessonTypeId.HasValue) lesson.LessonTypeId = dto.LessonTypeId.Value;
        if (dto.Content != null) lesson.Content = dto.Content;
        if (dto.VideoUrl != null) lesson.VideoUrl = dto.VideoUrl;
        if (dto.DurationMinutes.HasValue) lesson.DurationMinutes = dto.DurationMinutes;
        if (dto.LessonOrder.HasValue) lesson.LessonOrder = dto.LessonOrder.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

