using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Win32;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class EmployeeEditViewModel : BaseViewModel
{
    private readonly AdminEmployeesService _employees;
    private readonly AuthService _auth;
    private readonly int? _employeeId;

    public EmployeeEditViewModel(AdminEmployeesService employees, AuthService auth, int? employeeId = null)
    {
        _employees = employees;
        _auth = auth;
        _employeeId = employeeId;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
        PickAvatarCommand = new RelayCommand(_ => PickAvatarFromFile(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

    public bool IsCreate => !_employeeId.HasValue;

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PickAvatarCommand { get; }

    public ObservableCollection<RoleOption> Roles { get; } = new();

    private RoleOption? _selectedRole;
    public RoleOption? SelectedRole { get => _selectedRole; set => SetProperty(ref _selectedRole, value); }

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
                PickAvatarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    private string _email = "";

    public string Password { get => _password; set => SetProperty(ref _password, value); }
    private string _password = "";

    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }
    private string _firstName = "";

    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }
    private string _lastName = "";

    public string? Patronymic { get => _patronymic; set => SetProperty(ref _patronymic, value); }
    private string? _patronymic;

    public string? Phone { get => _phone; set => SetProperty(ref _phone, value); }
    private string? _phone;

    public DateTime? BirthDate { get => _birthDate; set => SetProperty(ref _birthDate, value); }
    private DateTime? _birthDate;

    public int? Experience { get => _experience; set => SetProperty(ref _experience, value); }
    private int? _experience;

    public string? AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }
    private string? _avatarUrl;

    public string? AvatarBase64 { get => _avatarBase64; set => SetProperty(ref _avatarBase64, value); }
    private string? _avatarBase64;

    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    private bool _isActive = true;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Roles.Clear();
        Roles.Add(new RoleOption(0, "Роль пользователя"));
        var roles = await _auth.GetAllRolesAsync(cancellationToken);
        foreach (var r in roles.Where(x => x.RoleId != EmployeeDesktopAccess.StudentRoleId))
            Roles.Add(new RoleOption(r.RoleId, r.RoleName));
        SelectedRole ??= Roles.FirstOrDefault();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!_employeeId.HasValue) return;

        Error = null;
        IsBusy = true;
        try
        {
            var d = await _employees.GetEmployeeAsync(_employeeId.Value, cancellationToken);
            Email = d.Email;
            FirstName = d.FirstName;
            LastName = d.LastName;
            Patronymic = d.Patronymic;
            Phone = d.Phone;
            BirthDate = d.DateOfBirth;
            Experience = d.Experience;
            AvatarUrl = d.AvatarUrl;
            IsActive = d.IsActive;
            SelectedRole = Roles.FirstOrDefault(r => r.RoleId == d.RoleId) ?? Roles.FirstOrDefault();
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        if (SelectedRole == null || SelectedRole.RoleId <= 0)
        {
            Error = "Выберите роль.";
            return;
        }
        if (SelectedRole.RoleId == EmployeeDesktopAccess.StudentRoleId)
        {
            Error = "Роль «ученик» недоступна для сотрудника.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Email))
        {
            Error = "Укажите email.";
            return;
        }
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            Error = "Укажите имя и фамилию.";
            return;
        }
        if (Experience is < 0 or > 80)
        {
            Error = "Опыт: допустимый диапазон 0–80.";
            return;
        }

        IsBusy = true;
        try
        {
            if (IsCreate)
            {
                if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
                {
                    Error = "Пароль должен быть не короче 6 символов.";
                    return;
                }

                await _employees.CreateAsync(new AdminEmployeeCreateDto
                {
                    Email = Email.Trim(),
                    Password = Password,
                    RoleId = SelectedRole.RoleId,
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic.Trim(),
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                    BirthDate = BirthDate.HasValue ? BirthDate.Value.ToString("yyyy-MM-dd") : null,
                    Experience = Experience,
                    AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim(),
                    AvatarBase64 = AvatarBase64,
                    IsActive = IsActive
                }, cancellationToken);
            }
            else
            {
                await _employees.UpdateAsync(_employeeId!.Value, new AdminEmployeeUpdateDto
                {
                    Email = Email.Trim(),
                    RoleId = SelectedRole.RoleId,
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic.Trim(),
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                    BirthDate = BirthDate.HasValue ? BirthDate.Value.ToString("yyyy-MM-dd") : null,
                    Experience = Experience,
                    AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim(),
                    AvatarBase64 = AvatarBase64,
                    IsActive = IsActive
                }, cancellationToken);
            }

            Saved?.Invoke();
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

    private void PickAvatarFromFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите аватар",
            Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.webp|Все файлы|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog() != true)
            return;
        var bytes = File.ReadAllBytes(dialog.FileName);
        AvatarBase64 = Convert.ToBase64String(bytes);
    }
}

