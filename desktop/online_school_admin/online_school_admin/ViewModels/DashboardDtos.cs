namespace online_school_admin.ViewModels;

public sealed class DashboardSummaryDto
{
    public int ActiveStudents { get; set; }
    public int NewApplications { get; set; }
    public int HomeworkPendingReview { get; set; }
    public int ActiveInstances { get; set; }
    public decimal PaymentsThisMonth { get; set; }
    public int OverdueInstallmentPayments { get; set; }
}

public sealed class RecentApplicationRowDto
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string Status { get; set; } = "";
    public DateTime? CreatedAt { get; set; }
}

public sealed class HomeworkReviewRowDto
{
    public int SubmissionId { get; set; }
    public string Student { get; set; } = "";
    public string Course { get; set; } = "";
    public string Lesson { get; set; } = "";
    public DateTime? SubmittedAt { get; set; }
    public string Status { get; set; } = "";
}

public sealed class UpcomingInstanceRowDto
{
    public int InstanceId { get; set; }
    public string Instance { get; set; } = "";
    public string Course { get; set; } = "";
    public DateTime StartDate { get; set; } // DateOnly из API десериализуется в DateTime
    public int StudentsCount { get; set; }
}

