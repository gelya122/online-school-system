using System.Collections.ObjectModel;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class StudentsViewModel : BaseViewModel
{
    private readonly AdminStudentsService _students;
    private readonly List<AdminStudentListRowDto> _all = new();
    private readonly DispatcherTimer _debounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadVersion;

    public StudentsViewModel(AdminStudentsService students)
    {
        _students = students;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await LoadAsync();
        };

        ClassOptions.Add(new StudentClassFilterOption(null, "Все классы"));
        for (var i = 0; i <= 11; i++)
            ClassOptions.Add(new StudentClassFilterOption(i, i == 0 ? "0 класс" : $"{i} класс"));
        SelectedClassFilter = ClassOptions[0];

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ResetCommand = new RelayCommand(async _ => await ResetAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.StudentId); }, _ => !IsBusy && Selected != null);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.StudentId); }, _ => !IsBusy && Selected != null);
        ToggleActiveCommand = new RelayCommand(async _ => await ToggleActiveAsync(), _ => !IsBusy && Selected != null);
        SoftDeleteCommand = new RelayCommand(async _ => await SoftDeleteAsync(), _ => !IsBusy && Selected != null);
        EnrollToInstanceCommand = new RelayCommand(_ => { if (Selected != null) EnrollRequested?.Invoke(Selected.StudentId); }, _ => !IsBusy && Selected != null);
        ExportCsvCommand = new RelayCommand(_ => ExportCsv(), _ => !IsBusy && HasAnyData);
        PrevPageCommand = new RelayCommand(_ => ShiftPage(-1), _ => !IsBusy && CurrentPage > 1);
        NextPageCommand = new RelayCommand(_ => ShiftPage(1), _ => !IsBusy && CurrentPage < TotalPages);
        FirstPageCommand = new RelayCommand(_ => GoFirstPage(), _ => !IsBusy && CurrentPage > 1);
        LastPageCommand = new RelayCommand(_ => GoLastPage(), _ => !IsBusy && CurrentPage < TotalPages);
    }

    public event Action? AddRequested;
    public event Action<int>? OpenRequested;
    public event Action<int>? EditRequested;
    public event Action<int>? EnrollRequested;

    public ObservableCollection<AdminStudentListRowDto> Items { get; } = new();
    public ObservableCollection<StudentClassFilterOption> ClassOptions { get; } = new();

    private StudentClassFilterOption? _selectedClassFilter;
    public StudentClassFilterOption? SelectedClassFilter
    {
        get => _selectedClassFilter;
        set
        {
            if (SetProperty(ref _selectedClassFilter, value))
            {
                ClassNumberFilter = value?.Value;
            }
        }
    }

    private AdminStudentListRowDto? _selected;
    public AdminStudentListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                OnPropertyChanged(nameof(ToggleActiveButtonCaption));
                OpenCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
                SoftDeleteCommand.RaiseCanExecuteChanged();
                EnrollToInstanceCommand.RaiseCanExecuteChanged();
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

    private bool? _isActiveFilter;
    public bool? IsActiveFilter
    {
        get => _isActiveFilter;
        set
        {
            if (SetProperty(ref _isActiveFilter, value))
                ScheduleAutoReload();
        }
    }

    private int? _classNumberFilter;
    /// <summary>null — все классы (синхронизируется с <see cref="SelectedClassFilter"/>).</summary>
    public int? ClassNumberFilter
    {
        get => _classNumberFilter;
        private set
        {
            if (SetProperty(ref _classNumberFilter, value))
                ScheduleAutoReload();
        }
    }

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
    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

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

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ToggleActiveCommand { get; }
    public RelayCommand SoftDeleteCommand { get; }
    public RelayCommand EnrollToInstanceCommand { get; }

    public string ToggleActiveButtonCaption =>
        Selected == null ? "Заблокировать / активировать" : Selected.IsActive ? "Отключить" : "Включить";
    public RelayCommand ExportCsvCommand { get; }
    public RelayCommand PrevPageCommand { get; }
    public RelayCommand NextPageCommand { get; }
    public RelayCommand FirstPageCommand { get; }
    public RelayCommand LastPageCommand { get; }

    private void RaiseAllCommands()
    {
        RefreshCommand.RaiseCanExecuteChanged();
        ResetCommand.RaiseCanExecuteChanged();
        AddCommand.RaiseCanExecuteChanged();
        OpenCommand.RaiseCanExecuteChanged();
        EditCommand.RaiseCanExecuteChanged();
        ToggleActiveCommand.RaiseCanExecuteChanged();
        SoftDeleteCommand.RaiseCanExecuteChanged();
        EnrollToInstanceCommand.RaiseCanExecuteChanged();
        ExportCsvCommand.RaiseCanExecuteChanged();
        PrevPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        FirstPageCommand.RaiseCanExecuteChanged();
        LastPageCommand.RaiseCanExecuteChanged();
    }

    private async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        Search = "";
        IsActiveFilter = null;
        SelectedClassFilter = ClassOptions.FirstOrDefault();
        await LoadAsync(cancellationToken);
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
            var list = await _students.GetStudentsAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                IsActiveFilter,
                ClassNumberFilter,
                ct);

            if (version != _loadVersion) return;

            _all.Clear();
            _all.AddRange(list.OrderByDescending(x => x.RegisteredAt));
            TotalCount = _all.Count;
            CurrentPage = 1;
            ApplyPage();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Students.Load");
            _all.Clear();
            TotalCount = 0;
            Items.Clear();
            Selected = null;
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Students.Load");
            _all.Clear();
            TotalCount = 0;
            Items.Clear();
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
        Items.Clear();
        Selected = null;
        foreach (var row in _all.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            Items.Add(row);
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
            if (!CsvExporter.PromptSaveAndExport(_all, "students_export.csv",
                    nameof(AdminStudentListRowDto.StudentId),
                    nameof(AdminStudentListRowDto.FullName),
                    nameof(AdminStudentListRowDto.Email),
                    nameof(AdminStudentListRowDto.Phone),
                    nameof(AdminStudentListRowDto.ClassNumber),
                    nameof(AdminStudentListRowDto.ParentPhone),
                    nameof(AdminStudentListRowDto.ParentEmail),
                    nameof(AdminStudentListRowDto.IsActive),
                    nameof(AdminStudentListRowDto.RegisteredAt)))
                return;
            UserDialogs.Info("Экспорт завершён.", "CSV");
        }
        catch (Exception ex)
        {
            Error = "Не удалось сохранить файл экспорта.";
            AppLogger.Log(ex, "Students.ExportCsv");
        }
    }

    private async Task ToggleActiveAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;

        var deactivate = Selected.IsActive;
        var msg = deactivate
            ? $"Заблокировать студента «{Selected.FullName}»?"
            : $"Активировать студента «{Selected.FullName}»?";
        if (!UserDialogs.Confirm(msg, "Студенты"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            if (deactivate)
                await _students.DeactivateAsync(Selected.StudentId, cancellationToken);
            else
                await _students.ActivateAsync(Selected.StudentId, cancellationToken);

            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Students.ToggleActive");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Students.ToggleActive");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SoftDeleteAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm(
                $"Мягко удалить студента «{Selected.FullName}»? Запись останется в базе с датой удаления, вход будет заблокирован.",
                "Студенты"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _students.SoftDeleteAsync(Selected.StudentId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Students.SoftDelete");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Students.SoftDelete");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
