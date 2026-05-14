using System.Net.Http;
using System.Windows;
using online_school_admin.Infrastructure;
using online_school_admin.Services;
using online_school_admin.Views;

namespace online_school_admin.ViewModels;

public sealed class LoginViewModel : BaseViewModel
{
    private readonly AuthService _auth;
    private readonly SessionService _session;
    private readonly PermissionService _permissions;
    private readonly AdminDashboardService _dashboard;
    private readonly AdminStudentsService _students;
    private readonly AdminEmployeesService _employees;
    private readonly AdminCoursesService _courses;
    private readonly AdminInstancesService _instances;
    private readonly AdminApplicationsService _applications;
    private readonly AdminHomeworkReviewService _homeworkReview;
    private readonly AdminProgressService _progress;
    private readonly AdminPromoCodesService _promoCodes;
    private readonly AdminPaymentsService _payments;
    private readonly AdminNotificationsService _notifications;
    private readonly AdminSettingsService _settings;
    private readonly AdminAnalyticsService _analytics;
    private readonly AdminProfileService _profile;
    private readonly AdminDictionariesService _dictionaries;
    private readonly AdminAuditLogService _auditLog;

    public LoginViewModel(AuthService auth, SessionService session, PermissionService permissions, AdminDashboardService dashboard, AdminStudentsService students, AdminEmployeesService employees, AdminCoursesService courses, AdminInstancesService instances, AdminApplicationsService applications, AdminHomeworkReviewService homeworkReview, AdminProgressService progress, AdminPromoCodesService promoCodes, AdminPaymentsService payments, AdminNotificationsService notifications, AdminSettingsService settings, AdminAnalyticsService analytics, AdminProfileService profile, AdminDictionariesService dictionaries, AdminAuditLogService auditLog)
    {
        _auth = auth;
        _session = session;
        _permissions = permissions;
        _dashboard = dashboard;
        _students = students;
        _employees = employees;
        _courses = courses;
        _instances = instances;
        _applications = applications;
        _homeworkReview = homeworkReview;
        _progress = progress;
        _promoCodes = promoCodes;
        _payments = payments;
        _notifications = notifications;
        _settings = settings;
        _analytics = analytics;
        _profile = profile;
        _dictionaries = dictionaries;
        _auditLog = auditLog;
        LoginCommand = new RelayCommand(async _ => await DoLoginAsync(), _ => !IsBusy);
        OpenRegisterCommand = new RelayCommand(_ => OpenRegister(), _ => !IsBusy);
    }

    private string _email = "";
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password = "";
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoginCommand.RaiseCanExecuteChanged();
                OpenRegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand LoginCommand { get; }
    public RelayCommand OpenRegisterCommand { get; }

    private async Task DoLoginAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await _auth.SignInAsync(Email.Trim(), Password, cancellationToken);

            var main = new MainWindow(_auth, _session, _permissions, _dashboard, _students, _employees, _courses, _instances, _applications, _homeworkReview, _progress, _promoCodes, _payments, _notifications, _settings, _analytics, _profile, _dictionaries, _auditLog);
            main.Show();

            var loginWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            loginWindow?.Close();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Login");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Login");
        }
        catch (Exception ex)
        {
            ErrorMessage = ApiErrorFormatter.Format(ex);
            AppLogger.Log(ex, "Login");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenRegister()
    {
        var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
        var dlg = new RegisterWindow(_auth) { Owner = owner };
        dlg.ShowDialog();
    }
}
