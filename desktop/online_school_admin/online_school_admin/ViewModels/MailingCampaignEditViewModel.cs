using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class MailingCampaignEditViewModel : BaseViewModel
{
    private readonly AdminNotificationsService _svc;
    private readonly int? _id;

    public MailingCampaignEditViewModel(AdminNotificationsService svc, int? id)
    {
        _svc = svc;
        _id = id;
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

    public string Title => _id.HasValue ? "Редактирование рассылки" : "Создание рассылки";

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public string CampaignTitle { get => _campaignTitle; set => SetProperty(ref _campaignTitle, value); }
    private string _campaignTitle = "";

    public string Message { get => _message; set => SetProperty(ref _message, value); }
    private string _message = "";

    public string Channel { get => _channel; set => SetProperty(ref _channel, value); }
    private string _channel = "internal";

    public string TargetType { get => _targetType; set => SetProperty(ref _targetType, value); }
    private string _targetType = "all_students";

    public string TargetCourseIdText { get => _targetCourseIdText; set => SetProperty(ref _targetCourseIdText, value); }
    private string _targetCourseIdText = "";

    public string TargetInstanceIdText { get => _targetInstanceIdText; set => SetProperty(ref _targetInstanceIdText, value); }
    private string _targetInstanceIdText = "";

    public string ScheduledAtText { get => _scheduledAtText; set => SetProperty(ref _scheduledAtText, value); }
    private string _scheduledAtText = "";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_id.HasValue)
            await LoadAsync(cancellationToken);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var c = await _svc.GetCampaignAsync(_id!.Value, cancellationToken);
            CampaignTitle = c.Title;
            Message = c.Message;
            Channel = c.Channel;
            TargetType = c.TargetType;
            TargetCourseIdText = c.TargetCourseId?.ToString(CultureInfo.InvariantCulture) ?? "";
            TargetInstanceIdText = c.TargetInstanceId?.ToString(CultureInfo.InvariantCulture) ?? "";
            ScheduledAtText = c.ScheduledAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "";
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (string.IsNullOrWhiteSpace(CampaignTitle)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите title");
            if (string.IsNullOrWhiteSpace(Message)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите message");
            if (string.IsNullOrWhiteSpace(Channel)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите channel");
            if (string.IsNullOrWhiteSpace(TargetType)) throw new ApiException(HttpStatusCode.BadRequest, "Укажите target_type");

            int? courseId = null;
            if (!string.IsNullOrWhiteSpace(TargetCourseIdText))
            {
                if (!int.TryParse(TargetCourseIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный target_course_id");
                courseId = v;
            }

            int? instanceId = null;
            if (!string.IsNullOrWhiteSpace(TargetInstanceIdText))
            {
                if (!int.TryParse(TargetInstanceIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный target_instance_id");
                instanceId = v;
            }

            DateTime? scheduled = null;
            if (!string.IsNullOrWhiteSpace(ScheduledAtText))
            {
                if (!DateTime.TryParseExact(ScheduledAtText.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный scheduled_at (yyyy-MM-dd HH:mm)");
                scheduled = dt;
            }

            var dto = new AdminMailingCampaignUpsertDto
            {
                Title = CampaignTitle.Trim(),
                Message = Message.Trim(),
                Channel = Channel.Trim(),
                TargetType = TargetType.Trim(),
                TargetCourseId = courseId,
                TargetInstanceId = instanceId,
                ScheduledAt = scheduled
            };

            if (_id.HasValue)
                await _svc.UpdateCampaignAsync(_id.Value, dto, cancellationToken);
            else
                await _svc.CreateCampaignAsync(dto, cancellationToken);

            Saved?.Invoke();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

