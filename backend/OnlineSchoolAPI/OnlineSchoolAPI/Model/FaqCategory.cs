using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class FaqCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? CategoryOrder { get; set; }

    public virtual ICollection<FaqItem> FaqItems { get; set; } = new List<FaqItem>();
}
