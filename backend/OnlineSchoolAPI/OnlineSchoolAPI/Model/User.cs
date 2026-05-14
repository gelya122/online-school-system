using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string? Login { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public bool? IsEmailConfirmed { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? PasswordChangedAt { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockedUntil { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Employee? CreatedByEmployee { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual UserRole Role { get; set; } = null!;

    public virtual Student? Student { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<MailingRecipient> MailingRecipients { get; set; } = new List<MailingRecipient>();

    public virtual ICollection<FileStorage> FileStorages { get; set; } = new List<FileStorage>();
}
