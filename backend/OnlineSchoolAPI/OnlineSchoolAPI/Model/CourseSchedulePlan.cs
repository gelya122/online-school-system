using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class CourseSchedulePlan
{
    public int PlanId { get; set; }

    public int InstanceId { get; set; }

    public int LessonId { get; set; }

    public int ReleaseDayOffset { get; set; }

    public TimeOnly? ReleaseTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public int? LessonOrder { get; set; }

    public bool IsPublished { get; set; } = true;

    public virtual CourseInstance Instance { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
