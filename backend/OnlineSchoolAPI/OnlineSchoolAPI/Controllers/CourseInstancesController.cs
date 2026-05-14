using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseInstancesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public CourseInstancesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseInstanceDto>>> GetCourseInstances()
    {
        var instances = await _context.CourseInstances
            .Select(i => new CourseInstanceDto
            {
                InstanceId = i.InstanceId,
                CourseId = i.CourseId,
                InstanceName = i.InstanceName,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                TotalWeeks = i.TotalWeeks,
                LessonsPerWeek = i.LessonsPerWeek,
                ScheduleDescription = i.ScheduleDescription,
                MaxStudents = i.MaxStudents,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
                StatusCode = i.InstanceStatus.Code
            })
            .ToListAsync();
        return Ok(instances);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseInstanceDto>> GetCourseInstance(int id)
    {
        var row = await _context.CourseInstances
            .Where(i => i.InstanceId == id)
            .Select(i => new CourseInstanceDto
            {
                InstanceId = i.InstanceId,
                CourseId = i.CourseId,
                InstanceName = i.InstanceName,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                TotalWeeks = i.TotalWeeks,
                LessonsPerWeek = i.LessonsPerWeek,
                ScheduleDescription = i.ScheduleDescription,
                MaxStudents = i.MaxStudents,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
                StatusCode = i.InstanceStatus.Code
            })
            .FirstOrDefaultAsync();
        if (row == null) return NotFound();

        return Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<CourseInstanceDto>> CreateCourseInstance(CreateCourseInstanceDto dto)
    {
        // Новый поток всегда planned (status_id = 1).
        var statusId = CourseInstanceStatusMap.DefaultStatusId;

        var instance = new CourseInstance
        {
            CourseId = dto.CourseId,
            InstanceName = dto.InstanceName,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalWeeks = dto.TotalWeeks,
            LessonsPerWeek = dto.LessonsPerWeek,
            ScheduleDescription = dto.ScheduleDescription,
            MaxStudents = dto.MaxStudents,
            IsActive = dto.IsActive,
            StatusId = statusId
        };

        _context.CourseInstances.Add(instance);
        await _context.SaveChangesAsync();

        var created = await _context.CourseInstances
            .Where(i => i.InstanceId == instance.InstanceId)
            .Select(i => new CourseInstanceDto
            {
                InstanceId = i.InstanceId,
                CourseId = i.CourseId,
                InstanceName = i.InstanceName,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                TotalWeeks = i.TotalWeeks,
                LessonsPerWeek = i.LessonsPerWeek,
                ScheduleDescription = i.ScheduleDescription,
                MaxStudents = i.MaxStudents,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
                StatusCode = i.InstanceStatus.Code
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetCourseInstance), new { id = instance.InstanceId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourseInstance(int id, UpdateCourseInstanceDto dto)
    {
        var instance = await _context.CourseInstances.FindAsync(id);
        if (instance == null) return NotFound();

        if (dto.InstanceName != null) instance.InstanceName = dto.InstanceName;
        if (dto.StartDate.HasValue) instance.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) instance.EndDate = dto.EndDate;
        if (dto.TotalWeeks.HasValue) instance.TotalWeeks = dto.TotalWeeks;
        if (dto.LessonsPerWeek.HasValue) instance.LessonsPerWeek = dto.LessonsPerWeek;
        if (dto.ScheduleDescription != null) instance.ScheduleDescription = dto.ScheduleDescription;
        if (dto.MaxStudents.HasValue) instance.MaxStudents = dto.MaxStudents;
        if (dto.IsActive.HasValue) instance.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseInstance(int id)
    {
        var instance = await _context.CourseInstances.FindAsync(id);
        if (instance == null) return NotFound();

        _context.CourseInstances.Remove(instance);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

