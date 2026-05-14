using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminNotificationsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminNotificationsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/notifications")]
    public async Task<ActionResult<IReadOnlyList<AdminNotificationListRowDto>>> GetList(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var list = await _context.Notifications
            .AsNoTracking()
            .Include(n => n.User)
            .OrderByDescending(n => n.CreatedAt ?? DateTime.MinValue)
            .Take(500)
            .Select(n => new AdminNotificationListRowDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Recipient = (n.User.Email ?? ("User #" + n.UserId)),
                Title = n.Title,
                Message = n.Message,
                Type = n.NotificationType,
                IsRead = n.IsRead ?? false,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/notifications")]
    public async Task<IActionResult> Create([FromBody] AdminCreateNotificationDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите title");
        if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest("Укажите message");

        if (dto.BroadcastToAllStudents)
        {
            if (dto.UserId is not null || dto.InstanceId is not null)
                return BadRequest("При broadcastToAllStudents не указывайте userId и instanceId.");

            var broadcastUserIds = await _context.Students.AsNoTracking()
                .Where(s => s.DeletedAt == null)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (broadcastUserIds.Count == 0) return BadRequest("Нет активных студентов");

            foreach (var uid in broadcastUserIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = uid,
                    Title = dto.Title.Trim(),
                    Message = dto.Message.Trim(),
                    NotificationType = string.IsNullOrWhiteSpace(dto.Type) ? null : dto.Type.Trim(),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        if (dto.UserId is null && dto.InstanceId is null)
            return BadRequest("Укажите userId, instanceId или broadcastToAllStudents=true");

        if (dto.UserId is not null && dto.InstanceId is not null)
            return BadRequest("Укажите только один из параметров: userId или instanceId");

        if (dto.UserId is not null)
        {
            var exists = await _context.Users.AsNoTracking().AnyAsync(u => u.UserId == dto.UserId.Value, cancellationToken);
            if (!exists) return BadRequest("Пользователь не найден");

            _context.Notifications.Add(new Notification
            {
                UserId = dto.UserId.Value,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                NotificationType = string.IsNullOrWhiteSpace(dto.Type) ? null : dto.Type.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        // instance -> all enrolled students (unique user ids)
        var userIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.InstanceId == dto.InstanceId!.Value && e.Student.DeletedAt == null)
            .Select(e => e.Student.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (userIds.Count == 0) return BadRequest("В потоке нет студентов");

        foreach (var uid in userIds)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                NotificationType = string.IsNullOrWhiteSpace(dto.Type) ? null : dto.Type.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/notifications/{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id, cancellationToken);
        if (n == null) return NotFound();

        n.IsRead = true;
        n.ReadAt = n.ReadAt ?? DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/notifications/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id, cancellationToken);
        if (n == null) return NotFound();

        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

