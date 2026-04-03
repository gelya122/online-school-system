using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class InstallmentPlan
{
    public int PlanId { get; set; }

    public int OrderId { get; set; }

    public decimal TotalAmount { get; set; }

    public int InstallmentCount { get; set; }

    public decimal MonthlyPayment { get; set; }

    public DateOnly? NextPaymentDate { get; set; }

    public string? PlanStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<InstallmentPayment> InstallmentPayments { get; set; } = new List<InstallmentPayment>();

    public virtual AppOrder Order { get; set; } = null!;
}
