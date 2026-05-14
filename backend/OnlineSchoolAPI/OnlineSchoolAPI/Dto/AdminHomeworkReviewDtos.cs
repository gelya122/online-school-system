namespace OnlineSchoolAPI.Dto;

/// <summary>Строка очереди ручной проверки: один test_student_answer (развёрнутый ответ).</summary>
public sealed class AdminHomeworkAnswerReviewQueueRowDto
{
    public int StudentAnswerId { get; set; }
    public int SubmissionId { get; set; }
    public int StudentId { get; set; }
    public string Student { get; set; } = "";
    public string? StudentEmail { get; set; }
    public string Course { get; set; } = "";
    public int CourseId { get; set; }
    public string Instance { get; set; } = "";
    public int InstanceId { get; set; }
    public string Lesson { get; set; } = "";
    public string Homework { get; set; } = "";
    /// <summary>Краткий текст задания (question_text).</summary>
    public string TaskPreview { get; set; } = "";
    public DateTime AnsweredAt { get; set; }
    public decimal MaxPoints { get; set; }
    public int? MentorEmployeeId { get; set; }
    public string? MentorEmployeeName { get; set; }
    /// <summary>Человекочитаемый статус проверки ответа.</summary>
    public string ReviewStatus { get; set; } = "";
    public string SubmissionStatus { get; set; } = "";
    public int? TotalScore { get; set; }
    public string? Reviewer { get; set; }
    public int? ReviewerId { get; set; }
}

public sealed class AdminHomeworkSubmissionDetailsDto
{
    public int SubmissionId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string? StudentEmail { get; set; }
    public string? StudentPhone { get; set; }

    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public int InstanceId { get; set; }
    public string InstanceName { get; set; } = "";
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public int HomeworkId { get; set; }
    public string HomeworkTitle { get; set; } = "";

    public string Status { get; set; } = "";
    public int? TotalScore { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CheckedAt { get; set; }
    public int? CheckedByEmployeeId { get; set; }
    public string? CheckedByEmployeeName { get; set; }

    public IReadOnlyList<AdminHomeworkTaskSubmissionDetailsDto> Tasks { get; set; } = [];
}

public sealed class AdminHomeworkTaskSubmissionDetailsDto
{
    public int TaskSubmissionId { get; set; }
    public int TaskId { get; set; }
    public string TaskType { get; set; } = "";
    public string? Title { get; set; }
    public string TaskText { get; set; } = "";
    public string? Explanation { get; set; }
    public int MaxScore { get; set; }
    public decimal MaxPointsDecimal { get; set; }

    public string? StudentAnswerText { get; set; }
    public string? AttachedFileUrl { get; set; }
    public string? AttachedFileName { get; set; }

    public string? AutoCheckResult { get; set; }
    public bool? IsCorrect { get; set; }

    public decimal? PointsAwarded { get; set; }
    public int? Score { get; set; }
    public string? TeacherComment { get; set; }
    public int? CheckedByEmployeeId { get; set; }
    public DateTime? CheckedAt { get; set; }

    /// <summary>Требуется ручная проверка (очередь преподавателя).</summary>
    public bool NeedsManualReview { get; set; }
}

public sealed class AdminHomeworkTaskSubmissionReviewDto
{
    /// <summary>Балл (points_awarded), 0…max_points.</summary>
    public decimal? Score { get; set; }
    public string? TeacherComment { get; set; }
    public bool? IsCorrect { get; set; }
}

public sealed class AdminAssignReviewerDto
{
    public int? ReviewerEmployeeId { get; set; }
}

