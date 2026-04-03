using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class ApplicationStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TrialApplication> TrialApplications { get; set; } = new List<TrialApplication>();
}
