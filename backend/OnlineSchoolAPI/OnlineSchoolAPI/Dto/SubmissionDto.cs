namespace OnlineSchoolAPI.Dto;

public class SubmissionDto
{
    public int SubmissionId { get; set; }
    public int ProgressId { get; set; }
    public int AssignmentId { get; set; }
    public string? StudentAnswerText { get; set; }
    public string? AttachedFileUrl { get; set; }
    public string? AttachedFileName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? SubmissionStatusId { get; set; }
    public int? Score { get; set; }
    public string? TeacherComment { get; set; }
    public DateTime? GradedAt { get; set; }
    public int? GradedByEmployeeId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateSubmissionDto
{
    public int ProgressId { get; set; }
    public int AssignmentId { get; set; }
    public string? StudentAnswerText { get; set; }
    public string? AttachedFileUrl { get; set; }
    public string? AttachedFileName { get; set; }
    public int? SubmissionStatusId { get; set; }
}

public class UpdateSubmissionDto
{
    public string? StudentAnswerText { get; set; }
    public string? AttachedFileUrl { get; set; }
    public string? AttachedFileName { get; set; }
    public int? SubmissionStatusId { get; set; }
    public int? Score { get; set; }
    public string? TeacherComment { get; set; }
    public int? GradedByEmployeeId { get; set; }
}

