using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class DiscountType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();
}
