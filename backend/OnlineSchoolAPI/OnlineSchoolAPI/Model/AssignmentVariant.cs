using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class AssignmentVariant
{
    public int VariantId { get; set; }

    public int AssignmentId { get; set; }

    public string VariantText { get; set; } = null!;

    public bool? IsCorrect { get; set; }

    public int? VariantOrder { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;
}
