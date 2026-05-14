using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class InstanceDetailsViewModel : BaseViewModel
{
    private readonly AdminInstancesService _instances;
    private readonly AdminStudentsService _students;
    private readonly AdminEmployeesService _employees;
    private readonly AdminCoursesService _courses;
    private readonly AdminPaymentsService _payments;
    private readonly int _id;
    private readonly bool _viewOnly;
    private readonly Action<int>? _openEnrolledStudentCard;

    public bool ViewOnly => _viewOnly;

    /// <summary>Режим полного редактирования карточки (кнопки, поля ввода).</summary>
    public bool ShowEditChrome => !_viewOnly;

    public InstanceDetailsViewModel(
        AdminInstancesService instances,
        AdminStudentsService students,
        AdminEmployeesService employees,
        AdminCoursesService courses,
        AdminPaymentsService payments,
        int id,
        bool viewOnly = false,
        Action<int>? openEnrolledStudentCard = null)
    {
        _instances = instances;
        _students = students;
        _employees = employees;
        _courses = courses;
        _payments = payments;
        _id = id;
        _viewOnly = viewOnly;
        _openEnrolledStudentCard = openEnrolledStudentCard;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ArchiveCommand = new RelayCommand(async _ => await ArchiveAsync(), _ => !IsBusy);
        PatchStatusCommand = new RelayCommand(async _ => await PatchStatusAsync(), _ => !IsBusy);
        PatchInstanceActiveCommand = new RelayCommand(async _ => await PatchInstanceActiveAsync(), _ => !IsBusy);

        AddTeacherCommand = new RelayCommand(async _ => await AddTeacherAsync(), _ => !IsBusy);
        RemoveTeacherCommand = new RelayCommand(async _ => await RemoveTeacherAsync(), _ => !IsBusy && SelectedTeacher != null);

        AddCoordinatorCommand = new RelayCommand(async _ => await AddCoordinatorAsync(), _ => !IsBusy);
        RemoveCoordinatorCommand = new RelayCommand(async _ => await RemoveCoordinatorAsync(), _ => !IsBusy && SelectedCoordinator != null);

        RefreshStudentsCommand = new RelayCommand(async _ => await LoadStudentsAsync(), _ => !IsBusy);
        EnrollStudentCommand = new RelayCommand(async _ => await EnrollStudentAsync(), _ => !IsBusy);
        EnrollBulkCommand = new RelayCommand(async _ => await EnrollBulkAsync(), _ => !IsBusy);
        RemoveEnrollmentCommand = new RelayCommand(async _ => await RemoveEnrollmentAsync(), _ => !IsBusy && SelectedStudent != null);
        PatchEnrollmentStatusCommand = new RelayCommand(async _ => await PatchEnrollmentStatusAsync(), _ => !IsBusy && SelectedStudent != null);
        PatchEnrollmentAssignedTeacherCommand =
            new RelayCommand(async _ => await PatchEnrollmentAssignedTeacherAsync(), _ => !IsBusy && SelectedStudent != null);
        OpenStudentCommand = new RelayCommand(_ => OpenStudent(), _ => !IsBusy && SelectedStudent != null);
        OpenProgressCommand = new RelayCommand(_ => OpenProgress(), _ => !IsBusy && SelectedStudent != null);

        RefreshScheduleCommand = new RelayCommand(async _ => await LoadScheduleAsync(), _ => !IsBusy);
        GenerateScheduleCommand = new RelayCommand(async _ => await GenerateScheduleAsync(), _ => !IsBusy);
        SaveScheduleRowCommand = new RelayCommand(async _ => await SaveScheduleRowAsync(), _ => !IsBusy && SelectedSchedule != null);
        OpenLessonForAllCommand = new RelayCommand(async _ => await OpenLessonForAllAsync(), _ => !IsBusy && SelectedSchedule != null);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ArchiveCommand { get; }
    public RelayCommand PatchStatusCommand { get; }
    public RelayCommand PatchInstanceActiveCommand { get; }

    public RelayCommand AddTeacherCommand { get; }
    public RelayCommand RemoveTeacherCommand { get; }
    public RelayCommand AddCoordinatorCommand { get; }
    public RelayCommand RemoveCoordinatorCommand { get; }

    // students tab
    public RelayCommand RefreshStudentsCommand { get; }
    public RelayCommand EnrollStudentCommand { get; }
    public RelayCommand EnrollBulkCommand { get; }
    public RelayCommand RemoveEnrollmentCommand { get; }
    public RelayCommand PatchEnrollmentStatusCommand { get; }
    public RelayCommand PatchEnrollmentAssignedTeacherCommand { get; }
    public RelayCommand OpenStudentCommand { get; }
    public RelayCommand OpenProgressCommand { get; }

    // schedule tab
    public RelayCommand RefreshScheduleCommand { get; }
    public RelayCommand GenerateScheduleCommand { get; }
    public RelayCommand SaveScheduleRowCommand { get; }
    public RelayCommand OpenLessonForAllCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                ArchiveCommand.RaiseCanExecuteChanged();
                PatchStatusCommand.RaiseCanExecuteChanged();
                PatchInstanceActiveCommand.RaiseCanExecuteChanged();
                AddTeacherCommand.RaiseCanExecuteChanged();
                RemoveTeacherCommand.RaiseCanExecuteChanged();
                AddCoordinatorCommand.RaiseCanExecuteChanged();
                RemoveCoordinatorCommand.RaiseCanExecuteChanged();
                RefreshStudentsCommand.RaiseCanExecuteChanged();
                EnrollStudentCommand.RaiseCanExecuteChanged();
                EnrollBulkCommand.RaiseCanExecuteChanged();
                RemoveEnrollmentCommand.RaiseCanExecuteChanged();
                PatchEnrollmentStatusCommand.RaiseCanExecuteChanged();
                PatchEnrollmentAssignedTeacherCommand.RaiseCanExecuteChanged();
                OpenStudentCommand.RaiseCanExecuteChanged();
                OpenProgressCommand.RaiseCanExecuteChanged();
                RefreshScheduleCommand.RaiseCanExecuteChanged();
                GenerateScheduleCommand.RaiseCanExecuteChanged();
                SaveScheduleRowCommand.RaiseCanExecuteChanged();
                OpenLessonForAllCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminCourseInstanceDetailsDto? _details;
    public AdminCourseInstanceDetailsDto? Details { get => _details; private set => SetProperty(ref _details, value); }

    public ObservableCollection<AdminInstanceTeacherRowDto> Teachers { get; } = new();
    public ObservableCollection<AdminInstanceCoordinatorRowDto> Coordinators { get; } = new();
    public ObservableCollection<AdminInstanceStudentRowDto> Students { get; } = new();
    public ObservableCollection<AdminInstanceScheduleRowDto> Schedule { get; } = new();
    public ObservableCollection<AdminEmployeeListRowDto> EmployeeOptions { get; } = new();
    public ObservableCollection<IdTitleOption> InstanceTeacherPickOptions { get; } = new();

    private AdminInstanceTeacherRowDto? _selectedTeacher;
    public AdminInstanceTeacherRowDto? SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            if (SetProperty(ref _selectedTeacher, value))
                RemoveTeacherCommand.RaiseCanExecuteChanged();
        }
    }

    private AdminInstanceCoordinatorRowDto? _selectedCoordinator;
    public AdminInstanceCoordinatorRowDto? SelectedCoordinator
    {
        get => _selectedCoordinator;
        set
        {
            if (SetProperty(ref _selectedCoordinator, value))
                RemoveCoordinatorCommand.RaiseCanExecuteChanged();
        }
    }

    private AdminInstanceStudentRowDto? _selectedStudent;
    public AdminInstanceStudentRowDto? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            if (SetProperty(ref _selectedStudent, value))
            {
                SyncEnrollmentTeacherPickerFromStudent();
                RemoveEnrollmentCommand.RaiseCanExecuteChanged();
                PatchEnrollmentStatusCommand.RaiseCanExecuteChanged();
                PatchEnrollmentAssignedTeacherCommand.RaiseCanExecuteChanged();
                OpenStudentCommand.RaiseCanExecuteChanged();
                OpenProgressCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private AdminInstanceScheduleRowDto? _selectedSchedule;
    public AdminInstanceScheduleRowDto? SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            if (SetProperty(ref _selectedSchedule, value))
            {
                SaveScheduleRowCommand.RaiseCanExecuteChanged();
                OpenLessonForAllCommand.RaiseCanExecuteChanged();

                if (value != null)
                {
                    ScheduleOpenDateText = value.OpenDate.ToString("yyyy-MM-dd");
                    ScheduleOpenTimeText = value.OpenTime?.ToString("HH:mm");
                    ScheduleReleaseDayOffsetText = value.ReleaseDayOffset.ToString();
                }
            }
        }
    }

    private string _scheduleReleaseDayOffsetText = "0";
    public string ScheduleReleaseDayOffsetText
    {
        get => _scheduleReleaseDayOffsetText;
        set => SetProperty(ref _scheduleReleaseDayOffsetText, value);
    }

    private bool _instanceIsActive = true;
    public bool InstanceIsActive
    {
        get => _instanceIsActive;
        set => SetProperty(ref _instanceIsActive, value);
    }

    private int _enrollmentAssignedTeacherId;
    public int EnrollmentAssignedTeacherId
    {
        get => _enrollmentAssignedTeacherId;
        set => SetProperty(ref _enrollmentAssignedTeacherId, value);
    }

    private int _teacherEmployeeId;
    public int TeacherEmployeeId { get => _teacherEmployeeId; set => SetProperty(ref _teacherEmployeeId, value); }

    private bool _teacherIsMain;
    public bool TeacherIsMain { get => _teacherIsMain; set => SetProperty(ref _teacherIsMain, value); }

    private int _coordinatorEmployeeId;
    public int CoordinatorEmployeeId { get => _coordinatorEmployeeId; set => SetProperty(ref _coordinatorEmployeeId, value); }

    private bool _coordinatorIsLead;
    public bool CoordinatorIsLead { get => _coordinatorIsLead; set => SetProperty(ref _coordinatorIsLead, value); }

    private string _newStatus = "active";
    public string NewStatus { get => _newStatus; set => SetProperty(ref _newStatus, value); }
    public IReadOnlyList<string> StatusOptions { get; } =
    [
        "planned",
        "enrollment_open",
        "enrollment_closed",
        "active",
        "completed",
        "cancelled",
        "paused"
    ];

    private int _enrollStudentId;
    public int EnrollStudentId { get => _enrollStudentId; set => SetProperty(ref _enrollStudentId, value); }

    private string _bulkStudentIds = "";
    public string BulkStudentIds { get => _bulkStudentIds; set => SetProperty(ref _bulkStudentIds, value); }

    private string _enrollmentNewStatus = "active";
    public string EnrollmentNewStatus { get => _enrollmentNewStatus; set => SetProperty(ref _enrollmentNewStatus, value); }

    /// <summary>Статусы, допустимые для ручной смены в админке (completed выставляется только автоматически).</summary>
    public IReadOnlyList<string> EnrollmentStatusOptions { get; } = ["active", "expelled", "frozen"];

    private string _scheduleOpenDateText = DateTime.Today.ToString("yyyy-MM-dd");
    public string ScheduleOpenDateText { get => _scheduleOpenDateText; set => SetProperty(ref _scheduleOpenDateText, value); }

    private string? _scheduleOpenTimeText = "09:00";
    public string? ScheduleOpenTimeText { get => _scheduleOpenTimeText; set => SetProperty(ref _scheduleOpenTimeText, value); }

    public ObservableCollection<AdminCourseHomeworkSummaryDto> InstanceHomeworks { get; } = new();
    public ObservableCollection<AdminOrderListRowDto> InstanceOrders { get; } = new();
    public ObservableCollection<AdminPaymentListRowDto> InstancePayments { get; } = new();

    private string _scheduleSummaryLessonsPerWeek = "—";
    public string ScheduleSummaryLessonsPerWeek { get => _scheduleSummaryLessonsPerWeek; private set => SetProperty(ref _scheduleSummaryLessonsPerWeek, value); }

    private string _scheduleSummaryStartDate = "";
    public string ScheduleSummaryStartDate { get => _scheduleSummaryStartDate; private set => SetProperty(ref _scheduleSummaryStartDate, value); }

    private string _scheduleSummaryReleaseTime = "—";
    public string ScheduleSummaryReleaseTime { get => _scheduleSummaryReleaseTime; private set => SetProperty(ref _scheduleSummaryReleaseTime, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _instances.GetInstanceAsync(_id, cancellationToken);
            Details = dto;
            NewStatus = dto.Status;
            InstanceIsActive = dto.IsActive;

            try
            {
                EmployeeOptions.Clear();
                foreach (var e in await _employees.GetEmployeesAsync(null, null, cancellationToken))
                    EmployeeOptions.Add(e);
            }
            catch
            {
                // справочник сотрудников не блокирует карточку
            }

            Replace(Teachers, dto.Teachers);
            Replace(Coordinators, dto.Coordinators);

            await LoadStudentsAsync(cancellationToken);
            await LoadScheduleAsync(cancellationToken);
            await LoadInstanceHomeworksAsync(cancellationToken);
            await LoadInstanceOrdersAsync(cancellationToken);
            await LoadInstancePaymentsAsync(cancellationToken);
            RefreshScheduleSummaryTexts();
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

    public async Task LoadStudentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var prevEnrollmentId = SelectedStudent?.EnrollmentId;
            var list = await _instances.GetInstanceStudentsAsync(_id, cancellationToken);
            Replace(Students, list);
            if (prevEnrollmentId.HasValue)
                SelectedStudent = Students.FirstOrDefault(s => s.EnrollmentId == prevEnrollmentId.Value);
            SyncEnrollmentTeacherPickerFromStudent();
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
    }

    public async Task LoadScheduleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _instances.GetScheduleAsync(_id, cancellationToken);
            Replace(Schedule, list);
            RefreshScheduleSummaryTexts();
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
    }

    private async Task LoadInstanceHomeworksAsync(CancellationToken cancellationToken = default)
    {
        InstanceHomeworks.Clear();
        if (Details == null) return;
        try
        {
            var course = await _courses.GetCourseAsync(Details.CourseId, cancellationToken);
            var lessonIds = new HashSet<int>(Schedule.Select(s => s.LessonId));
            var list = lessonIds.Count == 0
                ? Enumerable.Empty<AdminCourseHomeworkSummaryDto>()
                : course.HomeworkSummaries.Where(h => lessonIds.Contains(h.LessonId));
            foreach (var h in list.OrderBy(x => x.ModuleId).ThenBy(x => x.LessonId))
                InstanceHomeworks.Add(h);
        }
        catch
        {
            /* вкладка не блокирует карточку */
        }
    }

    private async Task LoadInstanceOrdersAsync(CancellationToken cancellationToken = default)
    {
        InstanceOrders.Clear();
        try
        {
            var all = await _payments.GetOrdersAsync(null, cancellationToken);
            foreach (var o in all.Where(x => x.InstanceId == _id).OrderByDescending(x => x.CreatedAt))
                InstanceOrders.Add(o);
        }
        catch
        {
            /* ignore */
        }
    }

    private async Task LoadInstancePaymentsAsync(CancellationToken cancellationToken = default)
    {
        InstancePayments.Clear();
        if (InstanceOrders.Count == 0) return;
        var orderIds = new HashSet<int>(InstanceOrders.Select(o => o.OrderId));
        try
        {
            var all = await _payments.GetPaymentsAsync(cancellationToken);
            foreach (var p in all.Where(x => orderIds.Contains(x.OrderId)).OrderByDescending(x => x.CreatedAt))
                InstancePayments.Add(p);
        }
        catch
        {
            /* ignore */
        }
    }

    private void RefreshScheduleSummaryTexts()
    {
        if (Details == null)
        {
            ScheduleSummaryLessonsPerWeek = "—";
            ScheduleSummaryStartDate = "";
            ScheduleSummaryReleaseTime = "—";
            return;
        }

        var lpw = Details.LessonsPerWeek;
        if ((lpw == null || lpw <= 0) && !string.IsNullOrWhiteSpace(Details.ScheduleRulesJson))
        {
            var n = TryCountScheduleRules(Details.ScheduleRulesJson);
            if (n is > 0)
                lpw = n;
        }

        ScheduleSummaryLessonsPerWeek = lpw is > 0 ? lpw.Value.ToString() : "—";
        ScheduleSummaryStartDate = Details.StartDate.ToString("dd.MM.yyyy");

        if (Schedule.Count > 0)
        {
            var r = Schedule.OrderBy(x => x.LessonOrder).First();
            if (r.OpenTime.HasValue)
                ScheduleSummaryReleaseTime = r.OpenTime.Value.ToString("HH:mm");
            else if (r.ScheduledAt.HasValue)
                ScheduleSummaryReleaseTime = r.ScheduledAt.Value.ToLocalTime().ToString("HH:mm");
            else
                ScheduleSummaryReleaseTime = "—";
        }
        else
            ScheduleSummaryReleaseTime = "—";
    }

    private static int? TryCountScheduleRules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json.Trim());
            return doc.RootElement.ValueKind == JsonValueKind.Array ? doc.RootElement.GetArrayLength() : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task ArchiveAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (!UserDialogs.Confirm("Архивировать поток?", "Поток"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.ArchiveAsync(_id, cancellationToken);
            await LoadAsync(cancellationToken);
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

    private async Task PatchStatusAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchStatusAsync(_id, new AdminInstanceStatusPatchDto { Status = NewStatus }, cancellationToken);
            await LoadAsync(cancellationToken);
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

    private async Task PatchInstanceActiveAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchInstanceActiveAsync(_id, new AdminInstanceIsActivePatchDto { IsActive = InstanceIsActive },
                cancellationToken);
            await LoadAsync(cancellationToken);
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

    private void SyncEnrollmentTeacherPickerFromStudent()
    {
        InstanceTeacherPickOptions.Clear();
        InstanceTeacherPickOptions.Add(new IdTitleOption(0, "— не назначен"));
        foreach (var m in Coordinators)
            InstanceTeacherPickOptions.Add(new IdTitleOption(m.EmployeeId, m.FullName.Trim()));

        if (SelectedStudent?.AssignedTeacherId is > 0 &&
            InstanceTeacherPickOptions.All(o => o.Id != SelectedStudent.AssignedTeacherId))
        {
            InstanceTeacherPickOptions.Add(new IdTitleOption(
                SelectedStudent.AssignedTeacherId.Value,
                string.IsNullOrWhiteSpace(SelectedStudent.AssignedTeacherName)
                    ? $"#{SelectedStudent.AssignedTeacherId}"
                    : SelectedStudent.AssignedTeacherName!));
        }

        EnrollmentAssignedTeacherId = SelectedStudent?.AssignedTeacherId ?? 0;
        OnPropertyChanged(nameof(EnrollmentAssignedTeacherId));
    }

    private async Task PatchEnrollmentAssignedTeacherAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedStudent == null) return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchEnrollmentAssignedTeacherAsync(SelectedStudent.EnrollmentId,
                new AdminEnrollmentAssignedTeacherPatchDto
                {
                    AssignedTeacherId = EnrollmentAssignedTeacherId <= 0 ? null : EnrollmentAssignedTeacherId
                },
                cancellationToken);
            await LoadStudentsAsync(cancellationToken);
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

    private async Task AddTeacherAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (TeacherEmployeeId <= 0)
        {
            UserDialogs.Warning("Укажите employeeId преподавателя.", "Преподаватели");
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.AddTeacherAsync(_id, new AdminAssignTeacherDto { EmployeeId = TeacherEmployeeId, IsMainTeacher = TeacherIsMain },
                cancellationToken);
            TeacherEmployeeId = 0;
            TeacherIsMain = false;
            await LoadAsync(cancellationToken);
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

    private async Task RemoveTeacherAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedTeacher == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _instances.RemoveTeacherAsync(_id, SelectedTeacher.EmployeeId, cancellationToken);
            await LoadAsync(cancellationToken);
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

    private async Task AddCoordinatorAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (CoordinatorEmployeeId <= 0)
        {
            UserDialogs.Warning("Укажите employeeId наставника.", "Наставники");
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.AddCoordinatorAsync(_id,
                new AdminAssignCoordinatorDto { EmployeeId = CoordinatorEmployeeId, IsLead = CoordinatorIsLead },
                cancellationToken);
            CoordinatorEmployeeId = 0;
            CoordinatorIsLead = false;
            await LoadAsync(cancellationToken);
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

    private async Task RemoveCoordinatorAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedCoordinator == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _instances.RemoveCoordinatorAsync(_id, SelectedCoordinator.EmployeeId, cancellationToken);
            await LoadAsync(cancellationToken);
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

    private async Task EnrollStudentAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (EnrollStudentId <= 0)
        {
            UserDialogs.Warning("Укажите studentId.", "Студенты");
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.EnrollStudentAsync(_id, EnrollStudentId, cancellationToken);
            EnrollStudentId = 0;
            await LoadStudentsAsync(cancellationToken);
            await LoadAsync(cancellationToken);
            if (Coordinators.Count == 0)
                UserDialogs.Warning("На поток не назначен наставник.", "Студенты");
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

    private async Task EnrollBulkAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        var raw = (BulkStudentIds ?? "").Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            UserDialogs.Warning("Укажите список studentId через запятую.", "Студенты");
            return;
        }

        var ids = new List<int>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var id) && id > 0)
                ids.Add(id);
        }

        ids = ids.Distinct().ToList();
        if (ids.Count == 0)
        {
            UserDialogs.Warning("Не удалось распознать studentId.", "Студенты");
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.EnrollStudentsBulkAsync(_id, ids, cancellationToken);
            BulkStudentIds = "";
            await LoadStudentsAsync(cancellationToken);
            await LoadAsync(cancellationToken);
            if (Coordinators.Count == 0)
                UserDialogs.Warning("На поток не назначен наставник.", "Студенты");
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

    private async Task RemoveEnrollmentAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedStudent == null) return;

        if (!UserDialogs.Confirm("Удалить запись студента из потока?", "Студенты"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.DeleteEnrollmentAsync(SelectedStudent.EnrollmentId, cancellationToken);
            await LoadStudentsAsync(cancellationToken);
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

    private async Task PatchEnrollmentStatusAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedStudent == null) return;

        var st = (EnrollmentNewStatus ?? "").Trim();
        if (string.IsNullOrWhiteSpace(st))
        {
            UserDialogs.Warning("Укажите статус.", "Студенты");
            return;
        }

        if (string.Equals(st, "completed", StringComparison.OrdinalIgnoreCase))
        {
            UserDialogs.Warning("Статус «completed» нельзя выбрать вручную.", "Студенты");
            return;
        }

        if (string.Equals(st, SelectedStudent.Status?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            UserDialogs.Warning("Новый статус совпадает с текущим.", "Студенты");
            return;
        }

        if (!UserDialogs.Confirm("Вы действительно хотите изменить статус записи студента?", "Студенты"))
            return;

        var reason = UserDialogs.PromptMultiline(
            "Укажите причину смены статуса (обязательно, не короче 3 символов).",
            "Смена статуса записи");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 3)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.PatchEnrollmentStatusAsync(SelectedStudent.EnrollmentId,
                new AdminEnrollmentStatusPatchDto { Status = st, Reason = reason.Trim() }, cancellationToken);
            await LoadStudentsAsync(cancellationToken);
            UserDialogs.Info("Статус изменён.", "Студенты");
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

    private void OpenStudent()
    {
        if (SelectedStudent == null) return;
        if (ViewOnly && _openEnrolledStudentCard != null)
        {
            _openEnrolledStudentCard(SelectedStudent.StudentId);
            return;
        }

        var details = new StudentDetailsViewModel(_students, SelectedStudent.StudentId);
        var shell = Application.Current?.MainWindow?.DataContext as MainShellViewModel;
        shell?.Navigation.Navigate(details);
        _ = details.LoadAsync();
    }

    private void OpenProgress()
    {
        if (SelectedStudent == null) return;
        UserDialogs.Info("Откройте раздел «Прогресс студентов» для детализации по записи.", "Прогресс");
    }

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> items)
    {
        target.Clear();
        foreach (var i in items)
            target.Add(i);
    }

    private async Task GenerateScheduleAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (Schedule.Count > 0 &&
            !UserDialogs.Confirm(
                "Расписание уже существует. Перегенерировать? Существующие даты в плане будут заменены (прогресс студентов по урокам не удаляется).",
                "Расписание"))
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.GenerateScheduleAsync(_id, new AdminGenerateInstanceScheduleDto { OverwriteExisting = true }, cancellationToken);
            await LoadScheduleAsync(cancellationToken);
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

    private async Task SaveScheduleRowAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedSchedule == null) return;

        int? releaseOffset = null;
        if (!string.IsNullOrWhiteSpace(ScheduleReleaseDayOffsetText) &&
            int.TryParse(ScheduleReleaseDayOffsetText.Trim(), out var ro) && ro >= 0)
            releaseOffset = ro;

        DateOnly? openDate = null;
        if (releaseOffset == null)
        {
            if (!DateOnly.TryParse(ScheduleOpenDateText?.Trim(), out var od))
            {
                UserDialogs.Warning("Укажите смещение дней (release_day_offset) или дату открытия yyyy-MM-dd.", "Расписание");
                return;
            }
            openDate = od;
        }

        TimeOnly? openTime = null;
        if (!string.IsNullOrWhiteSpace(ScheduleOpenTimeText))
        {
            if (!TimeOnly.TryParse(ScheduleOpenTimeText.Trim(), out var parsed))
            {
                UserDialogs.Warning("Некорректное время. Формат HH:mm.", "Расписание");
                return;
            }
            openTime = parsed;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _instances.UpdateScheduleAsync(SelectedSchedule.ScheduleId, new AdminUpdateInstanceScheduleDto
            {
                ReleaseDayOffset = releaseOffset,
                OpenDate = openDate,
                OpenTime = openTime,
                AutoOpen = true
            }, cancellationToken);
            await LoadScheduleAsync(cancellationToken);
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

    private async Task OpenLessonForAllAsync(CancellationToken cancellationToken = default)
    {
        if (ViewOnly) return;
        if (SelectedSchedule == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _instances.OpenLessonForAllAsync(SelectedSchedule.ScheduleId, cancellationToken);
            await LoadScheduleAsync(cancellationToken);
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

