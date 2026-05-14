using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class PromoCodesViewModel : BaseViewModel
{
    private readonly AdminPromoCodesService _promo;
    private readonly PermissionService _permissions;

    public PromoCodesViewModel(AdminPromoCodesService promo, PermissionService permissions)
    {
        _promo = promo;
        _permissions = permissions;

        ActiveOptions.Add(new ActiveOption(null, "Все"));
        ActiveOptions.Add(new ActiveOption(true, "Активные"));
        ActiveOptions.Add(new ActiveOption(false, "Неактивные"));
        SelectedActive = ActiveOptions.FirstOrDefault();

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy && _permissions.CanEditPromoCodes);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.PromoCodeId); }, _ => !IsBusy && Selected != null && _permissions.CanEditPromoCodes);
        ActivateCommand = new RelayCommand(async _ => await ActivateAsync(true), _ => !IsBusy && Selected != null && _permissions.CanEditPromoCodes);
        DeactivateCommand = new RelayCommand(async _ => await ActivateAsync(false), _ => !IsBusy && Selected != null && _permissions.CanEditPromoCodes);
        UsagesCommand = new RelayCommand(_ => { if (Selected != null) UsagesRequested?.Invoke(Selected.PromoCodeId, Selected.Code); }, _ => !IsBusy && Selected != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && Selected != null && _permissions.CanEditPromoCodes);
    }

    public event Action? AddRequested;
    public event Action<int>? EditRequested;
    public event Action<int, string>? UsagesRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ActivateCommand { get; }
    public RelayCommand DeactivateCommand { get; }
    public RelayCommand UsagesCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public ObservableCollection<AdminPromoCodeListRowDto> Rows { get; } = new();
    public ObservableCollection<ActiveOption> ActiveOptions { get; } = new();

    private ActiveOption? _selectedActive;
    public ActiveOption? SelectedActive
    {
        get => _selectedActive;
        set => SetProperty(ref _selectedActive, value);
    }

    private string _search = "";
    public string Search { get => _search; set => SetProperty(ref _search, value); }

    private AdminPromoCodeListRowDto? _selected;
    public AdminPromoCodeListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                ActivateCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
                UsagesCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
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
                ActivateCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
                UsagesCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
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
            var list = await _promo.GetPromoCodesAsync(
                string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
                SelectedActive?.Value,
                cancellationToken);

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

    private async Task ActivateAsync(bool active)
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            if (active)
                await _promo.ActivatePromoCodeAsync(Selected.PromoCodeId);
            else
                await _promo.DeactivatePromoCodeAsync(Selected.PromoCodeId);

            await LoadAsync();
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

    private async Task DeleteAsync()
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm($"Удалить промокод «{Selected.Code}»? (будет помечен как удалённый)", "Удаление промокода"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _promo.DeletePromoCodeAsync(Selected.PromoCodeId);
            await LoadAsync();
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

