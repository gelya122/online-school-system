using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class SubmissionReview
{
    public int ReviewId { get; set; }

    public int SubmissionId { get; set; }

    public int? QuestionNumber { get; set; }

    public int? StudentVariantId { get; set; }

    public bool? IsCorrect { get; set; }

    public string? TeacherComment { get; set; }

    public int? PointsAwarded { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Submission Submission { get; set; } = null!;
}
