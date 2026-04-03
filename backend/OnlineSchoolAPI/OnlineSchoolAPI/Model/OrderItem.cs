using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int CourseId { get; set; }

    public int? InstanceId { get; set; }

    public decimal Price { get; set; }

    public int? Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual CourseInstance? Instance { get; set; }

    public virtual AppOrder Order { get; set; } = null!;
}
