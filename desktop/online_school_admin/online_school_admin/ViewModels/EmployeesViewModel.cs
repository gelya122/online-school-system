using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class EmployeesViewModel : BaseViewModel
{
    private readonly AdminEmployeesService _employees;
    private readonly AuthService _auth;
    private readonly DispatcherTimer _debounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadVersion;

    public EmployeesViewModel(AdminEmployeesService employees, AuthService auth)
    {
        _employees = employees;
        _auth = auth;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await LoadAsync();
        };

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ResetCommand = new RelayCommand(async _ => await ResetAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.EmployeeId); }, _ => !IsBusy && Selected != null);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.EmployeeId); }, _ => !IsBusy && Selected != null);
        ToggleActiveCommand = new RelayCommand(async _ => await ToggleActiveAsync(), _ => !IsBusy && Selected != null);
        SoftDeleteCommand = new RelayCommand(async _ => await SoftDeleteAsync(), _ => !IsBusy && Selected != null);
    }

    public event Action? AddRequested;
    public event Action<int>? OpenRequested;
    public event Action<int>? EditRequested;

    public ObservableCollection<AdminEmployeeListRowDto> Items { get; } = new();
    public ObservableCollection<RoleOption> Roles { get; } = new();

    private AdminEmployeeListRowDto? _selected;
    public AdminEmployeeListRowDto? Selected
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

    private RoleOption? _selectedRoleFilter;
    public RoleOption? SelectedRoleFilter
    {
        get => _selectedRoleFilter;
        set
        {
            if (SetProperty(ref _selectedRoleFilter, value))
            {
                ScheduleAutoReload();
            }
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
                ResetCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged();
                OpenCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
                SoftDeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ToggleActiveCommand { get; }
    public RelayCommand SoftDeleteCommand { get; }

    public string ToggleActiveButtonCaption =>
        Selected == null ? "Включить / отключить" : Selected.IsActive ? "Отключить" : "Включить";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Roles.Clear();
        Roles.Add(new RoleOption(0, "Все"));
        try
        {
            var roles = await _auth.GetAllRolesAsync(cancellationToken);
            foreach (var r in roles.Where(x => x.RoleId != EmployeeDesktopAccess.StudentRoleId))
                Roles.Add(new RoleOption(r.RoleId, r.RoleName));
        }
        catch
        {
            // роли не критичны для отображения списка
        }

        SelectedRoleFilter ??= Roles.FirstOrDefault();
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
            var roleId = SelectedRoleFilter is { RoleId: > 0 } r ? r.RoleId : (int?)null;
            var list = await _employees.GetEmployeesAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                roleId,
                ct);

            if (version != _loadVersion) return;

            Items.Clear();
            foreach (var i in list)
                Items.Add(i);
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

    private async Task ToggleActiveAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;

        if (Selected.IsActive)
        {
            if (!UserDialogs.Confirm(
                    $"Отключить сотрудника «{Selected.FullName}»? Вход будет заблокирован, записи в базе сохраняются.",
                    "Сотрудники"))
                return;
        }
        else
        {
            if (!UserDialogs.Confirm(
                    $"Включить сотрудника «{Selected.FullName}»?",
                    "Сотрудники"))
                return;
        }

        IsBusy = true;
        try
        {
            if (Selected.IsActive)
                await _employees.DeactivateAsync(Selected.EmployeeId, cancellationToken);
            else
                await _employees.ActivateAsync(Selected.EmployeeId, cancellationToken);

            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
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
                $"Мягко удалить сотрудника «{Selected.FullName}»? Запись останется в базе с датой удаления, вход будет невозможен.",
                "Сотрудники"))
            return;

        IsBusy = true;
        try
        {
            await _employees.SoftDeleteAsync(Selected.EmployeeId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
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

    private async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        Search = "";
        SelectedRoleFilter = Roles.FirstOrDefault();
        await LoadAsync(cancellationToken);
    }

}

