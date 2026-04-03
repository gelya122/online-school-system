namespace OnlineSchoolAPI.Dto;

public class EnrollmentStatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateEnrollmentStatusDto
{
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateEnrollmentStatusDto
{
    public string? StatusName { get; set; }
    public string? Description { get; set; }
}

