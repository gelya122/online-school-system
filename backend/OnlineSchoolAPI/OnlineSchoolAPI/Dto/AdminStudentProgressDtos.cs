namespace OnlineSchoolAPI.Dto;

public sealed class AdminStudentProgressListRowDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string? Email { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = "";
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int Percent { get; set; }
    /// <summary>Всего заданий (assignment) по курсу.</summary>
    public int TotalHomeworkCount { get; set; }
    /// <summary>Сдано работ (submission с submitted_at).</summary>
    public int SubmittedHomeworkCount { get; set; }
    public decimal? AverageScore { get; set; }
    /// <summary>Сумма баллов по сданным работам (submission.score).</summary>
    public int? TotalHomeworkScore { get; set; }
    /// <summary>Сумма max_score по всем ДЗ курса (верхняя граница).</summary>
    public int TotalHomeworkMaxPoints { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public sealed class AdminProgressAnswerPreviewDto
{
    public int StudentAnswerId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionType { get; set; } = "";
    public string QuestionTextPreview { get; set; } = "";
    public string? ResponseTextPreview { get; set; }
    public decimal? PointsAwarded { get; set; }
    public decimal MaxPoints { get; set; }
}

public sealed class AdminProgressSubmissionDto
{
    public int SubmissionId { get; set; }
    public string? StatusName { get; set; }
    public int? Score { get; set; }
    public int MaxPointsFromQuestions { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public IReadOnlyList<AdminProgressAnswerPreviewDto> Answers { get; set; } = [];
}

public sealed class AdminProgressAssignmentDto
{
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = "";
    public int AssignmentMaxScore { get; set; }
    public int QuestionsMaxPoints { get; set; }
    public AdminProgressSubmissionDto? Submission { get; set; }
}

public sealed class AdminProgressLessonDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public int LessonOrder { get; set; }
    public bool IsOpened { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? OpenedAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int WatchTimeSeconds { get; set; }
    public DateTime? LastAccessed { get; set; }
    public IReadOnlyList<AdminProgressAssignmentDto> Assignments { get; set; } = [];
}

public sealed class AdminProgressModuleDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = "";
    public int ModuleOrder { get; set; }
    public IReadOnlyList<AdminProgressLessonDto> Lessons { get; set; } = [];
}

public sealed class AdminEnrollmentProgressDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string? Email { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = "";
    public DateOnly StartDate { get; set; }

    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int CompletionPercent { get; set; }
    public int TotalHomeworkCount { get; set; }
    public int SubmittedHomeworkCount { get; set; }
    public decimal? AverageHomeworkScore { get; set; }
    public int TotalHomeworkScore { get; set; }
    public int TotalHomeworkMaxPoints { get; set; }

    public IReadOnlyList<AdminProgressModuleDto> Modules { get; set; } = [];
}
