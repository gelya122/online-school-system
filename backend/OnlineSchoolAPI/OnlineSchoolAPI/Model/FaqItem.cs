using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class FaqItem
{
    public int FaqId { get; set; }

    public int? CategoryId { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? ItemOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FaqCategory? Category { get; set; }
}
