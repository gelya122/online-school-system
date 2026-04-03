namespace OnlineSchoolAPI.Dto;

public class PromoCodeDto
{
    public int PromoCodeId { get; set; }
    public string Code { get; set; } = null!;
    public int? TypeId { get; set; }
    public decimal DiscountValue { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int? CurrentUses { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreatePromoCodeDto
{
    public string Code { get; set; } = null!;
    public int? TypeId { get; set; }
    public decimal DiscountValue { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdatePromoCodeDto
{
    public int? TypeId { get; set; }
    public decimal? DiscountValue { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public bool? IsActive { get; set; }
}

