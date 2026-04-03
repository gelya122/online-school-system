namespace OnlineSchoolAPI.Dto;

public class ValidatePromoCodeDto
{
    public string Code { get; set; } = null!;
    public decimal CartTotal { get; set; }
}

public class ValidatePromoCodeResponseDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
}

