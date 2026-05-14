using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminSiteBannersController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminSiteBannersController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/site-banners")]
    public async Task<ActionResult<IReadOnlyList<AdminSiteBannerRowDto>>> GetList(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var list = await _context.SiteBanners
            .AsNoTracking()
            .OrderBy(b => b.BannerOrder)
            .ThenBy(b => b.BannerId)
            .Select(b => new AdminSiteBannerRowDto
            {
                BannerId = b.BannerId,
                Title = b.Title,
                Subtitle = b.Subtitle,
                ImageUrl = b.ImageUrl,
                ButtonText = b.ButtonText,
                ButtonUrl = b.ButtonUrl,
                BannerOrder = b.BannerOrder,
                IsActive = b.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/site-banners")]
    public async Task<ActionResult<AdminSiteBannerRowDto>> Create([FromBody] AdminSiteBannerUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите title");

        var maxOrder = await _context.SiteBanners.AsNoTracking()
            .Select(b => (int?)b.BannerOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var b = new SiteBanner
        {
            Title = dto.Title.Trim(),
            Subtitle = string.IsNullOrWhiteSpace(dto.Subtitle) ? null : dto.Subtitle.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            ButtonText = string.IsNullOrWhiteSpace(dto.ButtonText) ? null : dto.ButtonText.Trim(),
            ButtonUrl = string.IsNullOrWhiteSpace(dto.ButtonUrl) ? null : dto.ButtonUrl.Trim(),
            BannerOrder = dto.BannerOrder != 0 ? dto.BannerOrder : (maxOrder + 1),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SiteBanners.Add(b);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminSiteBannerRowDto
        {
            BannerId = b.BannerId,
            Title = b.Title,
            Subtitle = b.Subtitle,
            ImageUrl = b.ImageUrl,
            ButtonText = b.ButtonText,
            ButtonUrl = b.ButtonUrl,
            BannerOrder = b.BannerOrder,
            IsActive = b.IsActive
        });
    }

    [HttpPut("api/admin/site-banners/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminSiteBannerUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var b = await _context.SiteBanners.FirstOrDefaultAsync(x => x.BannerId == id, cancellationToken);
        if (b == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите title");

        b.Title = dto.Title.Trim();
        b.Subtitle = string.IsNullOrWhiteSpace(dto.Subtitle) ? null : dto.Subtitle.Trim();
        b.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        b.ButtonText = string.IsNullOrWhiteSpace(dto.ButtonText) ? null : dto.ButtonText.Trim();
        b.ButtonUrl = string.IsNullOrWhiteSpace(dto.ButtonUrl) ? null : dto.ButtonUrl.Trim();
        b.BannerOrder = dto.BannerOrder;
        b.IsActive = dto.IsActive;
        b.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/site-banners/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var b = await _context.SiteBanners.FirstOrDefaultAsync(x => x.BannerId == id, cancellationToken);
        if (b == null) return NotFound();

        _context.SiteBanners.Remove(b);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/site-banners/reorder")]
    public async Task<IActionResult> Reorder([FromBody] AdminReorderRequestDto2 dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (dto.Items == null || dto.Items.Count == 0) return BadRequest("items пуст");

        var ids = dto.Items.Select(x => x.Id).Distinct().ToList();
        var banners = await _context.SiteBanners.Where(b => ids.Contains(b.BannerId)).ToListAsync(cancellationToken);

        foreach (var item in dto.Items)
        {
            var b = banners.FirstOrDefault(x => x.BannerId == item.Id);
            if (b != null) b.BannerOrder = item.Order;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

