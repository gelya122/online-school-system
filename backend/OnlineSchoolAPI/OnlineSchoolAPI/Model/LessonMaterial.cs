using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class LessonMaterial
{
    public int MaterialId { get; set; }

    public int LessonId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public int? FileSizeKb { get; set; }

    public int? DownloadCount { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
