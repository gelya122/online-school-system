using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class OrdersTabViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;

    public OrdersTabViewModel(AdminPaymentsService pay)
    {
        _pay = pay;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        OpenCommand = new RelayCommand(_ => { if (Selected != null) OpenRequested?.Invoke(Selected.OrderId); }, _ => !IsBusy && Selected != null);
        CreateCommand = new RelayCommand(_ => CreateRequested?.Invoke(), _ => !IsBusy);
        MarkPaidCommand = new RelayCommand(async _ => await MarkPaidAsync(), _ => !IsBusy && Selected != null);
        CancelCommand = new RelayCommand(async _ => await CancelAsync(), _ => !IsBusy && Selected != null);
        ApplyPromoCommand = new RelayCommand(async _ => await ApplyPromoAsync(), _ => !IsBusy && Selected != null);
        PatchStatusCommand = new RelayCommand(async _ => await PatchStatusAsync(), _ => !IsBusy && Selected != null);
    }

    public event Action<int>? OpenRequested;
    public event Action? CreateRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand CreateCommand { get; }
    public RelayCommand MarkPaidCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand ApplyPromoCommand { get; }
    public RelayCommand PatchStatusCommand { get; }

    public ObservableCollection<AdminOrderListRowDto> Rows { get; } = new();

    private AdminOrderListRowDto? _selected;
    public AdminOrderListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                OpenCommand.RaiseCanExecuteChanged();
                MarkPaidCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                ApplyPromoCommand.RaiseCanExecuteChanged();
                PatchStatusCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _search = "";
    public string Search { get => _search; set => SetProperty(ref _search, value); }

    private string _statusText = "";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private string _promoCodeText = "";
    public string PromoCodeText { get => _promoCodeText; set => SetProperty(ref _promoCodeText, value); }

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
                CreateCommand.RaiseCanExecuteChanged();
                MarkPaidCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                ApplyPromoCommand.RaiseCanExecuteChanged();
                PatchStatusCommand.RaiseCanExecuteChanged();
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
            var list = await _pay.GetOrdersAsync(string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(), cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task MarkPaidAsync()
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try { await _pay.MarkPaidAsync(Selected.OrderId); await LoadAsync(); }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task CancelAsync()
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try { await _pay.CancelOrderAsync(Selected.OrderId); await LoadAsync(); }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task ApplyPromoAsync()
    {
        if (Selected == null) return;
        if (string.IsNullOrWhiteSpace(PromoCodeText)) { Error = "Введите промокод"; return; }
        Error = null;
        IsBusy = true;
        try
        {
            await _pay.ApplyPromoCodeAsync(Selected.OrderId, new AdminApplyPromoCodeDto { PromoCode = PromoCodeText.Trim() });
            PromoCodeText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task PatchStatusAsync()
    {
        if (Selected == null) return;
        if (string.IsNullOrWhiteSpace(StatusText)) { Error = "Введите статус"; return; }
        Error = null;
        IsBusy = true;
        try
        {
            await _pay.PatchOrderStatusAsync(Selected.OrderId, new AdminOrderStatusPatchDto { Status = StatusText.Trim() });
            StatusText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

