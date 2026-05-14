using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class DayOfWeekOption
{
    public int Value { get; init; }
    public string Name { get; init; } = "";
}

public sealed class WeeklyScheduleRuleRow : BaseViewModel
{
    private int _dayOfWeek = 1;
    public int DayOfWeek
    {
        get => _dayOfWeek;
        set
        {
            var v = value is >= 1 and <= 7 ? value : 1;
            SetProperty(ref _dayOfWeek, v);
        }
    }

    private string _time = "00:00";
    public string Time { get => _time; set => SetProperty(ref _time, value); }
}

public sealed class SchedulePreviewRow
{
    public int Ordinal { get; set; }
    public string LessonTitle { get; set; } = "";
    public string CurrentDateSummary { get; set; } = "";
    public string NewDateSummary { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed class InstanceEditViewModel : BaseViewModel
{
    public const int EmployeeRoleTeacher = 8;
    public const int EmployeeRoleMentor = 6;

    private readonly AdminInstancesService _instances;
    private readonly AdminCoursesService _courses;
    private readonly AdminEmployeesService _employees;
    private readonly int? _instanceId;

    private string? _loadedScheduleRulesJson;
    private int _loadedTeacherId;
    private List<int> _loadedMentorIds = [];
    private List<AdminInstanceScheduleRowDto> _existingScheduleOrdered = [];
    private bool _hadExistingSchedule;

    public static IReadOnlyList<DayOfWeekOption> WeekdayOptions { get; } =
    [
        new DayOfWeekOption { Value = 1, Name = "Понедельник" },
        new DayOfWeekOption { Value = 2, Name = "Вторник" },
        new DayOfWeekOption { Value = 3, Name = "Среда" },
        new DayOfWeekOption { Value = 4, Name = "Четверг" },
        new DayOfWeekOption { Value = 5, Name = "Пятница" },
        new DayOfWeekOption { Value = 6, Name = "Суббота" },
        new DayOfWeekOption { Value = 7, Name = "Воскресенье" }
    ];

    public InstanceEditViewModel(AdminInstancesService instances, AdminCoursesService courses, AdminEmployeesService employees, int? instanceId)
    {
        _instances = instances;
        _courses = courses;
        _employees = employees;
        _instanceId = instanceId;

        foreach (var tz in DefaultTimezones)
            TimezoneOptions.Add(tz);

        SaveCommand = new RelayCommand(async _ => await SaveOrWizardAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
        PreviewScheduleCommand = new RelayCommand(async _ => await BuildPreviewAsync(), _ => !IsBusy);
        AddScheduleRuleCommand = new RelayCommand(_ => AddScheduleRule(), _ => !IsBusy);
        RemoveScheduleRuleCommand = new RelayCommand(RemoveScheduleRule, _ => !IsBusy);
        AddMentorCommand = new RelayCommand(_ => AddMentor(), _ => !IsBusy && MentorToAddId > 0);
        RemoveMentorCommand = new RelayCommand(_ => RemoveSelectedMentor(), _ => !IsBusy && SelectedMentor != null);
    }

    public bool IsEditMode => _instanceId.HasValue;
    public bool IsCreateMode => !_instanceId.HasValue;

    public string BreadcrumbText => IsCreateMode ? "Потоки → Новый поток" : "Потоки → Редактирование потока";
    public string EditorTitle => IsCreateMode ? "Новый поток" : "Редактирование потока";

    public string PrimaryActionText =>
        IsEditMode
            ? "Сохранить изменения"
            : _editorTabIndex switch
            {
                0 => "Далее: расписание",
                1 => "Далее: сотрудники",
                _ => "Создать поток"
            };

    private static readonly string[] DefaultTimezones =
    [
        "UTC", "Europe/Kaliningrad", "Europe/Moscow", "Europe/Samara", "Asia/Yekaterinburg", "Asia/Omsk",
        "Asia/Krasnoyarsk", "Asia/Irkutsk", "Asia/Yakutsk", "Asia/Vladivostok", "Asia/Magadan", "Asia/Kamchatka",
        "Europe/London", "Europe/Berlin", "America/New_York"
    ];

    public ObservableCollection<string> TimezoneOptions { get; } = new();

    public event Action? Saved;
    public event Action? CancelRequested;

    public int? LastCreatedInstanceId { get; private set; }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PreviewScheduleCommand { get; }
    public RelayCommand AddScheduleRuleCommand { get; }
    public RelayCommand RemoveScheduleRuleCommand { get; }
    public RelayCommand AddMentorCommand { get; }
    public RelayCommand RemoveMentorCommand { get; }

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
                PreviewScheduleCommand.RaiseCanExecuteChanged();
                AddScheduleRuleCommand.RaiseCanExecuteChanged();
                RemoveScheduleRuleCommand.RaiseCanExecuteChanged();
                AddMentorCommand.RaiseCanExecuteChanged();
                RemoveMentorCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private int _editorTabIndex;
    public int EditorTabIndex
    {
        get => _editorTabIndex;
        set
        {
            if (SetProperty(ref _editorTabIndex, value))
                OnPropertyChanged(nameof(PrimaryActionText));
        }
    }

    public ObservableCollection<IdTitleOption> CourseOptions { get; } = new();

    private IdTitleOption? _selectedCourse;
    public IdTitleOption? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (SetProperty(ref _selectedCourse, value))
                _ = OnSelectedCourseChangedAsync();
        }
    }

    private AdminCourseDetailsDto? _courseInfo;
    public AdminCourseDetailsDto? CourseInfo { get => _courseInfo; private set => SetProperty(ref _courseInfo, value); }

    private string _courseSummaryTitle = "—";
    public string CourseSummaryTitle { get => _courseSummaryTitle; private set => SetProperty(ref _courseSummaryTitle, value); }

    private string _courseSummaryLine = "";
    public string CourseSummaryLine { get => _courseSummaryLine; private set => SetProperty(ref _courseSummaryLine, value); }

    private string _title = "";
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private string? _description;
    public string? Description { get => _description; set => SetProperty(ref _description, value); }

    private string _startDateText = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
    public string StartDateText { get => _startDateText; set { if (SetProperty(ref _startDateText, value)) RecalcWeeksAndMaybeTitle(); } }

    private string? _endDateText;
    public string? EndDateText { get => _endDateText; set { if (SetProperty(ref _endDateText, value)) RecalcWeeksAndMaybeTitle(); } }

    private string? _enrollmentStartDateText;
    public string? EnrollmentStartDateText { get => _enrollmentStartDateText; set => SetProperty(ref _enrollmentStartDateText, value); }

    private string? _enrollmentEndDateText;
    public string? EnrollmentEndDateText { get => _enrollmentEndDateText; set => SetProperty(ref _enrollmentEndDateText, value); }

    private int? _maxStudents = 30;
    public int? MaxStudents { get => _maxStudents; set => SetProperty(ref _maxStudents, value); }

    private int? _lessonsPerWeek;
    public int? LessonsPerWeek { get => _lessonsPerWeek; set => SetProperty(ref _lessonsPerWeek, value); }

    private int? _totalWeeks;
    public int? TotalWeeks { get => _totalWeeks; set => SetProperty(ref _totalWeeks, value); }

    private string? _timezone = "Europe/Moscow";
    public string? Timezone { get => _timezone; set => SetProperty(ref _timezone, value); }

    private string _status = "planned";
    public string Status { get => _status; set => SetProperty(ref _status, value); }
    public IReadOnlyList<string> StatusOptions { get; } =
    [
        "planned", "enrollment_open", "enrollment_closed", "active", "completed", "cancelled", "paused"
    ];

    private bool _isActive;
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    public ObservableCollection<WeeklyScheduleRuleRow> ScheduleRules { get; } = new();
    public ObservableCollection<SchedulePreviewRow> PreviewRows { get; } = new();

    public ObservableCollection<AdminEmployeeListRowDto> TeacherOptions { get; } = new();
    public ObservableCollection<AdminEmployeeListRowDto> MentorOptions { get; } = new();
    public ObservableCollection<AdminEmployeeListRowDto> SelectedMentors { get; } = new();

    private int _teacherEmployeeId;
    public int TeacherEmployeeId { get => _teacherEmployeeId; set => SetProperty(ref _teacherEmployeeId, value); }

    private int _mentorToAddId;
    public int MentorToAddId
    {
        get => _mentorToAddId;
        set
        {
            if (!SetProperty(ref _mentorToAddId, value))
                return;
            AddMentorCommand.RaiseCanExecuteChanged();
        }
    }

    private AdminEmployeeListRowDto? _selectedMentor;
    public AdminEmployeeListRowDto? SelectedMentor
    {
        get => _selectedMentor;
        set
        {
            if (SetProperty(ref _selectedMentor, value))
                RemoveMentorCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusReadOnlyLabel => "Запланирован (planned) — задаётся автоматически при создании.";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _courses.GetCoursesAsync(null, null, null, null, null, cancellationToken);
        CourseOptions.Clear();
        foreach (var c in courses.OrderBy(x => x.Title))
            CourseOptions.Add(new IdTitleOption(c.CourseId, c.Title));

        SelectedCourse = CourseOptions.FirstOrDefault();

        try
        {
            var teachers = await _employees.GetEmployeesAsync(null, EmployeeRoleTeacher, cancellationToken);
            TeacherOptions.Clear();
            foreach (var e in teachers.OrderBy(x => x.FullName))
                TeacherOptions.Add(e);
        }
        catch (ApiException)
        {
            /* ignore */
        }

        try
        {
            var mentors = await _employees.GetEmployeesAsync(null, EmployeeRoleMentor, cancellationToken);
            MentorOptions.Clear();
            foreach (var e in mentors.OrderBy(x => x.FullName))
                MentorOptions.Add(e);
        }
        catch (ApiException)
        {
            /* ignore */
        }

        if (IsCreateMode)
        {
            ClearScheduleRules();
            var r1 = new WeeklyScheduleRuleRow { DayOfWeek = 1, Time = "00:00" };
            var r2 = new WeeklyScheduleRuleRow { DayOfWeek = 3, Time = "00:00" };
            AttachRuleWatcher(r1);
            AttachRuleWatcher(r2);
            ScheduleRules.Add(r1);
            ScheduleRules.Add(r2);
            _existingScheduleOrdered = [];
            _hadExistingSchedule = false;
        }

        RefreshScheduleDescriptionFromRules();
    }

    public void PreselectCourse(int courseId) =>
        SelectedCourse = CourseOptions.FirstOrDefault(x => x.Id == courseId) ?? SelectedCourse;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_instanceId == null) return;

        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _instances.GetInstanceAsync(_instanceId.Value, cancellationToken);
            Title = dto.Title;
            Description = dto.Description;
            StartDateText = dto.StartDate.ToString("yyyy-MM-dd");
            EndDateText = dto.EndDate?.ToString("yyyy-MM-dd");
            EnrollmentStartDateText = dto.EnrollmentStartDate?.ToString("yyyy-MM-dd");
            EnrollmentEndDateText = dto.EnrollmentEndDate?.ToString("yyyy-MM-dd");
            MaxStudents = dto.MaxStudents;
            LessonsPerWeek = dto.LessonsPerWeek;
            TotalWeeks = dto.TotalWeeks;
            Timezone = dto.Timezone;
            Status = dto.Status;
            IsActive = dto.IsActive;

            SelectedCourse = CourseOptions.FirstOrDefault(x => x.Id == dto.CourseId) ?? CourseOptions.FirstOrDefault();
            await OnSelectedCourseChangedAsync(cancellationToken);

            ClearScheduleRules();
            if (!string.IsNullOrWhiteSpace(dto.ScheduleRulesJson))
                TryParseRulesIntoCollection(dto.ScheduleRulesJson);
            if (ScheduleRules.Count == 0)
            {
                var fallback = new WeeklyScheduleRuleRow { DayOfWeek = 1, Time = "00:00" };
                AttachRuleWatcher(fallback);
                ScheduleRules.Add(fallback);
            }

            _loadedScheduleRulesJson = dto.ScheduleRulesJson;

            _existingScheduleOrdered = [];
            _hadExistingSchedule = false;
            try
            {
                var sch = await _instances.GetScheduleAsync(_instanceId.Value, cancellationToken);
                _existingScheduleOrdered = sch.OrderBy(x => x.ModuleOrder).ThenBy(x => x.LessonOrder).ToList();
                _hadExistingSchedule = sch.Count > 0;
            }
            catch (ApiException)
            {
                /* ignore */
            }
            catch (HttpRequestException)
            {
                /* ignore */
            }

            TeacherEmployeeId = dto.Teachers.FirstOrDefault()?.EmployeeId ?? 0;
            _loadedTeacherId = TeacherEmployeeId;

            SelectedMentors.Clear();
            foreach (var c in dto.Coordinators)
            {
                var row = MentorOptions.FirstOrDefault(x => x.EmployeeId == c.EmployeeId);
                if (row != null)
                    SelectedMentors.Add(row);
                else
                    SelectedMentors.Add(new AdminEmployeeListRowDto { EmployeeId = c.EmployeeId, FullName = c.FullName.Trim() });
            }

            _loadedMentorIds = SelectedMentors.Select(x => x.EmployeeId).ToList();

            EnsureTimezoneInOptions();
            RefreshScheduleDescriptionFromRules();
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

    private async Task OnSelectedCourseChangedAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedCourse == null)
        {
            CourseInfo = null;
            CourseSummaryTitle = "—";
            CourseSummaryLine = "";
            return;
        }

        try
        {
            CourseInfo = await _courses.GetCourseAsync(SelectedCourse.Id, cancellationToken);
        }
        catch
        {
            CourseInfo = null;
        }

        if (CourseInfo == null)
        {
            CourseSummaryTitle = "—";
            CourseSummaryLine = "";
        }
        else
        {
            CourseSummaryTitle = CourseInfo.Title;
            CourseSummaryLine =
                $"Блоков: {CourseInfo.Modules.Count}  ·  Уроков: {CourseInfo.Lessons.Count}  ·  Цена: {CourseInfo.Price}  ·  Активен: {CourseInfo.IsActive}";
        }

        TryAutoTitle();
        OnPropertyChanged(nameof(CourseInfo));
    }

    private void TryAutoTitle()
    {
        if (!IsCreateMode || SelectedCourse == null || CourseInfo == null)
            return;
        if (!TryParseDate(StartDateText, out var sd, out _))
            return;
        var label = !string.IsNullOrWhiteSpace(CourseInfo.ExamName)
            ? CourseInfo.ExamName
            : (!string.IsNullOrWhiteSpace(CourseInfo.SubjectName) ? CourseInfo.SubjectName : CourseInfo.Title);
        if (string.IsNullOrWhiteSpace(label))
            return;
        Title = $"{label} — {MonthName(sd.Month)} {sd.Year}";
    }

    private static string MonthName(int m) => m switch
    {
        1 => "Январь", 2 => "Февраль", 3 => "Март", 4 => "Апрель", 5 => "Май", 6 => "Июнь",
        7 => "Июль", 8 => "Август", 9 => "Сентябрь", 10 => "Октябрь", 11 => "Ноябрь", 12 => "Декабрь",
        _ => m.ToString()
    };

    private void RecalcWeeksAndMaybeTitle()
    {
        if (TryParseDate(StartDateText, out var s, out _) && TryParseNullableDate(EndDateText, out var e, out _) && e.HasValue && e.Value >= s)
        {
            var days = e.Value.DayNumber - s.DayNumber + 1;
            TotalWeeks = Math.Max(1, (int)Math.Ceiling(days / 7.0));
        }

        TryAutoTitle();
    }

    private void ClearScheduleRules()
    {
        foreach (var row in ScheduleRules.ToList())
            DetachRuleWatcher(row);
        ScheduleRules.Clear();
    }

    private void AttachRuleWatcher(WeeklyScheduleRuleRow row)
    {
        row.PropertyChanged -= OnScheduleRulePropertyChanged;
        row.PropertyChanged += OnScheduleRulePropertyChanged;
    }

    private void DetachRuleWatcher(WeeklyScheduleRuleRow row)
        => row.PropertyChanged -= OnScheduleRulePropertyChanged;

    private void OnScheduleRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WeeklyScheduleRuleRow.DayOfWeek) or nameof(WeeklyScheduleRuleRow.Time))
            RefreshScheduleDescriptionFromRules();
    }

    private void AddScheduleRule()
    {
        var row = new WeeklyScheduleRuleRow { DayOfWeek = 1, Time = "00:00" };
        AttachRuleWatcher(row);
        ScheduleRules.Add(row);
        RefreshScheduleDescriptionFromRules();
    }

    private void RemoveScheduleRule(object? parameter)
    {
        if (parameter is not WeeklyScheduleRuleRow row)
            return;

        if (ScheduleRules.Count <= 1)
        {
            MessageBox.Show(
                "У потока должно быть хотя бы одно правило расписания.",
                "Расписание",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        DetachRuleWatcher(row);
        ScheduleRules.Remove(row);
        RefreshScheduleDescriptionFromRules();
    }

    private void AddMentor()
    {
        var row = MentorOptions.FirstOrDefault(x => x.EmployeeId == MentorToAddId);
        if (row == null) return;
        if (SelectedMentors.Any(x => x.EmployeeId == row.EmployeeId))
            return;
        SelectedMentors.Add(row);
    }

    private void RemoveSelectedMentor()
    {
        if (SelectedMentor != null)
            SelectedMentors.Remove(SelectedMentor);
    }

    private void RefreshScheduleDescriptionFromRules()
    {
        if (ScheduleRules.Count == 0)
        {
            Description = "";
            return;
        }

        var groups = ScheduleRules
            .Where(r => TryParseRuleTime(r.Time, out _))
            .GroupBy(r => NormalizeTimeKey(r.Time))
            .OrderBy(g => g.Key);

        var parts = new List<string>();
        foreach (var g in groups)
        {
            var days = g.Select(x => x.DayOfWeek).Distinct().OrderBy(d => d).ToList();
            if (days.Count == 0) continue;
            var names = days.Select(DayShort).ToList();
            var dayList = names.Count switch
            {
                1 => names[0],
                2 => $"{names[0]} и {names[1]}",
                _ => string.Join(", ", names)
            };
            parts.Add($"{dayList} в {g.Key}");
        }

        Description = parts.Count > 0 ? string.Join("; ", parts) : "";
    }

    private static string NormalizeTimeKey(string? time)
    {
        if (!TryParseRuleTime(time, out var t)) return (time ?? "").Trim();
        return t.ToString("HH:mm");
    }

    private static string DayShort(int d) => d switch
    {
        1 => "Пн", 2 => "Вт", 3 => "Ср", 4 => "Чт", 5 => "Пт", 6 => "Сб", 7 => "Вс",
        _ => $"Д{d}"
    };

    private static bool TryParseRuleTime(string? s, out TimeOnly t)
    {
        t = default;
        if (string.IsNullOrWhiteSpace(s)) return false;
        return TimeOnly.TryParse(s.Trim(), out t);
    }

    private bool TryValidateScheduleRules(out string message)
    {
        message = "";
        if (ScheduleRules.Count == 0)
        {
            message = "Добавьте хотя бы одно правило расписания.";
            return false;
        }

        foreach (var r in ScheduleRules)
        {
            if (r.DayOfWeek is < 1 or > 7)
            {
                message = "Выберите корректный день недели.";
                return false;
            }

            if (!TryParseRuleTime(r.Time, out _))
            {
                message = $"Некорректное время «{(r.Time ?? "").Trim()}». Используйте формат HH:mm (например, 18:00).";
                return false;
            }
        }

        return true;
    }

    private static List<AdminCourseLessonDto> GetOrderedLessons(AdminCourseDetailsDto course)
    {
        var moduleOrder = course.Modules.ToDictionary(m => m.ModuleId, m => m.ModuleOrder);
        return course.Lessons
            .OrderBy(l => moduleOrder.TryGetValue(l.ModuleId, out var mo) ? mo : 0)
            .ThenBy(l => l.LessonOrder)
            .ToList();
    }

    private static DateTime GetExistingPlanUtc(AdminInstanceScheduleRowDto row)
    {
        if (row.ScheduledAt.HasValue)
            return DateTime.SpecifyKind(row.ScheduledAt.Value, DateTimeKind.Utc);
        if (row.OpenTime.HasValue)
            return DateTime.SpecifyKind(row.OpenDate.ToDateTime(row.OpenTime.Value), DateTimeKind.Utc);
        // Строка плана есть, но без времени — показываем полночь по дате открытия (ориентир для предпросмотра).
        return DateTime.SpecifyKind(row.OpenDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    }

    private static string FormatScheduleLocal(DateTime utc)
    {
        try
        {
            return utc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        }
        catch
        {
            return utc.ToString("dd.MM.yyyy HH:mm");
        }
    }

    /// <summary>Время из правил (календарная дата + HH:mm) без конвертации в UTC — иначе 18:00 превращалось в 21:00 при ToLocalTime().</summary>
    private static string FormatPreviewRuleSlot(DateTime wallClockUnspecified)
    {
        var d = wallClockUnspecified.Kind == DateTimeKind.Unspecified
            ? wallClockUnspecified
            : DateTime.SpecifyKind(wallClockUnspecified, DateTimeKind.Unspecified);
        return d.ToString("dd.MM.yyyy HH:mm");
    }

    /// <summary>Сравнение момента из БД (UTC) с «настенным» временем из правил в локальной зоне ОС (как в колонке «Текущая дата»).</summary>
    private static bool SameOpenInstant(DateTime currentUtcFromPlan, DateTime ruleWallUnspecified)
    {
        var curLocal = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(currentUtcFromPlan.ToUniversalTime(), DateTimeKind.Utc),
            TimeZoneInfo.Local);
        var rule = DateTime.SpecifyKind(ruleWallUnspecified, DateTimeKind.Unspecified);
        var a = new DateTime(curLocal.Year, curLocal.Month, curLocal.Day, curLocal.Hour, curLocal.Minute, 0, DateTimeKind.Unspecified);
        var b = new DateTime(rule.Year, rule.Month, rule.Day, rule.Hour, rule.Minute, 0, DateTimeKind.Unspecified);
        return Math.Abs((a - b).TotalMinutes) < 1.5;
    }

    /// <summary>Предпросмотр: для каждого урока курса по порядку — слот из правил; «текущее» — из сохранённого плана потока в БД.</summary>
    private async Task BuildPreviewAsync()
    {
        if (IsEditMode && _instanceId is int instanceId)
        {
            try
            {
                var sch = await _instances.GetScheduleAsync(instanceId);
                _existingScheduleOrdered = sch.OrderBy(x => x.ModuleOrder).ThenBy(x => x.LessonOrder).ToList();
                _hadExistingSchedule = sch.Count > 0;
            }
            catch
            {
                /* оставляем данные последней LoadAsync */
            }
        }

        BuildPreviewCore();
    }

    private void BuildPreviewCore()
    {
        PreviewRows.Clear();
        if (SelectedCourse == null || CourseInfo == null)
        {
            MessageBox.Show("Выберите курс.", "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!TryValidateScheduleRules(out var valErr))
        {
            MessageBox.Show(valErr, "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!TryParseDate(StartDateText, out var start, out var err))
        {
            MessageBox.Show(err, "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!TryParseNullableDate(EndDateText, out var end, out var err2) || !end.HasValue)
        {
            MessageBox.Show(err2 ?? "Укажите дату окончания.", "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        RefreshScheduleDescriptionFromRules();
        var json = SerializeScheduleRulesJson();
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            MessageBox.Show("Добавьте правила расписания.", "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var lessons = GetOrderedLessons(CourseInfo);
        if (lessons.Count == 0)
        {
            MessageBox.Show("В курсе нет уроков.", "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!TryBuildSlotDates(start, end.Value, json, out var slots, out var parseErr))
        {
            MessageBox.Show(parseErr ?? "Ошибка правил.", "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (slots.Count < lessons.Count)
        {
            MessageBox.Show(
                $"Слотов в периоде: {slots.Count}, уроков: {lessons.Count}. Расширьте даты потока или добавьте правила.",
                "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        for (var i = 0; i < lessons.Count; i++)
        {
            var lesson = lessons[i];
            var curRow = _existingScheduleOrdered.FirstOrDefault(x => x.LessonId == lesson.LessonId);
            var ruleSlot = slots[i];
            var newSummary = FormatPreviewRuleSlot(ruleSlot);
            string curSummary;
            string status;
            if (curRow == null)
            {
                curSummary = "—";
                status = "Нет строки в плане";
            }
            else
            {
                var curUtc = GetExistingPlanUtc(curRow);
                curSummary = FormatScheduleLocal(curUtc);
                status = SameOpenInstant(curUtc, ruleSlot) ? "Совпадает с планом" : "Дата изменится";
            }

            PreviewRows.Add(new SchedulePreviewRow
            {
                Ordinal = i + 1,
                LessonTitle = lesson.Title,
                CurrentDateSummary = curSummary,
                NewDateSummary = newSummary,
                Status = status
            });
        }
    }

    private sealed class JsonRule
    {
        [JsonPropertyName("dayOfWeek")]
        public int DayOfWeek { get; set; }
        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }

    private string SerializeScheduleRulesJson()
    {
        var list = ScheduleRules.Select(r =>
        {
            TryParseRuleTime(r.Time, out var t);
            return new JsonRule { DayOfWeek = r.DayOfWeek, Time = t.ToString("HH:mm") };
        }).ToList();
        return JsonSerializer.Serialize(list);
    }

    private string BuildScheduleRulesJson()
    {
        RefreshScheduleDescriptionFromRules();
        return SerializeScheduleRulesJson();
    }

    private void TryParseRulesIntoCollection(string json)
    {
        try
        {
            var items = JsonSerializer.Deserialize<List<JsonRule>>(json.Trim(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (items == null) return;
            foreach (var it in items)
            {
                if (it.DayOfWeek is < 1 or > 7) continue;
                if (string.IsNullOrWhiteSpace(it.Time)) continue;
                var row = new WeeklyScheduleRuleRow { DayOfWeek = it.DayOfWeek, Time = it.Time.Trim() };
                AttachRuleWatcher(row);
                ScheduleRules.Add(row);
            }
        }
        catch
        {
            /* ignore */
        }
    }

    /// <summary>Слоты: календарная дата + время из правил. Kind = Unspecified (не UTC), чтобы предпросмотр совпадал с введённым HH:mm.</summary>
    private static bool TryBuildSlotDates(DateOnly start, DateOnly end, string json, out List<DateTime> slotWallClocks, out string? error)
    {
        slotWallClocks = [];
        error = null;
        try
        {
            var items = JsonSerializer.Deserialize<List<JsonRule>>(json.Trim(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (items == null || items.Count == 0)
            {
                error = "Пустые правила.";
                return false;
            }

            var rules = new List<(DayOfWeek Dow, TimeOnly Time)>();
            foreach (var it in items)
            {
                if (it.DayOfWeek is < 1 or > 7)
                {
                    error = "dayOfWeek: 1–7.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(it.Time) || !TimeOnly.TryParse(it.Time.Trim(), out var tm))
                {
                    error = "Некорректное время в правиле.";
                    return false;
                }

                var dow = it.DayOfWeek == 7 ? DayOfWeek.Sunday : (DayOfWeek)it.DayOfWeek;
                rules.Add((dow, tm));
            }

            var distinct = rules.GroupBy(x => (x.Dow, x.Time)).Select(g => g.Key).ToList();
            var list = new List<(DateOnly Day, TimeOnly Time)>();
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                foreach (var (dow, tm) in distinct.OrderBy(x => x.Time))
                {
                    if (d.DayOfWeek == dow)
                        list.Add((d, tm));
                }
            }

            list.Sort((a, b) =>
            {
                var c = a.Day.CompareTo(b.Day);
                return c != 0 ? c : a.Time.CompareTo(b.Time);
            });

            slotWallClocks = list.Select(x => DateTime.SpecifyKind(x.Day.ToDateTime(x.Time), DateTimeKind.Unspecified)).ToList();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private async Task SaveOrWizardAsync()
    {
        if (IsCreateMode)
        {
            if (EditorTabIndex == 0)
            {
                if (!TryValidateGeneralTab(out _, out _, out _, out _, out var genErr))
                {
                    MessageBox.Show(genErr!, "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                EditorTabIndex = 1;
                return;
            }

            if (EditorTabIndex == 1)
            {
                if (!TryValidateGeneralTab(out _, out _, out _, out _, out var genErr))
                {
                    MessageBox.Show(genErr!, "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TryValidateScheduleRules(out var scheduleRulesErr))
                {
                    MessageBox.Show(scheduleRulesErr, "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                EditorTabIndex = 2;
                return;
            }
        }

        await SaveAsync();
    }

    /// <summary>Вкладка «Основная информация»: курс, даты, лимит студентов, набор.</summary>
    private bool TryValidateGeneralTab(
        out DateOnly startDate,
        out DateOnly endDate,
        out DateOnly? enrollStart,
        out DateOnly? enrollEnd,
        out string? errorMessage)
    {
        startDate = default;
        endDate = default;
        enrollStart = null;
        enrollEnd = null;
        errorMessage = null;

        if (SelectedCourse == null)
        {
            errorMessage = "Выберите курс.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            errorMessage = "Укажите название потока.";
            return false;
        }

        if (!TryParseDate(StartDateText, out startDate, out var startErr))
        {
            errorMessage = startErr;
            return false;
        }

        if (!TryParseNullableDate(EndDateText, out var endNullable, out var endErr) || !endNullable.HasValue)
        {
            errorMessage = endErr ?? "Укажите дату окончания.";
            return false;
        }

        endDate = endNullable.Value;
        if (endDate < startDate)
        {
            errorMessage = "Дата окончания должна быть позже даты начала.";
            return false;
        }

        if (!MaxStudents.HasValue || MaxStudents.Value <= 0)
        {
            errorMessage = "Максимум студентов должен быть больше 0.";
            return false;
        }

        if (!TryParseNullableDate(EnrollmentStartDateText, out enrollStart, out var enrollStartErr))
        {
            errorMessage = enrollStartErr;
            return false;
        }

        if (!TryParseNullableDate(EnrollmentEndDateText, out enrollEnd, out var enrollEndErr))
        {
            errorMessage = enrollEndErr;
            return false;
        }

        if (enrollStart.HasValue && enrollEnd.HasValue && enrollStart.Value > enrollEnd.Value)
        {
            errorMessage = "Дата открытия набора не должна быть позже даты закрытия набора.";
            return false;
        }

        // Набор можно вести до конца 7-го календарного дня после старта потока: закрытие ≤ start + 7 дней.
        if (enrollEnd.HasValue && enrollEnd.Value > startDate.AddDays(7))
        {
            errorMessage =
                "Дата закрытия набора не может быть позже даты начала потока + 7 календарных дней (запись закрывается не позже чем через неделю после старта).";
            return false;
        }

        return true;
    }

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (!TryValidateGeneralTab(out var startDate, out var endDate, out var enrollStart, out var enrollEnd, out var genErr))
        {
            MessageBox.Show(genErr ?? "Проверьте данные.", "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!TryValidateScheduleRules(out var scheduleRulesErr))
        {
            MessageBox.Show(scheduleRulesErr, "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var rulesJson = BuildScheduleRulesJson();

        var lpw = ScheduleRules.Count > 0 ? ScheduleRules.Count : LessonsPerWeek;

        if (IsCreateMode)
        {
            if (TeacherEmployeeId <= 0)
            {
                MessageBox.Show("Выберите преподавателя.", "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedMentors.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одного наставника.", "Поток", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        Error = null;
        IsBusy = true;
        LastCreatedInstanceId = null;
        try
        {
            var upsert = new AdminCourseInstanceUpsertDto
            {
                CourseId = SelectedCourse!.Id,
                Title = Title.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                StartDate = startDate,
                EndDate = endDate,
                EnrollmentStartDate = enrollStart,
                EnrollmentEndDate = enrollEnd,
                MaxStudents = MaxStudents,
                LessonsPerWeek = lpw,
                TotalWeeks = TotalWeeks,
                Timezone = string.IsNullOrWhiteSpace(Timezone) ? null : Timezone.Trim(),
                Status = IsCreateMode ? "planned" : (string.IsNullOrWhiteSpace(Status) ? "planned" : Status.Trim()),
                IsActive = IsActive,
                ScheduleRulesJson = rulesJson
            };

            if (_instanceId == null)
            {
                var boot = new AdminCourseInstanceBootstrapDto
                {
                    Instance = upsert,
                    TeacherEmployeeId = TeacherEmployeeId,
                    MentorEmployeeIds = SelectedMentors.Select(x => x.EmployeeId).Distinct().ToList()
                };
                var created = await _instances.CreateBootstrapAsync(boot, cancellationToken);
                LastCreatedInstanceId = created.InstanceId;
                MessageBox.Show("Поток успешно создан.", "Поток", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var jsonNow = rulesJson.Trim();
                var regen = !string.Equals(jsonNow, (_loadedScheduleRulesJson ?? "").Trim(), StringComparison.Ordinal);
                if (regen && _hadExistingSchedule)
                {
                    if (!UserDialogs.Confirm(
                            "Расписание уже создано.\nПри сохранении даты открытия уроков будут пересчитаны.\nПродолжить?",
                            "Расписание"))
                        return;
                }

                await _instances.UpdateInstanceAsync(_instanceId.Value, upsert, cancellationToken);

                if (TeacherEmployeeId > 0 && TeacherEmployeeId != _loadedTeacherId)
                {
                    await _instances.AddTeacherAsync(_instanceId.Value,
                        new AdminAssignTeacherDto { EmployeeId = TeacherEmployeeId, IsMainTeacher = true }, cancellationToken);
                }

                var newMentorIds = SelectedMentors.Select(x => x.EmployeeId).Distinct().ToList();
                foreach (var rem in _loadedMentorIds.Where(id => !newMentorIds.Contains(id)))
                    await _instances.RemoveCoordinatorAsync(_instanceId.Value, rem, cancellationToken);
                foreach (var add in newMentorIds.Where(id => !_loadedMentorIds.Contains(id)))
                    await _instances.AddCoordinatorAsync(_instanceId.Value, new AdminAssignCoordinatorDto { EmployeeId = add, IsLead = false },
                        cancellationToken);

                if (regen)
                {
                    await _instances.GenerateScheduleAsync(_instanceId.Value,
                        new AdminGenerateInstanceScheduleDto { OverwriteExisting = true, DefaultOpenTime = new TimeOnly(9, 0) },
                        cancellationToken);
                }

                _loadedScheduleRulesJson = jsonNow;
                try
                {
                    var sch = await _instances.GetScheduleAsync(_instanceId.Value, cancellationToken);
                    _existingScheduleOrdered = sch.OrderBy(x => x.ModuleOrder).ThenBy(x => x.LessonOrder).ToList();
                    _hadExistingSchedule = sch.Count > 0;
                }
                catch
                {
                    /* ignore */
                }

                MessageBox.Show("Изменения сохранены.", "Поток", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Saved?.Invoke();
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            MessageBox.Show(ex.Message, "Поток", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
            MessageBox.Show(Error, "Поток", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool TryParseDate(string? text, out DateOnly value, out string error)
    {
        error = "";
        value = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Укажите дату начала в формате yyyy-MM-dd.";
            return false;
        }

        var t = text.Trim();
        if (!DateOnly.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
        {
            error = DescribeDateParseFailure(t);
            return false;
        }

        return true;
    }

    private static bool TryParseNullableDate(string? text, out DateOnly? value, out string error)
    {
        error = "";
        value = null;
        if (string.IsNullOrWhiteSpace(text))
            return true;
        var t = text.Trim();
        if (!DateOnly.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            error = DescribeDateParseFailure(t);
            return false;
        }

        value = parsed;
        return true;
    }

    private static readonly Regex IsoDateLikeRegex = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    /// <summary>Отличаем «31 июня» (календарь) от опечатки в формате.</summary>
    private static string DescribeDateParseFailure(string trimmed)
    {
        if (IsoDateLikeRegex.IsMatch(trimmed))
            return "Такой даты в календаре не существует. Проверьте день месяца (например, в июне 30 дней) и формат yyyy-MM-dd.";
        return "Некорректная дата. Используйте формат yyyy-MM-dd.";
    }

    private void EnsureTimezoneInOptions()
    {
        var tz = (Timezone ?? "").Trim();
        if (string.IsNullOrEmpty(tz)) return;
        if (!TimezoneOptions.Contains(tz))
            TimezoneOptions.Add(tz);
    }
}
