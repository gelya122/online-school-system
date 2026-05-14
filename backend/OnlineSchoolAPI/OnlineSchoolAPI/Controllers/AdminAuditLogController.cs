using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

/// <summary>
/// Общий журнал audit_log. Записи добавляются через <see cref="AuditLogWriter"/> в отдельных админ-операциях;
/// не все действия в системе могут быть залогированы — это нормально для поэтапного внедрения.
/// </summary>
[ApiController]
[Route("api/admin/audit-log")]
[Authorize]
public sealed class AdminAuditLogController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminAuditLogController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<AdminAuditLogPageDto>> GetPage(
        [FromQuery] int? employeeId = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] string? search = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        take = Math.Clamp(take, 1, 200);
        skip = Math.Max(0, skip);

        var q = _context.AuditLogs.AsNoTracking();

        if (employeeId is > 0)
            q = q.Where(a => a.EmployeeId == employeeId);
        if (userId is > 0)
            q = q.Where(a => a.UserId == userId);

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var et = entityType.Trim();
            q = q.Where(a => a.EntityType.ToLower() == et.ToLower());
        }

        if (from.HasValue)
        {
            var f = from.Value.ToDateTime(TimeOnly.MinValue);
            q = q.Where(a => a.CreatedAt >= f);
        }
        if (to.HasValue)
        {
            var t = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            q = q.Where(a => a.CreatedAt < t);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a =>
                a.Action.Contains(s) ||
                a.EntityType.Contains(s) ||
                (a.OldValues != null && a.OldValues.Contains(s)) ||
                (a.NewValues != null && a.NewValues.Contains(s)) ||
                (a.IpAddress != null && a.IpAddress.Contains(s)) ||
                (a.UserAgent != null && a.UserAgent.Contains(s)));
        }

        var total = await q.CountAsync(cancellationToken);

        var items = await (
            from a in q
            join e in _context.Employees.AsNoTracking() on a.EmployeeId equals e.EmployeeId into eg
            from e in eg.DefaultIfEmpty()
            join u in _context.Users.AsNoTracking() on a.UserId equals u.UserId into ug
            from u in ug.DefaultIfEmpty()
            orderby a.CreatedAt descending
            select new AdminAuditLogListRowDto
            {
                AuditLogId = a.AuditLogId,
                CreatedAt = a.CreatedAt,
                EmployeeId = a.EmployeeId,
                EmployeeDisplay = e == null
                    ? null
                    : ((e.LastName ?? "") + " " + (e.FirstName ?? "")).Trim(),
                UserId = a.UserId,
                UserDisplay = u == null ? null : u.Email,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            })
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(new AdminAuditLogPageDto { Items = items, TotalCount = total });
    }
}
