using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Mail;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly AuthService _auth;

    public RegisterViewModel(AuthService auth)
    {
        _auth = auth;
    }

    public ObservableCollection<RoleOption> Roles { get; } = new();

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _firstName = "";

    [ObservableProperty]
    private string _lastName = "";

    [ObservableProperty]
    private string? _patronymic;

    [ObservableProperty]
    private string _phone = "";

    [ObservableProperty]
    private DateTime? _birthDate;

    [ObservableProperty]
    private int? _experience;

    [ObservableProperty]
    private string? _avatarUrl;

    [ObservableProperty]
    private string? _avatarBase64;

    [ObservableProperty]
    private string? _avatarFileName;

    [ObservableProperty]
    private RoleOption? _selectedRole;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public async Task LoadRolesAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            Roles.Clear();
            Roles.Add(new RoleOption(0, "Роль пользователя"));
            var rows = await _auth.GetAllRolesAsync(cancellationToken);
            foreach (var r in rows.OrderBy(x => x.RoleId))
            {
                var cap = string.IsNullOrWhiteSpace(r.Description)
                    ? r.RoleName
                    : $"{r.RoleName} — {r.Description}";
                Roles.Add(new RoleOption(r.RoleId, cap));
            }

            SelectedRole = Roles.FirstOrDefault();
            if (Roles.Count == 0)
                ErrorMessage = "Нет доступных ролей сотрудника. Проверьте таблицу user_role в базе (роль ученика недоступна здесь).";
        }
        catch (AuthApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Не удалось загрузить роли. Проверьте, что API запущен.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<(bool ok, string? message)> TryRegisterAsync(string password, string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        ErrorMessage = null;
        if (SelectedRole == null || SelectedRole.RoleId <= 0)
        {
            ErrorMessage = "Выберите роль.";
            return (false, ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Укажите email.";
            return (false, ErrorMessage);
        }

        try
        {
            _ = new MailAddress(Email.Trim());
        }
        catch
        {
            ErrorMessage = "Введите корректный email.";
            return (false, ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Укажите имя и фамилию.";
            return (false, ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            ErrorMessage = "Укажите телефон.";
            return (false, ErrorMessage);
        }

        if (password != confirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return (false, ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            ErrorMessage = "Пароль должен быть не короче 6 символов.";
            return (false, ErrorMessage);
        }

        if (Experience is < 0 or > 80)
        {
            ErrorMessage = "Стаж: допустимый диапазон 0–80 лет.";
            return (false, ErrorMessage);
        }

        IsBusy = true;
        try
        {
            await _auth.RegisterEmployeeAsync(new AuthRegisterEmployeeRequest
            {
                Email = Email.Trim(),
                Password = password,
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic.Trim(),
                Phone = Phone.Trim(),
                BirthDate = BirthDate.HasValue ? BirthDate.Value.ToString("yyyy-MM-dd") : null,
                Experience = Experience,
                AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim(),
                AvatarBase64 = AvatarBase64,
                RoleId = SelectedRole.RoleId
            }, cancellationToken);
            return (true, null);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            return (false, ex.Message);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Не удалось связаться с сервером.";
            return (false, ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void PickAvatarFile()
    {
        ErrorMessage = null;
        var dialog = new OpenFileDialog
        {
            Title = "Выберите аватар",
            Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.webp|Все файлы|*.*",
            Multiselect = false
        };

        var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        var ok = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        if (ok != true)
            return;

        try
        {
            var bytes = File.ReadAllBytes(dialog.FileName);
            AvatarBase64 = Convert.ToBase64String(bytes);
            AvatarFileName = Path.GetFileName(dialog.FileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось прочитать файл: {ex.Message}";
        }
    }
}
