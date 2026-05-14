namespace OnlineSchoolAPI.Dto;

public sealed class AdminCourseInstanceListRowDto
{
    public int InstanceId { get; set; }
    public string Title { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    /// <summary>Код статуса (course_instance_status.code), например planned, enrollment_open, active.</summary>
    public string Status { get; set; } = "planned";
    public int? MaxStudents { get; set; }
    public int StudentsCount { get; set; }
    public bool IsActive { get; set; }

    /// <summary>ФИО первого назначенного преподавателя (роль teacher).</summary>
    public string? TeacherFullName { get; set; }

    /// <summary>Число наставников (роль mentor).</summary>
    public int MentorsCount { get; set; }
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
    /// <summary>Код статуса (course_instance_status.code).</summary>
    public string Status { get; set; } = "planned";
    public bool IsActive { get; set; }

    /// <summary>JSON правил расписания: [{ "dayOfWeek": 1, "time": "18:00" }, …] (1 — понедельник, 7 — воскресенье).</summary>
    public string? ScheduleRulesJson { get; set; }

    public IReadOnlyList<AdminInstanceTeacherRowDto> Teachers { get; set; } = [];
    public IReadOnlyList<AdminInstanceCoordinatorRowDto> Coordinators { get; set; } = [];
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
    /// <summary>Код статуса (course_instance_status.code). При POST создания потока не используется — всегда planned (id 1).</summary>
    public string Status { get; set; } = "planned";

    /// <summary>По умолчанию false: поток не публикуется автоматически при создании.</summary>
    public bool IsActive { get; set; }

    /// <summary>JSON правил расписания (день недели 1–7, время HH:mm).</summary>
    public string? ScheduleRulesJson { get; set; }
}

public sealed class AdminCourseInstanceBootstrapDto
{
    public AdminCourseInstanceUpsertDto Instance { get; set; } = new();

    public int TeacherEmployeeId { get; set; }

    public List<int> MentorEmployeeIds { get; set; } = [];
}

public sealed class AdminInstanceStatusPatchDto
{
    /// <summary>Код статуса (course_instance_status.code).</summary>
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

