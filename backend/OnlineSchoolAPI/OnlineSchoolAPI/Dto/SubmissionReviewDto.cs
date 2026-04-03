namespace OnlineSchoolAPI.Dto;

public class SubmissionReviewDto
{
    public int ReviewId { get; set; }
    public int SubmissionId { get; set; }
    public int? QuestionNumber { get; set; }
    public int? StudentVariantId { get; set; }
    public bool? IsCorrect { get; set; }
    public string? TeacherComment { get; set; }
    public int? PointsAwarded { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateSubmissionReviewDto
{
    public int SubmissionId { get; set; }
    public int? QuestionNumber { get; set; }
    public int? StudentVariantId { get; set; }
    public bool? IsCorrect { get; set; }
    public string? TeacherComment { get; set; }
    public int? PointsAwarded { get; set; }
}

public class UpdateSubmissionReviewDto
{
    public int? QuestionNumber { get; set; }
    public int? StudentVariantId { get; set; }
    public bool? IsCorrect { get; set; }
    public string? TeacherComment { get; set; }
    public int? PointsAwarded { get; set; }
}

