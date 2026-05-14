using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class StudentProgress
{
    public int ProgressId { get; set; }

    public int EnrollmentId { get; set; }

    public int LessonId { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? WatchTimeSeconds { get; set; }

    public DateTime? LastAccessed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
