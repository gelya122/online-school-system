namespace OnlineSchoolAPI.Dto;

public class LessonDto
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public int LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateLessonDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public int LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
}

public class UpdateLessonDto
{
    public string? Title { get; set; }
    public int? LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int? LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
}

