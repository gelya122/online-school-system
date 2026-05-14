using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class PromoCodeUsagesViewModel : BaseViewModel
{
    private readonly AdminPromoCodesService _promo;
    private readonly int _promoCodeId;

    public PromoCodeUsagesViewModel(AdminPromoCodesService promo, int promoCodeId, string promoCode)
    {
        _promo = promo;
        _promoCodeId = promoCodeId;
        PromoCode = promoCode;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? BackRequested;

    public string PromoCode { get; }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand BackCommand { get; }

    public ObservableCollection<AdminPromoCodeUsageRowDto> Rows { get; } = new();

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
            var list = await _promo.GetPromoCodeUsagesAsync(_promoCodeId, cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

