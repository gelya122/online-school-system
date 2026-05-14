using System.Collections.ObjectModel;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class StudentProgressForStudentViewModel : BaseViewModel
{
    private readonly AdminProgressService _progress;
    private readonly int _studentId;

    public StudentProgressForStudentViewModel(AdminProgressService progress, int studentId)
    {
        _progress = progress;
        _studentId = studentId;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        OpenEnrollmentCommand = new RelayCommand(_ =>
        {
            if (Selected != null) OpenEnrollmentRequested?.Invoke(Selected.EnrollmentId);
        }, _ => !IsBusy && Selected != null);
    }

    public event Action<int>? OpenEnrollmentRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand OpenEnrollmentCommand { get; }

    public ObservableCollection<AdminStudentProgressListRowDto> Rows { get; } = new();

    private AdminStudentProgressListRowDto? _selected;
    public AdminStudentProgressListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
                OpenEnrollmentCommand.RaiseCanExecuteChanged();
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                OpenEnrollmentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _progress.GetStudentProgressAsync(_studentId, cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
            Selected = Rows.FirstOrDefault();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "StudentProgressForStudent.Load");
        }
        catch (Exception ex)
        {
            Error = "Не удалось загрузить прогресс студента.";
            AppLogger.Log(ex, "StudentProgressForStudent.Load");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

