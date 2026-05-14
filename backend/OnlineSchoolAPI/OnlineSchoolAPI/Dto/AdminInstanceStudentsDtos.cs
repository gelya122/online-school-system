namespace OnlineSchoolAPI.Dto;

public sealed class AdminInstanceStudentRowDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public string Status { get; set; } = "";
    public int? AssignedTeacherId { get; set; }
    public string? AssignedTeacherName { get; set; }
    public bool IsPaid { get; set; }
    public int ProgressPercent { get; set; }
    public decimal? FinalScore { get; set; }
}

public sealed class AdminEnrollStudentDto
{
    public int StudentId { get; set; }
}

public sealed class AdminEnrollStudentsBulkDto
{
    public List<int> StudentIds { get; set; } = [];
}

public sealed class AdminEnrollmentStatusPatchDto
{
    public string Status { get; set; } = "";

    /// <summary>Причина смены статуса (для аудита). История enrollment_status_history в API пока не ведётся.</summary>
    public string? Reason { get; set; }
}

public sealed class AdminEnrollmentAssignedTeacherPatchDto
{
    /// <summary>null — снять назначение.</summary>
    public int? AssignedTeacherId { get; set; }
}

