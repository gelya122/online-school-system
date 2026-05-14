using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class PromoCode
{
    public int PromoCodeId { get; set; }

    public string Code { get; set; } = null!;

    public int? TypeId { get; set; }

    public decimal DiscountValue { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public int? MaxUses { get; set; }

    public int? CurrentUses { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? AppliesToCourseId { get; set; }

    public int? AppliesToInstanceId { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual DiscountType? DiscountType { get; set; }

    public virtual Course? AppliesToCourse { get; set; }

    public virtual CourseInstance? AppliesToInstance { get; set; }

    public virtual Employee? CreatedByEmployee { get; set; }
}
