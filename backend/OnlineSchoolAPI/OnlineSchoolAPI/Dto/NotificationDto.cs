namespace OnlineSchoolAPI.Dto;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? NotificationType { get; set; }
    public bool? IsRead { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? NotificationType { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
}

public class UpdateNotificationDto
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? NotificationType { get; set; }
    public bool? IsRead { get; set; }
}

