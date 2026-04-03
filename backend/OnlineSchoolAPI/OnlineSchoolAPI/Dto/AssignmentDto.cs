namespace OnlineSchoolAPI.Dto;

public class AssignmentDto
{
    public int AssignmentId { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int AssignmentTypeId { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public string? CorrectAnswer { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateAssignmentDto
{
    public int LessonId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int AssignmentTypeId { get; set; }
    public int MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public string? CorrectAnswer { get; set; }
}

public class UpdateAssignmentDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? AssignmentTypeId { get; set; }
    public int? MaxScore { get; set; }
    public int? DueDaysAfterLesson { get; set; }
    public string? CorrectAnswer { get; set; }
}

