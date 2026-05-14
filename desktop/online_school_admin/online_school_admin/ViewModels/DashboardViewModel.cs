using System.Collections.ObjectModel;
using online_school_admin.Infrastructure;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class DashboardViewModel : BaseViewModel
{
    private readonly AdminDashboardService _dashboard;

    public DashboardViewModel(AdminDashboardService dashboard)
    {
        _dashboard = dashboard;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }

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

    private int _activeStudents;
    public int ActiveStudents { get => _activeStudents; set => SetProperty(ref _activeStudents, value); }

    private int _newApplications;
    public int NewApplications { get => _newApplications; set => SetProperty(ref _newApplications, value); }

    private int _homeworkPendingReview;
    public int HomeworkPendingReview { get => _homeworkPendingReview; set => SetProperty(ref _homeworkPendingReview, value); }

    private int _activeInstances;
    public int ActiveInstances { get => _activeInstances; set => SetProperty(ref _activeInstances, value); }

    private decimal _paymentsThisMonth;
    public decimal PaymentsThisMonth { get => _paymentsThisMonth; set => SetProperty(ref _paymentsThisMonth, value); }

    private int _overduePayments;
    public int OverduePayments { get => _overduePayments; set => SetProperty(ref _overduePayments, value); }

    public ObservableCollection<RecentApplicationRowDto> RecentApplications { get; } = new();
    public ObservableCollection<HomeworkReviewRowDto> HomeworkReviewQueue { get; } = new();
    public ObservableCollection<UpcomingInstanceRowDto> UpcomingInstances { get; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var summaryTask = _dashboard.GetSummaryAsync(cancellationToken);
            var appsTask = _dashboard.GetRecentApplicationsAsync(cancellationToken);
            var hwTask = _dashboard.GetHomeworkReviewQueueAsync(cancellationToken);
            var instTask = _dashboard.GetUpcomingInstancesAsync(cancellationToken);

            await Task.WhenAll(summaryTask, appsTask, hwTask, instTask);

            var s = await summaryTask;
            ActiveStudents = s.ActiveStudents;
            NewApplications = s.NewApplications;
            HomeworkPendingReview = s.HomeworkPendingReview;
            ActiveInstances = s.ActiveInstances;
            PaymentsThisMonth = s.PaymentsThisMonth;
            OverduePayments = s.OverdueInstallmentPayments;

            Replace(RecentApplications, await appsTask);
            Replace(HomeworkReviewQueue, await hwTask);
            Replace(UpcomingInstances, await instTask);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Dashboard.Load");
        }
        catch (Exception ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Dashboard.Load");
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

