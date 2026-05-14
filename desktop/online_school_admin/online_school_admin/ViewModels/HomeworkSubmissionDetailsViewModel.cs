using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class HomeworkSubmissionDetailsViewModel : BaseViewModel
{
    private readonly AdminHomeworkReviewService _review;
    private readonly AdminEmployeesService _employees;
    private readonly int _submissionId;
    private readonly int? _focusStudentAnswerId;

    public HomeworkSubmissionDetailsViewModel(
        AdminHomeworkReviewService review,
        AdminEmployeesService employees,
        int submissionId,
        int? focusStudentAnswerId = null)
    {
        _review = review;
        _employees = employees;
        _submissionId = submissionId;
        _focusStudentAnswerId = focusStudentAnswerId;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        SaveTaskReviewCommand = new RelayCommand(async _ => await SaveTaskReviewAsync(),
            _ => !IsBusy && SelectedTask != null && SelectedTask.NeedsManualReview && !string.IsNullOrWhiteSpace(ScoreText));
        ApproveCommand = new RelayCommand(async _ => await ApproveAsync(), _ => !IsBusy);
        RequestRevisionCommand = new RelayCommand(async _ => await RequestRevisionAsync(), _ => !IsBusy);
        RejectCommand = new RelayCommand(async _ => await RejectAsync(), _ => !IsBusy);
        AssignReviewerCommand = new RelayCommand(async _ => await AssignReviewerAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand SaveTaskReviewCommand { get; }
    public RelayCommand ApproveCommand { get; }
    public RelayCommand RequestRevisionCommand { get; }
    public RelayCommand RejectCommand { get; }
    public RelayCommand AssignReviewerCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                SaveTaskReviewCommand.RaiseCanExecuteChanged();
                ApproveCommand.RaiseCanExecuteChanged();
                RequestRevisionCommand.RaiseCanExecuteChanged();
                RejectCommand.RaiseCanExecuteChanged();
                AssignReviewerCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminHomeworkSubmissionDetailsDto? _details;
    public AdminHomeworkSubmissionDetailsDto? Details { get => _details; private set => SetProperty(ref _details, value); }

    public ObservableCollection<AdminHomeworkTaskSubmissionDetailsDto> Tasks { get; } = new();

    private AdminHomeworkTaskSubmissionDetailsDto? _selectedTask;
    public AdminHomeworkTaskSubmissionDetailsDto? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (!SetProperty(ref _selectedTask, value))
                return;
            LoadTaskToEditor();
            SaveTaskReviewCommand.RaiseCanExecuteChanged();
        }
    }

    private string _scoreText = "";
    public string ScoreText
    {
        get => _scoreText;
        set
        {
            if (SetProperty(ref _scoreText, value))
                SaveTaskReviewCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _teacherComment;
    public string? TeacherComment { get => _teacherComment; set => SetProperty(ref _teacherComment, value); }

    public ObservableCollection<IdTitleOption> ReviewerOptions { get; } = new();
    private IdTitleOption? _selectedReviewer;
    public IdTitleOption? SelectedReviewer { get => _selectedReviewer; set => SetProperty(ref _selectedReviewer, value); }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        try
        {
            ReviewerOptions.Clear();
            ReviewerOptions.Add(new IdTitleOption(0, "—"));
            var list = await _employees.GetEmployeesAsync(null, null, cancellationToken);
            foreach (var e in list.OrderBy(x => x.FullName))
                ReviewerOptions.Add(new IdTitleOption(e.EmployeeId, e.FullName));
            SelectedReviewer = ReviewerOptions.FirstOrDefault();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _review.GetSubmissionAsync(_submissionId, cancellationToken);
            Details = dto;
            Replace(Tasks, dto.Tasks ?? Array.Empty<AdminHomeworkTaskSubmissionDetailsDto>());

            if (_focusStudentAnswerId.HasValue)
            {
                SelectedTask = Tasks.FirstOrDefault(t => t.TaskSubmissionId == _focusStudentAnswerId.Value)
                               ?? Tasks.FirstOrDefault();
            }
            else
                SelectedTask = Tasks.FirstOrDefault();

            if (dto.CheckedByEmployeeId.HasValue)
                SelectedReviewer = ReviewerOptions.FirstOrDefault(x => x.Id == dto.CheckedByEmployeeId.Value) ?? ReviewerOptions.FirstOrDefault();
            else
                SelectedReviewer = ReviewerOptions.FirstOrDefault();
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
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

    private async Task SaveTaskReviewAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedTask == null || !SelectedTask.NeedsManualReview)
            return;

        if (!decimal.TryParse(ScoreText.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var pts))
        {
            MessageBox.Show("Введите балл числом (например 0 или 7.5).", "Проверка ДЗ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (pts < 0m || pts > SelectedTask.MaxPointsDecimal)
        {
            MessageBox.Show($"Балл должен быть от 0 до {SelectedTask.MaxPointsDecimal.ToString(CultureInfo.InvariantCulture)}.", "Проверка ДЗ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _review.ReviewTaskAsync(SelectedTask.TaskSubmissionId, new AdminHomeworkTaskSubmissionReviewDto
            {
                Score = pts,
                TeacherComment = string.IsNullOrWhiteSpace(TeacherComment) ? null : TeacherComment.Trim(),
                IsCorrect = null
            }, cancellationToken);

            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApproveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            await _review.ApproveAsync(_submissionId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RequestRevisionAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            await _review.RequestRevisionAsync(_submissionId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RejectAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            await _review.RejectAsync(_submissionId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AssignReviewerAsync(CancellationToken cancellationToken = default)
    {
        if (Details == null) return;

        Error = null;
        IsBusy = true;
        try
        {
            var reviewerId = SelectedReviewer is { Id: > 0 } r ? r.Id : (int?)null;
            await _review.AssignReviewerAsync(_submissionId, reviewerId, cancellationToken);
            await LoadAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadTaskToEditor()
    {
        if (SelectedTask == null)
        {
            ScoreText = "";
            TeacherComment = null;
            return;
        }

        ScoreText = SelectedTask.PointsAwarded.HasValue
            ? SelectedTask.PointsAwarded.Value.ToString(CultureInfo.InvariantCulture)
            : SelectedTask.Score.HasValue
                ? SelectedTask.Score.Value.ToString(CultureInfo.InvariantCulture)
                : "";
        TeacherComment = SelectedTask.TeacherComment;
    }

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> items)
    {
        target.Clear();
        foreach (var i in items)
            target.Add(i);
    }
}
