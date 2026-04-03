namespace OnlineSchoolAPI.Dto;

public class SubjectDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = null!;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateSubjectDto
{
    public string SubjectName { get; set; } = null!;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateSubjectDto
{
    public string? SubjectName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

