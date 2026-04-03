using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class PaymentStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
