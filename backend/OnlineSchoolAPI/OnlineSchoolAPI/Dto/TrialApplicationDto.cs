namespace OnlineSchoolAPI.Dto;

public class TrialApplicationDto
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
}

public class CreateTrialApplicationDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public int ClassNumber { get; set; }
    public string SelectedSubjects { get; set; } = null!;
    public int? ApplicationStatusId { get; set; }
}

public class UpdateTrialApplicationDto
{
    public int? ApplicationStatusId { get; set; }
    public int? AssignedManagerId { get; set; }
    public string? ManagerComment { get; set; }
    public DateTime? ContactedAt { get; set; }
}

