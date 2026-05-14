namespace OnlineSchoolAPI.Dto;

public sealed class AdminPromoCodeListRowDto
{
    public int PromoCodeId { get; set; }
    public string Code { get; set; } = "";
    public int? DiscountTypeId { get; set; }
    public string? DiscountTypeName { get; set; }
    public decimal DiscountValue { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public bool IsActive { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? AppliesToCourseId { get; set; }
    public string? AppliesToCourseTitle { get; set; }
    public int? AppliesToInstanceId { get; set; }
    public string? AppliesToInstanceTitle { get; set; }
}

public sealed class AdminPromoCodeDetailsDto
{
    public int PromoCodeId { get; set; }
    public string Code { get; set; } = "";
    public int? DiscountTypeId { get; set; }
    public decimal DiscountValue { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public bool IsActive { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? AppliesToCourseId { get; set; }
    public int? AppliesToInstanceId { get; set; }
}

public sealed class AdminPromoCodeUpsertDto
{
    public string Code { get; set; } = "";
    public int? DiscountTypeId { get; set; }
    public decimal DiscountValue { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? AppliesToCourseId { get; set; }
    public int? AppliesToInstanceId { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminPromoCodeUsageRowDto
{
    public int UsageId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; }
}

public sealed class AdminDiscountTypeDictDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

