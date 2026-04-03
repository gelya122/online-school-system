using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Assignment
{
    public int AssignmentId { get; set; }

    public int LessonId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int AssignmentTypeId { get; set; }

    public int MaxScore { get; set; }

    public int? DueDaysAfterLesson { get; set; }

    public string? CorrectAnswer { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AssignmentType AssignmentType { get; set; } = null!;

    public virtual ICollection<AssignmentVariant> AssignmentVariants { get; set; } = new List<AssignmentVariant>();

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
