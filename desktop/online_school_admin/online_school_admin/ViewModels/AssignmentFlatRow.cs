using CommunityToolkit.Mvvm.ComponentModel;
using online_school_admin.Models.Admin;

namespace online_school_admin.ViewModels;

public partial class AssignmentFlatRow : ObservableObject
{
    public AssignmentFlatRow(AssignmentDto dto, string lessonTitle)
    {
        Dto = dto;
        LessonTitle = lessonTitle;
    }

    public AssignmentDto Dto { get; }
    public string LessonTitle { get; }
}
