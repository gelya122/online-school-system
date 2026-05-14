using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;
using online_school_admin.Views;

namespace online_school_admin.ViewModels;

public sealed class ApplicationDetailsViewModel : BaseViewModel
{
    private const int ManagerRoleId = 2;

    private readonly AdminApplicationsService _apps;
    private readonly AdminEmployeesService _employees;
    private readonly SessionService _session;
    private readonly PermissionService _permissions;
    private readonly int _id;
    private readonly Action<int, int?>? _navigateOrderCreate;
    private readonly Action<int>? _navigateEnrollStudent;

    public ApplicationDetailsViewModel(
        AdminApplicationsService apps,
        AdminEmployeesService employees,
        SessionService session,
        PermissionService permissions,
        int id,
        Action<int, int?>? navigateOrderCreate = null,
        Action<int>? navigateEnrollStudent = null)
    {
        _apps = apps;
        _employees = employees;
        _session = session;
        _permissions = permissions;
        _id = id;
        _navigateOrderCreate = navigateOrderCreate;
        _navigateEnrollStudent = navigateEnrollStudent;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        AddCommentCommand = new RelayCommand(async _ => await AddCommentAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(NewCommentText));
        ConvertToStudentCommand = new RelayCommand(async _ => await ConvertToStudentAsync(),
            _ => !IsBusy && Details != null && !Details.StudentId.HasValue);
        CreateOrderCommand = new RelayCommand(_ => OpenOrderCreate(),
            _ => !IsBusy && Details != null && Details.StudentId.HasValue && Details.StudentId.Value > 0);
        EnrollToInstanceCommand = new RelayCommand(_ => OpenEnroll(),
            _ => !IsBusy && Details != null && Details.StudentId.HasValue && Details.StudentId.Value > 0);
        ApplyStatusCommand = new RelayCommand(async _ => await ApplyStatusAsync(),
            _ => !IsBusy && Details != null && QuickStatus != null && QuickStatus.StatusId != Details.StatusId);

        TakeInWorkCommand = new RelayCommand(async _ => await TakeInWorkAsync(), _ => !IsBusy && CanTakeInWork());
        AssignManagerCommand = new RelayCommand(async _ => await AssignManagerAsync(), _ => !IsBusy && _permissions.CanAdministrateApplications);
        ChangeStatusDialogCommand = new RelayCommand(async _ => await ChangeStatusDialogAsync(), _ => !IsBusy && CanChangeStatus());
        MarkContactCommand = new RelayCommand(async _ => await MarkContactAsync(), _ => !IsBusy && CanChangeStatus());
        DeleteCommand = new RelayCommand(async _ => await SoftDeleteAsync(), _ => !IsBusy && _permissions.CanAdministrateApplications);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand AddCommentCommand { get; }
    public RelayCommand ConvertToStudentCommand { get; }
    public RelayCommand CreateOrderCommand { get; }
    public RelayCommand EnrollToInstanceCommand { get; }
    public RelayCommand ApplyStatusCommand { get; }
    public RelayCommand TakeInWorkCommand { get; }
    public RelayCommand AssignManagerCommand { get; }
    public RelayCommand ChangeStatusDialogCommand { get; }
    public RelayCommand MarkContactCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public bool ShowAdminActions => _permissions.CanAdministrateApplications;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                AddCommentCommand.RaiseCanExecuteChanged();
                ConvertToStudentCommand.RaiseCanExecuteChanged();
                CreateOrderCommand.RaiseCanExecuteChanged();
                EnrollToInstanceCommand.RaiseCanExecuteChanged();
                ApplyStatusCommand.RaiseCanExecuteChanged();
                TakeInWorkCommand.RaiseCanExecuteChanged();
                AssignManagerCommand.RaiseCanExecuteChanged();
                ChangeStatusDialogCommand.RaiseCanExecuteChanged();
                MarkContactCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminApplicationDetailsDto? _details;
    public AdminApplicationDetailsDto? Details
    {
        get => _details;
        private set
        {
            if (SetProperty(ref _details, value))
            {
                ConvertToStudentCommand.RaiseCanExecuteChanged();
                CreateOrderCommand.RaiseCanExecuteChanged();
                EnrollToInstanceCommand.RaiseCanExecuteChanged();
                ApplyStatusCommand.RaiseCanExecuteChanged();
                TakeInWorkCommand.RaiseCanExecuteChanged();
                ChangeStatusDialogCommand.RaiseCanExecuteChanged();
                MarkContactCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<AdminApplicationSubjectDto> Subjects { get; } = new();
    public ObservableCollection<AdminApplicationCommentDto> Comments { get; } = new();
    public ObservableCollection<AdminApplicationStatusHistoryRowDto> StatusHistory { get; } = new();
    public ObservableCollection<AdminApplicationStatusDictDto> StatusOptions { get; } = new();

    private AdminApplicationStatusDictDto? _quickStatus;
    public AdminApplicationStatusDictDto? QuickStatus
    {
        get => _quickStatus;
        set
        {
            if (SetProperty(ref _quickStatus, value))
                ApplyStatusCommand.RaiseCanExecuteChanged();
        }
    }

    private string _newCommentText = "";
    public string NewCommentText
    {
        get => _newCommentText;
        set
        {
            if (SetProperty(ref _newCommentText, value))
                AddCommentCommand.RaiseCanExecuteChanged();
        }
    }

    public static string FormatName(AdminApplicationDetailsDto d) => $"{d.FirstName} {d.LastName ?? ""}".Trim();

    private bool CanTakeInWork()
    {
        if (Details == null) return false;
        if (Details.ManagerId.HasValue) return false;
        if (!IsNewLike(Details)) return false;
        return _session.CurrentUser?.EmployeeId is > 0;
    }

    private bool CanChangeStatus()
    {
        if (Details == null) return false;
        if (_permissions.CanAdministrateApplications) return true;
        var my = _session.CurrentUser?.EmployeeId;
        return my.HasValue && Details.ManagerId == my.Value;
    }

    private static bool IsNewLike(AdminApplicationDetailsDto d)
    {
        if (!string.IsNullOrWhiteSpace(d.StatusName) && d.StatusName.Trim().Equals("Новая", StringComparison.OrdinalIgnoreCase))
            return true;
        return d.StatusId == 1;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (StatusOptions.Count == 0)
            {
                foreach (var s in await _apps.GetStatusesAsync(cancellationToken))
                    StatusOptions.Add(s);
            }

            var dto = await _apps.GetApplicationAsync(_id, cancellationToken);
            Details = dto;
            QuickStatus = StatusOptions.FirstOrDefault(x => x.StatusId == dto.StatusId) ?? StatusOptions.FirstOrDefault();
            Replace(Subjects, dto.Subjects);
            Replace(Comments, dto.Comments);
            Replace(StatusHistory, dto.StatusHistory);
            ApplyStatusCommand.RaiseCanExecuteChanged();
            TakeInWorkCommand.RaiseCanExecuteChanged();
            ChangeStatusDialogCommand.RaiseCanExecuteChanged();
            MarkContactCommand.RaiseCanExecuteChanged();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.Details.Load");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.Details.Load");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenOrderCreate()
    {
        if (Details == null || !Details.StudentId.HasValue || Details.StudentId.Value <= 0)
        {
            UserDialogs.Info("Сначала создайте студента из заявки.", "Заказ");
            return;
        }

        var sid = Details.StudentId.Value;
        if (_navigateOrderCreate != null)
            _navigateOrderCreate(sid, null);
        else
            UserDialogs.Info("Создайте заказ в разделе «Платежи» → «Заказы».", "Заказ");
    }

    private void OpenEnroll()
    {
        if (Details == null || !Details.StudentId.HasValue || Details.StudentId.Value <= 0)
        {
            UserDialogs.Info("Сначала создайте студента из заявки.", "Поток");
            return;
        }

        var sid = Details.StudentId.Value;
        if (_navigateEnrollStudent != null)
            _navigateEnrollStudent(sid);
        else
            UserDialogs.Info("Запись на поток выполняется после оплаты: «Потоки» → вкладка «Студенты».", "Потоки");
    }

    private async Task AddCommentAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(NewCommentText) || Details == null)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            var c = await _apps.AddCommentAsync(Details.ApplicationId, NewCommentText.Trim(), cancellationToken);
            Comments.Add(c);
            NewCommentText = "";
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.Comment");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.Comment");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplyStatusAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null || QuickStatus == null || QuickStatus.StatusId == Details.StatusId)
            return;

        if (!UserDialogs.Confirm($"Сменить статус на «{QuickStatus.StatusName}»?", "Заявка"))
            return;

        var reason = UserDialogs.PromptMultiline("Комментарий к смене статуса (необязательно, попадёт в ленту заявки):", "Заявка");

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.PatchStatusAsync(Details.ApplicationId, QuickStatus.StatusId,
                string.IsNullOrWhiteSpace(reason) ? null : reason, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.Details.Status");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.Details.Status");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TakeInWorkAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null || !CanTakeInWork())
        {
            UserDialogs.Warning("Заявка уже назначена другому менеджеру или недоступна для взятия в работу.", "Заявка");
            return;
        }

        if (!UserDialogs.Confirm("Вы хотите взять эту заявку в работу?", "Заявка"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.ClaimAsync(Details.ApplicationId, cancellationToken);
            UserDialogs.Info("Заявка взята в работу.", "Заявка");
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            UserDialogs.Warning(ex.Message, "Заявка");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AssignManagerAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null || !_permissions.CanAdministrateApplications)
            return;

        var mgrs = await _employees.GetEmployeesAsync(null, ManagerRoleId, cancellationToken);
        var options = mgrs.Select(m => new IdTitleOption(m.EmployeeId, m.FullName)).ToList();
        var dlg = new AssignApplicationManagerWindow(FormatName(Details), Details.ManagerName, options)
        {
            Owner = UserDialogs.TryGetOwner()
        };
        if (dlg.ShowDialog() != true || !dlg.SelectedManagerId.HasValue)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.PatchManagerAsync(Details.ApplicationId, dlg.SelectedManagerId, dlg.Note, cancellationToken);
            UserDialogs.Info("Менеджер назначен.", "Заявка");
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            UserDialogs.Warning(ex.Message, "Заявка");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ChangeStatusDialogAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null || !CanChangeStatus())
        {
            UserDialogs.Warning("Нет прав на смену статуса.", "Заявка");
            return;
        }

        var dlg = new ApplicationStatusChangeWindow(FormatName(Details), Details.Phone, Details.StatusName, Details.StatusId ?? 0,
            StatusOptions.Where(s => s.StatusId > 0).ToList())
        {
            Owner = UserDialogs.TryGetOwner()
        };
        if (dlg.ShowDialog() != true || !dlg.NewStatusId.HasValue)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.PatchStatusAsync(Details.ApplicationId, dlg.NewStatusId.Value, dlg.ReasonComment, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            UserDialogs.Warning(ex.Message, "Заявка");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkContactAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null || !CanChangeStatus())
        {
            UserDialogs.Warning("Нет прав на отметку контакта.", "Заявка");
            return;
        }

        var dlg = new ApplicationContactMarkWindow { Owner = UserDialogs.TryGetOwner() };
        if (dlg.ShowDialog() != true || dlg.Result == null)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.PatchContactAsync(Details.ApplicationId, dlg.Result, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            UserDialogs.Warning(ex.Message, "Заявка");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SoftDeleteAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null || !_permissions.CanAdministrateApplications)
            return;

        if (!UserDialogs.Confirm($"Удалить заявку #{Details.ApplicationId} безвозвратно?", "Заявка"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _apps.SoftDeleteAsync(Details.ApplicationId, cancellationToken);
            UserDialogs.Info("Заявка удалена. Закройте карточку или вернитесь к списку.", "Заявка");
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            UserDialogs.Warning(ex.Message, "Заявка");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ConvertToStudentAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null) return;

        Error = null;
        IsBusy = true;
        try
        {
            var res = await _apps.ConvertToStudentAsync(Details.ApplicationId, cancellationToken);
            UserDialogs.Info(
                $"Заявка конвертирована в студента.\nID студента: {res.StudentId}\nВременный пароль: {res.TemporaryPassword}",
                "Студент");
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.ConvertToStudent");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Applications.ConvertToStudent");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> items)
    {
        target.Clear();
        foreach (var i in items)
            target.Add(i);
    }
}
