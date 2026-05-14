using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class AnalyticsViewModel : BaseViewModel
{
    private readonly AdminAnalyticsService _analytics;

    public AnalyticsViewModel(AdminAnalyticsService analytics)
    {
        _analytics = analytics;

        PeriodOptions.Add(new PeriodOption("today", "Сегодня"));
        PeriodOptions.Add(new PeriodOption("week", "Неделя"));
        PeriodOptions.Add(new PeriodOption("month", "Месяц"));
        PeriodOptions.Add(new PeriodOption("quarter", "Квартал"));
        PeriodOptions.Add(new PeriodOption("year", "Год"));
        PeriodOptions.Add(new PeriodOption("custom", "Произвольный период"));
        SelectedPeriod = PeriodOptions.FirstOrDefault();

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }

    public ObservableCollection<PeriodOption> PeriodOptions { get; } = new();

    private PeriodOption? _selectedPeriod;
    public PeriodOption? SelectedPeriod { get => _selectedPeriod; set => SetProperty(ref _selectedPeriod, value); }

    public string FromText { get => _fromText; set => SetProperty(ref _fromText, value); }
    private string _fromText = "";

    public string ToText { get => _toText; set => SetProperty(ref _toText, value); }
    private string _toText = "";

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
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminAnalyticsSummaryDto? _summary;
    public AdminAnalyticsSummaryDto? Summary { get => _summary; set => SetProperty(ref _summary, value); }

    public ObservableCollection<AdminDateCountPointDto> ApplicationsByDay { get; } = new();
    public ObservableCollection<AdminDateAmountPointDto> RevenueByDay { get; } = new();
    public ObservableCollection<AdminNameCountPointDto> PaymentsByStatus { get; } = new();
    public ObservableCollection<AdminNamePercentPointDto> ProgressByCourse { get; } = new();
    public ObservableCollection<AdminNameCountPointDto> HomeworkOnReviewByCourse { get; } = new();
    public ObservableCollection<AdminPromoCodeUsageAggDto> PromoUsage { get; } = new();
    public ObservableCollection<AdminNameCountPointDto> PopularCourses { get; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var period = SelectedPeriod?.Id ?? "month";
            var from = period == "custom" ? (string.IsNullOrWhiteSpace(FromText) ? null : FromText.Trim()) : null;
            var to = period == "custom" ? (string.IsNullOrWhiteSpace(ToText) ? null : ToText.Trim()) : null;

            Summary = await _analytics.GetSummaryAsync(period, from, to, cancellationToken);

            var apps = await _analytics.GetApplicationsAsync(period, from, to, cancellationToken);
            ApplicationsByDay.Clear();
            foreach (var x in apps) ApplicationsByDay.Add(x);

            var rev = await _analytics.GetRevenueAsync(period, from, to, cancellationToken);
            RevenueByDay.Clear();
            foreach (var x in rev) RevenueByDay.Add(x);

            var payStatuses = await _analytics.GetOrdersAsync(period, from, to, cancellationToken);
            PaymentsByStatus.Clear();
            foreach (var x in payStatuses) PaymentsByStatus.Add(x);

            var prog = await _analytics.GetStudentProgressAsync(cancellationToken);
            ProgressByCourse.Clear();
            foreach (var x in prog) ProgressByCourse.Add(x);

            var hw = await _analytics.GetHomeworkAsync(period, from, to, cancellationToken);
            HomeworkOnReviewByCourse.Clear();
            foreach (var x in hw) HomeworkOnReviewByCourse.Add(x);

            var promo = await _analytics.GetPromoCodesAsync(period, from, to, cancellationToken);
            PromoUsage.Clear();
            foreach (var x in promo) PromoUsage.Add(x);

            var pop = await _analytics.GetPopularCoursesAsync(period, from, to, cancellationToken);
            PopularCourses.Clear();
            foreach (var x in pop) PopularCourses.Add(x);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

