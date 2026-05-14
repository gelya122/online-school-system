using CommunityToolkit.Mvvm.ComponentModel;

namespace online_school_admin.Models;

public sealed partial class StudentListItem : ObservableObject
{
    public required int StudentId { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string ClassName { get; init; }
    public DateTime? RegisteredAt { get; init; }
    public required string ActivityStatus { get; init; }
    public string StatisticsTag => $"{ClassName} · {ActivityStatus}";

    [ObservableProperty]
    private bool _isSelected;

    public List<StudentCourseProgress> Courses { get; } = new();
    public List<StudentPaymentRecord> PaymentHistory { get; } = new();
    public List<StudentHomeworkSubmission> HomeworkSubmissions { get; } = new();
    public List<StudentActivityStat> ActivityStats { get; } = new();
}

public sealed class StudentCourseProgress
{
    public required string CourseTitle { get; init; }
    public required int ProgressPercent { get; init; }
    public required string Status { get; init; }
}

public sealed class StudentPaymentRecord
{
    public DateTime? Date { get; init; }
    public required decimal Amount { get; init; }
    public required string Status { get; init; }
    public required string Method { get; init; }
}

public sealed class StudentHomeworkSubmission
{
    public required string CourseTitle { get; init; }
    public required string HomeworkTitle { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public required string Grade { get; init; }
}

public sealed class StudentActivityStat
{
    public required string Metric { get; init; }
    public required string Value { get; init; }
}

public sealed class AdminDataSnapshot
{
    public required IReadOnlyList<StudentListItem> Students { get; init; }
    public int ActiveCoursesCount { get; init; }
    public int NewTrialApplicationsCount { get; init; }
    public int OrdersInPaymentCount { get; init; }
    public int HomeworkPendingReviewCount { get; init; }
}
