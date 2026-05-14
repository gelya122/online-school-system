using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class StudentDetailsViewModel : BaseViewModel
{
    private readonly AdminStudentsService _students;
    private readonly int _studentId;

    public StudentDetailsViewModel(AdminStudentsService students, int studentId)
    {
        _students = students;
        _studentId = studentId;
        AddNoteCommand = new RelayCommand(async _ => await AddNoteAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(NewNoteText));
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
    }

    public RelayCommand AddNoteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                AddNoteCommand.RaiseCanExecuteChanged();
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    private AdminStudentDetailsDto? _details;
    public AdminStudentDetailsDto? Details
    {
        get => _details;
        private set
        {
            if (!SetProperty(ref _details, value))
                return;
            OnPropertyChanged(nameof(ProgressLessonsLine));
            OnPropertyChanged(nameof(ProgressWatchLine));
            OnPropertyChanged(nameof(StudentIdsLine));
            OnPropertyChanged(nameof(DobDisplay));
            OnPropertyChanged(nameof(ClassDisplay));
            OnPropertyChanged(nameof(ParentPhoneDisplay));
            OnPropertyChanged(nameof(ParentEmailDisplay));
            OnPropertyChanged(nameof(AvatarDisplay));
            OnPropertyChanged(nameof(RegisteredDisplay));
            OnPropertyChanged(nameof(ActiveDisplay));
        }
    }

    public ObservableCollection<AdminStudentEnrollmentDto> Enrollments { get; } = new();
    public ObservableCollection<AdminStudentPaymentDto> Payments { get; } = new();
    public ObservableCollection<AdminStudentHomeworkRowDto> Homework { get; } = new();
    public ObservableCollection<AdminStudentNoteDto> Notes { get; } = new();

    public string ProgressLessonsLine =>
        Details == null ? "" : $"Завершено уроков: {Details.Progress.CompletedLessons} из {Details.Progress.TotalLessons}";

    public string ProgressWatchLine =>
        Details == null ? "" : $"Просмотр (сек.): {Details.Progress.WatchTimeSeconds}";

    public string StudentIdsLine =>
        Details == null ? "" : $"ID: {Details.StudentId}  ·  Пользователь: {Details.UserId}";

    public string? DobDisplay =>
        Details?.DateOfBirth is { } d ? $"Дата рождения: {d:dd.MM.yyyy}" : null;

    public string ClassDisplay =>
        Details == null ? "" : $"Класс: {Details.ClassNumber}";

    public string? ParentPhoneDisplay =>
        string.IsNullOrWhiteSpace(Details?.ParentPhone) ? null : $"Телефон родителя: {Details!.ParentPhone}";

    public string? ParentEmailDisplay =>
        string.IsNullOrWhiteSpace(Details?.ParentEmail) ? null : $"Email родителя: {Details!.ParentEmail}";

    public string? AvatarDisplay =>
        string.IsNullOrWhiteSpace(Details?.AvatarUrl) ? null : $"Аватар: {Details!.AvatarUrl}";

    public string RegisteredDisplay =>
        Details?.RegisteredAt is { } r ? $"Регистрация: {r:dd.MM.yyyy HH:mm}" : "";

    public string ActiveDisplay =>
        Details == null ? "" : Details.IsActive ? "Учётная запись: активна" : "Учётная запись: отключена";

    private string _newNoteText = "";
    public string NewNoteText
    {
        get => _newNoteText;
        set
        {
            if (SetProperty(ref _newNoteText, value))
                AddNoteCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _students.GetStudentAsync(_studentId, cancellationToken);
            Details = dto;

            Replace(Enrollments, dto.Enrollments);
            Replace(Payments, dto.Payments);
            Replace(Homework, dto.Homework ?? []);
            Replace(Notes, dto.Notes);
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

    private async Task AddNoteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(NewNoteText))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            var note = await _students.AddNoteAsync(_studentId, new AdminStudentNoteCreateDto
            {
                NoteText = NewNoteText.Trim()
            }, cancellationToken);

            Notes.Insert(0, note);
            NewNoteText = "";
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

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> items)
    {
        target.Clear();
        foreach (var i in items)
            target.Add(i);
    }
}

