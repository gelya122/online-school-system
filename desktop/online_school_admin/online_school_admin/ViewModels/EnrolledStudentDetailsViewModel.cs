using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

/// <summary>Карточка студента в контексте потока (просмотр из вкладки «Студенты»).</summary>
public sealed class EnrolledStudentDetailsViewModel : BaseViewModel
{
    private readonly AdminStudentsService _students;
    private readonly Action _navigateBack;

    public EnrolledStudentDetailsViewModel(AdminStudentsService students, int studentId, int instanceId, Action navigateBack)
    {
        _students = students;
        StudentId = studentId;
        InstanceId = instanceId;
        _navigateBack = navigateBack;

        BackCommand = new RelayCommand(_ => _navigateBack(), _ => !IsBusy);
    }

    public int StudentId { get; }
    public int InstanceId { get; }

    public RelayCommand BackCommand { get; }

    public string BreadcrumbText => $"Потоки → Поток #{InstanceId} → Студент";

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                BackCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminStudentDetailsDto? _student;
    public AdminStudentDetailsDto? Student { get => _student; private set => SetProperty(ref _student, value); }

    public string FullNameDisplay =>
        Student == null
            ? ""
            : $"{Student.LastName} {Student.FirstName}".Trim();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            Student = await _students.GetStudentAsync(StudentId, cancellationToken);
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

        OnPropertyChanged(nameof(FullNameDisplay));
    }
}
