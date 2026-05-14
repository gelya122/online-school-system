using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;
using online_school_admin.Views;

namespace online_school_admin.ViewModels;

public sealed class ApplicationsViewModel : BaseViewModel
{
    private const int ManagerRoleId = 2;

    private readonly AdminApplicationsService _apps;
    private readonly AdminEmployeesService _employees;
    private readonly AdminCoursesService _courses;
    private readonly SessionService _session;
    private readonly PermissionService _permissions;
    private readonly bool _startWithMyApplicationsScope;
    private readonly DispatcherTimer _debounceTimer;

    public ApplicationsViewModel(
        AdminApplicationsService apps,
        AdminEmployeesService employees,
        AdminCoursesService courses,
        SessionService session,
        PermissionService permissions,
        bool startWithMyApplicationsScope = false)
    {
        _apps = apps;
        _employees = employees;
        _courses = courses;
        _session = session;
        _permissions = permissions;
        _startWithMyApplicationsScope = startWithMyApplicationsScope;
        ListTitle = startWithMyApplicationsScope ? "Мои заявки" : "Заявки";
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await LoadAsync();
        };

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.ApplicationId); }, _ => !IsBusy && Selected != null);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.ApplicationId); }, _ => !IsBusy && Selected != null);
        AssignManagerCommand = new RelayCommand(async _ => await AssignManagerForSelectedAsync(), _ => !IsBusy && Selected != null && _permissions.CanAdministrateApplications);
        TakeInWorkCommand = new RelayCommand(async _ => await TakeInWorkAsync(Selected), _ => !IsBusy && Selected != null && CanTakeInWork(Selected));
        ChangeStatusCommand = new RelayCommand(async _ => await ChangeStatusAsync(Selected), _ => !IsBusy && Selected != null && CanChangeStatus(Selected));
        DeleteCommand = new RelayCommand(async _ => await SoftDeleteAsync(Selected), _ => !IsBusy && Selected != null && _permissions.CanAdministrateApplications);

        OpenRowCommand = new RelayCommand(p => { if (p is AdminApplicationListRowDto r) OpenRequested?.Invoke(r.ApplicationId); }, _ => !IsBusy);
        EditRowCommand = new RelayCommand(p => { if (p is AdminApplicationListRowDto r) EditRequested?.Invoke(r.ApplicationId); }, _ => !IsBusy);
        TakeInWorkRowCommand = new RelayCommand(async p => { if (p is AdminApplicationListRowDto r) await TakeInWorkAsync(r); }, _ => !IsBusy);
        AssignManagerRowCommand = new RelayCommand(async p => { if (p is AdminApplicationListRowDto r) await AssignManagerForRowAsync(r); }, _ => !IsBusy);
        ChangeStatusRowCommand = new RelayCommand(async p => { if (p is AdminApplicationListRowDto r) await ChangeStatusAsync(r); }, _ => !IsBusy);
        DeleteRowCommand = new RelayCommand(async p => { if (p is AdminApplicationListRowDto r) await SoftDeleteAsync(r); }, _ => !IsBusy);
    }

    /// <summary>Заголовок списка: «Заявки» или «Мои заявки» (отдельный пункт меню).</summary>
    public string ListTitle { get; }

    /// <summary>Экран «Мои заявки»: без пресета, менеджера и периода дат; поиск только по фамилии (API).</summary>
    public bool IsMyApplicationsList => _startWithMyApplicationsScope;

    /// <summary>Пресет, подсказка про менеджера, фильтр по менеджеру и датам — только на экране «Заявки».</summary>
    public bool ShowExtendedApplicationFilters => !_startWithMyApplicationsScope;

    public string SearchFilterToolTip =>
        _startWithMyApplicationsScope ? "Поиск по фамилии" : "Поиск: ФИО, телефон, email";

    public string StatusFilterToolTip =>
        _startWithMyApplicationsScope ? "Статус" : "Статус (только «Все заявки»)";

    public event Action<int>? OpenRequested;
    public event Action? AddRequested;
    public event Action<int>? EditRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand AssignManagerCommand { get; }
    public RelayCommand TakeInWorkCommand { get; }
    public RelayCommand ChangeStatusCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public RelayCommand OpenRowCommand { get; }
    public RelayCommand EditRowCommand { get; }
    public RelayCommand TakeInWorkRowCommand { get; }
    public RelayCommand AssignManagerRowCommand { get; }
    public RelayCommand ChangeStatusRowCommand { get; }
    public RelayCommand DeleteRowCommand { get; }

    /// <summary>Пресет списка: all, new, mine, in_progress, completed (см. API scope).</summary>
    public ObservableCollection<IdTitleOption> ScopePresets { get; } = new();

    private IdTitleOption? _selectedScope;
    public IdTitleOption? SelectedScope
    {
        get => _selectedScope;
        set
        {
            if (SetProperty(ref _selectedScope, value))
                _ = LoadAsync();
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
                OpenCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                AssignManagerCommand.RaiseCanExecuteChanged();
                TakeInWorkCommand.RaiseCanExecuteChanged();
                ChangeStatusCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                OpenRowCommand.RaiseCanExecuteChanged();
                EditRowCommand.RaiseCanExecuteChanged();
                TakeInWorkRowCommand.RaiseCanExecuteChanged();
                AssignManagerRowCommand.RaiseCanExecuteChanged();
                ChangeStatusRowCommand.RaiseCanExecuteChanged();
                DeleteRowCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public ObservableCollection<AdminApplicationListRowDto> Rows { get; } = new();
    public ObservableCollection<AdminApplicationStatusDictDto> Statuses { get; } = new();
    public ObservableCollection<IdTitleOption> Managers { get; } = new();
    public ObservableCollection<IdTitleOption> Subjects { get; } = new();

    private AdminApplicationListRowDto? _selected;
    public AdminApplicationListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                OpenCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                AssignManagerCommand.RaiseCanExecuteChanged();
                TakeInWorkCommand.RaiseCanExecuteChanged();
                ChangeStatusCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
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

    private AdminApplicationStatusDictDto? _selectedStatus;
    public AdminApplicationStatusDictDto? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
                ScheduleAutoReload();
        }
    }

    private IdTitleOption? _selectedManager;
    public IdTitleOption? SelectedManager
    {
        get => _selectedManager;
        set
        {
            if (SetProperty(ref _selectedManager, value))
                ScheduleAutoReload();
        }
    }

    private IdTitleOption? _selectedSubject;
    public IdTitleOption? SelectedSubject
    {
        get => _selectedSubject;
        set
        {
            if (SetProperty(ref _selectedSubject, value))
                ScheduleAutoReload();
        }
    }

    private string? _createdFromText;
    public string? CreatedFromText
    {
        get => _createdFromText;
        set
        {
            if (SetProperty(ref _createdFromText, value))
                ScheduleAutoReload();
        }
    }

    private string? _createdToText;
    public string? CreatedToText
    {
        get => _createdToText;
        set
        {
            if (SetProperty(ref _createdToText, value))
                ScheduleAutoReload();
        }
    }

    private void ScheduleAutoReload()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private IdTitleOption? _managerToAssign;
    public IdTitleOption? ManagerToAssign { get => _managerToAssign; set => SetProperty(ref _managerToAssign, value); }

    public bool ShowAdminAssignBar => _permissions.CanAdministrateApplications;

    public static string FormatName(AdminApplicationListRowDto r) => $"{r.FirstName} {r.LastName ?? ""}".Trim();

    public bool CanTakeInWork(AdminApplicationListRowDto? row)
    {
        if (row == null) return false;
        if (row.ManagerId.HasValue) return false;
        if (!IsNewLikeStatus(row)) return false;
        return _session.CurrentUser?.EmployeeId is > 0;
    }

    public bool CanChangeStatus(AdminApplicationListRowDto? row)
    {
        if (row == null) return false;
        if (_permissions.CanAdministrateApplications) return true;
        var my = _session.CurrentUser?.EmployeeId;
        return my.HasValue && row.ManagerId == my.Value;
    }

    private static bool IsNewLikeStatus(AdminApplicationListRowDto row)
    {
        if (!string.IsNullOrWhiteSpace(row.StatusName) &&
            row.StatusName.Trim().Equals("Новая", StringComparison.OrdinalIgnoreCase))
            return true;
        return row.StatusId == 1;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ScopePresets.Clear();
        ScopePresets.Add(new IdTitleOption(0, "Все заявки"));
        ScopePresets.Add(new IdTitleOption(-1, "Новые"));
        ScopePresets.Add(new IdTitleOption(-2, "Мои заявки"));
        ScopePresets.Add(new IdTitleOption(-3, "В работе"));
        ScopePresets.Add(new IdTitleOption(-4, "Завершённые"));

        Statuses.Clear();
        Statuses.Add(new AdminApplicationStatusDictDto { StatusId = 0, StatusName = "Все статусы" });
        foreach (var s in await _apps.GetStatusesAsync(cancellationToken))
            Statuses.Add(s);

        var inProgressStatus = Statuses.FirstOrDefault(s => s.StatusId == 2)
            ?? Statuses.FirstOrDefault(s =>
                !string.IsNullOrWhiteSpace(s.StatusName) &&
                s.StatusName.Trim().Equals("В обработке", StringComparison.OrdinalIgnoreCase));
        SelectedStatus = inProgressStatus ?? Statuses.FirstOrDefault();

        Managers.Clear();
        Managers.Add(new IdTitleOption(0, "Все менеджеры"));
        var mgrs = await _employees.GetEmployeesAsync(null, ManagerRoleId, cancellationToken);
        foreach (var m in mgrs.OrderBy(x => x.FullName))
            Managers.Add(new IdTitleOption(m.EmployeeId, m.FullName));
        SelectedManager = Managers.FirstOrDefault();
        ManagerToAssign = Managers.Skip(1).FirstOrDefault();

        Subjects.Clear();
        Subjects.Add(new IdTitleOption(0, "Все предметы"));
        var subj = await _courses.GetSubjectsAsync(cancellationToken);
        foreach (var s in subj.OrderBy(x => x.SubjectName))
            Subjects.Add(new IdTitleOption(s.SubjectId, s.SubjectName));
        SelectedSubject = Subjects.FirstOrDefault();

        OnPropertyChanged(nameof(ShowAdminAssignBar));

        _selectedScope = _startWithMyApplicationsScope
            ? ScopePresets.FirstOrDefault(x => x.Id == -2) ?? ScopePresets.FirstOrDefault()
            : ScopePresets.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedScope));
        await LoadAsync(cancellationToken);
    }

    private string? ScopeKeyFromSelection()
    {
        var id = SelectedScope?.Id ?? 0;
        return id switch
        {
            -1 => "new",
            -2 => "mine",
            -3 => "in_progress",
            -4 => "completed",
            _ => null
        };
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var scope = ScopeKeyFromSelection();
            var allowStatusOutsideAllPreset = _startWithMyApplicationsScope;
            int? statusId = (string.IsNullOrEmpty(scope) || allowStatusOutsideAllPreset) && SelectedStatus is { StatusId: > 0 } st
                ? st.StatusId
                : null;
            int? managerId = string.IsNullOrEmpty(scope) && SelectedManager is { Id: > 0 } mg ? mg.Id : null;

            var list = await _apps.GetApplicationsAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                _startWithMyApplicationsScope,
                statusId,
                managerId,
                SelectedSubject is { Id: > 0 } sb ? sb.Id : null,
                _startWithMyApplicationsScope ? null : (string.IsNullOrWhiteSpace(CreatedFromText) ? null : CreatedFromText.Trim()),
                _startWithMyApplicationsScope ? null : (string.IsNullOrWhiteSpace(CreatedToText) ? null : CreatedToText.Trim()),
                scope,
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

    private async Task AssignManagerForSelectedAsync(CancellationToken cancellationToken = default)
    {
        if (Selected == null) return;
        await AssignManagerForRowAsync(Selected, cancellationToken);
    }

    private async Task AssignManagerForRowAsync(AdminApplicationListRowDto row, CancellationToken cancellationToken = default)
    {
        if (!_permissions.CanAdministrateApplications)
        {
            UserDialogs.Warning("Назначать менеджера может только администратор.", "Заявки");
            return;
        }

        var managers = Managers.Where(m => m.Id > 0).ToList();
        var dlg = new AssignApplicationManagerWindow(FormatName(row), row.ManagerName, managers)
        {
            Owner = UserDialogs.TryGetOwner()
        };
        if (dlg.ShowDialog() != true || !dlg.SelectedManagerId.HasValue)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.PatchManagerAsync(row.ApplicationId, dlg.SelectedManagerId, dlg.Note, cancellationToken);
            UserDialogs.Info("Менеджер назначен.", "Заявки");
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            UserDialogs.Warning(ex.Message, "Заявки");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TakeInWorkAsync(AdminApplicationListRowDto? row, CancellationToken cancellationToken = default)
    {
        if (row == null) return;
        if (!CanTakeInWork(row))
        {
            UserDialogs.Warning("Заявка уже назначена другому менеджеру или недоступна для взятия в работу.", "Заявки");
            return;
        }

        if (!UserDialogs.Confirm("Вы хотите взять эту заявку в работу?", "Заявки"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.ClaimAsync(row.ApplicationId, cancellationToken);
            UserDialogs.Info("Заявка взята в работу.", "Заявки");
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            UserDialogs.Warning(ex.Message, "Заявки");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ChangeStatusAsync(AdminApplicationListRowDto? row, CancellationToken cancellationToken = default)
    {
        if (row == null) return;
        if (!CanChangeStatus(row))
        {
            UserDialogs.Warning("Нет прав на смену статуса этой заявки.", "Заявки");
            return;
        }

        var statusList = Statuses.Where(s => s.StatusId > 0).ToList();
        var dlg = new ApplicationStatusChangeWindow(FormatName(row), row.Phone, row.StatusName, row.StatusId ?? 0, statusList)
        {
            Owner = UserDialogs.TryGetOwner()
        };
        if (dlg.ShowDialog() != true || !dlg.NewStatusId.HasValue)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.PatchStatusAsync(row.ApplicationId, dlg.NewStatusId.Value, dlg.ReasonComment, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            UserDialogs.Warning(ex.Message, "Заявки");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SoftDeleteAsync(AdminApplicationListRowDto? row, CancellationToken cancellationToken = default)
    {
        if (row == null) return;
        if (!_permissions.CanAdministrateApplications)
        {
            UserDialogs.Warning("Удаление доступно только администратору.", "Заявки");
            return;
        }

        if (!UserDialogs.Confirm($"Удалить заявку #{row.ApplicationId} ({FormatName(row)})? (будет скрыта из списка)", "Заявки"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.SoftDeleteAsync(row.ApplicationId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            UserDialogs.Warning(ex.Message, "Заявки");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
