using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class AssignmentType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
