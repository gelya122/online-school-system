namespace OnlineSchoolAPI.Dto;

public class CourseCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
}

public class CreateCourseCategoryDto
{
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
}

public class UpdateCourseCategoryDto
{
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
}

