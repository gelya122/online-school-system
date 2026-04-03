namespace OnlineSchoolAPI.Dto;

public class FaqItemDto
{
    public int FaqId { get; set; }
    public int? CategoryId { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public bool? IsActive { get; set; }
    public int? ItemOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateFaqItemDto
{
    public int? CategoryId { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public bool? IsActive { get; set; }
    public int? ItemOrder { get; set; }
}

public class UpdateFaqItemDto
{
    public int? CategoryId { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public bool? IsActive { get; set; }
    public int? ItemOrder { get; set; }
}

