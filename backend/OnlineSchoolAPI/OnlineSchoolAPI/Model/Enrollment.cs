using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int InstanceId { get; set; }

    public int? AssignedTeacherId { get; set; }

    public DateTime? EnrolledAt { get; set; }

    public int? EnrollmentStatusId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal? FinalScore { get; set; }

    public virtual Employee? AssignedTeacher { get; set; }

    public virtual EnrollmentStatus? EnrollmentStatus { get; set; }

    public virtual CourseInstance Instance { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<StudentLessonAccess> StudentLessonAccesses { get; set; } = new List<StudentLessonAccess>();

    public virtual ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
}
