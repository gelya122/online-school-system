namespace OnlineSchoolAPI.Dto;

public class PaymentMethodDto
{
    public int MethodId { get; set; }
    public string MethodName { get; set; } = null!;
    public string? Description { get; set; }
}
