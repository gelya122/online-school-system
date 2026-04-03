using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class EnrollmentStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
