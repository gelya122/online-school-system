using System.Globalization;
using System.Net.Http;
using System.Windows;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class HomeworkAnswerReviewWindowViewModel : BaseViewModel
{
    private readonly AdminHomeworkReviewService _review;
    private readonly int _submissionId;
    private readonly int _studentAnswerId;

    public HomeworkAnswerReviewWindowViewModel(AdminHomeworkReviewService review, int submissionId, int studentAnswerId)
    {
        _review = review;
        _submissionId = submissionId;
        _studentAnswerId = studentAnswerId;

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

    private AdminHomeworkSubmissionDetailsDto? _details;
    public AdminHomeworkSubmissionDetailsDto? Details { get => _details; private set => SetProperty(ref _details, value); }

    private AdminHomeworkTaskSubmissionDetailsDto? _task;
    public AdminHomeworkTaskSubmissionDetailsDto? Task { get => _task; private set => SetProperty(ref _task, value); }

    private string _scoreText = "";
    public string ScoreText { get => _scoreText; set => SetProperty(ref _scoreText, value); }

    private string? _teacherComment;
    public string? TeacherComment { get => _teacherComment; set => SetProperty(ref _teacherComment, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _review.GetSubmissionAsync(_submissionId, cancellationToken);
            Details = dto;
            Task = (dto.Tasks ?? Array.Empty<AdminHomeworkTaskSubmissionDetailsDto>())
                .FirstOrDefault(t => t.TaskSubmissionId == _studentAnswerId);

            if (Task == null)
            {
                Error = "Ответ не найден (возможно, уже проверен или удалён). Обновите очередь.";
                return;
            }

            ScoreText = "";
            TeacherComment = null;
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

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (Task == null)
            return;

        if (!decimal.TryParse((ScoreText ?? "").Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var pts))
        {
            MessageBox.Show("Введите балл числом (например 0 или 7.5).", "Проверка ДЗ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (pts < 0m || pts > Task.MaxPointsDecimal)
        {
            MessageBox.Show($"Балл должен быть от 0 до {Task.MaxPointsDecimal.ToString(CultureInfo.InvariantCulture)}.", "Проверка ДЗ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _review.ReviewTaskAsync(Task.TaskSubmissionId, new AdminHomeworkTaskSubmissionReviewDto
            {
                Score = pts,
                TeacherComment = string.IsNullOrWhiteSpace(TeacherComment) ? null : TeacherComment.Trim(),
                IsCorrect = null
            }, cancellationToken);

            Saved?.Invoke();
        }
        catch (ApiException ex)
        {
            // В т.ч. 409 Conflict: уже проверено другим сотрудником.
            var msg = ApiErrorFormatter.Format(ex);
            MessageBox.Show(msg, "Проверка ДЗ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (HttpRequestException)
        {
            MessageBox.Show("Не удалось связаться с сервером.", "Проверка ДЗ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

