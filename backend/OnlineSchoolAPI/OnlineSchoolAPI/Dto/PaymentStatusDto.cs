namespace OnlineSchoolAPI.Dto;

public class PaymentStatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreatePaymentStatusDto
{
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdatePaymentStatusDto
{
    public string? StatusName { get; set; }
    public string? Description { get; set; }
}

