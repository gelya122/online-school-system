using System.Windows;
using System.Windows.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Services;
using online_school_admin.ViewModels;
using online_school_admin.Views;

namespace online_school_admin.Views;

public partial class MainWindow : Window
{
    public MainWindow(AuthService auth, SessionService session, PermissionService permissions, AdminDashboardService dashboard, AdminStudentsService students, AdminEmployeesService employees, AdminCoursesService courses, AdminInstancesService instances, AdminApplicationsService applications, AdminHomeworkReviewService homeworkReview, AdminProgressService progress, AdminPromoCodesService promoCodes, AdminPaymentsService payments, AdminNotificationsService notifications, AdminSettingsService settings, AdminAnalyticsService analytics, AdminProfileService profile, AdminDictionariesService dictionaries, AdminAuditLogService auditLog)
    {
        InitializeComponent();
        var vm = new MainShellViewModel(auth, session, permissions, dashboard, students, employees, courses, instances, applications, homeworkReview, progress, promoCodes, payments, notifications, settings, analytics, profile, dictionaries, auditLog);
        DataContext = vm;

        var tokenWatch = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        tokenWatch.Tick += (_, _) =>
        {
            if (session.IsSignedIn && session.IsAccessTokenExpiredUtc())
            {
                AppLogger.Log("Session cleared: JWT expired");
                session.Clear();
            }
        };
        tokenWatch.Start();

        session.SignedOut += () =>
        {
            Dispatcher.Invoke(() =>
            {
                new LoginWindow(auth, session, permissions, dashboard, students, employees, courses, instances, applications, homeworkReview, progress, promoCodes, payments, notifications, settings, analytics, profile, dictionaries, auditLog).Show();
                Close();
            });
        };
    }
}
