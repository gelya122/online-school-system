namespace OnlineSchoolAPI.Dto;

public class CourseModuleDto
{
    public int ModuleId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateCourseModuleDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
}

public class UpdateCourseModuleDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? ModuleOrder { get; set; }
}

