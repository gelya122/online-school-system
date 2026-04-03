namespace OnlineSchoolAPI.Dto;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsPublished { get; set; }
}

public class UpdateReviewDto
{
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsPublished { get; set; }
}

