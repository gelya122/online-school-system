namespace online_school_admin.Models;

public sealed class AdminAuditLogListRowDto
{
    public int AuditLogId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeDisplay { get; set; }
    public int? UserId { get; set; }
    public string? UserDisplay { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public sealed class AdminAuditLogPageDto
{
    public List<AdminAuditLogListRowDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
