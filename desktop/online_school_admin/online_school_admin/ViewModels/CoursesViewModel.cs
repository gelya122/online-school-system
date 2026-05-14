using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class CoursesViewModel : BaseViewModel
{
    private readonly AdminCoursesService _courses;
    private readonly DispatcherTimer _debounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadVersion;

    public CoursesViewModel(AdminCoursesService courses)
    {
        _courses = courses;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await LoadAsync();
        };
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ResetCommand = new RelayCommand(async _ => await ResetAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.CourseId); }, _ => !IsBusy && Selected != null);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.CourseId); }, _ => !IsBusy && Selected != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && Selected != null);
        PublishSelectedCommand = new RelayCommand(async _ => await PublishSelectedAsync(), _ => !IsBusy && Selected != null && !Selected.IsActive);

        OpenRowCommand = new RelayCommand(p => { if (p is AdminCourseListRowDto r) OpenRequested?.Invoke(r.CourseId); }, _ => !IsBusy);
        EditRowCommand = new RelayCommand(p => { if (p is AdminCourseListRowDto r) EditRequested?.Invoke(r.CourseId); }, _ => !IsBusy);
        PublishToggleRowCommand = new RelayCommand(async p => await PublishToggleRowAsync(p), _ => !IsBusy);
    }

    public event Action? AddRequested;
    public event Action<int>? OpenRequested;
    public event Action<int>? EditRequested;

    public ObservableCollection<AdminCourseListRowDto> Items { get; } = new();
    public ObservableCollection<AdminCourseCategoryDictDto> Categories { get; } = new();
    public ObservableCollection<AdminSubjectDictDto> Subjects { get; } = new();
    public ObservableCollection<AdminExamDictDto> Exams { get; } = new();

    private AdminCourseListRowDto? _selected;
    public AdminCourseListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                OpenCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                PublishSelectedCommand.RaiseCanExecuteChanged();
                OpenRowCommand.RaiseCanExecuteChanged();
                EditRowCommand.RaiseCanExecuteChanged();
                PublishToggleRowCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value))
                ScheduleAutoReload();
        }
    }
    private string _search = "";

    public AdminCourseCategoryDictDto? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                ScheduleAutoReload();
        }
    }
    private AdminCourseCategoryDictDto? _selectedCategory;

    public AdminSubjectDictDto? SelectedSubject
    {
        get => _selectedSubject;
        set
        {
            if (SetProperty(ref _selectedSubject, value))
                ScheduleAutoReload();
        }
    }
    private AdminSubjectDictDto? _selectedSubject;

    public AdminExamDictDto? SelectedExam
    {
        get => _selectedExam;
        set
        {
            if (SetProperty(ref _selectedExam, value))
                ScheduleAutoReload();
        }
    }
    private AdminExamDictDto? _selectedExam;

    public bool? IsActiveFilter
    {
        get => _isActiveFilter;
        set
        {
            if (SetProperty(ref _isActiveFilter, value))
                ScheduleAutoReload();
        }
    }
    private bool? _isActiveFilter;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                ResetCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged();
                OpenCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                PublishSelectedCommand.RaiseCanExecuteChanged();
                OpenRowCommand.RaiseCanExecuteChanged();
                EditRowCommand.RaiseCanExecuteChanged();
                PublishToggleRowCommand.RaiseCanExecuteChanged();
            }
        }
    }
    private bool _isBusy;

    public string? Error { get => _error; set => SetProperty(ref _error, value); }
    private string? _error;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand PublishSelectedCommand { get; }
    public RelayCommand OpenRowCommand { get; }
    public RelayCommand EditRowCommand { get; }
    public RelayCommand PublishToggleRowCommand { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Categories.Clear();
            Categories.Add(new AdminCourseCategoryDictDto { CategoryId = 0, CategoryName = "Все категории" });
            foreach (var c in await _courses.GetCategoriesAsync(cancellationToken))
                Categories.Add(c);
            SelectedCategory ??= Categories.FirstOrDefault();

            Subjects.Clear();
            Subjects.Add(new AdminSubjectDictDto { SubjectId = 0, SubjectName = "Все предметы" });
            foreach (var s in await _courses.GetSubjectsAsync(cancellationToken))
                Subjects.Add(s);
            SelectedSubject ??= Subjects.FirstOrDefault();

            Exams.Clear();
            Exams.Add(new AdminExamDictDto { ExamId = 0, ExamName = "Все экзамены" });
            foreach (var e in await _courses.GetExamsAsync(cancellationToken))
                Exams.Add(e);
            SelectedExam ??= Exams.FirstOrDefault();
        }
        catch
        {
            // справочники не блокируют отображение
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var version = Interlocked.Increment(ref _loadVersion);
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var ct = _loadCts.Token;

        Error = null;
        IsBusy = true;
        try
        {
            var list = await _courses.GetCoursesAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                SelectedCategory is { CategoryId: > 0 } c ? c.CategoryId : null,
                SelectedSubject is { SubjectId: > 0 } s ? s.SubjectId : null,
                SelectedExam is { ExamId: > 0 } e ? e.ExamId : null,
                IsActiveFilter,
                ct);

            if (version != _loadVersion) return;

            Items.Clear();
            foreach (var i in list)
                Items.Add(i);

            PublishSelectedCommand.RaiseCanExecuteChanged();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
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

    private async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm(
                $"Скрыть курс «{Selected.Title}» из каталога и пометить как удалённый (мягкое удаление)?",
                "Курсы"))
            return;

        IsBusy = true;
        try
        {
            await _courses.DeleteAsync(Selected.CourseId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
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

    private async Task PublishSelectedAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null || Selected.IsActive)
            return;
        await PublishCourseRowAsync(Selected, cancellationToken);
    }

    private async Task PublishToggleRowAsync(object? param)
    {
        if (param is not AdminCourseListRowDto row)
            return;

        if (row.IsActive)
        {
            if (!UserDialogs.Confirm("Вы действительно хотите распубликовать курс?", "Курсы"))
                return;

            IsBusy = true;
            try
            {
                await _courses.HideAsync(row.CourseId);
                UserDialogs.Info("Курс снят с публикации.", "Курсы");
                await LoadAsync();
            }
            catch (ApiException ex)
            {
                Error = ex.Message;
            }
            catch (HttpRequestException)
            {
                Error = "Не удалось связаться с сервером.";
            }
            finally
            {
                IsBusy = false;
            }

            return;
        }

        await PublishCourseRowAsync(row, CancellationToken.None);
    }

    private async Task PublishCourseRowAsync(AdminCourseListRowDto row, CancellationToken cancellationToken)
    {
        if (!UserDialogs.Confirm("Вы действительно хотите опубликовать курс?", "Курсы"))
            return;

        if (row.ModulesCount == 0 || row.LessonsCount == 0)
        {
            if (!UserDialogs.Confirm("У курса нет блоков или уроков. Всё равно опубликовать?", "Курсы"))
                return;
        }

        IsBusy = true;
        try
        {
            await _courses.PublishAsync(row.CourseId, cancellationToken);
            UserDialogs.Info("Курс опубликован.", "Курсы");
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
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

    private async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        Search = "";
        SelectedCategory = Categories.FirstOrDefault();
        SelectedSubject = Subjects.FirstOrDefault();
        SelectedExam = Exams.FirstOrDefault();
        IsActiveFilter = null;
        await LoadAsync(cancellationToken);
    }

    private void ScheduleAutoReload()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }
}

