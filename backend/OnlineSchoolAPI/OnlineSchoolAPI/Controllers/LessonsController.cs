using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public LessonsController(OnlineSchoolDbContext context)
    {
        _context = context;
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
                IsFreePreview = l.IsFreePreview,
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
            IsFreePreview = lesson.IsFreePreview,
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
            LessonOrder = dto.LessonOrder,
            IsFreePreview = dto.IsFreePreview
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
            IsFreePreview = lesson.IsFreePreview,
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
        if (dto.IsFreePreview.HasValue) lesson.IsFreePreview = dto.IsFreePreview;

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

