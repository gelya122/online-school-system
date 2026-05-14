using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class InstallmentPlanCreateViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;

    public InstallmentPlanCreateViewModel(AdminPaymentsService pay)
    {
        _pay = pay;
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

    public string InstallmentCountText { get => _installmentCountText; set => SetProperty(ref _installmentCountText, value); }
    private string _installmentCountText = "3";

    public string MonthlyPaymentText { get => _monthlyPaymentText; set => SetProperty(ref _monthlyPaymentText, value); }
    private string _monthlyPaymentText = "";

    public string NextPaymentDateText { get => _nextPaymentDateText; set => SetProperty(ref _nextPaymentDateText, value); }
    private string _nextPaymentDateText = "";

    public string PlanStatusText { get => _planStatusText; set => SetProperty(ref _planStatusText, value); }
    private string _planStatusText = "";

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (!int.TryParse(OrderIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var orderId))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректный ID заказа");

            if (!int.TryParse(InstallmentCountText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var cnt) || cnt < 2)
                throw new ApiException(HttpStatusCode.BadRequest, "Число платежей должно быть не меньше 2");

            decimal? monthly = null;
            if (!string.IsNullOrWhiteSpace(MonthlyPaymentText))
            {
                if (!decimal.TryParse(MonthlyPaymentText.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var m))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректная сумма платежа");
                monthly = m;
            }

            DateOnly? next = null;
            if (!string.IsNullOrWhiteSpace(NextPaymentDateText))
            {
                if (!DateOnly.TryParse(NextPaymentDateText.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var nd))
                    throw new ApiException(HttpStatusCode.BadRequest, "Дата следующего платежа: формат yyyy-MM-dd");
                next = nd;
            }

            await _pay.CreateInstallmentAsync(new AdminInstallmentCreateDto
            {
                OrderId = orderId,
                InstallmentCount = cnt,
                MonthlyPayment = monthly,
                NextPaymentDate = next,
                Status = string.IsNullOrWhiteSpace(PlanStatusText) ? null : PlanStatusText.Trim()
            }, cancellationToken);

            Saved?.Invoke();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
