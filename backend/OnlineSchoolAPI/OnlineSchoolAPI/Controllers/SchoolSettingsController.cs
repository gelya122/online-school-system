using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolSettingsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public SchoolSettingsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SchoolSettingDto>>> GetSchoolSettings()
    {
        var settings = await _context.SchoolSettings
            .Select(s => new SchoolSettingDto
            {
                SettingId = s.SettingId,
                SchoolName = s.SchoolName,
                LogoUrl = s.LogoUrl,
                ContactPhone = s.ContactPhone,
                ContactEmail = s.ContactEmail,
                Address = s.Address,
                AboutSchoolText = s.AboutSchoolText,
                PrivacyPolicyUrl = s.PrivacyPolicyUrl,
                TermsOfUseUrl = s.TermsOfUseUrl,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();
        return Ok(settings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolSettingDto>> GetSchoolSetting(int id)
    {
        var setting = await _context.SchoolSettings.FindAsync(id);
        if (setting == null) return NotFound();

        return Ok(new SchoolSettingDto
        {
            SettingId = setting.SettingId,
            SchoolName = setting.SchoolName,
            LogoUrl = setting.LogoUrl,
            ContactPhone = setting.ContactPhone,
            ContactEmail = setting.ContactEmail,
            Address = setting.Address,
            AboutSchoolText = setting.AboutSchoolText,
            PrivacyPolicyUrl = setting.PrivacyPolicyUrl,
            TermsOfUseUrl = setting.TermsOfUseUrl,
            UpdatedAt = setting.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<SchoolSettingDto>> CreateSchoolSetting(CreateSchoolSettingDto dto)
    {
        var setting = new SchoolSetting
        {
            SchoolName = dto.SchoolName,
            LogoUrl = dto.LogoUrl,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            Address = dto.Address,
            AboutSchoolText = dto.AboutSchoolText,
            PrivacyPolicyUrl = dto.PrivacyPolicyUrl,
            TermsOfUseUrl = dto.TermsOfUseUrl
        };

        _context.SchoolSettings.Add(setting);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSchoolSetting), new { id = setting.SettingId }, new SchoolSettingDto
        {
            SettingId = setting.SettingId,
            SchoolName = setting.SchoolName,
            LogoUrl = setting.LogoUrl,
            ContactPhone = setting.ContactPhone,
            ContactEmail = setting.ContactEmail,
            Address = setting.Address,
            AboutSchoolText = setting.AboutSchoolText,
            PrivacyPolicyUrl = setting.PrivacyPolicyUrl,
            TermsOfUseUrl = setting.TermsOfUseUrl,
            UpdatedAt = setting.UpdatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchoolSetting(int id, UpdateSchoolSettingDto dto)
    {
        var setting = await _context.SchoolSettings.FindAsync(id);
        if (setting == null) return NotFound();

        if (dto.SchoolName != null) setting.SchoolName = dto.SchoolName;
        if (dto.LogoUrl != null) setting.LogoUrl = dto.LogoUrl;
        if (dto.ContactPhone != null) setting.ContactPhone = dto.ContactPhone;
        if (dto.ContactEmail != null) setting.ContactEmail = dto.ContactEmail;
        if (dto.Address != null) setting.Address = dto.Address;
        if (dto.AboutSchoolText != null) setting.AboutSchoolText = dto.AboutSchoolText;
        if (dto.PrivacyPolicyUrl != null) setting.PrivacyPolicyUrl = dto.PrivacyPolicyUrl;
        if (dto.TermsOfUseUrl != null) setting.TermsOfUseUrl = dto.TermsOfUseUrl;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchoolSetting(int id)
    {
        var setting = await _context.SchoolSettings.FindAsync(id);
        if (setting == null) return NotFound();

        _context.SchoolSettings.Remove(setting);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

