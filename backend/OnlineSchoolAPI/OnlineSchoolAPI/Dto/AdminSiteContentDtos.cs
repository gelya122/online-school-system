namespace OnlineSchoolAPI.Dto;

public sealed class AdminFaqCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? CategoryOrder { get; set; }
    public List<AdminFaqItemDto> Items { get; set; } = [];
}

public sealed class AdminFaqItemDto
{
    public int FaqId { get; set; }
    public int? CategoryId { get; set; }
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public int? ItemOrder { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class AdminFaqCategoryUpsertDto
{
    public string CategoryName { get; set; } = "";
    public int? CategoryOrder { get; set; }
}

public sealed class AdminFaqItemUpsertDto
{
    public int? CategoryId { get; set; }
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public int? ItemOrder { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class AdminReviewListRowDto
{
    public int ReviewId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminReviewPublishedDto
{
    public bool IsPublished { get; set; }
}
