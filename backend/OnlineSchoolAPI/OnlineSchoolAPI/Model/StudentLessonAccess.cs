using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class StudentLessonAccess
{
    public int AccessId { get; set; }

    public int EnrollmentId { get; set; }

    public int LessonId { get; set; }

    public int? PlanId { get; set; }

    public DateOnly PlannedAccessDate { get; set; }

    public TimeOnly? PlannedAccessTime { get; set; }

    public DateTime? ActualOpenDatetime { get; set; }

    public bool? IsAvailable { get; set; }

    public int? OpenedByEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Employee? OpenedByEmployee { get; set; }

    public virtual CourseSchedulePlan? Plan { get; set; }

    public virtual ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
}
