namespace OnlineSchoolAPI.Dto;

public class StudentCabinetCourseSummaryDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? CoverImgUrl { get; set; }
    public int? TotalHours { get; set; }
    public string? WhatYouGet { get; set; }
}

public class StudentCabinetInstanceSummaryDto
{
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
}

public class StudentCabinetEnrollmentSummaryDto
{
    public int EnrollmentId { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public int? EnrollmentStatusId { get; set; }
    public string? EnrollmentStatusName { get; set; }
    public StudentCabinetCourseSummaryDto Course { get; set; } = null!;
    public StudentCabinetInstanceSummaryDto Instance { get; set; } = null!;
}

public class StudentCabinetLessonOutlineDto
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public int LessonOrder { get; set; }
    public int LessonTypeId { get; set; }
    public int? DurationMinutes { get; set; }
}

public class StudentCabinetModuleOutlineDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ModuleOrder { get; set; }
    public List<StudentCabinetLessonOutlineDto> Lessons { get; set; } = new();
}

public class StudentCabinetEnrollmentDetailDto
{
    public int EnrollmentId { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public int? EnrollmentStatusId { get; set; }
    public string? EnrollmentStatusName { get; set; }
    public StudentCabinetCourseSummaryDto Course { get; set; } = null!;
    public StudentCabinetInstanceSummaryDto Instance { get; set; } = null!;
    public List<StudentCabinetModuleOutlineDto> Modules { get; set; } = new();
}

public class StudentCabinetLessonMaterialDto
{
    public int MaterialId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
    public int? FileSizeKb { get; set; }
    public int? DownloadCount { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public class StudentCabinetAssignmentDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int AssignmentTypeId { get; set; }
    public string? AssignmentTypeName { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public DateOnly? CalculatedDueDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class StudentCabinetLessonAccessDto
{
    public int AccessId { get; set; }
    public DateOnly PlannedAccessDate { get; set; }
    public string? PlannedAccessTime { get; set; }
    public DateTime? ActualOpenDatetime { get; set; }
    public bool? IsAvailable { get; set; }
}

public class StudentCabinetLessonProgressDto
{
    public int ProgressId { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? WatchTimeSeconds { get; set; }
    public DateTime? LastAccessed { get; set; }
}

public class StudentCabinetSubmissionDto
{
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }
    public string? StudentAnswerText { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? Score { get; set; }
    public string? SubmissionStatusName { get; set; }
    public string? TeacherComment { get; set; }
}

public class SubmitCabinetAssignmentDto
{
    public string? AnswerText { get; set; }
}

public class StudentCabinetQuestionDto
{
    public int QuestionId { get; set; }
    public int QuestionOrder { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? QuestionType { get; set; }
    public decimal MaxPoints { get; set; }
    public string? StudentAnswer { get; set; }
    public decimal? PointsAwarded { get; set; }
}

public class SubmitCabinetQuestionAnswerDto
{
    public string? AnswerText { get; set; }
}

public class StudentCabinetAssignmentResultDto
{
    public int AssignmentId { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
}

public class StudentCabinetLessonDetailDto
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int LessonTypeId { get; set; }
    public string? LessonTypeName { get; set; }
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public int LessonOrder { get; set; }
    public bool? IsFreePreview { get; set; }
    public DateTime? CreatedAt { get; set; }
    public StudentCabinetLessonAccessDto? Access { get; set; }
    public StudentCabinetLessonProgressDto? Progress { get; set; }
    public List<StudentCabinetLessonMaterialDto> Materials { get; set; } = new();
    public List<StudentCabinetAssignmentDto> Assignments { get; set; } = new();
    public List<StudentCabinetSubmissionDto> Submissions { get; set; } = new();
}

public class StudentCabinetHomeworkRowDto
{
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public string ModuleTitle { get; set; } = null!;
    public string LessonTitle { get; set; } = null!;
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = null!;
    public string? AssignmentDescription { get; set; }
    public int AssignmentTypeId { get; set; }
    public string? AssignmentTypeName { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public DateOnly? PlannedLessonAccessDate { get; set; }
    public DateOnly? CalculatedDueDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? SubmissionScore { get; set; }
    public string? SubmissionStatusName { get; set; }
}

public class StudentCabinetProgressRowDto
{
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public string ModuleTitle { get; set; } = null!;
    public int ModuleOrder { get; set; }
    public string LessonTitle { get; set; } = null!;
    public int LessonOrder { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? WatchTimeSeconds { get; set; }
    public DateTime? LastAccessed { get; set; }
}
