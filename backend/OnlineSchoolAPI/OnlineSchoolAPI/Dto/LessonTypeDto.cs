namespace OnlineSchoolAPI.Dto;

public class LessonTypeDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateLessonTypeDto
{
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateLessonTypeDto
{
    public string? TypeName { get; set; }
    public string? Description { get; set; }
}

