namespace OnlineSchoolAPI.Dto;

public class LessonMaterialDto
{
    public int MaterialId { get; set; }
    public int LessonId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
    public int? DownloadCount { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public class CreateLessonMaterialDto
{
    public int LessonId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
}

public class UpdateLessonMaterialDto
{
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
}

