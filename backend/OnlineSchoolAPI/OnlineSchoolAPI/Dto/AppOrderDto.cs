namespace OnlineSchoolAPI.Dto;

public class AppOrderDto
{
    public int OrderId { get; set; }
    public int StudentId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int? OrderStatusId { get; set; }
    public int? MethodId { get; set; }
    public string? MethodName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class CreateAppOrderDto
{
    public int StudentId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int? OrderStatusId { get; set; }
    public int? MethodId { get; set; }
}

public class UpdateAppOrderDto
{
    public decimal? DiscountAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public int? OrderStatusId { get; set; }
    public int? MethodId { get; set; }
    public DateTime? PaidAt { get; set; }
}

