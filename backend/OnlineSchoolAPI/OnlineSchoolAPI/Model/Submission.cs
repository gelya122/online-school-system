using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    /// <summary>Зачисление (script4: submission.enrollment_id вместо progress_id).</summary>
    public int? EnrollmentId { get; set; }

    public int AssignmentId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int SubmissionStatusId { get; set; }

    public int? Score { get; set; }

    public string? TeacherComment { get; set; }

    public DateTime? GradedAt { get; set; }

    public int? GradedByEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual Employee? GradedByEmployee { get; set; }

    public virtual Enrollment? Enrollment { get; set; }

    public virtual SubmissionStatus SubmissionStatus { get; set; } = null!;

    public virtual ICollection<TestStudentAnswer> TestStudentAnswers { get; set; } = new List<TestStudentAnswer>();
}
