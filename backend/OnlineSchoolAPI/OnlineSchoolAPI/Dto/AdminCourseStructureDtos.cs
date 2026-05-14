namespace OnlineSchoolAPI.Dto;

public sealed class AdminCourseModuleRowDto
{
    public int ModuleId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminCourseModuleUpsertDto
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminLessonRowDto
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = "";
    public int LessonTypeId { get; set; }
    public string? LessonTypeName { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool IsActive { get; set; }
}

public class AdminLessonCreateDto
{
    public string Title { get; set; } = "";
    public int LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminLessonUpdateDto : AdminLessonCreateDto
{
}

public sealed class AdminReorderItemDto
{
    public int Id { get; set; }
    public int Order { get; set; }
}

public sealed class AdminReorderRequestDto
{
    public List<AdminReorderItemDto> Items { get; set; } = [];
}

public sealed class AdminLessonMaterialRowDto
{
    public int MaterialId { get; set; }
    public int LessonId { get; set; }
    public string FileName { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public sealed class AdminLessonMaterialCreateDto
{
    public string FileName { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
}

public sealed class AdminLessonMaterialUpdateDto
{
    public string FileName { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
}

