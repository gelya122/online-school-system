namespace online_school_admin.Models;

public sealed class AdminProfileDto
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Role { get; set; } = "";
    public DateOnly? BirthDate { get; set; }
    public int? Experience { get; set; }
    public string? AvatarUrl { get; set; }

    public IReadOnlyList<AdminProfileInstanceRowDto> AssignedInstances { get; set; } = [];
    public int HomeworkOnReview { get; set; }
    public IReadOnlyList<AdminProfileCheckedHomeworkRowDto> RecentChecked { get; set; } = [];
}

public sealed class AdminProfileUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int? Experience { get; set; }
    public string? Email { get; set; }
}

public sealed class AdminChangePasswordDto
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public sealed class AdminUploadAvatarDto
{
    public string Base64 { get; set; } = "";
}

public sealed class AdminAvatarUploadResultDto
{
    public string AvatarUrl { get; set; } = "";
}

public sealed class AdminProfileInstanceRowDto
{
    public int InstanceId { get; set; }
    public string Title { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public string Status { get; set; } = "";
}

public sealed class AdminProfileCheckedHomeworkRowDto
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string InstanceTitle { get; set; } = "";
    public string LessonTitle { get; set; } = "";
    public int? TotalScore { get; set; }
    public DateTime? CheckedAt { get; set; }
}

