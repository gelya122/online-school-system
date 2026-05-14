namespace OnlineSchoolAPI.Dto;

public sealed class AdminDashboardSummaryDto
{
    public int ActiveStudents { get; set; }
    public int NewApplications { get; set; }
    public int HomeworkPendingReview { get; set; }
    public int ActiveInstances { get; set; }
    public decimal PaymentsThisMonth { get; set; }
    public int OverdueInstallmentPayments { get; set; }
}

public sealed class AdminRecentApplicationDto
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string Status { get; set; } = "";
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminHomeworkReviewQueueRowDto
{
    public int SubmissionId { get; set; }
    public string Student { get; set; } = "";
    public string Course { get; set; } = "";
    public string Lesson { get; set; } = "";
    public DateTime? SubmittedAt { get; set; }
    public string Status { get; set; } = "";
}

public sealed class AdminUpcomingInstanceDto
{
    public int InstanceId { get; set; }
    public string Instance { get; set; } = "";
    public string Course { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public int StudentsCount { get; set; }
}

