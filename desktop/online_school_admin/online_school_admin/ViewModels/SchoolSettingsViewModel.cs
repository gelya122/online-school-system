using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class SchoolSettingsViewModel : BaseViewModel
{
    private readonly AdminSettingsService _settings;

    public SchoolSettingsViewModel(AdminSettingsService settings)
    {
        _settings = settings;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }
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
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public string SchoolName { get => _schoolName; set => SetProperty(ref _schoolName, value); }
    private string _schoolName = "";

    public string LogoUrl { get => _logoUrl; set => SetProperty(ref _logoUrl, value); }
    private string _logoUrl = "";

    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
    private string _phone = "";

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    private string _email = "";

    public string Address { get => _address; set => SetProperty(ref _address, value); }
    private string _address = "";

    public string AboutText { get => _aboutText; set => SetProperty(ref _aboutText, value); }
    private string _aboutText = "";

    public string PrivacyPolicyUrl { get => _privacyPolicyUrl; set => SetProperty(ref _privacyPolicyUrl, value); }
    private string _privacyPolicyUrl = "";

    public string TermsUrl { get => _termsUrl; set => SetProperty(ref _termsUrl, value); }
    private string _termsUrl = "";

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var s = await _settings.GetSchoolSettingsAsync(cancellationToken);
            SchoolName = s.SchoolName ?? "";
            LogoUrl = s.LogoUrl ?? "";
            Phone = s.Phone ?? "";
            Email = s.Email ?? "";
            Address = s.Address ?? "";
            AboutText = s.AboutText ?? "";
            PrivacyPolicyUrl = s.PrivacyPolicyUrl ?? "";
            TermsUrl = s.TermsUrl ?? "";
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
            await _settings.UpdateSchoolSettingsAsync(new AdminSchoolSettingsUpdateDto
            {
                SchoolName = SchoolName.Trim(),
                LogoUrl = string.IsNullOrWhiteSpace(LogoUrl) ? null : LogoUrl.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                AboutText = string.IsNullOrWhiteSpace(AboutText) ? null : AboutText.Trim(),
                PrivacyPolicyUrl = string.IsNullOrWhiteSpace(PrivacyPolicyUrl) ? null : PrivacyPolicyUrl.Trim(),
                TermsUrl = string.IsNullOrWhiteSpace(TermsUrl) ? null : TermsUrl.Trim()
            }, cancellationToken);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

