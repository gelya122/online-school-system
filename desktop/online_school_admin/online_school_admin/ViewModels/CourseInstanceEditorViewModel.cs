using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using online_school_admin.Models.Admin;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public partial class CourseInstanceEditorViewModel : ObservableObject
{
    private readonly AuthApiService _api;
    public int InstanceId { get; }

    [ObservableProperty] private string _courseTitle = "";
    [ObservableProperty] private string _instanceName = "";
    [ObservableProperty] private DateTime? _startDate;
    [ObservableProperty] private DateTime? _endDate;
    [ObservableProperty] private int? _totalWeeks;
    [ObservableProperty] private int? _lessonsPerWeek;
    [ObservableProperty] private string? _scheduleDescription;
    [ObservableProperty] private int? _maxStudents;
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private bool _isBusy;

    public CourseInstanceEditorViewModel(AuthApiService api, int instanceId)
    {
        _api = api;
        InstanceId = instanceId;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            var list = await _api.GetCourseInstancesAsync(cancellationToken);
            var inst = list.FirstOrDefault(x => x.InstanceId == InstanceId)
                       ?? throw new InvalidOperationException("Поток не найден.");

            var courses = await _api.GetCoursesAsync(cancellationToken);
            var course = courses.FirstOrDefault(c => c.CourseId == inst.CourseId);
            CourseTitle = course?.Title ?? $"Курс #{inst.CourseId}";

            InstanceName = inst.InstanceName;
            StartDate = inst.StartDate.ToDateTime(TimeOnly.MinValue);
            EndDate = inst.EndDate?.ToDateTime(TimeOnly.MinValue);
            TotalWeeks = inst.TotalWeeks;
            LessonsPerWeek = inst.LessonsPerWeek;
            ScheduleDescription = inst.ScheduleDescription;
            MaxStudents = inst.MaxStudents;
            IsActive = inst.IsActive == true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (StartDate is not { } sd)
        {
            MessageBox.Show("Укажите дату начала потока.", "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            await _api.UpdateCourseInstanceAsync(InstanceId, new UpdateCourseInstanceRequest
            {
                InstanceName = InstanceName,
                StartDate = DateOnly.FromDateTime(sd),
                EndDate = EndDate is { } ed ? DateOnly.FromDateTime(ed) : null,
                TotalWeeks = TotalWeeks,
                LessonsPerWeek = LessonsPerWeek,
                ScheduleDescription = ScheduleDescription,
                MaxStudents = MaxStudents,
                IsActive = IsActive
            }, cancellationToken);

            MessageBox.Show("Поток сохранён.", "Поток", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Поток", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
