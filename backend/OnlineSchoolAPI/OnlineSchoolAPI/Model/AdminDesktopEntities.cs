using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
 
namespace OnlineSchoolAPI.Models;
 
public partial class AuditLog
{
    public int AuditLogId { get; set; }
    public int? EmployeeId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
 
    public virtual Employee? Employee { get; set; }
    public virtual User? User { get; set; }
}
 
public partial class StudentNote
{
    public int NoteId { get; set; }
    public int StudentId { get; set; }
    public int? EmployeeId { get; set; }
    public string? NoteType { get; set; }
    public string NoteText { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
 
    public virtual Student Student { get; set; } = null!;
    public virtual Employee? Employee { get; set; }
}
 
public partial class MailingCampaign
{
    public int CampaignId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Channel { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string TargetType { get; set; } = null!;
    public int? CreatedByEmployeeId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
 
    public virtual Employee? CreatedByEmployee { get; set; }
    public virtual ICollection<MailingRecipient> MailingRecipients { get; set; } = new List<MailingRecipient>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
 
public partial class MailingRecipient
{
    public int RecipientId { get; set; }
    public int CampaignId { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
 
    public virtual MailingCampaign Campaign { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
 
public partial class SiteSetting
{
    public int SettingId { get; set; }
    public string? SiteName { get; set; }
    public string? MainPageTitle { get; set; }
    public string? MainPageDescription { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? VkUrl { get; set; }
    public string? TelegramUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public bool IsMaintenanceMode { get; set; }
    public int? UpdatedByEmployeeId { get; set; }
    public DateTime UpdatedAt { get; set; }
 
    public virtual Employee? UpdatedByEmployee { get; set; }
}
 
public partial class SiteBanner
{
    public int BannerId { get; set; }
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int BannerOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
 
public partial class FileStorage
{
    public int FileId { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
    public string? MimeType { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? UploadedByUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
 
    public virtual User? UploadedByUser { get; set; }
}
 
