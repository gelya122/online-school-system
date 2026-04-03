using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class CourseInstanceCoordinator
{
    public int CoordinatorId { get; set; }

    public int InstanceId { get; set; }

    public int EmployeeId { get; set; }

    public bool? IsLead { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual CourseInstance Instance { get; set; } = null!;
}
