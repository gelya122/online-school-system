using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class SubmissionStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
