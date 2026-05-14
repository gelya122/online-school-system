using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class InstallmentsTabViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;

    public InstallmentsTabViewModel(AdminPaymentsService pay)
    {
        _pay = pay;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        PatchStatusCommand = new RelayCommand(async _ => await PatchStatusAsync(), _ => !IsBusy && Selected != null);
        CreatePlanCommand = new RelayCommand(_ => CreatePlanRequested?.Invoke(), _ => !IsBusy);
        OpenPlanCommand = new RelayCommand(_ => { if (Selected != null) OpenPlanRequested?.Invoke(Selected.PlanId); }, _ => !IsBusy && Selected != null);
    }

    public event Action? CreatePlanRequested;
    public event Action<int>? OpenPlanRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand PatchStatusCommand { get; }
    public RelayCommand CreatePlanCommand { get; }
    public RelayCommand OpenPlanCommand { get; }

    public ObservableCollection<AdminInstallmentListRowDto> Rows { get; } = new();

    private AdminInstallmentListRowDto? _selected;
    public AdminInstallmentListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                PatchStatusCommand.RaiseCanExecuteChanged();
                OpenPlanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _statusText = "";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

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
                CreatePlanCommand.RaiseCanExecuteChanged();
                OpenPlanCommand.RaiseCanExecuteChanged();
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
            var list = await _pay.GetInstallmentsAsync(cancellationToken);
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
        if (string.IsNullOrWhiteSpace(StatusText)) { Error = "Введите статус плана"; return; }
        Error = null;
        IsBusy = true;
        try
        {
            await _pay.PatchInstallmentStatusAsync(Selected.PlanId, new AdminInstallmentStatusPatchDto { Status = StatusText.Trim() });
            StatusText = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
