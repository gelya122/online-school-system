using System.Collections.ObjectModel;
using System.Collections.Specialized;
using online_school_admin.Models;

namespace online_school_admin.ViewModels;

/// <summary>Узел дерева: блок и дочерние уроки (для отображения).</summary>
public sealed class CourseModuleTreeGroup : BaseViewModel
{
    public AdminCourseModuleRowDto Module { get; }

    public ObservableCollection<LessonTreeNode> LessonItems { get; } = new();

    public string HeaderText => $"{Module.ModuleOrder}. {Module.Title}";

    public CourseModuleTreeGroup(AdminCourseModuleRowDto module)
    {
        Module = module;
        LessonItems.CollectionChanged += OnLessonItemsChanged;
    }

    private void OnLessonItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaiseHeaderChanged();
        OnPropertyChanged(nameof(LessonItems));
    }

    public void RaiseHeaderChanged() => OnPropertyChanged(nameof(HeaderText));
}
