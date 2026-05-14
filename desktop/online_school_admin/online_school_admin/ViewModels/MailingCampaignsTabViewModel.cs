using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class MailingCampaignsTabViewModel : BaseViewModel
{
    private readonly AdminNotificationsService _svc;

    public MailingCampaignsTabViewModel(AdminNotificationsService svc)
    {
        _svc = svc;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.CampaignId); }, _ => !IsBusy && Selected != null);
        SendCommand = new RelayCommand(async _ => await SendAsync(), _ => !IsBusy && Selected != null && Selected.Status != "sent" && Selected.Status != "canceled");
        CancelCommand = new RelayCommand(async _ => await CancelAsync(), _ => !IsBusy && Selected != null);
        RecipientsCommand = new RelayCommand(_ => { if (Selected != null) RecipientsRequested?.Invoke(Selected.CampaignId, Selected.Title); }, _ => !IsBusy && Selected != null);
    }

    public event Action? AddRequested;
    public event Action<int>? EditRequested;
    public event Action<int, string>? RecipientsRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand SendCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand RecipientsCommand { get; }

    public ObservableCollection<AdminMailingCampaignListRowDto> Rows { get; } = new();

    private AdminMailingCampaignListRowDto? _selected;
    public AdminMailingCampaignListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                SendCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RecipientsCommand.RaiseCanExecuteChanged();
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
                AddCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();
                SendCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RecipientsCommand.RaiseCanExecuteChanged();
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
            var list = await _svc.GetCampaignsAsync(cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task SendAsync()
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try { await _svc.SendCampaignAsync(Selected.CampaignId); await LoadAsync(); }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task CancelAsync()
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try { await _svc.CancelCampaignAsync(Selected.CampaignId); await LoadAsync(); }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

