namespace OnlineSchoolAPI.Dto;

public class SubmissionStatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateSubmissionStatusDto
{
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateSubmissionStatusDto
{
    public string? StatusName { get; set; }
    public string? Description { get; set; }
}

