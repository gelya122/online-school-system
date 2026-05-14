using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class LessonDetailsViewModel : BaseViewModel
{
    public LessonDetailsViewModel(AdminCoursesService courses, AdminLessonRowDto lesson, bool homeworkReadOnly = false)
    {
        LessonId = lesson.LessonId;
        LessonTitle = lesson.Title;
        Homework = new HomeworkEditorViewModel(courses, lesson.LessonId, homeworkReadOnly);
    }

    public int LessonId { get; }
    public string LessonTitle { get; }

    public HomeworkEditorViewModel Homework { get; }
}

