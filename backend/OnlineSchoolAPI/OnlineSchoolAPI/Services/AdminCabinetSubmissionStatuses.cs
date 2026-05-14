using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Services;

/// <summary>
/// Служебные имена статусов submission для админского потока проверки (совместимость с прежними строками HomeworkSubmission.Status).
/// </summary>
public static class AdminCabinetSubmissionStatuses
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string ManualCheckRequired = "manual_check_required";
    public const string Checked = "checked";
    public const string Approved = "approved";
    public const string RevisionRequested = "revision_requested";
    public const string Rejected = "rejected";

    public static async Task<int> GetOrCreateStatusIdAsync(OnlineSchoolDbContext db, string statusKey, CancellationToken cancellationToken = default)
    {
        var name = statusKey.Trim().ToLowerInvariant();
        var existing = await db.SubmissionStatuses.AsNoTracking()
            .Where(s => s.StatusName != null && s.StatusName.ToLower() == name)
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing.HasValue)
            return existing.Value;

        var entity = new SubmissionStatus
        {
            StatusName = name,
            Description = "Системный статус для админ-потока проверки заданий."
        };
        db.SubmissionStatuses.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.StatusId;
    }

    public static async Task<string?> GetStatusNameAsync(OnlineSchoolDbContext db, int statusId, CancellationToken cancellationToken = default)
        => await db.SubmissionStatuses.AsNoTracking()
            .Where(s => s.StatusId == statusId)
            .Select(s => s.StatusName)
            .FirstOrDefaultAsync(cancellationToken);
}
