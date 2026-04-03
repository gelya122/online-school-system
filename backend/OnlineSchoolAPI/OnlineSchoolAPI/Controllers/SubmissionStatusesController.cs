using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionStatusesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public SubmissionStatusesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubmissionStatusDto>>> GetSubmissionStatuses()
    {
        var statuses = await _context.SubmissionStatuses
            .Select(s => new SubmissionStatusDto
            {
                StatusId = s.StatusId,
                StatusName = s.StatusName,
                Description = s.Description
            })
            .ToListAsync();
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubmissionStatusDto>> GetSubmissionStatus(int id)
    {
        var status = await _context.SubmissionStatuses.FindAsync(id);
        if (status == null) return NotFound();

        return Ok(new SubmissionStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionStatusDto>> CreateSubmissionStatus(CreateSubmissionStatusDto dto)
    {
        var status = new SubmissionStatus
        {
            StatusName = dto.StatusName,
            Description = dto.Description
        };

        _context.SubmissionStatuses.Add(status);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubmissionStatus), new { id = status.StatusId }, new SubmissionStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubmissionStatus(int id, UpdateSubmissionStatusDto dto)
    {
        var status = await _context.SubmissionStatuses.FindAsync(id);
        if (status == null) return NotFound();

        if (dto.StatusName != null) status.StatusName = dto.StatusName;
        if (dto.Description != null) status.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubmissionStatus(int id)
    {
        var status = await _context.SubmissionStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _context.SubmissionStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

