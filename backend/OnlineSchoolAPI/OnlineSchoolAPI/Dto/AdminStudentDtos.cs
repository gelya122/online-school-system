namespace OnlineSchoolAPI.Dto;

public sealed class AdminStudentHomeworkRowDto
{
    public int SubmissionId { get; set; }
    public string AssignmentTitle { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string InstanceName { get; set; } = "";
    public int? Score { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public string StatusName { get; set; } = "";
}

public sealed class AdminStudentListRowDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public int ClassNumber { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RegisteredAt { get; set; }
}

public sealed class AdminStudentEnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public sealed class AdminStudentPaymentDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminStudentProgressDto
{
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int WatchTimeSeconds { get; set; }
}

public sealed class AdminStudentNoteDto
{
    public int NoteId { get; set; }
    public string? NoteType { get; set; }
    public string NoteText { get; set; } = "";
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AdminStudentDetailsDto
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int ClassNumber { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? RegisteredAt { get; set; }

    public List<AdminStudentEnrollmentDto> Enrollments { get; set; } = [];
    public List<AdminStudentPaymentDto> Payments { get; set; } = [];
    public AdminStudentProgressDto Progress { get; set; } = new();
    public List<AdminStudentHomeworkRowDto> Homework { get; set; } = [];
    public List<AdminStudentNoteDto> Notes { get; set; } = [];
}

public class AdminStudentUpsertDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? BirthDate { get; set; } // yyyy-MM-dd
    public int ClassNumber { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarBase64 { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminStudentCreateDto : AdminStudentUpsertDto
{
    /// <summary>Пароль для входа ученика (не короче 6 символов).</summary>
    public string Password { get; set; } = "";
}

public sealed class AdminStudentNoteCreateDto
{
    public string? NoteType { get; set; }
    public string NoteText { get; set; } = "";
}

