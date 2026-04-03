using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class TrialApplication
{
    public int ApplicationId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public int ClassNumber { get; set; }

    public string SelectedSubjects { get; set; } = null!;

    public int? ApplicationStatusId { get; set; }

    public int? AssignedManagerId { get; set; }

    public string? ManagerComment { get; set; }

    public DateTime? ContactedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ApplicationStatus? ApplicationStatus { get; set; }

    public virtual Employee? AssignedManager { get; set; }
}
