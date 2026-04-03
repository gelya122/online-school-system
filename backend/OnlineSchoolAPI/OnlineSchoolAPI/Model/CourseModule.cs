using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class CourseModule
{
    public int ModuleId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int ModuleOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
