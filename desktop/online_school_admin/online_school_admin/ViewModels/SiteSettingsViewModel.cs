using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class SiteSettingsViewModel : BaseViewModel
{
    private readonly AdminSettingsService _settings;

    public SiteSettingsViewModel(AdminSettingsService settings)
    {
        _settings = settings;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        OpenBannersCommand = new RelayCommand(_ => OpenBannersRequested?.Invoke(), _ => !IsBusy);
        OpenFaqCommand = new RelayCommand(_ => OpenFaqRequested?.Invoke(), _ => !IsBusy);
        OpenReviewsCommand = new RelayCommand(_ => OpenReviewsRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? OpenBannersRequested;
    public event Action? OpenFaqRequested;
    public event Action? OpenReviewsRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand OpenBannersCommand { get; }
    public RelayCommand OpenFaqCommand { get; }
    public RelayCommand OpenReviewsCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                OpenBannersCommand.RaiseCanExecuteChanged();
                OpenFaqCommand.RaiseCanExecuteChanged();
                OpenReviewsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public string SiteName { get => _siteName; set => SetProperty(ref _siteName, value); }
    private string _siteName = "";

    public string MainPageTitle { get => _mainPageTitle; set => SetProperty(ref _mainPageTitle, value); }
    private string _mainPageTitle = "";

    public string MainPageDescription { get => _mainPageDescription; set => SetProperty(ref _mainPageDescription, value); }
    private string _mainPageDescription = "";

    public string ContactPhone { get => _contactPhone; set => SetProperty(ref _contactPhone, value); }
    private string _contactPhone = "";

    public string ContactEmail { get => _contactEmail; set => SetProperty(ref _contactEmail, value); }
    private string _contactEmail = "";

    public string VkUrl { get => _vkUrl; set => SetProperty(ref _vkUrl, value); }
    private string _vkUrl = "";

    public string TelegramUrl { get => _telegramUrl; set => SetProperty(ref _telegramUrl, value); }
    private string _telegramUrl = "";

    public string YoutubeUrl { get => _youtubeUrl; set => SetProperty(ref _youtubeUrl, value); }
    private string _youtubeUrl = "";

    public string SeoTitle { get => _seoTitle; set => SetProperty(ref _seoTitle, value); }
    private string _seoTitle = "";

    public string SeoDescription { get => _seoDescription; set => SetProperty(ref _seoDescription, value); }
    private string _seoDescription = "";

    public bool MaintenanceMode { get => _maintenanceMode; set => SetProperty(ref _maintenanceMode, value); }
    private bool _maintenanceMode;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var s = await _settings.GetSiteSettingsAsync(cancellationToken);
            SiteName = s.SiteName ?? "";
            MainPageTitle = s.MainPageTitle ?? "";
            MainPageDescription = s.MainPageDescription ?? "";
            ContactPhone = s.ContactPhone ?? "";
            ContactEmail = s.ContactEmail ?? "";
            VkUrl = s.VkUrl ?? "";
            TelegramUrl = s.TelegramUrl ?? "";
            YoutubeUrl = s.YoutubeUrl ?? "";
            SeoTitle = s.SeoTitle ?? "";
            SeoDescription = s.SeoDescription ?? "";
            MaintenanceMode = s.MaintenanceMode;
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
            await _settings.UpdateSiteSettingsAsync(new AdminSiteSettingsUpdateDto
            {
                SiteName = string.IsNullOrWhiteSpace(SiteName) ? null : SiteName.Trim(),
                MainPageTitle = string.IsNullOrWhiteSpace(MainPageTitle) ? null : MainPageTitle.Trim(),
                MainPageDescription = string.IsNullOrWhiteSpace(MainPageDescription) ? null : MainPageDescription.Trim(),
                ContactPhone = string.IsNullOrWhiteSpace(ContactPhone) ? null : ContactPhone.Trim(),
                ContactEmail = string.IsNullOrWhiteSpace(ContactEmail) ? null : ContactEmail.Trim(),
                VkUrl = string.IsNullOrWhiteSpace(VkUrl) ? null : VkUrl.Trim(),
                TelegramUrl = string.IsNullOrWhiteSpace(TelegramUrl) ? null : TelegramUrl.Trim(),
                YoutubeUrl = string.IsNullOrWhiteSpace(YoutubeUrl) ? null : YoutubeUrl.Trim(),
                SeoTitle = string.IsNullOrWhiteSpace(SeoTitle) ? null : SeoTitle.Trim(),
                SeoDescription = string.IsNullOrWhiteSpace(SeoDescription) ? null : SeoDescription.Trim(),
                MaintenanceMode = MaintenanceMode
            }, cancellationToken);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

