using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace online_school_admin.ViewModels;

public partial class CourseStructureNode : ObservableObject
{
    public required string Kind { get; init; }
    public required int Id { get; init; }
    public int? ParentModuleId { get; init; }
    public int? ParentLessonId { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    private string _title = "";

    public ObservableCollection<CourseStructureNode> Children { get; } = new();

    public string Label => Kind switch
    {
        "Module" => $"[+] Блок: {Title}",
        "Lesson" => $"    [+] Урок: {Title}",
        _ => $"        [ДЗ] {Title}"
    };
}
