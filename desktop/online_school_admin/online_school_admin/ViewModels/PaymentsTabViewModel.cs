using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class PaymentsTabViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;

    public PaymentsTabViewModel(AdminPaymentsService pay)
    {
        _pay = pay;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        PatchStatusCommand = new RelayCommand(async _ => await PatchStatusAsync(), _ => !IsBusy && Selected != null);
        PatchOrderLinkCommand = new RelayCommand(async _ => await PatchOrderLinkAsync(), _ => !IsBusy && Selected != null);
        CreateCommand = new RelayCommand(_ => CreateRequested?.Invoke(Selected?.OrderId), _ => !IsBusy);
    }

    public event Action<int?>? CreateRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand PatchStatusCommand { get; }
    public RelayCommand PatchOrderLinkCommand { get; }
    public RelayCommand CreateCommand { get; }

    public ObservableCollection<AdminPaymentListRowDto> Rows { get; } = new();

    private AdminPaymentListRowDto? _selected;
    public AdminPaymentListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                PatchStatusCommand.RaiseCanExecuteChanged();
                PatchOrderLinkCommand.RaiseCanExecuteChanged();
                NewOrderIdText = value != null
                    ? value.OrderId.ToString(CultureInfo.InvariantCulture)
                    : "";
            }
        }
    }

    private string _statusText = "";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private string _newOrderIdText = "";
    public string NewOrderIdText { get => _newOrderIdText; set => SetProperty(ref _newOrderIdText, value); }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                PatchStatusCommand.RaiseCanExecuteChanged();
                PatchOrderLinkCommand.RaiseCanExecuteChanged();
                CreateCommand.RaiseCanExecuteChanged();
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
            var list = await _pay.GetPaymentsAsync(cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
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
            await _pay.PatchPaymentStatusAsync(Selected.PaymentId, new AdminPaymentStatusPatchDto { Status = StatusText.Trim() });
            StatusText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task PatchOrderLinkAsync()
    {
        if (Selected == null) return;
        if (!int.TryParse(NewOrderIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid))
        {
            Error = "Некорректный номер заказа";
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _pay.PatchPaymentOrderAsync(Selected.PaymentId, new AdminPaymentOrderPatchDto { OrderId = oid });
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
