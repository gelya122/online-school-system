using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class SiteBannerEditViewModel : BaseViewModel
{
    private readonly AdminSettingsService _settings;
    private readonly int? _id;

    public SiteBannerEditViewModel(AdminSettingsService settings, int? id)
    {
        _settings = settings;
        _id = id;
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

    public string TitleLine => _id.HasValue ? "Редактировать баннер" : "Добавить баннер";

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

    public string BannerTitle { get => _bannerTitle; set => SetProperty(ref _bannerTitle, value); }
    private string _bannerTitle = "";

    public string Subtitle { get => _subtitle; set => SetProperty(ref _subtitle, value); }
    private string _subtitle = "";

    public string ImageUrl { get => _imageUrl; set => SetProperty(ref _imageUrl, value); }
    private string _imageUrl = "";

    public string ButtonText { get => _buttonText; set => SetProperty(ref _buttonText, value); }
    private string _buttonText = "";

    public string ButtonUrl { get => _buttonUrl; set => SetProperty(ref _buttonUrl, value); }
    private string _buttonUrl = "";

    public string OrderText { get => _orderText; set => SetProperty(ref _orderText, value); }
    private string _orderText = "0";

    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    private bool _isActive = true;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!_id.HasValue) return;

        Error = null;
        IsBusy = true;
        try
        {
            var list = await _settings.GetBannersAsync(cancellationToken);
            var b = list.FirstOrDefault(x => x.BannerId == _id.Value);
            if (b == null) { Error = "Баннер не найден"; return; }

            BannerTitle = b.Title;
            Subtitle = b.Subtitle ?? "";
            ImageUrl = b.ImageUrl ?? "";
            ButtonText = b.ButtonText ?? "";
            ButtonUrl = b.ButtonUrl ?? "";
            OrderText = b.BannerOrder.ToString();
            IsActive = b.IsActive;
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
            if (string.IsNullOrWhiteSpace(BannerTitle))
            {
                Error = "Укажите title";
                return;
            }

            var order = 0;
            int.TryParse(OrderText?.Trim(), out order);

            var dto = new AdminSiteBannerUpsertDto
            {
                Title = BannerTitle.Trim(),
                Subtitle = string.IsNullOrWhiteSpace(Subtitle) ? null : Subtitle.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim(),
                ButtonText = string.IsNullOrWhiteSpace(ButtonText) ? null : ButtonText.Trim(),
                ButtonUrl = string.IsNullOrWhiteSpace(ButtonUrl) ? null : ButtonUrl.Trim(),
                BannerOrder = order,
                IsActive = IsActive
            };

            if (_id.HasValue)
                await _settings.UpdateBannerAsync(_id.Value, dto, cancellationToken);
            else
                await _settings.CreateBannerAsync(dto, cancellationToken);

            Saved?.Invoke();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

