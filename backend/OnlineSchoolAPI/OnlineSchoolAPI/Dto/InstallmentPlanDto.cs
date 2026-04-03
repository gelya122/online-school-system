namespace OnlineSchoolAPI.Dto;

public class InstallmentPlanDto
{
    public int PlanId { get; set; }
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateOnly? NextPaymentDate { get; set; }
    public string? PlanStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateInstallmentPlanDto
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateOnly? NextPaymentDate { get; set; }
    public string? PlanStatus { get; set; }
}

public class UpdateInstallmentPlanDto
{
    public DateOnly? NextPaymentDate { get; set; }
    public string? PlanStatus { get; set; }
}

