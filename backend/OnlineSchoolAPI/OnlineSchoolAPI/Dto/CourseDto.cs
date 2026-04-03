namespace OnlineSchoolAPI.Dto;

public class CourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public int CategoryId { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
    public string? CoverImgUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int? TotalHours { get; set; }
    public string? WhatYouGet { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateCourseDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public int CategoryId { get; set; }
    public string? CoverImgUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int? TotalHours { get; set; }
    public string? WhatYouGet { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateCourseDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public int? CategoryId { get; set; }
    public string? CoverImgUrl { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int? TotalHours { get; set; }
    public string? WhatYouGet { get; set; }
    public bool? IsActive { get; set; }
}

