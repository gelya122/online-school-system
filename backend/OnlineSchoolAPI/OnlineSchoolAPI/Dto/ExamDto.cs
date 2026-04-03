namespace OnlineSchoolAPI.Dto;

public class ExamDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = null!;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateExamDto
{
    public string ExamName { get; set; } = null!;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateExamDto
{
    public string? ExamName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

