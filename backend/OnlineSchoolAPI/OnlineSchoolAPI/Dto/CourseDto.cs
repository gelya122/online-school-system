namespace OnlineSchoolAPI.Dto;

/// <summary>Модуль курса для публичной страницы курса (структура программы).</summary>
public class CourseModuleOutlineDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public int LessonCount { get; set; }
}

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

    /// <summary>Заполняется только в GET /Courses/{id} — модули и число уроков из БД.</summary>
    public List<CourseModuleOutlineDto>? Modules { get; set; }

    /// <summary>Средняя оценка по опубликованным отзывам с рейтингом (GET /Courses/{id}).</summary>
    public double? ReviewAverage { get; set; }

    /// <summary>Число таких отзывов.</summary>
    public int? ReviewCount { get; set; }
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

