using System.Net.Http;
using System.IO;
using Microsoft.Win32;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class StudentEditViewModel : BaseViewModel
{
    private readonly AdminStudentsService _students;
    private readonly int? _studentId;

    public StudentEditViewModel(AdminStudentsService students, int? studentId = null)
    {
        _students = students;
        _studentId = studentId;
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
        PickAvatarCommand = new RelayCommand(_ => PickAvatarFromFile(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PickAvatarCommand { get; }

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
    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public bool IsCreate => !_studentId.HasValue;

    private string _firstName = "";
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

    private string _lastName = "";
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

    private string _email = "";
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string? _phone;
    public string? Phone { get => _phone; set => SetProperty(ref _phone, value); }

    private DateTime? _birthDate;
    public DateTime? BirthDate { get => _birthDate; set => SetProperty(ref _birthDate, value); }

    private int _classNumber;
    public int ClassNumber { get => _classNumber; set => SetProperty(ref _classNumber, value); }

    private string? _parentPhone;
    public string? ParentPhone { get => _parentPhone; set => SetProperty(ref _parentPhone, value); }

    private string? _parentEmail;
    public string? ParentEmail { get => _parentEmail; set => SetProperty(ref _parentEmail, value); }

    private string? _avatarUrl;
    public string? AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }

    private string? _avatarBase64;
    public string? AvatarBase64 { get => _avatarBase64; set => SetProperty(ref _avatarBase64, value); }

    private bool _isActive = true;
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    private string _password = "";
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!_studentId.HasValue) return;
        Error = null;
        IsBusy = true;
        try
        {
            var d = await _students.GetStudentAsync(_studentId.Value, cancellationToken);
            FirstName = d.FirstName;
            LastName = d.LastName;
            Email = d.Email;
            Phone = d.Phone;
            BirthDate = d.DateOfBirth;
            ClassNumber = d.ClassNumber;
            ParentPhone = d.ParentPhone;
            ParentEmail = d.ParentEmail;
            AvatarUrl = d.AvatarUrl;
            IsActive = d.IsActive;
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Student.Edit.Load");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Student.Edit.Load");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            Error = "Укажите имя и фамилию.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Email))
        {
            Error = "Укажите email.";
            return;
        }
        if (!SimpleEmailValidator.IsValid(Email))
        {
            Error = "Введите корректный email.";
            return;
        }
        if (ClassNumber is < 0 or > 11)
        {
            Error = "Класс должен быть в диапазоне 0–11.";
            return;
        }

        if (IsCreate)
        {
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                Error = "Пароль для входа ученика: не короче 6 символов.";
                return;
            }
        }

        IsBusy = true;
        try
        {
            if (IsCreate)
            {
                await _students.CreateStudentAsync(new AdminStudentCreateDto
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    Password = Password,
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                    BirthDate = BirthDate.HasValue ? BirthDate.Value.ToString("yyyy-MM-dd") : null,
                    ClassNumber = ClassNumber,
                    ParentPhone = string.IsNullOrWhiteSpace(ParentPhone) ? null : ParentPhone.Trim(),
                    ParentEmail = string.IsNullOrWhiteSpace(ParentEmail) ? null : ParentEmail.Trim(),
                    AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim(),
                    AvatarBase64 = AvatarBase64,
                    IsActive = IsActive
                }, cancellationToken);
            }
            else
            {
                await _students.UpdateStudentAsync(_studentId!.Value, new AdminStudentUpsertDto
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                    BirthDate = BirthDate.HasValue ? BirthDate.Value.ToString("yyyy-MM-dd") : null,
                    ClassNumber = ClassNumber,
                    ParentPhone = string.IsNullOrWhiteSpace(ParentPhone) ? null : ParentPhone.Trim(),
                    ParentEmail = string.IsNullOrWhiteSpace(ParentEmail) ? null : ParentEmail.Trim(),
                    AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim(),
                    AvatarBase64 = AvatarBase64,
                    IsActive = IsActive
                }, cancellationToken);
            }

            Saved?.Invoke();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Student.Edit.Save");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Student.Edit.Save");
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

