using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class HomeworkReviewViewModel : BaseViewModel
{
    private readonly AdminHomeworkReviewService _review;
    private readonly AdminCoursesService _courses;
    private readonly AdminEmployeesService _employees;
    private readonly AdminInstancesService _instances;

    public HomeworkReviewViewModel(
        AdminHomeworkReviewService review,
        AdminCoursesService courses,
        AdminEmployeesService employees,
        AdminInstancesService instances)
    {
        _review = review;
        _courses = courses;
        _employees = employees;
        _instances = instances;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ReviewSelectedCommand = new RelayCommand(
            _ =>
            {
                if (Selected != null)
                    ReviewRequested?.Invoke(Selected.SubmissionId, Selected.StudentAnswerId);
            },
            _ => !IsBusy && Selected != null);

        ReviewRowCommand = new RelayCommand(
            p =>
            {
                if (p is AdminHomeworkAnswerReviewQueueRowDto r)
                    ReviewRequested?.Invoke(r.SubmissionId, r.StudentAnswerId);
            },
            _ => !IsBusy);
    }

    public event Action<int, int>? ReviewRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ReviewSelectedCommand { get; }
    public RelayCommand ReviewRowCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                ReviewSelectedCommand.RaiseCanExecuteChanged();
                ReviewRowCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public ObservableCollection<AdminHomeworkAnswerReviewQueueRowDto> Rows { get; } = new();
    public ObservableCollection<IdTitleOption> CourseOptions { get; } = new();
    public ObservableCollection<IdTitleOption> InstanceOptions { get; } = new();
    public ObservableCollection<IdTitleOption> MentorOptions { get; } = new();

    public IReadOnlyList<IdTitleOption> ReviewStateOptions { get; } =
    [
        new IdTitleOption(1, "Только непроверенные"),
        new IdTitleOption(2, "Проверенные"),
        new IdTitleOption(3, "Все")
    ];

    private string _search = "";
    public string Search { get => _search; set => SetProperty(ref _search, value); }

    private IdTitleOption? _selectedReviewState;
    public IdTitleOption? SelectedReviewState { get => _selectedReviewState; set => SetProperty(ref _selectedReviewState, value); }

    private IdTitleOption? _selectedCourse;
    public IdTitleOption? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (SetProperty(ref _selectedCourse, value))
                _ = ReloadInstanceOptionsAsync();
        }
    }

    private IdTitleOption? _selectedInstance;
    public IdTitleOption? SelectedInstance { get => _selectedInstance; set => SetProperty(ref _selectedInstance, value); }

    private IdTitleOption? _selectedMentor;
    public IdTitleOption? SelectedMentor { get => _selectedMentor; set => SetProperty(ref _selectedMentor, value); }

    private string _studentIdFilter = "";
    public string StudentIdFilter { get => _studentIdFilter; set => SetProperty(ref _studentIdFilter, value); }

    private string? _fromDateText;
    public string? FromDateText { get => _fromDateText; set => SetProperty(ref _fromDateText, value); }

    private string? _toDateText;
    public string? ToDateText { get => _toDateText; set => SetProperty(ref _toDateText, value); }

    private AdminHomeworkAnswerReviewQueueRowDto? _selected;
    public AdminHomeworkAnswerReviewQueueRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
                ReviewSelectedCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        try
        {
            CourseOptions.Clear();
            CourseOptions.Add(new IdTitleOption(0, "Все курсы"));
            var courses = await _courses.GetCoursesAsync(null, null, null, null, null, cancellationToken);
            foreach (var c in (courses ?? []).OrderBy(x => x.Title))
                CourseOptions.Add(new IdTitleOption(c.CourseId, c.Title));
            SelectedCourse = CourseOptions.FirstOrDefault();

            MentorOptions.Clear();
            MentorOptions.Add(new IdTitleOption(0, "Все наставники"));
            var emps = await _employees.GetEmployeesAsync(null, null, cancellationToken);
            foreach (var e in (emps ?? []).OrderBy(x => x.FullName))
                MentorOptions.Add(new IdTitleOption(e.EmployeeId, e.FullName));
            SelectedMentor = MentorOptions.FirstOrDefault();

            SelectedReviewState = ReviewStateOptions.FirstOrDefault();

            await ReloadInstanceOptionsAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
    }

    private async Task ReloadInstanceOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            InstanceOptions.Clear();
            InstanceOptions.Add(new IdTitleOption(0, "Все потоки"));
            if (SelectedCourse is { Id: > 0 } c)
            {
                var list = await _instances.GetInstancesAsync(null, c.Id, null, null, cancellationToken);
                foreach (var i in (list ?? []).OrderBy(x => x.Title))
                    InstanceOptions.Add(new IdTitleOption(i.InstanceId, i.Title));
            }

            SelectedInstance = InstanceOptions.FirstOrDefault();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            int? studentId = null;
            if (!string.IsNullOrWhiteSpace(StudentIdFilter) &&
                int.TryParse(StudentIdFilter.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sid))
                studentId = sid;

            DateTime? from = null;
            if (!string.IsNullOrWhiteSpace(FromDateText) &&
                DateTime.TryParse(FromDateText.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var f))
                from = f.ToUniversalTime();

            DateTime? to = null;
            if (!string.IsNullOrWhiteSpace(ToDateText) &&
                DateTime.TryParse(ToDateText.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var t))
                to = t.ToUniversalTime();

            var list = await _review.GetQueueAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                status: null,
                reviewState: SelectedReviewState?.Id switch
                {
                    2 => "reviewed",
                    3 => "all",
                    _ => "pending"
                },
                SelectedCourse is { Id: > 0 } c ? c.Id : null,
                SelectedInstance is { Id: > 0 } ins ? ins.Id : null,
                reviewerId: null,
                mentorId: SelectedMentor is { Id: > 0 } m ? m.Id : null,
                studentId,
                from,
                to,
                cancellationToken);

            Rows.Clear();
            if (list != null)
            {
                foreach (var r in list)
                    Rows.Add(r);
            }
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
