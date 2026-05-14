using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class EnrollStudentToInstanceViewModel : BaseViewModel
{
    private readonly AdminInstancesService _instances;
    private readonly AdminStudentsService _students;
    private readonly int _studentId;
    private readonly DispatcherTimer _debounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadVersion;

    public EnrollStudentToInstanceViewModel(AdminInstancesService instances, AdminStudentsService students, int studentId)
    {
        _instances = instances;
        _students = students;
        _studentId = studentId;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await LoadAsync();
        };

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        EnrollCommand = new RelayCommand(async _ => await EnrollAsync(), _ => !IsBusy && Selected != null);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Enrolled;
    public event Action? CancelRequested;

    public ObservableCollection<AdminCourseInstanceListRowDto> Items { get; } = new();

    private AdminCourseInstanceListRowDto? _selected;
    public AdminCourseInstanceListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
                EnrollCommand.RaiseCanExecuteChanged();
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

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                EnrollCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand EnrollCommand { get; }
    public RelayCommand CancelCommand { get; }

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
                courseId: null,
                status: null,
                isActive: true,
                ct);

            if (version != _loadVersion) return;

            HashSet<int> already = new();
            try
            {
                var st = await _students.GetStudentAsync(_studentId, ct);
                foreach (var e in st.Enrollments)
                    already.Add(e.InstanceId);
            }
            catch
            {
                // если карточка недоступна — показываем все потоки; сервер всё равно проверит дубликат
            }

            Items.Clear();
            foreach (var i in list
                         .Where(x => !already.Contains(x.InstanceId))
                         .OrderByDescending(x => x.StartDate))
                Items.Add(i);
            Selected = Items.FirstOrDefault();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "EnrollStudentToInstance.Load");
        }
        catch (Exception ex)
        {
            Error = "Не удалось загрузить потоки.";
            AppLogger.Log(ex, "EnrollStudentToInstance.Load");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnrollAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;

        if (!UserDialogs.Confirm($"Записать студента в поток «{Selected.Title}»?", "Запись на поток"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.EnrollStudentAsync(Selected.InstanceId, _studentId, cancellationToken);
            Enrolled?.Invoke();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "EnrollStudentToInstance.Enroll");
        }
        catch (Exception ex)
        {
            Error = "Не удалось записать студента на поток.";
            AppLogger.Log(ex, "EnrollStudentToInstance.Enroll");
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
}

