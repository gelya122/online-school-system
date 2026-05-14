namespace online_school_admin.Models;

public sealed class AdminCourseInstanceListRowDto
{
    public int InstanceId { get; set; }
    public string Title { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = "planned";
    public int? MaxStudents { get; set; }
    public int StudentsCount { get; set; }
    public bool IsActive { get; set; }

    public string? TeacherFullName { get; set; }
    public int MentorsCount { get; set; }

    /// <summary>Для таблицы списка потоков: «число / максимум».</summary>
    public string StudentsSlashMax =>
        MaxStudents is { } m ? $"{StudentsCount} / {m}" : $"{StudentsCount} / —";
}

public sealed class AdminCourseInstanceUpsertDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? EnrollmentStartDate { get; set; }
    public DateOnly? EnrollmentEndDate { get; set; }
    public int? MaxStudents { get; set; }
    public int? LessonsPerWeek { get; set; }
    public int? TotalWeeks { get; set; }
    public string? Timezone { get; set; }
    /// <summary>При создании потока API игнорирует — всегда planned (id 1).</summary>
    public string Status { get; set; } = "planned";

    /// <summary>По умолчанию false при создании потока.</summary>
    public bool IsActive { get; set; }

    public string? ScheduleRulesJson { get; set; }
}

public sealed class AdminCourseInstanceBootstrapDto
{
    public AdminCourseInstanceUpsertDto Instance { get; set; } = new();
    public int TeacherEmployeeId { get; set; }
    public List<int> MentorEmployeeIds { get; set; } = [];
}

public sealed class AdminCourseInstanceDetailsDto
{
    public int InstanceId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? EnrollmentStartDate { get; set; }
    public DateOnly? EnrollmentEndDate { get; set; }
    public int? MaxStudents { get; set; }
    public int? LessonsPerWeek { get; set; }
    public int? TotalWeeks { get; set; }
    public string? Timezone { get; set; }
    public string Status { get; set; } = "planned";
    public bool IsActive { get; set; }

    public string? ScheduleRulesJson { get; set; }

    public IReadOnlyList<AdminInstanceTeacherRowDto> Teachers { get; set; } = [];
    public IReadOnlyList<AdminInstanceCoordinatorRowDto> Coordinators { get; set; } = [];
}

public sealed class AdminInstanceStatusPatchDto
{
    public string Status { get; set; } = "planned";
}

public sealed class AdminInstanceIsActivePatchDto
{
    public bool IsActive { get; set; }
}

public sealed class AdminInstanceTeacherRowDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public bool IsMainTeacher { get; set; }
}

public sealed class AdminInstanceCoordinatorRowDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public bool IsLead { get; set; }
}

public sealed class AdminAssignTeacherDto
{
    public int EmployeeId { get; set; }
    public bool IsMainTeacher { get; set; }
}

public sealed class AdminAssignCoordinatorDto
{
    public int EmployeeId { get; set; }
    public bool IsLead { get; set; }
}

