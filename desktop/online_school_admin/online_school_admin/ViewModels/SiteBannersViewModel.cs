using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class SiteBannersViewModel : BaseViewModel
{
    private readonly AdminSettingsService _settings;

    public SiteBannersViewModel(AdminSettingsService settings)
    {
        _settings = settings;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        AddCommand = new RelayCommand(_ => AddRequested?.Invoke(), _ => !IsBusy);
        EditCommand = new RelayCommand(_ => { if (Selected != null) EditRequested?.Invoke(Selected.BannerId); }, _ => !IsBusy && Selected != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && Selected != null);
        MoveUpCommand = new RelayCommand(async _ => await MoveAsync(-1), _ => !IsBusy && CanMove(-1));
        MoveDownCommand = new RelayCommand(async _ => await MoveAsync(1), _ => !IsBusy && CanMove(1));
        ToggleActiveCommand = new RelayCommand(async _ => await ToggleActiveAsync(), _ => !IsBusy && Selected != null);
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? AddRequested;
    public event Action<int>? EditRequested;
    public event Action? BackRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand ToggleActiveCommand { get; }
    public RelayCommand BackCommand { get; }

    public ObservableCollection<AdminSiteBannerRowDto> Rows { get; } = new();

    private AdminSiteBannerRowDto? _selected;
    public AdminSiteBannerRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                MoveUpCommand.RaiseCanExecuteChanged();
                MoveDownCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
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
                DeleteCommand.RaiseCanExecuteChanged();
                MoveUpCommand.RaiseCanExecuteChanged();
                MoveDownCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
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
            var list = await _settings.GetBannersAsync(cancellationToken);
            Rows.Clear();
            foreach (var r in list.OrderBy(x => x.BannerOrder))
                Rows.Add(r);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private bool CanMove(int delta)
    {
        if (Selected == null) return false;
        var idx = Rows.IndexOf(Selected);
        var newIdx = idx + delta;
        return idx >= 0 && newIdx >= 0 && newIdx < Rows.Count;
    }

    private async Task MoveAsync(int delta)
    {
        if (!CanMove(delta) || Selected == null) return;

        var idx = Rows.IndexOf(Selected);
        var newIdx = idx + delta;

        Rows.Move(idx, newIdx);

        // rebuild order: 1..N
        var req = new AdminReorderRequestDto2
        {
            Items = Rows.Select((b, i) => new AdminReorderItemDto2 { Id = b.BannerId, Order = i + 1 }).ToList()
        };

        Error = null;
        IsBusy = true;
        try
        {
            await _settings.ReorderBannersAsync(req);
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task ToggleActiveAsync()
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _settings.UpdateBannerAsync(Selected.BannerId, new AdminSiteBannerUpsertDto
            {
                Title = Selected.Title,
                Subtitle = Selected.Subtitle,
                ImageUrl = Selected.ImageUrl,
                ButtonText = Selected.ButtonText,
                ButtonUrl = Selected.ButtonUrl,
                BannerOrder = Selected.BannerOrder,
                IsActive = !Selected.IsActive
            });
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync()
    {
        if (Selected == null) return;
        if (!UserDialogs.Confirm($"Удалить баннер «{Selected.Title}»?", "Баннеры"))
            return;
        Error = null;
        IsBusy = true;
        try
        {
            await _settings.DeleteBannerAsync(Selected.BannerId);
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Banners.Delete"); }
        catch (Exception ex) { Error = ApiErrorFormatter.Format(ex); AppLogger.Log(ex, "Banners.Delete"); }
        finally { IsBusy = false; }
    }
}

