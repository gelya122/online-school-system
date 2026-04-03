using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseSchedulePlansController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public CourseSchedulePlansController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseSchedulePlanDto>>> GetCourseSchedulePlans()
    {
        var plans = await _context.CourseSchedulePlans
            .Select(p => new CourseSchedulePlanDto
            {
                PlanId = p.PlanId,
                InstanceId = p.InstanceId,
                LessonId = p.LessonId,
                ReleaseDayOffset = p.ReleaseDayOffset,
                ReleaseTime = p.ReleaseTime,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseSchedulePlanDto>> GetCourseSchedulePlan(int id)
    {
        var plan = await _context.CourseSchedulePlans.FindAsync(id);
        if (plan == null) return NotFound();

        return Ok(new CourseSchedulePlanDto
        {
            PlanId = plan.PlanId,
            InstanceId = plan.InstanceId,
            LessonId = plan.LessonId,
            ReleaseDayOffset = plan.ReleaseDayOffset,
            ReleaseTime = plan.ReleaseTime,
            CreatedAt = plan.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<CourseSchedulePlanDto>> CreateCourseSchedulePlan(CreateCourseSchedulePlanDto dto)
    {
        var plan = new CourseSchedulePlan
        {
            InstanceId = dto.InstanceId,
            LessonId = dto.LessonId,
            ReleaseDayOffset = dto.ReleaseDayOffset,
            ReleaseTime = dto.ReleaseTime
        };

        _context.CourseSchedulePlans.Add(plan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourseSchedulePlan), new { id = plan.PlanId }, new CourseSchedulePlanDto
        {
            PlanId = plan.PlanId,
            InstanceId = plan.InstanceId,
            LessonId = plan.LessonId,
            ReleaseDayOffset = plan.ReleaseDayOffset,
            ReleaseTime = plan.ReleaseTime,
            CreatedAt = plan.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourseSchedulePlan(int id, UpdateCourseSchedulePlanDto dto)
    {
        var plan = await _context.CourseSchedulePlans.FindAsync(id);
        if (plan == null) return NotFound();

        if (dto.ReleaseDayOffset.HasValue) plan.ReleaseDayOffset = dto.ReleaseDayOffset.Value;
        if (dto.ReleaseTime.HasValue) plan.ReleaseTime = dto.ReleaseTime;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseSchedulePlan(int id)
    {
        var plan = await _context.CourseSchedulePlans.FindAsync(id);
        if (plan == null) return NotFound();

        _context.CourseSchedulePlans.Remove(plan);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

