using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminSiteSettingsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminSiteSettingsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/site-settings")]
    public async Task<ActionResult<AdminSiteSettingsDto>> Get(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var s = await _context.SiteSettings.AsNoTracking()
            .OrderBy(x => x.SettingId)
            .FirstOrDefaultAsync(cancellationToken);

        if (s == null)
        {
            s = new SiteSetting
            {
                SiteName = "Онлайн-школа",
                IsMaintenanceMode = false,
                UpdatedAt = DateTime.UtcNow
            };
            _context.SiteSettings.Add(s);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Ok(new AdminSiteSettingsDto
        {
            SettingId = s.SettingId,
            SiteName = s.SiteName,
            MainPageTitle = s.MainPageTitle,
            MainPageDescription = s.MainPageDescription,
            ContactPhone = s.ContactPhone,
            ContactEmail = s.ContactEmail,
            VkUrl = s.VkUrl,
            TelegramUrl = s.TelegramUrl,
            YoutubeUrl = s.YoutubeUrl,
            SeoTitle = s.SeoTitle,
            SeoDescription = s.SeoDescription,
            MaintenanceMode = s.IsMaintenanceMode,
            UpdatedAt = s.UpdatedAt
        });
    }

    [HttpPut("api/admin/site-settings")]
    public async Task<IActionResult> Update([FromBody] AdminSiteSettingsUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var s = await _context.SiteSettings
            .OrderBy(x => x.SettingId)
            .FirstOrDefaultAsync(cancellationToken);
        if (s == null)
        {
            s = new SiteSetting();
            _context.SiteSettings.Add(s);
        }

        s.SiteName = string.IsNullOrWhiteSpace(dto.SiteName) ? null : dto.SiteName.Trim();
        s.MainPageTitle = string.IsNullOrWhiteSpace(dto.MainPageTitle) ? null : dto.MainPageTitle.Trim();
        s.MainPageDescription = string.IsNullOrWhiteSpace(dto.MainPageDescription) ? null : dto.MainPageDescription.Trim();
        s.ContactPhone = string.IsNullOrWhiteSpace(dto.ContactPhone) ? null : dto.ContactPhone.Trim();
        s.ContactEmail = string.IsNullOrWhiteSpace(dto.ContactEmail) ? null : dto.ContactEmail.Trim();
        s.VkUrl = string.IsNullOrWhiteSpace(dto.VkUrl) ? null : dto.VkUrl.Trim();
        s.TelegramUrl = string.IsNullOrWhiteSpace(dto.TelegramUrl) ? null : dto.TelegramUrl.Trim();
        s.YoutubeUrl = string.IsNullOrWhiteSpace(dto.YoutubeUrl) ? null : dto.YoutubeUrl.Trim();
        s.SeoTitle = string.IsNullOrWhiteSpace(dto.SeoTitle) ? null : dto.SeoTitle.Trim();
        s.SeoDescription = string.IsNullOrWhiteSpace(dto.SeoDescription) ? null : dto.SeoDescription.Trim();
        s.IsMaintenanceMode = dto.MaintenanceMode;
        s.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

