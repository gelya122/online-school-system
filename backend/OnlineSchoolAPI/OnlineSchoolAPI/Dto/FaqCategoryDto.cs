namespace OnlineSchoolAPI.Dto;

public class FaqCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int? CategoryOrder { get; set; }
}

public class CreateFaqCategoryDto
{
    public string CategoryName { get; set; } = null!;
    public int? CategoryOrder { get; set; }
}

public class UpdateFaqCategoryDto
{
    public string? CategoryName { get; set; }
    public int? CategoryOrder { get; set; }
}

