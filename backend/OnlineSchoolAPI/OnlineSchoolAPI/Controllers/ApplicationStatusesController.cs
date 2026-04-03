using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationStatusesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public ApplicationStatusesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationStatusDto>>> GetApplicationStatuses()
    {
        var statuses = await _context.ApplicationStatuses
            .Select(s => new ApplicationStatusDto
            {
                StatusId = s.StatusId,
                StatusName = s.StatusName,
                Description = s.Description
            })
            .ToListAsync();
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationStatusDto>> GetApplicationStatus(int id)
    {
        var status = await _context.ApplicationStatuses.FindAsync(id);
        if (status == null) return NotFound();

        return Ok(new ApplicationStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationStatusDto>> CreateApplicationStatus(CreateApplicationStatusDto dto)
    {
        var status = new ApplicationStatus
        {
            StatusName = dto.StatusName,
            Description = dto.Description
        };

        _context.ApplicationStatuses.Add(status);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApplicationStatus), new { id = status.StatusId }, new ApplicationStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, UpdateApplicationStatusDto dto)
    {
        var status = await _context.ApplicationStatuses.FindAsync(id);
        if (status == null) return NotFound();

        if (dto.StatusName != null) status.StatusName = dto.StatusName;
        if (dto.Description != null) status.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApplicationStatus(int id)
    {
        var status = await _context.ApplicationStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _context.ApplicationStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

