using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class OrderDetailsViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;
    private readonly int _orderId;

    public OrderDetailsViewModel(AdminPaymentsService pay, int orderId)
    {
        _pay = pay;
        _orderId = orderId;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
    }

    public event Action? BackRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand SaveCommand { get; }

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
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminOrderDetailsDto? _details;
    public AdminOrderDetailsDto? Details { get => _details; set => SetProperty(ref _details, value); }

    private string _methodIdText = "";
    public string MethodIdText { get => _methodIdText; set => SetProperty(ref _methodIdText, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            Details = await _pay.GetOrderAsync(_orderId, cancellationToken);
            MethodIdText = Details.MethodId?.ToString(CultureInfo.InvariantCulture) ?? "";
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
            int? methodId = null;
            if (!string.IsNullOrWhiteSpace(MethodIdText))
            {
                if (!int.TryParse(MethodIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var mid))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный ID способа оплаты");
                methodId = mid;
            }

            await _pay.UpdateOrderAsync(_orderId, new AdminOrderUpdateDto { MethodId = methodId }, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
