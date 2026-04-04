using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string? ExternalPaymentId { get; set; }

    public decimal Amount { get; set; }

    public int? PaymentStatusId { get; set; }

    public int? MethodId { get; set; }

    public string? CardLastFour { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AppOrder Order { get; set; } = null!;

    public virtual PaymentStatus? PaymentStatus { get; set; }

    public virtual PaymentMethod? Method { get; set; }
}
