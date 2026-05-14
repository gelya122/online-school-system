using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminPromoCodesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminPromoCodesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/promo-codes/discount-types")]
    public async Task<ActionResult<IReadOnlyList<AdminDiscountTypeDictDto>>> GetDiscountTypes(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var list = await _context.DiscountTypes
            .AsNoTracking()
            .Where(t => t.IsActive == null || t.IsActive == true)
            .OrderBy(t => t.TypeName)
            .Select(t => new AdminDiscountTypeDictDto
            {
                TypeId = t.TypeId,
                TypeName = t.TypeName,
                Description = t.Description,
                IsActive = t.IsActive != false
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("api/admin/promo-codes")]
    public async Task<ActionResult<IReadOnlyList<AdminPromoCodeListRowDto>>> GetList(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        // manager: view only; admin: full
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var q = _context.PromoCodes
            .AsNoTracking()
            .Where(p => p.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(p => (p.Code ?? "").ToLower().Contains(s));
        }

        if (isActive.HasValue)
            q = q.Where(p => (p.IsActive ?? false) == isActive.Value);

        var usageCounts = await _context.AppOrders
            .AsNoTracking()
            .Where(o => o.PromoCodeId != null)
            .GroupBy(o => o.PromoCodeId!.Value)
            .Select(g => new { PromoCodeId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.PromoCodeId, x => x.Cnt, cancellationToken);

        var list = await q
            .Include(p => p.DiscountType)
            .Include(p => p.AppliesToCourse)
            .Include(p => p.AppliesToInstance)
            .OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue)
            .Take(500)
            .Select(p => new AdminPromoCodeListRowDto
            {
                PromoCodeId = p.PromoCodeId,
                Code = p.Code,
                DiscountTypeId = p.TypeId,
                DiscountTypeName = p.DiscountType != null ? p.DiscountType.TypeName : null,
                DiscountValue = p.DiscountValue,
                ValidFrom = p.ValidFrom,
                ValidUntil = p.ValidUntil,
                MaxUses = p.MaxUses,
                CurrentUses = 0,
                IsActive = p.IsActive ?? false,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                AppliesToCourseId = p.AppliesToCourseId,
                AppliesToCourseTitle = p.AppliesToCourse != null ? p.AppliesToCourse.Title : null,
                AppliesToInstanceId = p.AppliesToInstanceId,
                AppliesToInstanceTitle = p.AppliesToInstance != null ? p.AppliesToInstance.InstanceName : null
            })
            .ToListAsync(cancellationToken);

        foreach (var row in list)
            row.CurrentUses = usageCounts.TryGetValue(row.PromoCodeId, out var cnt) ? cnt : 0;

        return Ok(list);
    }

    [HttpGet("api/admin/promo-codes/{id:int}")]
    public async Task<ActionResult<AdminPromoCodeDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var p = await _context.PromoCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PromoCodeId == id && x.DeletedAt == null, cancellationToken);
        if (p == null) return NotFound();

        var uses = await _context.AppOrders.AsNoTracking()
            .CountAsync(o => o.PromoCodeId == id, cancellationToken);

        return Ok(new AdminPromoCodeDetailsDto
        {
            PromoCodeId = p.PromoCodeId,
            Code = p.Code,
            DiscountTypeId = p.TypeId,
            DiscountValue = p.DiscountValue,
            ValidFrom = p.ValidFrom,
            ValidUntil = p.ValidUntil,
            MaxUses = p.MaxUses,
            CurrentUses = uses,
            IsActive = p.IsActive ?? false,
            MinOrderAmount = p.MinOrderAmount,
            MaxDiscountAmount = p.MaxDiscountAmount,
            AppliesToCourseId = p.AppliesToCourseId,
            AppliesToInstanceId = p.AppliesToInstanceId
        });
    }

    [HttpPost("api/admin/promo-codes")]
    public async Task<ActionResult<AdminPromoCodeDetailsDto>> Create([FromBody] AdminPromoCodeUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var code = (dto.Code ?? "").Trim();
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Укажите code");

        var exists = await _context.PromoCodes.AsNoTracking()
            .AnyAsync(p => p.DeletedAt == null && p.Code.ToLower() == code.ToLower(), cancellationToken);
        if (exists) return BadRequest("Промокод с таким code уже существует");

        var entity = new PromoCode
        {
            Code = code,
            TypeId = dto.DiscountTypeId,
            DiscountValue = dto.DiscountValue,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            MaxUses = dto.MaxUses,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            AppliesToCourseId = dto.AppliesToCourseId,
            AppliesToInstanceId = dto.AppliesToInstanceId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PromoCodes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = entity.PromoCodeId }, new AdminPromoCodeDetailsDto
        {
            PromoCodeId = entity.PromoCodeId,
            Code = entity.Code,
            DiscountTypeId = entity.TypeId,
            DiscountValue = entity.DiscountValue,
            ValidFrom = entity.ValidFrom,
            ValidUntil = entity.ValidUntil,
            MaxUses = entity.MaxUses,
            CurrentUses = 0,
            IsActive = entity.IsActive ?? false,
            MinOrderAmount = entity.MinOrderAmount,
            MaxDiscountAmount = entity.MaxDiscountAmount,
            AppliesToCourseId = entity.AppliesToCourseId,
            AppliesToInstanceId = entity.AppliesToInstanceId
        });
    }

    [HttpPut("api/admin/promo-codes/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminPromoCodeUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var entity = await _context.PromoCodes
            .FirstOrDefaultAsync(x => x.PromoCodeId == id && x.DeletedAt == null, cancellationToken);
        if (entity == null) return NotFound();

        var code = (dto.Code ?? "").Trim();
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Укажите code");

        var exists = await _context.PromoCodes.AsNoTracking()
            .AnyAsync(p => p.PromoCodeId != id && p.DeletedAt == null && p.Code.ToLower() == code.ToLower(), cancellationToken);
        if (exists) return BadRequest("Промокод с таким code уже существует");

        entity.Code = code;
        entity.TypeId = dto.DiscountTypeId;
        entity.DiscountValue = dto.DiscountValue;
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidUntil = dto.ValidUntil;
        entity.MaxUses = dto.MaxUses;
        entity.MinOrderAmount = dto.MinOrderAmount;
        entity.MaxDiscountAmount = dto.MaxDiscountAmount;
        entity.AppliesToCourseId = dto.AppliesToCourseId;
        entity.AppliesToInstanceId = dto.AppliesToInstanceId;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/promo-codes/{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var entity = await _context.PromoCodes
            .FirstOrDefaultAsync(x => x.PromoCodeId == id && x.DeletedAt == null, cancellationToken);
        if (entity == null) return NotFound();

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/promo-codes/{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var entity = await _context.PromoCodes
            .FirstOrDefaultAsync(x => x.PromoCodeId == id && x.DeletedAt == null, cancellationToken);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/promo-codes/{id:int}/usages")]
    public async Task<ActionResult<IReadOnlyList<AdminPromoCodeUsageRowDto>>> GetUsages(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var exists = await _context.PromoCodes.AsNoTracking()
            .AnyAsync(p => p.PromoCodeId == id && p.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var list = await _context.AppOrders
            .AsNoTracking()
            .Where(o => o.PromoCodeId == id)
            .OrderByDescending(o => o.CreatedAt)
            .Take(1000)
            .Select(o => new AdminPromoCodeUsageRowDto
            {
                UsageId = o.OrderId,
                StudentId = o.StudentId,
                StudentName = ((o.Student.FirstName ?? "") + " " + (o.Student.LastName ?? "")).Trim(),
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                DiscountAmount = o.DiscountAmount ?? 0m,
                UsedAt = o.CreatedAt ?? DateTime.UtcNow
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpDelete("api/admin/promo-codes/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var entity = await _context.PromoCodes
            .FirstOrDefaultAsync(x => x.PromoCodeId == id && x.DeletedAt == null, cancellationToken);
        if (entity == null) return NotFound();

        entity.DeletedAt = DateTime.UtcNow;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

