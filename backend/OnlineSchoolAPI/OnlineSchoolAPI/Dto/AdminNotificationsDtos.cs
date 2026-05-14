namespace OnlineSchoolAPI.Dto;

public sealed class AdminNotificationListRowDto
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Recipient { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminCreateNotificationDto
{
    /// <summary>Если true — рассылка всем студентам (userId/instanceId не используются).</summary>
    public bool BroadcastToAllStudents { get; set; }

    public int? UserId { get; set; }
    public int? InstanceId { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Type { get; set; }
}

public sealed class AdminMailingCampaignListRowDto
{
    public int CampaignId { get; set; }
    public string Title { get; set; } = "";
    public string Channel { get; set; } = "";
    public string Status { get; set; } = "";
    public int RecipientsCount { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? CreatedBy { get; set; }
}

public sealed class AdminMailingCampaignDetailsDto
{
    public int CampaignId { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Channel { get; set; } = "";
    public string Status { get; set; } = "";
    public string TargetType { get; set; } = "";
    public int? TargetCourseId { get; set; }
    public int? TargetInstanceId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class AdminMailingCampaignUpsertDto
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Channel { get; set; } = "";
    public string TargetType { get; set; } = "";
    public int? TargetCourseId { get; set; }
    public int? TargetInstanceId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public sealed class AdminMailingRecipientRowDto
{
    public int RecipientId { get; set; }
    public int UserId { get; set; }
    public string Recipient { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorMessage { get; set; }
}

