namespace online_school_admin.Models;

public sealed class AdminApplicationListRowDto
{
    public string FullName => $"{FirstName} {LastName ?? ""}".Trim();

    public int ApplicationId { get; set; }
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int? ClassNumber { get; set; }
    public string Subjects { get; set; } = "";
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ContactedAt { get; set; }
}

public sealed class AdminApplicationDetailsDto
{
    public int ApplicationId { get; set; }
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int? ClassNumber { get; set; }
    public string? ManagerComment { get; set; }
    public int? StudentId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ContactedAt { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public IReadOnlyList<AdminApplicationSubjectDto> Subjects { get; set; } = [];
    public IReadOnlyList<AdminApplicationCommentDto> Comments { get; set; } = [];
    public IReadOnlyList<AdminApplicationStatusHistoryRowDto> StatusHistory { get; set; } = [];
}

public sealed class AdminApplicationSubjectDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
}

public sealed class AdminApplicationCommentDto
{
    public int CommentId { get; set; }
    public string CommentText { get; set; } = "";
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AdminApplicationStatusHistoryRowDto
{
    public DateTime? CreatedAt { get; set; }
    public string StatusName { get; set; } = "";
    public string? EmployeeName { get; set; }
}

public sealed class AdminApplicationUpsertDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int? ClassNumber { get; set; }
    public List<int> SubjectIds { get; set; } = [];
    public string? Comment { get; set; }
    public int? StatusId { get; set; }
    public int? AssignedManagerId { get; set; }
    public string? ManagerComment { get; set; }
}

public sealed class AdminApplicationCommentCreateDto
{
    public string CommentText { get; set; } = "";
}

public sealed class AdminApplicationManagerPatchDto
{
    public int? ManagerId { get; set; }
    public string? ReasonComment { get; set; }
}

public sealed class AdminApplicationStatusPatchDto
{
    public int StatusId { get; set; }
    public string? ReasonComment { get; set; }
}

public sealed class AdminApplicationContactPatchDto
{
    public string? Comment { get; set; }
}

public sealed class AdminConvertApplicationToStudentResultDto
{
    public int StudentId { get; set; }
    public string TemporaryPassword { get; set; } = "";
}

public sealed class AdminApplicationStatusDictDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = "";
}
