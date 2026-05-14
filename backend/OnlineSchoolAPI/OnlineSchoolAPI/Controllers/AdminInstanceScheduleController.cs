using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminInstanceScheduleController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminInstanceScheduleController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/course-instances/{instanceId:int}/schedule")]
    public async Task<ActionResult<IReadOnlyList<AdminInstanceScheduleRowDto>>> GetSchedule(int instanceId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var instance = await _context.CourseInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InstanceId == instanceId && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        var startDate = instance.StartDate;

        var rows = await _context.CourseSchedulePlans
            .AsNoTracking()
            .Where(p => p.InstanceId == instanceId)
            .OrderBy(p => p.Lesson.Module.ModuleOrder)
            .ThenBy(p => p.Lesson.LessonOrder)
            .Select(p => new AdminInstanceScheduleRowDto
            {
                ScheduleId = p.PlanId,
                LessonId = p.LessonId,
                ModuleTitle = p.Lesson.Module.Title,
                LessonTitle = p.Lesson.Title,
                ModuleOrder = p.Lesson.Module.ModuleOrder,
                LessonOrder = p.LessonOrder ?? p.Lesson.LessonOrder,
                ReleaseDayOffset = p.ReleaseDayOffset,
                OpenDate = startDate.AddDays(p.ReleaseDayOffset),
                OpenTime = p.ReleaseTime,
                AutoOpen = true,
                OpenedManually = false,
                ScheduledAt = p.ScheduledAt,
                IsPublished = p.IsPublished
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPost("api/admin/course-instances/{instanceId:int}/schedule/generate")]
    public async Task<IActionResult> Generate(int instanceId, [FromBody] AdminGenerateInstanceScheduleDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var instance = await _context.CourseInstances
            .FirstOrDefaultAsync(i => i.InstanceId == instanceId && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        if (dto.OverwriteExisting)
        {
            var existing = await _context.CourseSchedulePlans.Where(p => p.InstanceId == instanceId).ToListAsync(cancellationToken);
            if (existing.Count > 0)
                _context.CourseSchedulePlans.RemoveRange(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var defaultTime = dto.DefaultOpenTime ?? new TimeOnly(9, 0);
        var (plans, err) = await CourseInstanceSchedulePlanGenerator.TryBuildPlansAsync(
            _context,
            instanceId,
            instance.CourseId,
            instance.StartDate,
            instance.EndDate,
            instance.ScheduleRulesJson,
            instance.LessonsPerWeek,
            defaultTime,
            cancellationToken);

        if (plans == null)
            return BadRequest(err ?? "Не удалось построить расписание.");

        _context.CourseSchedulePlans.AddRange(plans);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("api/admin/course-instance-schedule/{scheduleId:int}")]
    public async Task<IActionResult> Update(int scheduleId, [FromBody] AdminUpdateInstanceScheduleDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var plan = await _context.CourseSchedulePlans
            .Include(p => p.Instance)
            .FirstOrDefaultAsync(p => p.PlanId == scheduleId, cancellationToken);
        if (plan == null) return NotFound();

        var start = plan.Instance.StartDate;

        if (dto.ReleaseDayOffset.HasValue)
            plan.ReleaseDayOffset = Math.Max(0, dto.ReleaseDayOffset.Value);
        else if (dto.OpenDate.HasValue)
        {
            var offset = dto.OpenDate.Value.DayNumber - start.DayNumber;
            if (offset < 0) offset = 0;
            plan.ReleaseDayOffset = offset;
        }
        else
            return BadRequest("Укажите release_day_offset или open_date.");

        if (dto.OpenTime.HasValue)
            plan.ReleaseTime = dto.OpenTime;

        var releaseTime = plan.ReleaseTime ?? new TimeOnly(9, 0);
        var openDay = start.AddDays(plan.ReleaseDayOffset);
        plan.ScheduledAt = DateTime.SpecifyKind(openDay.ToDateTime(releaseTime), DateTimeKind.Utc);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/course-instance-schedule/{scheduleId:int}/open-for-all")]
    public async Task<IActionResult> OpenForAll(int scheduleId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var plan = await _context.CourseSchedulePlans.FirstOrDefaultAsync(p => p.PlanId == scheduleId, cancellationToken);
        if (plan == null) return NotFound();

        var now = DateTime.UtcNow;
        plan.IsPublished = true;
        plan.ScheduledAt = now;

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.InstanceId == plan.InstanceId)
            .Select(e => e.EnrollmentId)
            .ToListAsync(cancellationToken);

        foreach (var enrollmentId in enrollments)
        {
            var exists = await _context.StudentProgresses.AnyAsync(
                p => p.EnrollmentId == enrollmentId && p.LessonId == plan.LessonId, cancellationToken);
            if (exists)
                continue;
            _context.StudentProgresses.Add(new StudentProgress
            {
                EnrollmentId = enrollmentId,
                LessonId = plan.LessonId,
                IsCompleted = false,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/course-instance-schedule/{scheduleId:int}/open-for-student")]
    public Task<IActionResult> OpenForStudent(int scheduleId, [FromBody] AdminOpenForStudentDto dto, CancellationToken cancellationToken)
    {
        _ = scheduleId;
        _ = dto;
        _ = cancellationToken;
        return Task.FromResult<IActionResult>(BadRequest(
            "Отключено: доступ к уроку определяется course_schedule_plan (scheduled_at, is_published), а не индивидуальным открытием."));
    }

    [HttpPost("api/admin/course-instance-schedule/{scheduleId:int}/close-for-student")]
    public Task<IActionResult> CloseForStudent(int scheduleId, [FromBody] AdminOpenForStudentDto dto, CancellationToken cancellationToken)
    {
        _ = scheduleId;
        _ = dto;
        _ = cancellationToken;
        return Task.FromResult<IActionResult>(BadRequest(
            "Отключено: закрытие урока для одного студента не используется; см. расписание потока и статус записи."));
    }
}

