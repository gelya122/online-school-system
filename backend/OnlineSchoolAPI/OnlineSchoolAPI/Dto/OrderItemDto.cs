namespace OnlineSchoolAPI.Dto;

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int CourseId { get; set; }
    public int? InstanceId { get; set; }
    public decimal Price { get; set; }
    public int? Quantity { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateOrderItemDto
{
    public int OrderId { get; set; }
    public int CourseId { get; set; }
    public int? InstanceId { get; set; }
    public decimal Price { get; set; }
    public int? Quantity { get; set; }
}

public class UpdateOrderItemDto
{
    public int? InstanceId { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
}

