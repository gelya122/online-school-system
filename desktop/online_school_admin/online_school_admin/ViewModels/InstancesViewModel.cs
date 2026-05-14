using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class InstancesViewModel : BaseViewModel
{
    private readonly AdminInstancesService _instances;
    private readonly AdminCoursesService _courses;
    private readonly PermissionService _permissions;
    private readonly List<AdminCourseInstanceListRowDto> _all = new();
    private readonly DispatcherTimer _debounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadVersion;

    public InstancesViewModel(AdminInstancesService instances, AdminCoursesService courses, PermissionService permissions)
    {
        _instances = instances;
        _courses = courses;
        _permissions = permissions;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await LoadAsync();
        };

        StatusFilters.Add(new StatusFilterOption(null, "Все статусы"));
        foreach (var (code, label) in new (string Code, string Label)[]
                 {
                     ("planned", "Запланирован"),
                     ("enrollment_open", "Открыт набор"),
                     ("enrollment_closed", "Набор закрыт"),
                     ("active", "Активный"),
                     ("completed", "Завершён"),
                     ("cancelled", "Отменён"),
                     ("paused", "Приостановлен")
                 })
            StatusFilters.Add(new StatusFilterOption(code, $"{label} ({code})"));
        _selectedStatusFilter = StatusFilters[0];

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ResetCommand = new RelayCommand(async _ => await ResetAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy && _permissions.CanEditStreams);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.InstanceId); }, _ => !IsBusy && Selected != null);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.InstanceId); }, _ => !IsBusy && Selected != null && _permissions.CanEditStreams);
        ArchiveCommand = new RelayCommand(async _ => await ArchiveAsync(), _ => !IsBusy && Selected != null && _permissions.CanEditStreams);
        PublishCommand = new RelayCommand(async _ => await PublishAsync(), _ => !IsBusy && Selected != null && _permissions.CanEditStreams && string.Equals(Selected.Status, "planned", StringComparison.OrdinalIgnoreCase));
        UnpublishCommand = new RelayCommand(async _ => await UnpublishAsync(), _ => !IsBusy && Selected != null && _permissions.CanEditStreams && string.Equals(Selected.Status, "enrollment_open", StringComparison.OrdinalIgnoreCase));
        DeactivateCommand = new RelayCommand(async _ => await PatchInstanceActiveAsync(false), _ => !IsBusy && Selected != null && Selected.IsActive && _permissions.CanEditStreams);
        ActivateCommand = new RelayCommand(async _ => await PatchInstanceActiveAsync(true), _ => !IsBusy && Selected != null && !Selected.IsActive && _permissions.CanEditStreams);
        ExportCsvCommand = new RelayCommand(_ => ExportCsv(), _ => !IsBusy && HasAnyData);
        PrevPageCommand = new RelayCommand(_ => ShiftPage(-1), _ => !IsBusy && CurrentPage > 1);
        NextPageCommand = new RelayCommand(_ => ShiftPage(1), _ => !IsBusy && CurrentPage < TotalPages);
        FirstPageCommand = new RelayCommand(_ => GoFirstPage(), _ => !IsBusy && CurrentPage > 1);
        LastPageCommand = new RelayCommand(_ => GoLastPage(), _ => !IsBusy && CurrentPage < TotalPages);
    }

    public event Action? AddRequested;
    public event Action<int>? OpenRequested;
    public event Action<int>? EditRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ArchiveCommand { get; }
    public RelayCommand PublishCommand { get; }
    public RelayCommand UnpublishCommand { get; }
    public RelayCommand DeactivateCommand { get; }
    public RelayCommand ActivateCommand { get; }
    public RelayCommand ExportCsvCommand { get; }
    public RelayCommand PrevPageCommand { get; }
    public RelayCommand NextPageCommand { get; }
    public RelayCommand FirstPageCommand { get; }
    public RelayCommand LastPageCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                RaiseAllCommands();
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public ObservableCollection<AdminCourseInstanceListRowDto> Rows { get; } = new();
    public ObservableCollection<IdTitleOption> CourseOptions { get; } = new();
    public ObservableCollection<StatusFilterOption> StatusFilters { get; } = new();

    private AdminCourseInstanceListRowDto? _selected;
    public AdminCourseInstanceListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                OpenCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                ArchiveCommand.RaiseCanExecuteChanged();
                PublishCommand.RaiseCanExecuteChanged();
                UnpublishCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
                ActivateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _search = "";
    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value))
                ScheduleAutoReload();
        }
    }

    private IdTitleOption? _selectedCourse;
    public IdTitleOption? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (SetProperty(ref _selectedCourse, value))
                ScheduleAutoReload();
        }
    }

    private StatusFilterOption? _selectedStatusFilter;
    public StatusFilterOption? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
                ScheduleAutoReload();
        }
    }

    private bool? _isActiveFilter;
    public bool? IsActiveFilter { get => _isActiveFilter; set => SetProperty(ref _isActiveFilter, value); }

    public IReadOnlyList<BoolOption> ActiveOptions { get; } =
    [
        new BoolOption(null, "Все"),
        new BoolOption(true, "Активные"),
        new BoolOption(false, "Неактивные")
    ];

    private BoolOption? _selectedActive;
    public BoolOption? SelectedActive
    {
        get => _selectedActive;
        set
        {
            if (SetProperty(ref _selectedActive, value))
            {
                IsActiveFilter = value?.Value;
                ScheduleAutoReload();
            }
        }
    }

    // ScheduleAutoReload определён ниже (через DispatcherTimer)

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(HasAnyData));
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageSummary));
                if (_currentPage > TotalPages)
                {
                    _currentPage = TotalPages;
                    OnPropertyChanged(nameof(CurrentPage));
                }
                PrevPageCommand.RaiseCanExecuteChanged();
                NextPageCommand.RaiseCanExecuteChanged();
                FirstPageCommand.RaiseCanExecuteChanged();
                LastPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasAnyData => TotalCount > 0;
    public int PageSize { get; } = 25;

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            var v = Math.Clamp(value, 1, Math.Max(1, TotalPages));
            if (SetProperty(ref _currentPage, v))
            {
                OnPropertyChanged(nameof(PageSummary));
                PrevPageCommand.RaiseCanExecuteChanged();
                NextPageCommand.RaiseCanExecuteChanged();
                FirstPageCommand.RaiseCanExecuteChanged();
                LastPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int TotalPages => TotalCount <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public string PageSummary => $"{CurrentPage} / {TotalPages} · записей: {TotalCount}";

    private void RaiseAllCommands()
    {
        RefreshCommand.RaiseCanExecuteChanged();
        ResetCommand.RaiseCanExecuteChanged();
        AddCommand.RaiseCanExecuteChanged();
        OpenCommand.RaiseCanExecuteChanged();
        EditCommand.RaiseCanExecuteChanged();
        ArchiveCommand.RaiseCanExecuteChanged();
        PublishCommand.RaiseCanExecuteChanged();
        UnpublishCommand.RaiseCanExecuteChanged();
        DeactivateCommand.RaiseCanExecuteChanged();
        ActivateCommand.RaiseCanExecuteChanged();
        ExportCsvCommand.RaiseCanExecuteChanged();
        PrevPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        FirstPageCommand.RaiseCanExecuteChanged();
        LastPageCommand.RaiseCanExecuteChanged();
    }

    private async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        Search = "";
        SelectedCourse = CourseOptions.FirstOrDefault();
        SelectedStatusFilter = StatusFilters.FirstOrDefault();
        SelectedActive = ActiveOptions.FirstOrDefault();
        await LoadAsync(cancellationToken);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        CourseOptions.Clear();
        CourseOptions.Add(new IdTitleOption(0, "Все курсы"));

        try
        {
            var courses = await _courses.GetCoursesAsync(null, null, null, null, null, cancellationToken);
            foreach (var c in courses.OrderBy(x => x.Title))
                CourseOptions.Add(new IdTitleOption(c.CourseId, c.Title));
        }
        catch (ApiException)
        {
            // нет прав / API недоступен — остаётся только «Все курсы»
        }

        SelectedCourse = CourseOptions.FirstOrDefault();
        SelectedActive = ActiveOptions.FirstOrDefault();
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
            var list = await _instances.GetInstancesAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                SelectedCourse is { Id: > 0 } sc ? sc.Id : null,
                string.IsNullOrWhiteSpace(SelectedStatusFilter?.Code) ? null : SelectedStatusFilter.Code,
                IsActiveFilter,
                ct);

            if (version != _loadVersion) return;

            _all.Clear();
            _all.AddRange(list.OrderByDescending(x => x.StartDate));
            TotalCount = _all.Count;
            CurrentPage = 1;
            ApplyPage();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Load");
            _all.Clear();
            TotalCount = 0;
            Rows.Clear();
            Selected = null;
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Load");
            _all.Clear();
            TotalCount = 0;
            Rows.Clear();
            Selected = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ScheduleAutoReload()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void ApplyPage()
    {
        Rows.Clear();
        Selected = null;
        foreach (var row in _all.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            Rows.Add(row);
        OnPropertyChanged(nameof(HasAnyData));
        ExportCsvCommand.RaiseCanExecuteChanged();
    }

    private void ShiftPage(int delta)
    {
        CurrentPage += delta;
        ApplyPage();
    }

    private void GoFirstPage()
    {
        CurrentPage = 1;
        ApplyPage();
    }

    private void GoLastPage()
    {
        CurrentPage = TotalPages;
        ApplyPage();
    }

    private void ExportCsv()
    {
        try
        {
            if (!CsvExporter.PromptSaveAndExport(_all, "instances_export.csv",
                    nameof(AdminCourseInstanceListRowDto.InstanceId),
                    nameof(AdminCourseInstanceListRowDto.Title),
                    nameof(AdminCourseInstanceListRowDto.CourseTitle),
                    nameof(AdminCourseInstanceListRowDto.StartDate),
                    nameof(AdminCourseInstanceListRowDto.EndDate),
                    nameof(AdminCourseInstanceListRowDto.Status),
                    nameof(AdminCourseInstanceListRowDto.MaxStudents),
                    nameof(AdminCourseInstanceListRowDto.StudentsCount),
                    nameof(AdminCourseInstanceListRowDto.TeacherFullName),
                    nameof(AdminCourseInstanceListRowDto.MentorsCount),
                    nameof(AdminCourseInstanceListRowDto.IsActive)))
                return;
            UserDialogs.Info("Экспорт завершён.", "CSV");
        }
        catch (Exception ex)
        {
            Error = "Не удалось сохранить файл экспорта.";
            AppLogger.Log(ex, "Instances.ExportCsv");
        }
    }

    private async Task PublishAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm($"Открыть набор для потока «{Selected.Title}» (статус enrollment_open)?", "Потоки"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchStatusAsync(Selected.InstanceId, new AdminInstanceStatusPatchDto { Status = "enrollment_open" },
                cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Publish");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Publish");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UnpublishAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm($"Вернуть поток «{Selected.Title}» в статус planned?", "Потоки"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchStatusAsync(Selected.InstanceId, new AdminInstanceStatusPatchDto { Status = "planned" },
                cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Unpublish");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Unpublish");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ArchiveAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;

        if (!UserDialogs.Confirm(
                $"Архивировать поток «{Selected.Title}»? Студенты останутся в истории, управление потоком будет ограничено.",
                "Потоки"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.ArchiveAsync(Selected.InstanceId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Archive");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.Archive");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PatchInstanceActiveAsync(bool isActive, CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;

        if (!UserDialogs.Confirm(
                isActive
                    ? $"Включить активность потока «{Selected.Title}» (is_active = true)? Статус потока не меняется."
                    : $"Деактивировать поток «{Selected.Title}» (is_active = false)? Архивация и статус не меняются.",
                "Потоки"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchInstanceActiveAsync(Selected.InstanceId, new AdminInstanceIsActivePatchDto { IsActive = isActive },
                cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.PatchActive");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Instances.PatchActive");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed record IdTitleOption(int Id, string Title)
{
    public override string ToString() => Title;
}

public sealed record BoolOption(bool? Value, string Title)
{
    public override string ToString() => Title;
}
