using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class InstallmentPayment
{
    public int InstallmentPaymentId { get; set; }

    public int PlanId { get; set; }

    public int InstallmentNumber { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentStatus { get; set; }

    public int? PaymentStatusId { get; set; }

    public int? PaymentId { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual InstallmentPlan Plan { get; set; } = null!;

    public virtual PaymentStatus? PaymentStatusNavigation { get; set; }

    public virtual Payment? Payment { get; set; }
}
