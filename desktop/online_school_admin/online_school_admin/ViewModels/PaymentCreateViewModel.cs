using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class PaymentCreateViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;

    public PaymentCreateViewModel(AdminPaymentsService pay, int? prefillOrderId = null)
    {
        _pay = pay;
        if (prefillOrderId.HasValue)
            _orderIdText = prefillOrderId.Value.ToString(CultureInfo.InvariantCulture);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

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

    public string OrderIdText { get => _orderIdText; set => SetProperty(ref _orderIdText, value); }
    private string _orderIdText = "";

    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value); }
    private string _amountText = "";

    public string MethodIdText { get => _methodIdText; set => SetProperty(ref _methodIdText, value); }
    private string _methodIdText = "";

    public string ExternalPaymentId { get => _externalPaymentId; set => SetProperty(ref _externalPaymentId, value); }
    private string _externalPaymentId = "";

    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    private string _statusText = "";

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (!int.TryParse(OrderIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var orderId))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректный orderId");

            if (!decimal.TryParse(AmountText.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректная сумма");

            int? methodId = null;
            if (!string.IsNullOrWhiteSpace(MethodIdText))
            {
                if (!int.TryParse(MethodIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var mi))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный methodId");
                methodId = mi;
            }

            await _pay.CreatePaymentAsync(new AdminPaymentCreateDto
            {
                OrderId = orderId,
                Amount = amount,
                MethodId = methodId,
                ExternalPaymentId = string.IsNullOrWhiteSpace(ExternalPaymentId) ? null : ExternalPaymentId.Trim(),
                Status = string.IsNullOrWhiteSpace(StatusText) ? null : StatusText.Trim(),
                PaidAt = null
            }, cancellationToken);

            Saved?.Invoke();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

