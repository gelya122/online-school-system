using System.Windows;
using online_school_admin.Services;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(AuthService auth, SessionService session, PermissionService permissions, AdminDashboardService dashboard, AdminStudentsService students, AdminEmployeesService employees, AdminCoursesService courses, AdminInstancesService instances, AdminApplicationsService applications, AdminHomeworkReviewService homeworkReview, AdminProgressService progress, AdminPromoCodesService promoCodes, AdminPaymentsService payments, AdminNotificationsService notifications, AdminSettingsService settings, AdminAnalyticsService analytics, AdminProfileService profile, AdminDictionariesService dictionaries, AdminAuditLogService auditLog)
    {
        InitializeComponent();
        DataContext = new LoginViewModel(auth, session, permissions, dashboard, students, employees, courses, instances, applications, homeworkReview, progress, promoCodes, payments, notifications, settings, analytics, profile, dictionaries, auditLog);
    }
}
