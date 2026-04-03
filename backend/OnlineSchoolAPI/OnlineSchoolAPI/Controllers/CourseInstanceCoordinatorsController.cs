using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseInstanceCoordinatorsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public CourseInstanceCoordinatorsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseInstanceCoordinatorDto>>> GetCourseInstanceCoordinators()
    {
        var coordinators = await _context.CourseInstanceCoordinators
            .Select(c => new CourseInstanceCoordinatorDto
            {
                CoordinatorId = c.CoordinatorId,
                InstanceId = c.InstanceId,
                EmployeeId = c.EmployeeId,
                IsLead = c.IsLead,
                AssignedAt = c.AssignedAt
            })
            .ToListAsync();
        return Ok(coordinators);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseInstanceCoordinatorDto>> GetCourseInstanceCoordinator(int id)
    {
        var coordinator = await _context.CourseInstanceCoordinators.FindAsync(id);
        if (coordinator == null) return NotFound();

        return Ok(new CourseInstanceCoordinatorDto
        {
            CoordinatorId = coordinator.CoordinatorId,
            InstanceId = coordinator.InstanceId,
            EmployeeId = coordinator.EmployeeId,
            IsLead = coordinator.IsLead,
            AssignedAt = coordinator.AssignedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<CourseInstanceCoordinatorDto>> CreateCourseInstanceCoordinator(CreateCourseInstanceCoordinatorDto dto)
    {
        var coordinator = new CourseInstanceCoordinator
        {
            InstanceId = dto.InstanceId,
            EmployeeId = dto.EmployeeId,
            IsLead = dto.IsLead
        };

        _context.CourseInstanceCoordinators.Add(coordinator);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourseInstanceCoordinator), new { id = coordinator.CoordinatorId }, new CourseInstanceCoordinatorDto
        {
            CoordinatorId = coordinator.CoordinatorId,
            InstanceId = coordinator.InstanceId,
            EmployeeId = coordinator.EmployeeId,
            IsLead = coordinator.IsLead,
            AssignedAt = coordinator.AssignedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourseInstanceCoordinator(int id, UpdateCourseInstanceCoordinatorDto dto)
    {
        var coordinator = await _context.CourseInstanceCoordinators.FindAsync(id);
        if (coordinator == null) return NotFound();

        if (dto.IsLead.HasValue) coordinator.IsLead = dto.IsLead;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseInstanceCoordinator(int id)
    {
        var coordinator = await _context.CourseInstanceCoordinators.FindAsync(id);
        if (coordinator == null) return NotFound();

        _context.CourseInstanceCoordinators.Remove(coordinator);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

