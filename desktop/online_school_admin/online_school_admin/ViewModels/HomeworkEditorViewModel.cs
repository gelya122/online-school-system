using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using online_school_admin.Models;
using online_school_admin.Models.Admin;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class HomeworkEditorViewModel : BaseViewModel
{
    private readonly AdminCoursesService _courses;
    private readonly int _lessonId;

    private readonly bool _readOnly;

    private DispatcherTimer? _taskAutosaveTimer;
    private bool _suppressTaskAutosave;

    public HomeworkEditorViewModel(AdminCoursesService courses, int lessonId, bool readOnly = false)
    {
        _courses = courses;
        _lessonId = lessonId;
        _readOnly = readOnly;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);

        AddHomeworkCommand = new RelayCommand(async _ => await AddHomeworkAsync(), _ => !IsBusy && !_readOnly && Homeworks.Count < 1);
        SaveHomeworkCommand = new RelayCommand(async _ => await SaveHomeworkAsync(), _ => !IsBusy && !_readOnly && SelectedHomework != null);
        DeleteHomeworkCommand = new RelayCommand(async _ => await DeleteHomeworkAsync(), _ => !IsBusy && !_readOnly && SelectedHomework != null);

        AddTaskCommand = new RelayCommand(async _ => await AddTaskAsync(), _ => !IsBusy && !_readOnly && SelectedHomework != null);
        MoveTaskUpCommand = new RelayCommand(async _ => await MoveTaskAsync(-1), _ => !IsBusy && !_readOnly && CanMoveTask(-1));
        MoveTaskDownCommand = new RelayCommand(async _ => await MoveTaskAsync(1), _ => !IsBusy && !_readOnly && CanMoveTask(1));

        PropertyChanged += (_, e) =>
        {
            if (_suppressTaskAutosave || _readOnly || SelectedTask == null || string.IsNullOrEmpty(e.PropertyName))
                return;
            if (IsTaskEditorAutosaveProperty(e.PropertyName))
                ScheduleTaskAutosave();
        };
    }

    public bool ReadOnly => _readOnly;

    public RelayCommand RefreshCommand { get; }

    public RelayCommand AddHomeworkCommand { get; }
    public RelayCommand SaveHomeworkCommand { get; }
    public RelayCommand DeleteHomeworkCommand { get; }

    public RelayCommand AddTaskCommand { get; }
    public RelayCommand MoveTaskUpCommand { get; }
    public RelayCommand MoveTaskDownCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                RaiseAllCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private string _homeworkOverviewText = "";
    /// <summary>Текст для вкладки «Обзор» окна урока: ДЗ и нумерованные формулировки заданий.</summary>
    public string HomeworkOverviewText { get => _homeworkOverviewText; private set => SetProperty(ref _homeworkOverviewText, value); }

    public ObservableCollection<AdminHomeworkRowDto> Homeworks { get; } = new();
    public ObservableCollection<AssignmentTypeDto> AssignmentTypes { get; } = new();
    public ObservableCollection<AdminHomeworkTaskRowDto> Tasks { get; } = new();

    private AdminHomeworkRowDto? _selectedHomework;
    public AdminHomeworkRowDto? SelectedHomework
    {
        get => _selectedHomework;
        set
        {
            if (!SetProperty(ref _selectedHomework, value))
                return;

            LoadHomeworkToEditor();
            _ = LoadTasksAsync();
            RaiseAllCanExecuteChanged();
        }
    }

    private AdminHomeworkTaskRowDto? _selectedTask;
    public AdminHomeworkTaskRowDto? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (ReferenceEquals(_selectedTask, value))
                return;

            StopTaskAutosaveTimer();
            var previous = _selectedTask;
            if (previous != null)
                _ = PersistTaskSnapshotAsync(previous.TaskId, CaptureTaskEditorSnapshot(), silent: true);

            if (!SetProperty(ref _selectedTask, value))
                return;

            LoadTaskToEditor();
            OnPropertyChanged(nameof(IsShortAnswerTask));
            OnPropertyChanged(nameof(IsDetailedAnswerTask));
            RaiseAllCanExecuteChanged();
        }
    }

    // homework editor
    private string _homeworkTitle = "";
    public string HomeworkTitle { get => _homeworkTitle; set => SetProperty(ref _homeworkTitle, value); }

    private string? _homeworkDescription;
    public string? HomeworkDescription { get => _homeworkDescription; set => SetProperty(ref _homeworkDescription, value); }

    private int _homeworkMaxScore;
    public int HomeworkMaxScore { get => _homeworkMaxScore; set => SetProperty(ref _homeworkMaxScore, value); }

    private int? _homeworkDueDays;
    public int? HomeworkDueDaysAfterLesson { get => _homeworkDueDays; set => SetProperty(ref _homeworkDueDays, value); }

    private int _homeworkAssignmentTypeId;
    public int HomeworkAssignmentTypeId
    {
        get => _homeworkAssignmentTypeId;
        set => SetProperty(ref _homeworkAssignmentTypeId, value);
    }

    private bool _homeworkIsRequired = true;
    public bool HomeworkIsRequired { get => _homeworkIsRequired; set => SetProperty(ref _homeworkIsRequired, value); }

    private int _homeworkOrder;
    public int HomeworkOrder { get => _homeworkOrder; set => SetProperty(ref _homeworkOrder, value); }

    private bool _homeworkIsActive = true;
    public bool HomeworkIsActive { get => _homeworkIsActive; set => SetProperty(ref _homeworkIsActive, value); }

    // task editor
    private string _taskType = "short_answer";
    public string TaskType
    {
        get => _taskType;
        set
        {
            if (SetProperty(ref _taskType, value))
            {
                if (_taskType == "detailed_answer")
                    AutoCheckEnabled = false;
                OnPropertyChanged(nameof(IsShortAnswerTask));
                OnPropertyChanged(nameof(IsDetailedAnswerTask));
                RaiseAllCanExecuteChanged();
            }
        }
    }

    public bool IsShortAnswerTask => string.Equals(TaskType, "short_answer", StringComparison.OrdinalIgnoreCase);

    public bool IsDetailedAnswerTask => string.Equals(TaskType, "detailed_answer", StringComparison.OrdinalIgnoreCase);

    /// <summary>Элементы для ComboBox: значение API + подпись на русском.</summary>
    public sealed record TaskTypeUiItem(string Slug, string Title);

    public IReadOnlyList<TaskTypeUiItem> TaskTypeItems { get; } =
    [
        new("short_answer", "Краткий ответ (автопроверка)"),
        new("detailed_answer", "Развёрнутый ответ (вручную)"),
    ];

    private string? _taskTitle;
    public string? TaskTitle { get => _taskTitle; set => SetProperty(ref _taskTitle, value); }

    private string _taskText = "";
    public string TaskText { get => _taskText; set => SetProperty(ref _taskText, value); }

    private string? _taskExplanation;
    public string? TaskExplanation { get => _taskExplanation; set => SetProperty(ref _taskExplanation, value); }

    private int _taskMaxScore = 1;
    public int TaskMaxScore { get => _taskMaxScore; set => SetProperty(ref _taskMaxScore, value); }

    private int _taskOrder;
    public int TaskOrder { get => _taskOrder; set => SetProperty(ref _taskOrder, value); }

    /// <summary>Для краткого ответа: один или несколько вариантов через | (колонка correct_answer).</summary>
    private string _taskCorrectAnswer = "";
    public string TaskCorrectAnswer
    {
        get => _taskCorrectAnswer;
        set => SetProperty(ref _taskCorrectAnswer, value);
    }

    private bool _autoCheckEnabled;
    public bool AutoCheckEnabled
    {
        get => _autoCheckEnabled;
        set
        {
            if (TaskType == "detailed_answer")
                value = false;
            SetProperty(ref _autoCheckEnabled, value);
        }
    }

    private bool _allowPartialCredit;
    public bool AllowPartialCredit
    {
        get => _allowPartialCredit;
        set
        {
            if (TaskType == "detailed_answer")
                value = false;
            SetProperty(ref _allowPartialCredit, value);
        }
    }

    private decimal? _numericTolerance;
    public decimal? NumericTolerance
    {
        get => _numericTolerance;
        set
        {
            if (TaskType == "detailed_answer")
                value = null;
            SetProperty(ref _numericTolerance, value);
        }
    }

    private bool _taskIsActive = true;
    public bool TaskIsActive { get => _taskIsActive; set => SetProperty(ref _taskIsActive, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (AssignmentTypes.Count == 0)
            {
                try
                {
                    foreach (var t in await _courses.GetAssignmentTypesAsync(cancellationToken))
                        AssignmentTypes.Add(t);
                    if (HomeworkAssignmentTypeId == 0 && AssignmentTypes.Count > 0)
                        HomeworkAssignmentTypeId = AssignmentTypes[0].TypeId;
                }
                catch
                {
                    // справочник не блокирует список ДЗ
                }
            }

            var list = await _courses.GetHomeworksAsync(_lessonId, cancellationToken);
            Homeworks.Clear();
            foreach (var h in list.OrderBy(x => x.HomeworkOrder).ThenBy(x => x.HomeworkId))
                Homeworks.Add(h);

            SelectedHomework = Homeworks.FirstOrDefault();
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

    private void RebuildHomeworkOverviewText()
    {
        if (SelectedHomework == null)
        {
            HomeworkOverviewText = Homeworks.Count == 0
                ? "К этому уроку не привязано домашнее задание."
                : "Выберите домашнее задание на вкладке «Домашнее задание».";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Домашнее задание: {SelectedHomework.Title}");
        if (!string.IsNullOrWhiteSpace(SelectedHomework.Description))
        {
            sb.AppendLine(SelectedHomework.Description.Trim());
            sb.AppendLine();
        }

        if (Tasks.Count == 0)
        {
            sb.Append("В этом ДЗ пока нет заданий.");
            HomeworkOverviewText = sb.ToString().TrimEnd();
            return;
        }

        var n = 1;
        foreach (var t in Tasks.OrderBy(x => x.TaskOrder).ThenBy(x => x.TaskId))
        {
            var line = string.IsNullOrWhiteSpace(t.TaskText)
                ? (string.IsNullOrWhiteSpace(t.Title) ? "—" : t.Title.Trim())
                : t.TaskText.Trim();
            sb.AppendLine($"{n}. {line}");
            n++;
        }

        HomeworkOverviewText = sb.ToString().TrimEnd();
    }

    private async Task LoadTasksAsync(CancellationToken cancellationToken = default)
    {
        Tasks.Clear();
        SelectedTask = null;

        if (SelectedHomework == null)
        {
            RebuildHomeworkOverviewText();
            return;
        }

        try
        {
            var list = await _courses.GetHomeworkTasksAsync(SelectedHomework.HomeworkId, cancellationToken);
            foreach (var t in list.OrderBy(x => x.TaskOrder).ThenBy(x => x.TaskId))
                Tasks.Add(t);
            SelectedTask = Tasks.FirstOrDefault();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            RebuildHomeworkOverviewText();
        }
    }

    private async Task AddHomeworkAsync()
    {
        if (Homeworks.Count >= 1)
        {
            MessageBox.Show(
                "К этому уроку уже привязано домашнее задание. Допускается только одно ДЗ на урок.",
                "ДЗ",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var title = (HomeworkTitle ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Укажите название домашнего задания.", "ДЗ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            var nextOrder = Homeworks.Count == 0 ? 1 : Homeworks.Max(x => x.HomeworkOrder) + 1;
            var created = await _courses.CreateHomeworkAsync(_lessonId, new AdminHomeworkUpsertDto
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(HomeworkDescription) ? null : HomeworkDescription,
                AssignmentTypeId = 0,
                MaxScore = HomeworkMaxScore,
                DueDaysAfterLesson = HomeworkDueDaysAfterLesson,
                IsRequired = HomeworkIsRequired,
                HomeworkOrder = nextOrder,
                IsActive = HomeworkIsActive
            });

            Homeworks.Add(created);
            SelectedHomework = created;
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveHomeworkAsync()
    {
        if (SelectedHomework == null) return;

        var title = (HomeworkTitle ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Укажите название домашнего задания.", "ДЗ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.UpdateHomeworkAsync(SelectedHomework.HomeworkId, new AdminHomeworkUpsertDto
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(HomeworkDescription) ? null : HomeworkDescription,
                AssignmentTypeId = 0,
                MaxScore = HomeworkMaxScore,
                DueDaysAfterLesson = HomeworkDueDaysAfterLesson,
                IsRequired = HomeworkIsRequired,
                HomeworkOrder = HomeworkOrder,
                IsActive = HomeworkIsActive
            });

            await LoadAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteHomeworkAsync()
    {
        if (SelectedHomework == null) return;

        if (MessageBox.Show(
                "Удалить это домашнее задание? Если есть сдачи студентов, сервер отклонит удаление.",
                "ДЗ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.DeleteHomeworkAsync(SelectedHomework.HomeworkId);
            await LoadAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddTaskAsync()
    {
        if (SelectedHomework == null) return;

        if (TaskType == "detailed_answer" && TaskMaxScore <= 0)
        {
            MessageBox.Show("Для развёрнутого ответа укажите max_points больше 0.", "Задание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sumOther = Tasks.Sum(t => t.MaxScore);
        if (sumOther + TaskMaxScore > HomeworkMaxScore)
        {
            MessageBox.Show(
                $"Сумма max_points заданий не может превышать max_score ДЗ ({HomeworkMaxScore}).",
                "Задание",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TaskText))
        {
            MessageBox.Show("Укажите текст задания.", "Задание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            var nextOrder = Tasks.Count == 0 ? 1 : Tasks.Max(x => x.TaskOrder) + 1;

            var correctForCreate = TaskType == "short_answer" ? BuildCorrectAnswerForApi() : null;
            if (TaskType == "short_answer" && string.IsNullOrWhiteSpace(correctForCreate))
            {
                MessageBox.Show(
                    "Для краткого ответа укажите правильный ответ (один или несколько вариантов через символ |).",
                    "Задание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var created = await _courses.CreateHomeworkTaskAsync(SelectedHomework.HomeworkId, new AdminHomeworkTaskUpsertDto
            {
                TaskType = TaskType,
                Title = string.IsNullOrWhiteSpace(TaskTitle) ? null : TaskTitle,
                TaskText = TaskText,
                Explanation = string.IsNullOrWhiteSpace(TaskExplanation) ? null : TaskExplanation,
                MaxScore = TaskMaxScore,
                TaskOrder = nextOrder,
                CorrectAnswer = correctForCreate,
                AllowPartialCredit = false,
                NumericTolerance = null,
                AutoCheckEnabled = true,
                IsActive = TaskIsActive
            });

            Tasks.Add(created);
            SelectedTask = created;
            RebuildHomeworkOverviewText();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private sealed record TaskEditorSnapshot(
        string TaskType,
        string? TaskTitle,
        string TaskText,
        string? TaskExplanation,
        int TaskMaxScore,
        int TaskOrder,
        string TaskCorrectAnswer,
        bool AutoCheckEnabled,
        bool AllowPartialCredit,
        decimal? NumericTolerance,
        bool TaskIsActive);

    private TaskEditorSnapshot CaptureTaskEditorSnapshot() =>
        new(TaskType, TaskTitle, TaskText, TaskExplanation, TaskMaxScore, TaskOrder, TaskCorrectAnswer,
            AutoCheckEnabled, AllowPartialCredit, NumericTolerance, TaskIsActive);

    private static bool IsTaskEditorAutosaveProperty(string name) =>
        name is nameof(TaskText) or nameof(TaskType) or nameof(TaskTitle) or nameof(TaskExplanation)
            or nameof(TaskMaxScore) or nameof(TaskOrder) or nameof(TaskCorrectAnswer)
            or nameof(AutoCheckEnabled) or nameof(AllowPartialCredit) or nameof(NumericTolerance)
            or nameof(TaskIsActive);

    private void EnsureTaskAutosaveTimer()
    {
        if (_taskAutosaveTimer != null)
            return;
        _taskAutosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(750) };
        _taskAutosaveTimer.Tick += (_, _) =>
        {
            _taskAutosaveTimer!.Stop();
            if (SelectedTask == null)
                return;
            var id = SelectedTask.TaskId;
            _ = PersistTaskSnapshotAsync(id, CaptureTaskEditorSnapshot(), silent: true);
        };
    }

    private void StopTaskAutosaveTimer() => _taskAutosaveTimer?.Stop();

    private void ScheduleTaskAutosave()
    {
        if (_suppressTaskAutosave || _readOnly || SelectedTask == null || IsBusy)
            return;
        EnsureTaskAutosaveTimer();
        _taskAutosaveTimer!.Stop();
        _taskAutosaveTimer.Start();
    }

    private string? BuildCorrectAnswerFromSnapshot(TaskEditorSnapshot s) =>
        string.Equals(s.TaskType, "short_answer", StringComparison.OrdinalIgnoreCase)
            ? string.IsNullOrWhiteSpace(s.TaskCorrectAnswer) ? null : s.TaskCorrectAnswer.Trim()
            : null;

    private bool TryValidateTaskSnapshot(TaskEditorSnapshot s, int editingTaskId, bool silent, out string? message)
    {
        message = null;
        if (string.IsNullOrWhiteSpace(s.TaskText))
        {
            message = "Укажите текст задания.";
            if (!silent) MessageBox.Show(message, "Задание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.Equals(s.TaskType, "detailed_answer", StringComparison.OrdinalIgnoreCase) && s.TaskMaxScore <= 0)
        {
            message = "Для развёрнутого ответа укажите max_points больше 0.";
            if (!silent) MessageBox.Show(message, "Задание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var sumOther = Tasks.Where(t => t.TaskId != editingTaskId).Sum(t => t.MaxScore);
        if (sumOther + s.TaskMaxScore > HomeworkMaxScore)
        {
            message = $"Сумма max_points заданий не может превышать max_score ДЗ ({HomeworkMaxScore}).";
            if (!silent) MessageBox.Show(message, "Задание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.Equals(s.TaskType, "short_answer", StringComparison.OrdinalIgnoreCase))
        {
            var merged = BuildCorrectAnswerFromSnapshot(s);
            if (string.IsNullOrWhiteSpace(merged))
            {
                message =
                    "Для краткого ответа укажите правильный ответ (один или несколько вариантов через символ |).";
                if (!silent) MessageBox.Show(message, "Задание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        return true;
    }

    private static AdminHomeworkTaskUpsertDto BuildUpsertDto(TaskEditorSnapshot s) =>
        new()
        {
            TaskType = s.TaskType,
            Title = string.IsNullOrWhiteSpace(s.TaskTitle) ? null : s.TaskTitle,
            TaskText = s.TaskText,
            Explanation = string.IsNullOrWhiteSpace(s.TaskExplanation) ? null : s.TaskExplanation,
            MaxScore = s.TaskMaxScore,
            TaskOrder = s.TaskOrder,
            CorrectAnswer = string.Equals(s.TaskType, "short_answer", StringComparison.OrdinalIgnoreCase)
                ? (string.IsNullOrWhiteSpace(s.TaskCorrectAnswer) ? null : s.TaskCorrectAnswer.Trim())
                : null,
            AllowPartialCredit = false,
            NumericTolerance = null,
            AutoCheckEnabled = true,
            IsActive = s.TaskIsActive
        };

    private void ApplySnapshotToRow(AdminHomeworkTaskRowDto row, TaskEditorSnapshot s)
    {
        row.TaskType = s.TaskType;
        row.Title = s.TaskTitle;
        row.TaskText = s.TaskText;
        row.Explanation = s.TaskExplanation;
        row.MaxScore = s.TaskMaxScore;
        row.TaskOrder = s.TaskOrder;
        row.CorrectAnswer = BuildCorrectAnswerFromSnapshot(s);
        row.AllowPartialCredit = false;
        row.NumericTolerance = null;
        row.AutoCheckEnabled = true;
        row.IsActive = s.TaskIsActive;
    }

    private async Task PersistTaskSnapshotAsync(int taskId, TaskEditorSnapshot snapshot, bool silent)
    {
        if (_readOnly)
            return;

        if (!TryValidateTaskSnapshot(snapshot, taskId, silent, out var validationMessage))
        {
            if (silent && validationMessage != null)
                Error = validationMessage;
            return;
        }

        Error = null;
        if (!silent)
            IsBusy = true;
        try
        {
            await _courses.UpdateHomeworkTaskAsync(taskId, BuildUpsertDto(snapshot));
            var row = Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (row != null)
            {
                ApplySnapshotToRow(row, snapshot);
                RebuildHomeworkOverviewText();
            }
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            if (!silent)
                IsBusy = false;
        }
    }

    public async Task DeleteTaskForRowAsync(AdminHomeworkTaskRowDto row)
    {
        if (_readOnly)
            return;

        if (MessageBox.Show(
                "Удалить задание? Если есть ответы студентов, сервер отклонит удаление.",
                "Задание",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        StopTaskAutosaveTimer();
        Error = null;
        IsBusy = true;
        try
        {
            await _courses.DeleteHomeworkTaskAsync(row.TaskId);
            await LoadTasksAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanMoveTask(int delta)
    {
        if (SelectedTask == null) return false;
        var idx = Tasks.IndexOf(SelectedTask);
        var newIdx = idx + delta;
        return idx >= 0 && newIdx >= 0 && newIdx < Tasks.Count;
    }

    private async Task MoveTaskAsync(int delta)
    {
        if (!CanMoveTask(delta) || SelectedTask == null) return;

        var idx = Tasks.IndexOf(SelectedTask);
        var newIdx = idx + delta;

        var tmp = Tasks[newIdx];
        Tasks[newIdx] = Tasks[idx];
        Tasks[idx] = tmp;

        for (var i = 0; i < Tasks.Count; i++)
            Tasks[i].TaskOrder = i + 1;

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.ReorderHomeworkTasksAsync(new AdminReorderRequestDto
            {
                Items = Tasks.Select(t => new AdminReorderItemDto { Id = t.TaskId, Order = t.TaskOrder }).ToList()
            });
            await LoadTasksAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadHomeworkToEditor()
    {
        if (SelectedHomework == null)
        {
            HomeworkTitle = "";
            HomeworkDescription = null;
            HomeworkMaxScore = 0;
            HomeworkDueDaysAfterLesson = null;
            HomeworkAssignmentTypeId = AssignmentTypes.FirstOrDefault()?.TypeId ?? 0;
            HomeworkIsRequired = true;
            HomeworkOrder = 0;
            HomeworkIsActive = true;
            return;
        }

        HomeworkTitle = SelectedHomework.Title;
        HomeworkDescription = SelectedHomework.Description;
        HomeworkMaxScore = SelectedHomework.MaxScore;
        HomeworkDueDaysAfterLesson = SelectedHomework.DueDaysAfterLesson;
        HomeworkAssignmentTypeId = SelectedHomework.AssignmentTypeId > 0
            ? SelectedHomework.AssignmentTypeId
            : AssignmentTypes.FirstOrDefault()?.TypeId ?? 0;
        HomeworkIsRequired = SelectedHomework.IsRequired;
        HomeworkOrder = SelectedHomework.HomeworkOrder;
        HomeworkIsActive = SelectedHomework.IsActive;
    }

    /// <summary>Собирает correct_answer для API из поля «Правильный ответ» (| между вариантами).</summary>
    private string? BuildCorrectAnswerForApi()
    {
        var raw = (TaskCorrectAnswer ?? "").Trim();
        return string.IsNullOrEmpty(raw) ? null : raw;
    }

    private void LoadTaskToEditor()
    {
        _suppressTaskAutosave = true;
        try
        {
            if (SelectedTask == null)
            {
                TaskType = "short_answer";
                TaskTitle = null;
                TaskText = "";
                TaskExplanation = null;
                TaskMaxScore = 1;
                TaskOrder = 0;
                TaskCorrectAnswer = "";
                AutoCheckEnabled = false;
                AllowPartialCredit = false;
                NumericTolerance = null;
                TaskIsActive = true;
                return;
            }

            TaskType = SelectedTask.TaskType;
            TaskTitle = SelectedTask.Title;
            TaskText = SelectedTask.TaskText;
            TaskExplanation = SelectedTask.Explanation;
            TaskMaxScore = SelectedTask.MaxScore;
            TaskOrder = SelectedTask.TaskOrder;
            TaskCorrectAnswer = SelectedTask.CorrectAnswer ?? "";
            AllowPartialCredit = SelectedTask.AllowPartialCredit;
            NumericTolerance = SelectedTask.NumericTolerance;
            AutoCheckEnabled = SelectedTask.AutoCheckEnabled;
            TaskIsActive = SelectedTask.IsActive;
        }
        finally
        {
            _suppressTaskAutosave = false;
        }
    }

    private void RaiseAllCanExecuteChanged()
    {
        AddHomeworkCommand.RaiseCanExecuteChanged();
        SaveHomeworkCommand.RaiseCanExecuteChanged();
        DeleteHomeworkCommand.RaiseCanExecuteChanged();

        AddTaskCommand.RaiseCanExecuteChanged();
        MoveTaskUpCommand.RaiseCanExecuteChanged();
        MoveTaskDownCommand.RaiseCanExecuteChanged();
    }
}

