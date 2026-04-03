using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int ModuleId { get; set; }

    public string Title { get; set; } = null!;

    public int LessonTypeId { get; set; }

    public string? Content { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public int LessonOrder { get; set; }

    public bool? IsFreePreview { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<CourseSchedulePlan> CourseSchedulePlans { get; set; } = new List<CourseSchedulePlan>();

    public virtual ICollection<LessonMaterial> LessonMaterials { get; set; } = new List<LessonMaterial>();

    public virtual LessonType LessonType { get; set; } = null!;

    public virtual CourseModule Module { get; set; } = null!;

    public virtual ICollection<StudentLessonAccess> StudentLessonAccesses { get; set; } = new List<StudentLessonAccess>();

    public virtual ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
}
