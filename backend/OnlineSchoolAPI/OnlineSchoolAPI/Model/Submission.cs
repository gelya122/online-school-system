using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    public int ProgressId { get; set; }

    public int AssignmentId { get; set; }

    public string? StudentAnswerText { get; set; }

    public string? AttachedFileUrl { get; set; }

    public string? AttachedFileName { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? SubmissionStatusId { get; set; }

    public int? Score { get; set; }

    public string? TeacherComment { get; set; }

    public DateTime? GradedAt { get; set; }

    public int? GradedByEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual Employee? GradedByEmployee { get; set; }

    public virtual StudentProgress Progress { get; set; } = null!;

    public virtual ICollection<SubmissionReview> SubmissionReviews { get; set; } = new List<SubmissionReview>();

    public virtual SubmissionStatus? SubmissionStatus { get; set; }
}
