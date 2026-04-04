namespace OnlineSchoolAPI.Dto;

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string? ExternalPaymentId { get; set; }
    public decimal Amount { get; set; }
    public int? PaymentStatusId { get; set; }
    public int? MethodId { get; set; }
    public string? MethodName { get; set; }
    public string? CardLastFour { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreatePaymentDto
{
    public int OrderId { get; set; }
    public string? ExternalPaymentId { get; set; }
    public decimal Amount { get; set; }
    public int? PaymentStatusId { get; set; }
    public int? MethodId { get; set; }
    public string? CardLastFour { get; set; }
}

public class UpdatePaymentDto
{
    public string? ExternalPaymentId { get; set; }
    public int? PaymentStatusId { get; set; }
    public int? MethodId { get; set; }
    public string? CardLastFour { get; set; }
    public DateTime? PaidAt { get; set; }
}

