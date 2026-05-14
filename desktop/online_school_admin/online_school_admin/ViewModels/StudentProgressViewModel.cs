using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class StudentProgressViewModel : BaseViewModel
{
    private readonly AdminProgressService _progress;
    private readonly AdminCoursesService _courses;
    private readonly AdminInstancesService _instances;

    public StudentProgressViewModel(AdminProgressService progress, AdminCoursesService courses, AdminInstancesService instances)
    {
        _progress = progress;
        _courses = courses;
        _instances = instances;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.EnrollmentId); }, _ => !IsBusy && Selected != null);
    }

    public event Action<int>? OpenRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand OpenCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                OpenCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public ObservableCollection<AdminStudentProgressListRowDto> Rows { get; } = new();
    public ObservableCollection<IdTitleOption> CourseOptions { get; } = new();
    public ObservableCollection<IdTitleOption> InstanceOptions { get; } = new();

    private string _search = "";
    public string Search { get => _search; set => SetProperty(ref _search, value); }

    private bool _suppressInstanceReload;
    private IdTitleOption? _selectedCourse;
    public IdTitleOption? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (SetProperty(ref _selectedCourse, value) && !_suppressInstanceReload)
                _ = ReloadInstanceOptionsAsync();
        }
    }

    private IdTitleOption? _selectedInstance;
    public IdTitleOption? SelectedInstance { get => _selectedInstance; set => SetProperty(ref _selectedInstance, value); }

    private AdminStudentProgressListRowDto? _selected;
    public AdminStudentProgressListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
                OpenCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        CourseOptions.Clear();
        CourseOptions.Add(new IdTitleOption(0, "Все курсы"));
        var courses = await _courses.GetCoursesAsync(null, null, null, null, null, cancellationToken);
        foreach (var c in courses.OrderBy(x => x.Title))
            CourseOptions.Add(new IdTitleOption(c.CourseId, c.Title));

        _suppressInstanceReload = true;
        SelectedCourse = CourseOptions.FirstOrDefault();
        _suppressInstanceReload = false;
        await ReloadInstanceOptionsAsync(cancellationToken);
    }

    private async Task ReloadInstanceOptionsAsync(CancellationToken cancellationToken = default)
    {
        InstanceOptions.Clear();
        InstanceOptions.Add(new IdTitleOption(0, "Все потоки"));
        if (SelectedCourse is { Id: > 0 } c)
        {
            var list = await _instances.GetInstancesAsync(null, c.Id, null, null, cancellationToken);
            foreach (var i in list.OrderByDescending(x => x.StartDate))
                InstanceOptions.Add(new IdTitleOption(i.InstanceId, i.Title));
        }

        SelectedInstance = InstanceOptions.FirstOrDefault();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _progress.GetProgressAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                SelectedCourse is { Id: > 0 } c ? c.Id : null,
                SelectedInstance is { Id: > 0 } i ? i.Id : null,
                cancellationToken);

            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
