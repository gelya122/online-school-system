using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class InstallmentDetailsViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;
    private readonly int _planId;

    public InstallmentDetailsViewModel(AdminPaymentsService pay, int planId)
    {
        _pay = pay;
        _planId = planId;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
        PatchPaymentStatusCommand = new RelayCommand(async _ => await PatchPaymentStatusAsync(), _ => !IsBusy && SelectedPayment != null);
    }

    public event Action? BackRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand PatchPaymentStatusCommand { get; }

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
                PatchPaymentStatusCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminInstallmentDetailsDto? _details;
    public AdminInstallmentDetailsDto? Details { get => _details; private set => SetProperty(ref _details, value); }

    public ObservableCollection<AdminInstallmentPaymentRowDto> PaymentRows { get; } = new();

    private AdminInstallmentPaymentRowDto? _selectedPayment;
    public AdminInstallmentPaymentRowDto? SelectedPayment
    {
        get => _selectedPayment;
        set
        {
            if (SetProperty(ref _selectedPayment, value))
                PatchPaymentStatusCommand.RaiseCanExecuteChanged();
        }
    }

    private string _paymentStatusText = "";
    public string PaymentStatusText { get => _paymentStatusText; set => SetProperty(ref _paymentStatusText, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _pay.GetInstallmentAsync(_planId, cancellationToken);
            Details = dto;
            PaymentRows.Clear();
            foreach (var p in dto.Payments)
                PaymentRows.Add(p);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task PatchPaymentStatusAsync()
    {
        if (SelectedPayment == null) return;
        if (string.IsNullOrWhiteSpace(PaymentStatusText)) { Error = "Введите статус платежа"; return; }
        Error = null;
        IsBusy = true;
        try
        {
            await _pay.PatchInstallmentPaymentStatusAsync(
                SelectedPayment.InstallmentPaymentId,
                new AdminInstallmentPaymentStatusPatchDto { Status = PaymentStatusText.Trim() });
            PaymentStatusText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
