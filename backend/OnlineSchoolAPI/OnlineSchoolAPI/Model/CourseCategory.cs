using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class CourseCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public int? SubjectId { get; set; }

    public int? ExamId { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Subject? Subject { get; set; }

    public virtual Exam? Exam { get; set; }
}
