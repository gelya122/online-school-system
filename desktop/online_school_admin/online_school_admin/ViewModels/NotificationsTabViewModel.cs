using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class NotificationsTabViewModel : BaseViewModel
{
    private readonly AdminNotificationsService _svc;

    public NotificationsTabViewModel(AdminNotificationsService svc)
    {
        _svc = svc;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        CreateForUserCommand = new RelayCommand(async _ => await CreateForUserAsync(), _ => !IsBusy);
        CreateForInstanceCommand = new RelayCommand(async _ => await CreateForInstanceAsync(), _ => !IsBusy);
        CreateForAllStudentsCommand = new RelayCommand(async _ => await CreateForAllStudentsAsync(), _ => !IsBusy);
        MarkReadCommand = new RelayCommand(async _ => await MarkReadAsync(), _ => !IsBusy && Selected != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && Selected != null);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand CreateForUserCommand { get; }
    public RelayCommand CreateForInstanceCommand { get; }
    public RelayCommand CreateForAllStudentsCommand { get; }
    public RelayCommand MarkReadCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public ObservableCollection<AdminNotificationListRowDto> Rows { get; } = new();

    private AdminNotificationListRowDto? _selected;
    public AdminNotificationListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                MarkReadCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string UserIdText { get => _userIdText; set => SetProperty(ref _userIdText, value); }
    private string _userIdText = "";

    public string InstanceIdText { get => _instanceIdText; set => SetProperty(ref _instanceIdText, value); }
    private string _instanceIdText = "";

    public string TitleText { get => _titleText; set => SetProperty(ref _titleText, value); }
    private string _titleText = "";

    public string MessageText { get => _messageText; set => SetProperty(ref _messageText, value); }
    private string _messageText = "";

    public string TypeText { get => _typeText; set => SetProperty(ref _typeText, value); }
    private string _typeText = "internal";

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                CreateForUserCommand.RaiseCanExecuteChanged();
                CreateForInstanceCommand.RaiseCanExecuteChanged();
                CreateForAllStudentsCommand.RaiseCanExecuteChanged();
                MarkReadCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _svc.GetNotificationsAsync(cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.List"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.List"); }
        finally { IsBusy = false; }
    }

    private async Task CreateForUserAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (!int.TryParse(UserIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректный userId");
            if (string.IsNullOrWhiteSpace(TitleText)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите заголовок");
            if (string.IsNullOrWhiteSpace(MessageText)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите текст");

            await _svc.CreateNotificationAsync(new AdminCreateNotificationDto
            {
                UserId = uid,
                Title = TitleText.Trim(),
                Message = MessageText.Trim(),
                Type = string.IsNullOrWhiteSpace(TypeText) ? null : TypeText.Trim()
            });

            TitleText = "";
            MessageText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.CreateUser"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.CreateUser"); }
        finally { IsBusy = false; }
    }

    private async Task CreateForInstanceAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (!int.TryParse(InstanceIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var iid))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректный instanceId");
            if (string.IsNullOrWhiteSpace(TitleText)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите заголовок");
            if (string.IsNullOrWhiteSpace(MessageText)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите текст");

            await _svc.CreateNotificationAsync(new AdminCreateNotificationDto
            {
                InstanceId = iid,
                Title = TitleText.Trim(),
                Message = MessageText.Trim(),
                Type = string.IsNullOrWhiteSpace(TypeText) ? null : TypeText.Trim()
            });

            TitleText = "";
            MessageText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.CreateInstance"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.CreateInstance"); }
        finally { IsBusy = false; }
    }

    private async Task CreateForAllStudentsAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (string.IsNullOrWhiteSpace(TitleText)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите заголовок");
            if (string.IsNullOrWhiteSpace(MessageText)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите текст");
            if (!UserDialogs.Confirm("Отправить это уведомление всем студентам?", "Уведомления"))
                return;

            await _svc.CreateNotificationAsync(new AdminCreateNotificationDto
            {
                BroadcastToAllStudents = true,
                Title = TitleText.Trim(),
                Message = MessageText.Trim(),
                Type = string.IsNullOrWhiteSpace(TypeText) ? null : TypeText.Trim()
            });

            TitleText = "";
            MessageText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.CreateAllStudents"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.CreateAllStudents"); }
        finally { IsBusy = false; }
    }

    private async Task MarkReadAsync()
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try { await _svc.MarkReadAsync(Selected.NotificationId); await LoadAsync(); }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.MarkRead"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.MarkRead"); }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync()
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm("Удалить уведомление без возможности восстановления?", "Уведомления"))
            return;
        Error = null;
        IsBusy = true;
        try { await _svc.DeleteAsync(Selected.NotificationId); await LoadAsync(); }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.Delete"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Notifications.Delete"); }
        finally { IsBusy = false; }
    }
}

