using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

/// <summary>Справочник статусов потока (таблица course_instance_status, script5).</summary>
public partial class CourseInstanceStatus
{
    public int StatusId { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();
}
