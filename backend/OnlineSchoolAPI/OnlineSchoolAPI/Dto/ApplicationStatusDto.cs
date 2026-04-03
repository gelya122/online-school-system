namespace OnlineSchoolAPI.Dto;

public class ApplicationStatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateApplicationStatusDto
{
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateApplicationStatusDto
{
    public string? StatusName { get; set; }
    public string? Description { get; set; }
}

