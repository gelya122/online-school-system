namespace OnlineSchoolAPI.Dto;

public sealed class AdminEmployeeListRowDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public int? Experience { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminEmployeeAssignedInstanceDto
{
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string Kind { get; set; } = ""; // teacher/coordinator
    public bool IsLeadOrMain { get; set; }
    public DateTime? AssignedAt { get; set; }
}

public sealed class AdminEmployeeHomeworkCheckedDto
{
    public int SubmissionId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string HomeworkTitle { get; set; } = "";
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CheckedAt { get; set; }
}

public sealed class AdminAuditLogRowDto
{
    public int AuditLogId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public sealed class AdminEmployeeDetailsDto
{
    public int EmployeeId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public int? Experience { get; set; }

    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";

    public List<AdminEmployeeAssignedInstanceDto> AssignedInstances { get; set; } = [];
    public List<AdminEmployeeHomeworkCheckedDto> HomeworkChecked { get; set; } = [];
    public List<AdminAuditLogRowDto> AuditLog { get; set; } = [];
}

public sealed class AdminEmployeeCreateDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public int RoleId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public string? BirthDate { get; set; } // yyyy-MM-dd
    public int? Experience { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarBase64 { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminEmployeeUpdateDto
{
    public string Email { get; set; } = "";
    public int RoleId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public string? BirthDate { get; set; } // yyyy-MM-dd
    public int? Experience { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarBase64 { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminEmployeeChangeRoleDto
{
    public int RoleId { get; set; }
}

