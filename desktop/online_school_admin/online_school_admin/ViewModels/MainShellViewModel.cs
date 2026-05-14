using System.Collections.ObjectModel;
using System.Windows;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class MainShellViewModel : BaseViewModel
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

    public ObservableCollection<SidebarNavItem> NavItems { get; } = new();

    private SidebarNavItem? _selectedNav;
    public SidebarNavItem? SelectedNav
    {
        get => _selectedNav;
        set
        {
            if (SetProperty(ref _selectedNav, value) && value != null)
                NavigateTo(value);
        }
    }

    public NavigationService Navigation { get; } = new();

    private string _userDisplayName = "";
    public string UserDisplayName
    {
        get => _userDisplayName;
        set => SetProperty(ref _userDisplayName, value);
    }

    private string _userEmail = "";
    public string UserEmail
    {
        get => _userEmail;
        set => SetProperty(ref _userEmail, value);
    }

    private string _userRole = "";
    public string UserRole
    {
        get => _userRole;
        set
        {
            if (SetProperty(ref _userRole, value))
                UserRoleLine = string.IsNullOrWhiteSpace(value) ? "" : $"Роль: {value}";
        }
    }

    private string _userRoleLine = "";
    public string UserRoleLine
    {
        get => _userRoleLine;
        private set => SetProperty(ref _userRoleLine, value);
    }

    public RelayCommand LogoutCommand { get; }

    public MainShellViewModel(AuthService auth, SessionService session, PermissionService permissions, AdminDashboardService dashboard, AdminStudentsService students, AdminEmployeesService employees, AdminCoursesService courses, AdminInstancesService instances, AdminApplicationsService applications, AdminHomeworkReviewService homeworkReview, AdminProgressService progress, AdminPromoCodesService promoCodes, AdminPaymentsService payments, AdminNotificationsService notifications, AdminSettingsService settings, AdminAnalyticsService analytics, AdminProfileService profile, AdminDictionariesService dictionaries, AdminAuditLogService auditLog)
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

        LogoutCommand = new RelayCommand(async _ => await _auth.LogoutAsync(), _ => true);

        AddNavIfAllowed("home", "Главная", "Сводка");
        AddNavIfAllowed("employees", "Сотрудники", "");
        AddNavIfAllowed("students", "Студенты", "");
        AddNavIfAllowed("courses", "Курсы", "");
        AddNavIfAllowed("streams", "Потоки", "");
        AddNavIfAllowed("applications", "Заявки", "");
        AddNavIfAllowed("my-applications", "Мои заявки", "");
        AddNavIfAllowed("homework-review", "Проверка ДЗ", "");
        AddNavIfAllowed("progress", "Прогресс студента", "");
        AddNavIfAllowed("payments", "Платежи", "");
        AddNavIfAllowed("promo", "Промокоды", "");
        AddNavIfAllowed("notifications", "Уведомления", "");
        AddNavIfAllowed("analytics", "Аналитика", "");
        AddNavIfAllowed("site-settings", "Настройки сайта", "");
        AddNavIfAllowed("school-settings", "Настройки школы", "");
        AddNavIfAllowed("dictionaries", "Справочники", "");
        AddNavIfAllowed("audit-log", "Журнал действий", "");
        AddNavIfAllowed("profile", "Личный кабинет", "");

        RefreshWelcome();
        SelectedNav = NavItems.FirstOrDefault();
    }

    private void AddNavIfAllowed(string id, string title, string subtitle)
    {
        if (_permissions.CanViewSection(id))
            NavItems.Add(new SidebarNavItem { Id = id, Title = title, Subtitle = subtitle });
    }

    public void RefreshWelcome()
    {
        var u = _session.CurrentUser;
        if (u == null)
        {
            UserDisplayName = "";
            UserEmail = "";
            UserRoleLine = "";
            UserRole = "";
            return;
        }
        UserDisplayName = u.DisplayName;
        UserEmail = u.Email;
        UserRole = (u.RoleName ?? "").Trim();
    }

    private void NavigateTo(SidebarNavItem item)
    {
        if (item.Id == "home")
        {
            var vm = new DashboardViewModel(_dashboard);
            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "students")
        {
            var vm = new StudentsViewModel(_students);
            vm.AddRequested += () =>
            {
                var edit = new StudentEditViewModel(_students, null);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
            };
            vm.OpenRequested += (id) =>
            {
                var role = (_session.CurrentUser?.RoleName ?? "").Trim().ToLowerInvariant();
                if (role == "teacher")
                {
                    var p = new StudentProgressForStudentViewModel(_progress, id);
                    p.OpenEnrollmentRequested += (enrollmentId) =>
                    {
                        var details = new StudentProgressDetailsViewModel(_progress, enrollmentId);
                        Navigation.Navigate(details);
                        _ = details.LoadAsync();
                    };
                    Navigation.Navigate(p);
                    _ = p.LoadAsync();
                }
                else
                {
                    var details = new StudentDetailsViewModel(_students, id);
                    Navigation.Navigate(details);
                    _ = details.LoadAsync();
                }
            };
            vm.EditRequested += (id) =>
            {
                var edit = new StudentEditViewModel(_students, id);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                _ = edit.LoadAsync();
            };
            vm.EnrollRequested += (studentId) =>
            {
                var enroll = new EnrollStudentToInstanceViewModel(_instances, _students, studentId);
                enroll.Enrolled += () => NavigateTo(item);
                enroll.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(enroll);
                _ = enroll.LoadAsync();
            };
            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "employees")
        {
            var vm = new EmployeesViewModel(_employees, _auth);
            _ = vm.InitializeAsync();

            vm.AddRequested += () =>
            {
                var edit = new EmployeeEditViewModel(_employees, _auth, null);
                _ = edit.InitializeAsync();
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
            };

            vm.OpenRequested += (id) =>
            {
                var details = new EmployeeDetailsViewModel(_employees, id);
                Navigation.Navigate(details);
                _ = details.LoadAsync();
            };

            vm.EditRequested += (id) =>
            {
                var edit = new EmployeeEditViewModel(_employees, _auth, id);
                _ = edit.InitializeAsync();
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                _ = edit.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "courses")
        {
            var vm = new CoursesViewModel(_courses);
            _ = vm.InitializeAsync();

            void WireCourseDetails(CourseDetailsViewModel details)
            {
                details.NavigateBackRequested += () =>
                {
                    Navigation.Navigate(vm);
                    _ = vm.LoadAsync();
                };

                details.CreateInstanceRequested += async courseId =>
                {
                    var edit = new InstanceEditViewModel(_instances, _courses, _employees, null);
                    edit.Saved += () =>
                    {
                        if (edit.LastCreatedInstanceId is int nid)
                        {
                            var shell = new InstanceDetailsViewModel(_instances, _students, _employees, _courses, _payments, nid);
                            Navigation.Navigate(shell);
                            _ = shell.LoadAsync();
                        }
                        else
                        {
                            Navigation.Navigate(details);
                            _ = details.LoadAsync();
                        }
                    };
                    edit.CancelRequested += () =>
                    {
                        Navigation.Navigate(details);
                        _ = details.LoadAsync();
                    };
                    Navigation.Navigate(edit);
                    await edit.InitializeAsync();
                    edit.PreselectCourse(courseId);
                };

                details.OpenStreamRequested += streamId =>
                {
                    var shell = new InstanceDetailsViewModel(_instances, _students, _employees, _courses, _payments, streamId);
                    Navigation.Navigate(shell);
                    _ = shell.LoadAsync();
                };
            }

            vm.AddRequested += () =>
            {
                var details = new CourseDetailsViewModel(_courses, _instances, null, CourseDetailsPageMode.Create);
                WireCourseDetails(details);
                Navigation.Navigate(details);
                _ = details.InitializeAsync();
            };

            vm.OpenRequested += id =>
            {
                var details = new CourseDetailsViewModel(_courses, _instances, id, CourseDetailsPageMode.View);
                WireCourseDetails(details);
                Navigation.Navigate(details);
                _ = details.InitializeAsync();
                _ = details.LoadAsync();
            };

            vm.EditRequested += id =>
            {
                var details = new CourseDetailsViewModel(_courses, _instances, id, CourseDetailsPageMode.Edit);
                WireCourseDetails(details);
                Navigation.Navigate(details);
                _ = details.InitializeAsync();
                _ = details.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "streams")
        {
            var vm = new InstancesViewModel(_instances, _courses, _permissions);
            _ = vm.InitializeAsync();

            vm.AddRequested += () =>
            {
                var edit = new InstanceEditViewModel(_instances, _courses, _employees, null);
                _ = edit.InitializeAsync();
                edit.Saved += () =>
                {
                    if (edit.LastCreatedInstanceId is int nid)
                    {
                        var d = new InstanceDetailsViewModel(_instances, _students, _employees, _courses, _payments, nid, viewOnly: true);
                        Navigation.Navigate(d);
                        _ = d.LoadAsync();
                    }
                    else
                    {
                        NavigateTo(item);
                    }
                };
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
            };

            vm.OpenRequested += streamId =>
            {
                InstanceDetailsViewModel CreateDetails()
                    => new InstanceDetailsViewModel(_instances, _students, _employees, _courses, _payments, streamId, viewOnly: true,
                        OnOpenStudentFromStream);

                void OnOpenStudentFromStream(int studentId)
                {
                    var card = new EnrolledStudentDetailsViewModel(_students, studentId, streamId, () =>
                    {
                        var again = CreateDetails();
                        Navigation.Navigate(again);
                        _ = again.LoadAsync();
                    });
                    Navigation.Navigate(card);
                    _ = card.LoadAsync();
                }

                var details = CreateDetails();
                Navigation.Navigate(details);
                _ = details.LoadAsync();
            };

            vm.EditRequested += async id =>
            {
                var edit = new InstanceEditViewModel(_instances, _courses, _employees, id);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                await edit.InitializeAsync();
                await edit.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "applications" || item.Id == "my-applications")
        {
            var startMine = item.Id == "my-applications";
            var vm = new ApplicationsViewModel(_applications, _employees, _courses, _session, _permissions, startMine);
            _ = vm.InitializeAsync();

            vm.AddRequested += () =>
            {
                var edit = new ApplicationEditViewModel(_applications, _employees, _courses, null);
                _ = edit.InitializeAsync();
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
            };

            vm.OpenRequested += (id) =>
            {
                ApplicationDetailsViewModel? detailsRef = null;
                var details = new ApplicationDetailsViewModel(_applications, _employees, _session, _permissions, id,
                    (studentId, courseId) =>
                    {
                        var c = new OrderCreateViewModel(_payments);
                        c.PrefillFromApplication(studentId, courseId);
                        c.Saved += () =>
                        {
                            if (detailsRef != null)
                            {
                                Navigation.Navigate(detailsRef);
                                _ = detailsRef.LoadAsync();
                            }
                        };
                        c.CancelRequested += () =>
                        {
                            if (detailsRef != null)
                            {
                                Navigation.Navigate(detailsRef);
                                _ = detailsRef.LoadAsync();
                            }
                        };
                        Navigation.Navigate(c);
                    },
                    studentId =>
                    {
                        var enroll = new EnrollStudentToInstanceViewModel(_instances, _students, studentId);
                        enroll.Enrolled += () =>
                        {
                            if (detailsRef != null)
                            {
                                Navigation.Navigate(detailsRef);
                                _ = detailsRef.LoadAsync();
                            }
                        };
                        enroll.CancelRequested += () =>
                        {
                            if (detailsRef != null)
                                Navigation.Navigate(detailsRef);
                        };
                        Navigation.Navigate(enroll);
                        _ = enroll.LoadAsync();
                    });
                detailsRef = details;
                Navigation.Navigate(details);
                _ = details.LoadAsync();
            };

            vm.EditRequested += (id) =>
            {
                var edit = new ApplicationEditViewModel(_applications, _employees, _courses, id);
                _ = edit.InitializeAsync();
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                _ = edit.LoadAsync();
            };

            Navigation.Navigate(vm);
            return;
        }

        if (item.Id == "homework-review")
        {
            var vm = new HomeworkReviewViewModel(_homeworkReview, _courses, _employees, _instances);
            _ = vm.InitializeAsync();

            vm.ReviewRequested += (submissionId, studentAnswerId) =>
            {
                var wvm = new HomeworkAnswerReviewWindowViewModel(_homeworkReview, submissionId, studentAnswerId);
                var w = new Views.HomeworkAnswerReviewWindow(wvm)
                {
                    Owner = Application.Current.MainWindow
                };
                _ = wvm.LoadAsync();
                var ok = w.ShowDialog();
                if (ok == true)
                {
                    _ = vm.LoadAsync();
                    MessageBox.Show("Ответ проверен.", "Проверка ДЗ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "progress")
        {
            var vm = new StudentProgressViewModel(_progress, _courses, _instances);
            vm.OpenRequested += (enrollmentId) =>
            {
                var details = new StudentProgressDetailsViewModel(_progress, enrollmentId);
                Navigation.Navigate(details);
                _ = details.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = OpenStudentProgressPageAsync(vm);
            return;
        }

        if (item.Id == "promo")
        {
            var vm = new PromoCodesViewModel(_promoCodes, _permissions);

            vm.AddRequested += () =>
            {
                var edit = new PromoCodeEditViewModel(_promoCodes, _courses, _instances, null);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                _ = edit.InitializeAsync();
            };

            vm.EditRequested += (id) =>
            {
                var edit = new PromoCodeEditViewModel(_promoCodes, _courses, _instances, id);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                _ = edit.InitializeAsync();
            };

            vm.UsagesRequested += (id, code) =>
            {
                var u = new PromoCodeUsagesViewModel(_promoCodes, id, code);
                u.BackRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(u);
                _ = u.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "payments")
        {
            var orders = new OrdersTabViewModel(_payments);
            var paymentsVm = new PaymentsTabViewModel(_payments);
            var inst = new InstallmentsTabViewModel(_payments);

            var vm = new PaymentsViewModel(orders, paymentsVm, inst);

            orders.OpenRequested += (orderId) =>
            {
                var d = new OrderDetailsViewModel(_payments, orderId);
                d.BackRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(d);
                _ = d.LoadAsync();
            };

            orders.CreateRequested += () =>
            {
                var c = new OrderCreateViewModel(_payments);
                c.Saved += () => NavigateTo(item);
                c.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(c);
            };

            paymentsVm.CreateRequested += (prefillOrderId) =>
            {
                var c = new PaymentCreateViewModel(_payments, prefillOrderId);
                c.Saved += () => NavigateTo(item);
                c.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(c);
            };

            inst.CreatePlanRequested += () =>
            {
                var c = new InstallmentPlanCreateViewModel(_payments);
                c.Saved += () => NavigateTo(item);
                c.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(c);
            };

            inst.OpenPlanRequested += (planId) =>
            {
                var d = new InstallmentDetailsViewModel(_payments, planId);
                d.BackRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(d);
                _ = d.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = orders.LoadAsync();
            _ = paymentsVm.LoadAsync();
            _ = inst.LoadAsync();
            return;
        }

        if (item.Id == "notifications")
        {
            var n = new NotificationsTabViewModel(_notifications);
            var c = new MailingCampaignsTabViewModel(_notifications);
            var vm = new NotificationsViewModel(n, c);

            c.AddRequested += () =>
            {
                var edit = new MailingCampaignEditViewModel(_notifications, null);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
            };

            c.EditRequested += (id) =>
            {
                var edit = new MailingCampaignEditViewModel(_notifications, id);
                edit.Saved += () => NavigateTo(item);
                edit.CancelRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(edit);
                _ = edit.InitializeAsync();
            };

            c.RecipientsRequested += (id, title) =>
            {
                var r = new MailingRecipientsViewModel(_notifications, id, title);
                r.BackRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(r);
                _ = r.LoadAsync();
            };

            Navigation.Navigate(vm);
            _ = n.LoadAsync();
            _ = c.LoadAsync();
            return;
        }

        if (item.Id == "school-settings")
        {
            var vm = new SchoolSettingsViewModel(_settings);
            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "site-settings")
        {
            var vm = new SiteSettingsViewModel(_settings);
            vm.OpenBannersRequested += () =>
            {
                var b = new SiteBannersViewModel(_settings);
                b.BackRequested += () => Navigation.Navigate(vm);
                b.AddRequested += () =>
                {
                    var edit = new SiteBannerEditViewModel(_settings, null);
                    edit.Saved += async () => { Navigation.Navigate(b); await b.LoadAsync(); };
                    edit.CancelRequested += () => Navigation.Navigate(b);
                    Navigation.Navigate(edit);
                };
                b.EditRequested += (id) =>
                {
                    var edit = new SiteBannerEditViewModel(_settings, id);
                    edit.Saved += async () => { Navigation.Navigate(b); await b.LoadAsync(); };
                    edit.CancelRequested += () => Navigation.Navigate(b);
                    Navigation.Navigate(edit);
                    _ = edit.InitializeAsync();
                };
                Navigation.Navigate(b);
                _ = b.LoadAsync();
            };
            vm.OpenFaqRequested += () =>
            {
                var f = new FaqAdminViewModel(_settings);
                f.BackRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(f);
                _ = f.LoadAsync();
            };
            vm.OpenReviewsRequested += () =>
            {
                var r = new ReviewsAdminViewModel(_settings);
                r.BackRequested += () => Navigation.Navigate(vm);
                Navigation.Navigate(r);
                _ = r.LoadAsync();
            };
            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "analytics")
        {
            var vm = new AnalyticsViewModel(_analytics);
            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "profile")
        {
            var vm = new ProfileViewModel(_profile, _auth);
            Navigation.Navigate(vm);
            _ = vm.LoadAsync();
            return;
        }

        if (item.Id == "dictionaries")
        {
            var vm = new DictionariesViewModel(_dictionaries);
            Navigation.Navigate(vm);
            _ = vm.InitializeAsync();
            return;
        }

        if (item.Id == "audit-log")
        {
            var vm = new AuditLogViewModel(_auditLog, _employees);
            Navigation.Navigate(vm);
            _ = vm.InitializeAsync();
            return;
        }

        Navigation.Navigate(new PlaceholderPageViewModel { Title = item.Title });
    }

    private static async Task OpenStudentProgressPageAsync(StudentProgressViewModel vm)
    {
        await vm.InitializeAsync();
        await vm.LoadAsync();
    }
}

