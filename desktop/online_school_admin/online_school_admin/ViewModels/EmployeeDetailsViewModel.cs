using System.Linq;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class EmployeeDetailsViewModel : BaseViewModel
{
    private readonly AdminEmployeesService _employees;
    private readonly int _employeeId;

    public EmployeeDetailsViewModel(AdminEmployeesService employees, int employeeId)
    {
        _employees = employees;
        _employeeId = employeeId;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }

    public string HeaderFullName =>
        Details == null
            ? ""
            : string.Join(" ", new[] { Details.LastName, Details.FirstName, Details.Patronymic }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

    public string HeaderIds =>
        Details == null ? "" : $"ID сотрудника: {Details.EmployeeId}  ·  ID пользователя: {Details.UserId}";

    public string? DobLine =>
        Details?.DateOfBirth is { } d ? $"Дата рождения: {d:dd.MM.yyyy}" : null;

    public string ExperienceLine =>
        Details?.Experience is int x ? $"Опыт работы: {x} лет" : "Опыт работы: —";

    public string? AvatarLine =>
        string.IsNullOrWhiteSpace(Details?.AvatarUrl) ? null : $"Аватар (URL): {Details.AvatarUrl}";

    public string ActiveLine =>
        Details == null ? "" : Details.IsActive ? "Учётная запись: активна" : "Учётная запись: отключена";

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                RefreshCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    private AdminEmployeeDetailsDto? _details;
    public AdminEmployeeDetailsDto? Details
    {
        get => _details;
        private set
        {
            if (!SetProperty(ref _details, value))
                return;
            OnPropertyChanged(nameof(HeaderFullName));
            OnPropertyChanged(nameof(HeaderIds));
            OnPropertyChanged(nameof(DobLine));
            OnPropertyChanged(nameof(ExperienceLine));
            OnPropertyChanged(nameof(AvatarLine));
            OnPropertyChanged(nameof(ActiveLine));
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            Details = await _employees.GetEmployeeAsync(_employeeId, cancellationToken);
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

