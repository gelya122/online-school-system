using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class LessonType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
