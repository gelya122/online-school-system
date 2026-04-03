namespace OnlineSchoolAPI.Dto;

public class AssignmentVariantDto
{
    public int VariantId { get; set; }
    public int AssignmentId { get; set; }
    public string VariantText { get; set; } = null!;
    public bool? IsCorrect { get; set; }
    public int? VariantOrder { get; set; }
}

public class CreateAssignmentVariantDto
{
    public int AssignmentId { get; set; }
    public string VariantText { get; set; } = null!;
    public bool? IsCorrect { get; set; }
    public int? VariantOrder { get; set; }
}

public class UpdateAssignmentVariantDto
{
    public string? VariantText { get; set; }
    public bool? IsCorrect { get; set; }
    public int? VariantOrder { get; set; }
}

