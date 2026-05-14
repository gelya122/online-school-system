using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using online_school_admin.Infrastructure;
using online_school_admin.Services;
using online_school_admin.Views;

namespace online_school_admin;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppLogger.Log($"Application startup v{typeof(App).Assembly.GetName().Version}");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var baseUrl = configuration["Api:BaseUrl"]?.Trim() ?? "http://localhost:5189/";
        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";

        var http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromMinutes(30) };

        var session = new SessionService();
        var apiClient = new ApiClient(http, session);
        var auth = new AuthService(apiClient, session);
        var permissions = new PermissionService(session);
        var dashboard = new AdminDashboardService(apiClient);
        var students = new AdminStudentsService(apiClient);
        var employees = new AdminEmployeesService(apiClient);
        var courses = new AdminCoursesService(apiClient);
        var instances = new AdminInstancesService(apiClient);
        var applications = new AdminApplicationsService(apiClient);
        var homeworkReview = new AdminHomeworkReviewService(apiClient);
        var progress = new AdminProgressService(apiClient);
        var promoCodes = new AdminPromoCodesService(apiClient);
        var payments = new AdminPaymentsService(apiClient);
        var notifications = new AdminNotificationsService(apiClient);
        var settings = new AdminSettingsService(apiClient);
        var analytics = new AdminAnalyticsService(apiClient);
        var profile = new AdminProfileService(apiClient);
        var dictionaries = new AdminDictionariesService(apiClient);
        var auditLog = new AdminAuditLogService(apiClient);

        var login = new LoginWindow(auth, session, permissions, dashboard, students, employees, courses, instances, applications, homeworkReview, progress, promoCodes, payments, notifications, settings, analytics, profile, dictionaries, auditLog);
        login.Show();
    }
}
