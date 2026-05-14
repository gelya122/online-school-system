using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminSchoolSettingsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminSchoolSettingsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/school-settings")]
    public async Task<ActionResult<AdminSchoolSettingsDto>> Get(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var s = await _context.SchoolSettings.AsNoTracking()
            .OrderBy(x => x.SettingId)
            .FirstOrDefaultAsync(cancellationToken);

        if (s == null)
        {
            s = new SchoolSetting
            {
                SchoolName = "Онлайн-школа",
                UpdatedAt = DateTime.UtcNow
            };
            _context.SchoolSettings.Add(s);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Ok(new AdminSchoolSettingsDto
        {
            SettingId = s.SettingId,
            SchoolName = s.SchoolName,
            LogoUrl = s.LogoUrl,
            Phone = s.ContactPhone,
            Email = s.ContactEmail,
            Address = s.Address,
            AboutText = s.AboutSchoolText,
            PrivacyPolicyUrl = s.PrivacyPolicyUrl,
            TermsUrl = s.TermsOfUseUrl,
            UpdatedAt = s.UpdatedAt
        });
    }

    [HttpPut("api/admin/school-settings")]
    public async Task<IActionResult> Update([FromBody] AdminSchoolSettingsUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.SchoolName))
            return BadRequest("Укажите schoolName");

        var s = await _context.SchoolSettings
            .OrderBy(x => x.SettingId)
            .FirstOrDefaultAsync(cancellationToken);
        if (s == null)
        {
            s = new SchoolSetting();
            _context.SchoolSettings.Add(s);
        }

        s.SchoolName = dto.SchoolName.Trim();
        s.LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl.Trim();
        s.ContactPhone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        s.ContactEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        s.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        s.AboutSchoolText = string.IsNullOrWhiteSpace(dto.AboutText) ? null : dto.AboutText.Trim();
        s.PrivacyPolicyUrl = string.IsNullOrWhiteSpace(dto.PrivacyPolicyUrl) ? null : dto.PrivacyPolicyUrl.Trim();
        s.TermsOfUseUrl = string.IsNullOrWhiteSpace(dto.TermsUrl) ? null : dto.TermsUrl.Trim();
        s.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

