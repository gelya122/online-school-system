using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrialApplicationsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public TrialApplicationsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrialApplicationDto>>> GetTrialApplications()
    {
        var applications = await _context.TrialApplications
            .Select(a => new TrialApplicationDto
            {
                ApplicationId = a.ApplicationId,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Phone = a.Phone,
                Email = a.Email,
                ClassNumber = a.ClassNumber,
                SelectedSubjects = a.SelectedSubjects,
                ApplicationStatusId = a.ApplicationStatusId,
                AssignedManagerId = a.AssignedManagerId,
                ManagerComment = a.ManagerComment,
                ContactedAt = a.ContactedAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
        return Ok(applications);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrialApplicationDto>> GetTrialApplication(int id)
    {
        var application = await _context.TrialApplications.FindAsync(id);
        if (application == null) return NotFound();

        return Ok(new TrialApplicationDto
        {
            ApplicationId = application.ApplicationId,
            FirstName = application.FirstName,
            LastName = application.LastName,
            Phone = application.Phone,
            Email = application.Email,
            ClassNumber = application.ClassNumber,
            SelectedSubjects = application.SelectedSubjects,
            ApplicationStatusId = application.ApplicationStatusId,
            AssignedManagerId = application.AssignedManagerId,
            ManagerComment = application.ManagerComment,
            ContactedAt = application.ContactedAt,
            CreatedAt = application.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<TrialApplicationDto>> CreateTrialApplication(CreateTrialApplicationDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Email) && !EmailValidator.IsValid(dto.Email))
            return BadRequest("Введите корректный адрес электронной почты.");

        var application = new TrialApplication
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            Email = dto.Email,
            ClassNumber = dto.ClassNumber,
            SelectedSubjects = dto.SelectedSubjects,
            // Значение по умолчанию в БД = 1, но при создании явно задаём,
            // чтобы EF не отправлял NULL и не ломал default.
            ApplicationStatusId = dto.ApplicationStatusId ?? 1
        };

        _context.TrialApplications.Add(application);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrialApplication), new { id = application.ApplicationId }, new TrialApplicationDto
        {
            ApplicationId = application.ApplicationId,
            FirstName = application.FirstName,
            LastName = application.LastName,
            Phone = application.Phone,
            Email = application.Email,
            ClassNumber = application.ClassNumber,
            SelectedSubjects = application.SelectedSubjects,
            ApplicationStatusId = application.ApplicationStatusId,
            AssignedManagerId = application.AssignedManagerId,
            ManagerComment = application.ManagerComment,
            ContactedAt = application.ContactedAt,
            CreatedAt = application.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrialApplication(int id, UpdateTrialApplicationDto dto)
    {
        var application = await _context.TrialApplications.FindAsync(id);
        if (application == null) return NotFound();

        if (dto.ApplicationStatusId.HasValue) application.ApplicationStatusId = dto.ApplicationStatusId;
        if (dto.AssignedManagerId.HasValue) application.AssignedManagerId = dto.AssignedManagerId;
        if (dto.ManagerComment != null) application.ManagerComment = dto.ManagerComment;
        if (dto.ContactedAt.HasValue) application.ContactedAt = dto.ContactedAt;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrialApplication(int id)
    {
        var application = await _context.TrialApplications.FindAsync(id);
        if (application == null) return NotFound();

        _context.TrialApplications.Remove(application);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

