using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class MailingRecipientsViewModel : BaseViewModel
{
    private readonly AdminNotificationsService _svc;
    private readonly int _campaignId;

    public MailingRecipientsViewModel(AdminNotificationsService svc, int campaignId, string title)
    {
        _svc = svc;
        _campaignId = campaignId;
        CampaignTitle = title;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? BackRequested;

    public string CampaignTitle { get; }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand BackCommand { get; }

    public ObservableCollection<AdminMailingRecipientRowDto> Rows { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                BackCommand.RaiseCanExecuteChanged();
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
            var list = await _svc.GetRecipientsAsync(_campaignId, cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

