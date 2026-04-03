using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class OrderStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<AppOrder> AppOrders { get; set; } = new List<AppOrder>();
}
