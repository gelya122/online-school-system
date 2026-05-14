using System;

namespace OnlineSchoolAPI.Models;

public partial class TestStudentAnswer
{
    public int StudentAnswerId { get; set; }

    public int SubmissionId { get; set; }

    public int QuestionId { get; set; }

    public string? ResponseText { get; set; }

    public decimal? PointsAwarded { get; set; }

    public bool IsFullyAutoGraded { get; set; }

    public string? TeacherComment { get; set; }

    public DateTime AnsweredAt { get; set; }

    public int? ReviewedByEmployeeId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Submission Submission { get; set; } = null!;

    public virtual TestQuestion Question { get; set; } = null!;

    public virtual Employee? ReviewedByEmployee { get; set; }
}
