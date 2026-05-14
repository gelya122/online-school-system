using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;
using online_school_admin.Views;

namespace online_school_admin.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AuthApiService _api;

    public ObservableCollection<SidebarNavItem> NavItems { get; } = new();

    [ObservableProperty]
    private SidebarNavItem? _selectedNav;

    [ObservableProperty]
    private string _welcomeLine = "";

    [ObservableProperty]
    private string _contentTitle = "";

    [ObservableProperty]
    private string _contentHint = "";

    [ObservableProperty]
    private bool _isUsersSectionActive;

    [ObservableProperty]
    private bool _isCoursesSectionActive;

    [ObservableProperty]
    private string? _loadError;

    public UserManagementViewModel UsersModule { get; }

    public CourseManagementViewModel CoursesModule { get; }

    [ObservableProperty] private int _totalUsers;
    [ObservableProperty] private int _newUsers;
    [ObservableProperty] private int _activeCourses;
    [ObservableProperty] private int _newTrialApplications;
    [ObservableProperty] private int _ordersInPayment;
    [ObservableProperty] private int _homeworkPendingReview;

    public MainViewModel(AuthApiService api)
    {
        _api = api;
        UsersModule = new UserManagementViewModel();
        UsersModule.ProfileRequested += OnStudentProfileRequested;
        CoursesModule = new CourseManagementViewModel(api);

        NavItems.Add(new SidebarNavItem { Id = "dashboard", Title = "Дашборд" });
        NavItems.Add(new SidebarNavItem { Id = "users", Title = "Пользователи" });
        NavItems.Add(new SidebarNavItem { Id = "courses", Title = "Курсы" });
        NavItems.Add(new SidebarNavItem { Id = "trials", Title = "Заявки на пробные" });
        NavItems.Add(new SidebarNavItem { Id = "orders", Title = "Заказы" });
        NavItems.Add(new SidebarNavItem { Id = "homework", Title = "ДЗ на проверку" });
        NavItems.Add(new SidebarNavItem { Id = "progress", Title = "Прогресс студента" });
        NavItems.Add(new SidebarNavItem { Id = "mailings", Title = "Рассылки" });
        NavItems.Add(new SidebarNavItem { Id = "settings", Title = "Настройки" });

        RefreshWelcome();
        ApplyStatsToNavItems();
        NavigateTo("dashboard");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LoadError = null;
            var snapshot = await UsersModule.LoadAsync(_api, cancellationToken);
            TotalUsers = UsersModule.TotalStudentsCount;
            NewUsers = UsersModule.NewStudentsCount;
            ActiveCourses = snapshot.ActiveCoursesCount;
            NewTrialApplications = snapshot.NewTrialApplicationsCount;
            OrdersInPayment = snapshot.OrdersInPaymentCount;
            HomeworkPendingReview = snapshot.HomeworkPendingReviewCount;

            await CoursesModule.ReloadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
            MessageBox.Show(
                $"Не удалось загрузить данные админки из API.\n\n{ex.Message}",
                "Ошибка загрузки",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        finally
        {
            ApplyStatsToNavItems();
        }
    }

    public void RefreshWelcome()
    {
        if (!AdminSession.IsSignedIn || AdminSession.Current is not { } c)
        {
            WelcomeLine = "";
            return;
        }

        var role = string.IsNullOrWhiteSpace(c.RoleLabel) ? "роль" : c.RoleLabel!;
        WelcomeLine = $"{AdminSession.DisplayName}  ·  {c.Email}  ·  {role}";
    }

    partial void OnSelectedNavChanged(SidebarNavItem? value)
    {
        if (value != null)
            ApplyContentForNavId(value.Id);
    }

    partial void OnTotalUsersChanged(int value) => ApplyStatsToNavItems();
    partial void OnNewUsersChanged(int value) => ApplyStatsToNavItems();
    partial void OnActiveCoursesChanged(int value) => ApplyStatsToNavItems();
    partial void OnNewTrialApplicationsChanged(int value) => ApplyStatsToNavItems();
    partial void OnOrdersInPaymentChanged(int value) => ApplyStatsToNavItems();
    partial void OnHomeworkPendingReviewChanged(int value) => ApplyStatsToNavItems();

    private void ApplyStatsToNavItems()
    {
        foreach (var item in NavItems)
        {
            item.Subtitle = item.Id switch
            {
                "dashboard" => "Сводная информация",
                "users" => $"Всего: {TotalUsers} · Новых: {NewUsers}",
                "courses" => $"Активных: {ActiveCourses}",
                "trials" => $"Новых: {NewTrialApplications}",
                "orders" => $"В оплате: {OrdersInPayment}",
                "homework" => $"На проверке: {HomeworkPendingReview}",
                "progress" => "По курсам и урокам",
                "mailings" => "Кампании и шаблоны",
                "settings" => "Система и школа",
                _ => ""
            };
        }
    }

    [RelayCommand]
    private void NavigateTo(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        var nav = NavItems.FirstOrDefault(x => x.Id == id);
        if (nav != null)
        {
            SelectedNav = nav;
            return;
        }

        SelectedNav = null;
        ApplyContentForStandaloneId(id.Trim());
    }

    [RelayCommand]
    private void FileExport()
    {
        MessageBox.Show(
            "Экспорт данных будет доступен после подключения отчётов к API.",
            "Экспорт",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void FileBackup()
    {
        MessageBox.Show(
            "Резервное копирование настраивается на сервере БД. Здесь появится запуск сценария или ссылка на документацию.",
            "Резервная копия",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private static void FileExit() => Application.Current.Shutdown();

    private void ApplyContentForNavId(string id)
    {
        var (title, hint) = id switch
        {
            "dashboard" => ("Дашборд", "Сводные показатели и быстрые действия."),
            "users" => ("Управление пользователями", "Таблица учеников, поиск, фильтры, массовые действия и профиль."),
            "courses" => ("Курсы", "Шаблоны курсов (Course) и потоки (CourseInstance): фильтры, создание и редактирование."),
            "trials" => ("Заявки на пробные уроки", "Обработка входящих заявок."),
            "orders" => ("Заказы", "Оплаты, статусы, выставление счетов."),
            "homework" => ("ДЗ на проверку", "Очередь работ студентов."),
            "progress" => ("Прогресс студента", "Прохождение уроков и успеваемость."),
            "mailings" => ("Рассылки", "Email и другие каналы."),
            "settings" => ("Настройки", "Параметры системы и школы."),
            _ => ("Раздел", "Содержимое появится здесь.")
        };

        IsUsersSectionActive = id == "users";
        IsCoursesSectionActive = id == "courses";
        ContentTitle = title;
        ContentHint = hint;
    }

    private void ApplyContentForStandaloneId(string id)
    {
        var (title, hint) = id switch
        {
            "lessons" => ("Уроки", "Редактор уроков, расписание, привязка к курсам."),
            "homework-materials" => ("Домашние задания", "Задания по урокам, дедлайны, шаблоны."),
            "notifications" => ("Уведомления", "Push, email-триггеры, журнал доставки."),
            "reports" => ("Отчёты", "Выгрузки и регламентные отчёты."),
            "statistics" => ("Статистика", "Воронки, посещаемость, конверсии."),
            "settings-system" => ("Настройки: система", "Безопасность, логи, окружение."),
            "settings-school" => ("Настройки: школа", "Реквизиты, бренд, контакты."),
            "settings-integrations" => ("Настройки: интеграции", "Платежи, CRM, внешние API."),
            _ => ("Раздел", "Содержимое появится здесь.")
        };

        IsUsersSectionActive = false;
        IsCoursesSectionActive = false;
        ContentTitle = title;
        ContentHint = hint;
    }

    private static void OnStudentProfileRequested(StudentListItem student)
    {
        var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;
        var profile = new StudentProfileWindow(student);
        if (owner != null)
            profile.Owner = owner;
        profile.ShowDialog();
    }
}
