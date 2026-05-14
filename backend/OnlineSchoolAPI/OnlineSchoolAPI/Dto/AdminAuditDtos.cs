namespace OnlineSchoolAPI.Dto;

/// <summary>Строка журнала для общего списка (с отображаемыми именами сотрудника и пользователя).</summary>
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
    public IReadOnlyList<AdminAuditLogListRowDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
