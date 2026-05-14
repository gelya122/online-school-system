namespace online_school_admin.Models.Admin;

public sealed class CourseModuleOutlineDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public int LessonCount { get; set; }
}

public sealed class CourseTemplateDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
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
    public List<CourseModuleOutlineDto>? Modules { get; set; }
    public double? ReviewAverage { get; set; }
    public int? ReviewCount { get; set; }
}

public sealed class CreateCourseRequest
{
    public string Title { get; set; } = "";
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

public sealed class UpdateCourseRequest
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

public sealed class CourseCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
}

public sealed class SubjectDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
}

public sealed class ExamDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = "";
}

public sealed class CourseModuleDto
{
    public int ModuleId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class CreateCourseModuleRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
}

public sealed class UpdateCourseModuleRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? ModuleOrder { get; set; }
}

public sealed class LessonDto
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = "";
    public int LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class CreateLessonRequest
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = "";
    public int LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
}

public sealed class UpdateLessonRequest
{
    public string? Title { get; set; }
    public int? LessonTypeId { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int? LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
}

public sealed class LessonMaterialDto
{
    public int MaterialId { get; set; }
    public int LessonId { get; set; }
    public string FileName { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
    public int? DownloadCount { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public sealed class AssignmentDto
{
    public int AssignmentId { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class CreateAssignmentRequest
{
    public int LessonId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
}

public sealed class UpdateAssignmentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
}

public sealed class ReviewDto
{
    public int ReviewId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class UpdateReviewRequest
{
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsPublished { get; set; }
}

public sealed class CourseInstanceDto
{
    public int InstanceId { get; set; }
    public int CourseId { get; set; }
    public string InstanceName { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
    public int? MaxStudents { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class CreateCourseInstanceRequest
{
    public int CourseId { get; set; }
    public string InstanceName { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
    public int? MaxStudents { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class UpdateCourseInstanceRequest
{
    public string? InstanceName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
    public int? MaxStudents { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int InstanceId { get; set; }
    public int? AssignedTeacherId { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public int? EnrollmentStatusId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? FinalScore { get; set; }
}

public sealed class LessonTypeDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
}

public sealed class AssignmentTypeDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
}
