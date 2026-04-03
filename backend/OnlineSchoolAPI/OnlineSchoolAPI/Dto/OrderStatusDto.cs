namespace OnlineSchoolAPI.Dto;

public class OrderStatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateOrderStatusDto
{
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateOrderStatusDto
{
    public string? StatusName { get; set; }
    public string? Description { get; set; }
}

