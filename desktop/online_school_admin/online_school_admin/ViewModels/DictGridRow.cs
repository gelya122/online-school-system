namespace online_school_admin.ViewModels;

/// <summary>Унифицированная строка таблицы справочника для UI.</summary>
public sealed class DictGridRow
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
    public int? SubjectId { get; init; }
    public int? ExamId { get; init; }
}
