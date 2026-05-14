using online_school_admin.Models;

namespace online_school_admin.ViewModels;

/// <summary>Урок в дереве структуры курса вместе с номером блока для подписи вида «1.2. Название».</summary>
public sealed class LessonTreeNode
{
    public int ModuleOrder { get; init; }

    public AdminLessonRowDto Lesson { get; init; } = null!;

    public string DisplayText => $"{ModuleOrder}.{Lesson.LessonOrder}. {Lesson.Title}";
}
