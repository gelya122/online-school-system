namespace online_school_admin.Models;

public sealed class AdminDictionaryRegistryItemDto
{
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public bool SupportsDeactivate { get; set; }
}

public sealed class AdminDictActiveDto
{
    public bool IsActive { get; set; }
}

public sealed class AdminDictNameDescUpsertDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class AdminCourseCategoryDictDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
}

public sealed class AdminCourseCategoryUpsertDto
{
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? ExamId { get; set; }
}

public sealed class AdminSubjectDictDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminSubjectUpsertDto
{
    public string SubjectName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminExamDictDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminExamUpsertDto
{
    public string ExamName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminUserRoleDictDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class AdminSimpleStatusDictDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class AdminAssignmentTypeDictDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class AdminLessonTypeDictDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class AdminPaymentMethodDictDto
{
    public int MethodId { get; set; }
    public string MethodName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminPaymentMethodUpsertDto
{
    public string MethodName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminDiscountTypeUpsertDto
{
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
