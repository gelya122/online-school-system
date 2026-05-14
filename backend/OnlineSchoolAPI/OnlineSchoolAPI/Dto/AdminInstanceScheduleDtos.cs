namespace OnlineSchoolAPI.Dto;

public sealed class AdminInstanceScheduleRowDto
{
    public int ScheduleId { get; set; } // plan_id
    public int LessonId { get; set; }
    public string ModuleTitle { get; set; } = "";
    public string LessonTitle { get; set; } = "";
    public int ModuleOrder { get; set; }
    public int LessonOrder { get; set; }
    /// <summary>Смещение в днях от start_date потока (course_schedule_plan.release_day_offset).</summary>
    public int ReleaseDayOffset { get; set; }
    public DateOnly OpenDate { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public bool AutoOpen { get; set; }
    public bool OpenedManually { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public bool IsPublished { get; set; } = true;
}

public sealed class AdminGenerateInstanceScheduleDto
{
    public bool OverwriteExisting { get; set; } = true;
    public TimeOnly? DefaultOpenTime { get; set; } = new TimeOnly(9, 0);
}

public sealed class AdminUpdateInstanceScheduleDto
{
    /// <summary>Если задано — имеет приоритет над <see cref="OpenDate"/>.</summary>
    public int? ReleaseDayOffset { get; set; }

    public DateOnly? OpenDate { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public bool AutoOpen { get; set; } = true;
}

public sealed class AdminOpenForStudentDto
{
    public int StudentId { get; set; }
}

