namespace OnlineSchoolAPI.Dto;

public class InstallmentPaymentDto
{
    public int InstallmentPaymentId { get; set; }
    public int PlanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateInstallmentPaymentDto
{
    public int PlanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentStatus { get; set; }
}

public class UpdateInstallmentPaymentDto
{
    public string? PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
}

