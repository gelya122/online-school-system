using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentStatusesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public EnrollmentStatusesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentStatusDto>>> GetEnrollmentStatuses()
    {
        var statuses = await _context.EnrollmentStatuses
            .Select(s => new EnrollmentStatusDto
            {
                StatusId = s.StatusId,
                StatusName = s.StatusName,
                Description = s.Description
            })
            .ToListAsync();
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentStatusDto>> GetEnrollmentStatus(int id)
    {
        var status = await _context.EnrollmentStatuses.FindAsync(id);
        if (status == null) return NotFound();

        return Ok(new EnrollmentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentStatusDto>> CreateEnrollmentStatus(CreateEnrollmentStatusDto dto)
    {
        var status = new EnrollmentStatus
        {
            StatusName = dto.StatusName,
            Description = dto.Description
        };

        _context.EnrollmentStatuses.Add(status);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEnrollmentStatus), new { id = status.StatusId }, new EnrollmentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEnrollmentStatus(int id, UpdateEnrollmentStatusDto dto)
    {
        var status = await _context.EnrollmentStatuses.FindAsync(id);
        if (status == null) return NotFound();

        if (dto.StatusName != null) status.StatusName = dto.StatusName;
        if (dto.Description != null) status.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollmentStatus(int id)
    {
        var status = await _context.EnrollmentStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _context.EnrollmentStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

