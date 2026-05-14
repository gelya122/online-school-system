namespace online_school_admin.Models;

public sealed class AdminOrderListRowDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public int? InstanceId { get; set; }
    public string? InstanceTitle { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? PromoCode { get; set; }
    public string? Status { get; set; }
    public string? Method { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public sealed class AdminOrderItemRowDto
{
    public int OrderItemId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public int? InstanceId { get; set; }
    public string? InstanceTitle { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public sealed class AdminOrderDetailsDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int? PromoCodeId { get; set; }
    public string? PromoCode { get; set; }
    public int? OrderStatusId { get; set; }
    public string? Status { get; set; }
    public int? MethodId { get; set; }
    public string? Method { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public IReadOnlyList<AdminOrderItemRowDto> Items { get; set; } = [];
}

public sealed class AdminOrderCreateDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? InstanceId { get; set; }
    public int Quantity { get; set; } = 1;
    public int? MethodId { get; set; }
}

public sealed class AdminOrderUpdateDto
{
    public int? MethodId { get; set; }
}

public sealed class AdminOrderStatusPatchDto
{
    public string Status { get; set; } = "";
}

public sealed class AdminApplyPromoCodeDto
{
    public string PromoCode { get; set; } = "";
}

public sealed class AdminPaymentListRowDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? Method { get; set; }
    public string? ExternalPaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminPaymentDetailsDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public int? PaymentStatusId { get; set; }
    public string? Status { get; set; }
    public int? MethodId { get; set; }
    public string? Method { get; set; }
    public string? ExternalPaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminPaymentCreateDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public int? MethodId { get; set; }
    public string? ExternalPaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Status { get; set; }
}

public sealed class AdminPaymentStatusPatchDto
{
    public string Status { get; set; } = "";
}

public sealed class AdminPaymentOrderPatchDto
{
    public int OrderId { get; set; }
}

public sealed class AdminInstallmentListRowDto
{
    public int PlanId { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateOnly? NextPaymentDate { get; set; }
    public string? Status { get; set; }
}

public sealed class AdminInstallmentDetailsDto
{
    public int PlanId { get; set; }
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateOnly? NextPaymentDate { get; set; }
    public string? Status { get; set; }
    public IReadOnlyList<AdminInstallmentPaymentRowDto> Payments { get; set; } = [];
}

public sealed class AdminInstallmentPaymentRowDto
{
    public int InstallmentPaymentId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }
    public int? PaymentStatusId { get; set; }
    public string? StatusName { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
}

public sealed class AdminInstallmentPaymentStatusPatchDto
{
    public string Status { get; set; } = "";
}

public sealed class AdminInstallmentCreateDto
{
    public int OrderId { get; set; }
    public int InstallmentCount { get; set; }
    public decimal? MonthlyPayment { get; set; }
    public DateOnly? NextPaymentDate { get; set; }
    public string? Status { get; set; }
}

public sealed class AdminInstallmentStatusPatchDto
{
    public string Status { get; set; } = "";
}

