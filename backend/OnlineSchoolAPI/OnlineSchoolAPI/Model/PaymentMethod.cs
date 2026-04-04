using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class PaymentMethod
{
    public int MethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<AppOrder> AppOrders { get; set; } = new List<AppOrder>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
