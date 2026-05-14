using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminMailingCampaignsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminMailingCampaignsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/mailing-campaigns")]
    public async Task<ActionResult<IReadOnlyList<AdminMailingCampaignListRowDto>>> GetList(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var counts = await _context.MailingRecipients.AsNoTracking()
            .GroupBy(r => r.CampaignId)
            .Select(g => new { CampaignId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.CampaignId, x => x.Cnt, cancellationToken);

        var list = await _context.MailingCampaigns
            .AsNoTracking()
            .Include(c => c.CreatedByEmployee)
            .OrderByDescending(c => c.CreatedAt)
            .Take(300)
            .Select(c => new AdminMailingCampaignListRowDto
            {
                CampaignId = c.CampaignId,
                Title = c.Title,
                Channel = c.Channel,
                Status = c.Status,
                RecipientsCount = 0,
                ScheduledAt = c.ScheduledAt,
                SentAt = c.SentAt,
                CreatedBy = c.CreatedByEmployee != null
                    ? ((c.CreatedByEmployee.FirstName ?? "") + " " + (c.CreatedByEmployee.LastName ?? "")).Trim()
                    : null
            })
            .ToListAsync(cancellationToken);

        foreach (var row in list)
            row.RecipientsCount = counts.TryGetValue(row.CampaignId, out var cnt) ? cnt : 0;

        return Ok(list);
    }

    [HttpGet("api/admin/mailing-campaigns/{id:int}")]
    public async Task<ActionResult<AdminMailingCampaignDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var c = await _context.MailingCampaigns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CampaignId == id, cancellationToken);
        if (c == null) return NotFound();

        ParseStoredTargetType(c.TargetType, out var uiTarget, out var courseId, out var instanceId);

        return Ok(new AdminMailingCampaignDetailsDto
        {
            CampaignId = c.CampaignId,
            Title = c.Title,
            Message = c.Message,
            Channel = c.Channel,
            Status = c.Status,
            TargetType = uiTarget,
            TargetCourseId = courseId,
            TargetInstanceId = instanceId,
            ScheduledAt = c.ScheduledAt,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }

    [HttpPost("api/admin/mailing-campaigns")]
    public async Task<ActionResult<AdminMailingCampaignDetailsDto>> Create([FromBody] AdminMailingCampaignUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите title");
        if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest("Укажите message");
        if (string.IsNullOrWhiteSpace(dto.Channel)) return BadRequest("Укажите channel");

        var storedTarget = BuildStoredTargetType(dto);
        if (string.IsNullOrWhiteSpace(storedTarget)) return BadRequest("Укажите target_type или target_course_id / target_instance_id");

        var entity = new MailingCampaign
        {
            Title = dto.Title.Trim(),
            Message = dto.Message.Trim(),
            Channel = dto.Channel.Trim(),
            Status = "draft",
            TargetType = storedTarget,
            ScheduledAt = dto.ScheduledAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MailingCampaigns.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return await Get(entity.CampaignId, cancellationToken);
    }

    [HttpPut("api/admin/mailing-campaigns/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminMailingCampaignUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var c = await _context.MailingCampaigns.FirstOrDefaultAsync(x => x.CampaignId == id, cancellationToken);
        if (c == null) return NotFound();

        if (c.Status is "sent" or "canceled")
            return BadRequest("Нельзя менять отправленную или отменённую кампанию.");

        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите title");
        if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest("Укажите message");
        if (string.IsNullOrWhiteSpace(dto.Channel)) return BadRequest("Укажите channel");

        var storedTarget = BuildStoredTargetType(dto);
        if (string.IsNullOrWhiteSpace(storedTarget)) return BadRequest("Укажите target_type или target_course_id / target_instance_id");

        c.Title = dto.Title.Trim();
        c.Message = dto.Message.Trim();
        c.Channel = dto.Channel.Trim();
        c.TargetType = storedTarget;
        c.ScheduledAt = dto.ScheduledAt;
        c.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/mailing-campaigns/{id:int}/send")]
    public async Task<IActionResult> Send(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var c = await _context.MailingCampaigns.FirstOrDefaultAsync(x => x.CampaignId == id, cancellationToken);
        if (c == null) return NotFound();

        if (c.Status == "canceled") return BadRequest("Кампания отменена");
        if (c.Status == "sent") return BadRequest("Кампания уже отправлена");

        var now = DateTime.UtcNow;

        if (c.Status == "draft" && c.ScheduledAt.HasValue && c.ScheduledAt.Value > now)
        {
            c.Status = "scheduled";
            c.UpdatedAt = now;
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        if (c.Status == "scheduled" && c.ScheduledAt.HasValue && c.ScheduledAt.Value > now)
            return BadRequest("Рассылку можно отправить не раньше запланированного времени (UTC).");

        var userIds = await ResolveUserIdsAsync(c.TargetType, cancellationToken);
        if (userIds.Count == 0) return BadRequest("Получатели не найдены");

        var existing = await _context.MailingRecipients.AsNoTracking()
            .Where(r => r.CampaignId == id)
            .Select(r => r.UserId)
            .ToHashSetAsync(cancellationToken);

        foreach (var uid in userIds)
        {
            if (!existing.Add(uid)) continue;

            _context.MailingRecipients.Add(new MailingRecipient
            {
                CampaignId = id,
                UserId = uid,
                Status = "sent",
                SentAt = now,
                CreatedAt = now
            });

            _context.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = c.Title,
                Message = c.Message,
                NotificationType = c.Channel,
                CampaignId = id,
                IsRead = false,
                CreatedAt = now
            });
        }

        c.Status = "sent";
        c.SentAt = now;
        c.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/mailing-campaigns/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var c = await _context.MailingCampaigns.FirstOrDefaultAsync(x => x.CampaignId == id, cancellationToken);
        if (c == null) return NotFound();

        c.Status = "canceled";
        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/mailing-campaigns/{id:int}/recipients")]
    public async Task<ActionResult<IReadOnlyList<AdminMailingRecipientRowDto>>> GetRecipients(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var exists = await _context.MailingCampaigns.AsNoTracking()
            .AnyAsync(x => x.CampaignId == id, cancellationToken);
        if (!exists) return NotFound();

        var list = await _context.MailingRecipients
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.CampaignId == id)
            .OrderByDescending(r => r.SentAt ?? r.CreatedAt)
            .Take(2000)
            .Select(r => new AdminMailingRecipientRowDto
            {
                RecipientId = r.RecipientId,
                UserId = r.UserId,
                Recipient = r.User.Email ?? ("User #" + r.UserId),
                Status = r.Status,
                SentAt = r.SentAt,
                ReadAt = r.ReadAt,
                ErrorMessage = r.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    private static string BuildStoredTargetType(AdminMailingCampaignUpsertDto dto)
    {
        if (dto.TargetInstanceId is > 0)
            return "instance:" + dto.TargetInstanceId.Value;
        if (dto.TargetCourseId is > 0)
            return "course:" + dto.TargetCourseId.Value;
        return (dto.TargetType ?? "").Trim();
    }

    private static void ParseStoredTargetType(string stored, out string uiTargetType, out int? courseId, out int? instanceId)
    {
        var s = (stored ?? "").Trim();
        courseId = null;
        instanceId = null;

        if (s.StartsWith("instance:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(s.AsSpan("instance:".Length), out var iid))
        {
            uiTargetType = "instance";
            instanceId = iid;
            return;
        }

        if (s.StartsWith("course:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(s.AsSpan("course:".Length), out var cid))
        {
            uiTargetType = "course";
            courseId = cid;
            return;
        }

        uiTargetType = s;
    }

    private async Task<List<int>> ResolveUserIdsAsync(string storedTargetType, CancellationToken cancellationToken)
    {
        var t = (storedTargetType ?? "").Trim();

        if (t.StartsWith("instance:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(t.AsSpan("instance:".Length), out var instanceId))
        {
            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.InstanceId == instanceId && e.Student.DeletedAt == null)
                .Select(e => e.Student.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        if (t.StartsWith("course:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(t.AsSpan("course:".Length), out var courseId))
        {
            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.Instance.CourseId == courseId && e.Student.DeletedAt == null && e.Instance.DeletedAt == null)
                .Select(e => e.Student.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        var key = t.ToLowerInvariant();

        if (key == "all_users")
            return await _context.Users.AsNoTracking().Select(u => u.UserId).ToListAsync(cancellationToken);

        if (key == "all_students")
            return await _context.Students.AsNoTracking().Where(s => s.DeletedAt == null).Select(s => s.UserId).Distinct().ToListAsync(cancellationToken);

        if (key == "all_employees")
            return await _context.Employees.AsNoTracking().Where(e => e.DeletedAt == null).Select(e => e.UserId).Distinct().ToListAsync(cancellationToken);

        return [];
    }
}
