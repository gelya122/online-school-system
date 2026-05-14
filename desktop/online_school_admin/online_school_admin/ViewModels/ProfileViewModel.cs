using System.Globalization;
using System.Net.Http;
using System.Text;
using System.IO;
using Microsoft.Win32;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class ProfileViewModel : BaseViewModel
{
    private readonly AdminProfileService _profile;
    private readonly AuthService _auth;

    public ProfileViewModel(AdminProfileService profile, AuthService auth)
    {
        _profile = profile;
        _auth = auth;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        ChangePasswordCommand = new RelayCommand(async _ => await ChangePasswordAsync(), _ => !IsBusy);
        UploadAvatarCommand = new RelayCommand(async _ => await UploadAvatarAsync(), _ => !IsBusy);
        LogoutCommand = new RelayCommand(async _ => await _auth.LogoutAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand ChangePasswordCommand { get; }
    public RelayCommand UploadAvatarCommand { get; }
    public RelayCommand LogoutCommand { get; }

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
                ChangePasswordCommand.RaiseCanExecuteChanged();
                UploadAvatarCommand.RaiseCanExecuteChanged();
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminProfileDto? _dto;
    public AdminProfileDto? Dto { get => _dto; private set => SetProperty(ref _dto, value); }

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    private string _email = "";

    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }
    private string _firstName = "";

    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }
    private string _lastName = "";

    public string Patronymic { get => _patronymic; set => SetProperty(ref _patronymic, value); }
    private string _patronymic = "";

    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
    private string _phone = "";

    public string BirthDateText { get => _birthDateText; set => SetProperty(ref _birthDateText, value); }
    private string _birthDateText = "";

    public string ExperienceText { get => _experienceText; set => SetProperty(ref _experienceText, value); }
    private string _experienceText = "";

    public string AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }
    private string _avatarUrl = "";

    public string Role { get => _role; set => SetProperty(ref _role, value); }
    private string _role = "";

    public int HomeworkOnReview => Dto?.HomeworkOnReview ?? 0;
    public IReadOnlyList<AdminProfileInstanceRowDto> AssignedInstances => Dto?.AssignedInstances ?? [];
    public IReadOnlyList<AdminProfileCheckedHomeworkRowDto> RecentChecked => Dto?.RecentChecked ?? [];

    public bool IsTeacher
    {
        get
        {
            var r = (Role ?? "").ToLowerInvariant();
            return r.Contains("teacher") || r.Contains("препод");
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            Dto = await _profile.GetAsync(cancellationToken);
            Email = Dto.Email;
            Role = Dto.Role;
            Phone = Dto.Phone ?? "";
            BirthDateText = Dto.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
            ExperienceText = Dto.Experience?.ToString(CultureInfo.InvariantCulture) ?? "";
            AvatarUrl = Dto.AvatarUrl ?? "";

            // split full name best-effort
            var parts = (Dto.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            LastName = parts.ElementAtOrDefault(0) ?? "";
            FirstName = parts.ElementAtOrDefault(1) ?? "";
            Patronymic = parts.ElementAtOrDefault(2) ?? "";

            OnPropertyChanged(nameof(HomeworkOnReview));
            OnPropertyChanged(nameof(AssignedInstances));
            OnPropertyChanged(nameof(RecentChecked));
            OnPropertyChanged(nameof(IsTeacher));
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
            DateOnly? birth = null;
            if (!string.IsNullOrWhiteSpace(BirthDateText))
            {
                if (!DateOnly.TryParseExact(BirthDateText.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                {
                    Error = "Некорректная дата рождения (yyyy-MM-dd)";
                    return;
                }
                birth = d;
            }

            int? exp = null;
            if (!string.IsNullOrWhiteSpace(ExperienceText))
            {
                if (!int.TryParse(ExperienceText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var e))
                {
                    Error = "Некорректный опыт";
                    return;
                }
                exp = e;
            }

            await _profile.UpdateAsync(new AdminProfileUpdateDto
            {
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                FirstName = string.IsNullOrWhiteSpace(FirstName) ? null : FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(LastName) ? null : LastName.Trim(),
                Patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                BirthDate = birth,
                Experience = exp
            }, cancellationToken);

            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }
    private string _currentPassword = "";

    public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
    private string _newPassword = "";

    private async Task ChangePasswordAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            await _profile.ChangePasswordAsync(new AdminChangePasswordDto
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword
            }, cancellationToken);

            CurrentPassword = "";
            NewPassword = "";
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task UploadAvatarAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.gif;*.webp",
                Multiselect = false
            };
            if (dlg.ShowDialog() != true)
                return;

            var bytes = await File.ReadAllBytesAsync(dlg.FileName, cancellationToken);
            var base64 = Convert.ToBase64String(bytes);

            var res = await _profile.UploadAvatarAsync(new AdminUploadAvatarDto { Base64 = base64 }, cancellationToken);
            AvatarUrl = res.AvatarUrl;
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

