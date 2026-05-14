using System.Globalization;
using System.Text.Json.Serialization;

namespace online_school_admin.Models;

public sealed class AdminInstanceScheduleRowDto
{
    public int ScheduleId { get; set; }
    public int LessonId { get; set; }
    public string ModuleTitle { get; set; } = "";
    public string LessonTitle { get; set; } = "";
    public int ModuleOrder { get; set; }
    public int LessonOrder { get; set; }
    public int ReleaseDayOffset { get; set; }
    public DateOnly OpenDate { get; set; }
    public TimeOnly? OpenTime { get; set; }

    /// <summary>Для сетки WPF: <see cref="TimeOnly"/> в DataGrid часто не отображается.</summary>
    [JsonIgnore]
    public string OpenTimeDisplay => OpenTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "—";

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
    public int? ReleaseDayOffset { get; set; }
    public DateOnly? OpenDate { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public bool AutoOpen { get; set; } = true;
}

public sealed class AdminOpenForStudentDto
{
    public int StudentId { get; set; }
}

