namespace online_school_admin.Models;

public sealed class AdminCourseListRowDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public int? ExamId { get; set; }
    public string? ExamName { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool IsActive { get; set; }
    public int ModulesCount { get; set; }
    public int LessonsCount { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminCourseModuleDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = "";
    public int ModuleOrder { get; set; }
    public int LessonsCount { get; set; }
}

public sealed class AdminCourseLessonDto
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public int LessonOrder { get; set; }
    public int LessonTypeId { get; set; }
    public string LessonTypeName { get; set; } = "";
}

public sealed class AdminCourseHomeworkSummaryDto
{
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = "";
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = "";
    public int QuestionsCount { get; set; }
}

public sealed class AdminCourseDetailsDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public int? ExamId { get; set; }
    public string? ExamName { get; set; }
    public string? CoverImgUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int? TotalHours { get; set; }
    public string? WhatYouGet { get; set; }
    public bool IsActive { get; set; }
    public List<AdminCourseModuleDto> Modules { get; set; } = [];
    public List<AdminCourseLessonDto> Lessons { get; set; } = [];
    public List<AdminCourseHomeworkSummaryDto> HomeworkSummaries { get; set; } = [];
}

public sealed class AdminCourseUpsertDto
{
    public string Title { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CoverImgUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int? TotalHours { get; set; }
    public string? WhatYouGet { get; set; }
    public bool IsActive { get; set; } = true;
}

