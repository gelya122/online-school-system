namespace online_school_admin.Models;

public sealed class AdminHomeworkRowDto
{
    public int HomeworkId { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int AssignmentTypeId { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public bool IsRequired { get; set; }
    public int HomeworkOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminHomeworkUpsertDto
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int AssignmentTypeId { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public bool IsRequired { get; set; } = true;
    public int HomeworkOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminHomeworkTaskRowDto
{
    public int TaskId { get; set; }
    public int HomeworkId { get; set; }
    public string TaskType { get; set; } = "";
    public string? Title { get; set; }
    public string TaskText { get; set; } = "";
    public string? Explanation { get; set; }
    public int MaxScore { get; set; }
    public int TaskOrder { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool AllowPartialCredit { get; set; }
    public decimal? NumericTolerance { get; set; }
    public bool AutoCheckEnabled { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminHomeworkTaskUpsertDto
{
    public string TaskType { get; set; } = "short_answer";
    public string? Title { get; set; }
    public string TaskText { get; set; } = "";
    public string? Explanation { get; set; }
    public int MaxScore { get; set; } = 1;
    public int TaskOrder { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool AllowPartialCredit { get; set; }
    public decimal? NumericTolerance { get; set; }
    public bool AutoCheckEnabled { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminHomeworkTaskAnswerRowDto
{
    public int AnswerId { get; set; }
    public int TaskId { get; set; }
    public string AnswerText { get; set; } = "";
    public decimal? AnswerNumber { get; set; }
    public bool IsCorrect { get; set; }
    public int? AnswerOrder { get; set; }
}

public sealed class AdminHomeworkTaskAnswerUpsertDto
{
    public string AnswerText { get; set; } = "";
    public decimal? AnswerNumber { get; set; }
    public bool IsCorrect { get; set; } = true;
    public int? AnswerOrder { get; set; }
}

