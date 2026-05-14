using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class ApplicationEditViewModel : BaseViewModel
{
    private const int ManagerRoleId = 2;

    private readonly AdminApplicationsService _apps;
    private readonly AdminEmployeesService _employees;
    private readonly AdminCoursesService _courses;
    private readonly int? _id;

    public ApplicationEditViewModel(AdminApplicationsService apps, AdminEmployeesService employees, AdminCoursesService courses, int? id)
    {
        _apps = apps;
        _employees = employees;
        _courses = courses;
        _id = id;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

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

    private string _firstName = "";
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

    private string _lastName = "";
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

    private string _phone = "";
    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

    private string? _email;
    public string? Email { get => _email; set => SetProperty(ref _email, value); }

    private int? _classNumber;
    public int? ClassNumber { get => _classNumber; set => SetProperty(ref _classNumber, value); }

    private string? _managerComment;
    public string? ManagerComment { get => _managerComment; set => SetProperty(ref _managerComment, value); }

    private string? _comment;
    public string? Comment { get => _comment; set => SetProperty(ref _comment, value); }

    public ObservableCollection<SelectableSubject> SubjectOptions { get; } = new();
    public ObservableCollection<AdminApplicationStatusDictDto> StatusOptions { get; } = new();
    public ObservableCollection<IdTitleOption> ManagerOptions { get; } = new();

    private AdminApplicationStatusDictDto? _selectedStatus;
    public AdminApplicationStatusDictDto? SelectedStatus { get => _selectedStatus; set => SetProperty(ref _selectedStatus, value); }

    private IdTitleOption? _selectedManager;
    public IdTitleOption? SelectedManager { get => _selectedManager; set => SetProperty(ref _selectedManager, value); }

    private readonly HashSet<int> _selectedSubjectIds = new();
    public IReadOnlyCollection<int> SelectedSubjectIds => _selectedSubjectIds;

    public void ToggleSubject(int subjectId, bool isSelected)
    {
        if (isSelected) _selectedSubjectIds.Add(subjectId);
        else _selectedSubjectIds.Remove(subjectId);
        OnPropertyChanged(nameof(SelectedSubjectIds));
    }

    public bool IsSubjectSelected(int subjectId) => _selectedSubjectIds.Contains(subjectId);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        SubjectOptions.Clear();
        foreach (var s in await _courses.GetSubjectsAsync(cancellationToken))
            SubjectOptions.Add(new SelectableSubject(s.SubjectId, s.SubjectName, false));

        StatusOptions.Clear();
        foreach (var s in await _apps.GetStatusesAsync(cancellationToken))
            StatusOptions.Add(s);
        SelectedStatus = StatusOptions.FirstOrDefault();

        ManagerOptions.Clear();
        ManagerOptions.Add(new IdTitleOption(0, "—"));
        var mgrs = await _employees.GetEmployeesAsync(null, ManagerRoleId, cancellationToken);
        foreach (var m in mgrs.OrderBy(x => x.FullName))
            ManagerOptions.Add(new IdTitleOption(m.EmployeeId, m.FullName));
        SelectedManager = ManagerOptions.FirstOrDefault();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_id == null) return;

        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _apps.GetApplicationAsync(_id.Value, cancellationToken);
            FirstName = dto.FirstName;
            LastName = dto.LastName ?? "";
            Phone = dto.Phone;
            Email = dto.Email;
            ClassNumber = dto.ClassNumber;
            ManagerComment = dto.ManagerComment;

            SelectedStatus = StatusOptions.FirstOrDefault(x => x.StatusId == dto.StatusId) ?? StatusOptions.FirstOrDefault();
            SelectedManager = ManagerOptions.FirstOrDefault(x => x.Id == dto.ManagerId) ?? ManagerOptions.FirstOrDefault();

            _selectedSubjectIds.Clear();
            foreach (var s in dto.Subjects)
            {
                if (s.SubjectId > 0)
                    _selectedSubjectIds.Add(s.SubjectId);
                else
                {
                    var opt = SubjectOptions.FirstOrDefault(o =>
                        string.Equals(o.SubjectName.Trim(), s.SubjectName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (opt != null)
                        _selectedSubjectIds.Add(opt.SubjectId);
                }
            }

            OnPropertyChanged(nameof(SelectedSubjectIds));

            foreach (var opt in SubjectOptions)
                opt.IsSelected = _selectedSubjectIds.Contains(opt.SubjectId);
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

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            MessageBox.Show("Укажите имя и фамилию.", "Заявка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            MessageBox.Show("Укажите телефон.", "Заявка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            var dto = new AdminApplicationUpsertDto
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Phone = Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                ClassNumber = ClassNumber,
                SubjectIds = SubjectOptions.Where(x => x.IsSelected).Select(x => x.SubjectId).ToList(),
                Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim(),
                StatusId = SelectedStatus?.StatusId,
                AssignedManagerId = SelectedManager is { Id: > 0 } m ? m.Id : null,
                ManagerComment = string.IsNullOrWhiteSpace(ManagerComment) ? null : ManagerComment.Trim(),
            };

            if (_id == null)
                await _apps.CreateAsync(dto, cancellationToken);
            else
                await _apps.UpdateAsync(_id.Value, dto, cancellationToken);

            Saved?.Invoke();
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
}

public sealed class SelectableSubject : BaseViewModel
{
    public SelectableSubject(int subjectId, string subjectName, bool isSelected)
    {
        SubjectId = subjectId;
        SubjectName = subjectName;
        _isSelected = isSelected;
    }

    public int SubjectId { get; }
    public string SubjectName { get; }

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
}
