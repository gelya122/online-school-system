using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class AppOrder
{
    public int OrderId { get; set; }

    public int StudentId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public int? OrderStatusId { get; set; }

    public int? MethodId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual ICollection<InstallmentPlan> InstallmentPlans { get; set; } = new List<InstallmentPlan>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus? OrderStatus { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Student Student { get; set; } = null!;

    public virtual PaymentMethod? Method { get; set; }
}
