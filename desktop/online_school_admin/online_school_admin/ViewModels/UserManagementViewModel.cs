using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    public ObservableCollection<StudentListItem> Students { get; } = new();
    public ICollectionView FilteredStudents { get; }

    public ObservableCollection<string> ClassFilterOptions { get; } = ["Все классы"];

    public ObservableCollection<string> StatisticsFilterOptions { get; } =
    [
        "Любая активность",
        "Высокая активность",
        "Средняя активность",
        "Низкая активность"
    ];

    public ObservableCollection<string> RegistrationDateFilterOptions { get; } =
    [
        "Любая дата",
        "Сегодня",
        "За 7 дней",
        "За 30 дней",
        "Старше 30 дней"
    ];

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string _selectedClassFilter = "Все классы";

    [ObservableProperty]
    private string _selectedStatisticsFilter = "Любая активность";

    [ObservableProperty]
    private string _selectedRegistrationDateFilter = "Любая дата";

    [ObservableProperty]
    private StudentListItem? _selectedStudent;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public event Action<StudentListItem>? ProfileRequested;

    public int TotalStudentsCount => Students.Count;

    public int NewStudentsCount => Students.Count(x => x.RegisteredAt.HasValue && x.RegisteredAt.Value.Date >= DateTime.Today.AddDays(-30));

    public UserManagementViewModel()
    {
        Students.CollectionChanged += OnStudentsChanged;
        FilteredStudents = CollectionViewSource.GetDefaultView(Students);
        FilteredStudents.Filter = FilterStudent;
        FilteredStudents.SortDescriptions.Add(new SortDescription(nameof(StudentListItem.RegisteredAt), ListSortDirection.Descending));
    }

    public async Task<AdminDataSnapshot> LoadAsync(AuthApiService api, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var snapshot = await api.GetAdminDataSnapshotAsync(cancellationToken);
            SetStudents(snapshot.Students);
            return snapshot;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SetStudents(IEnumerable<StudentListItem> students)
    {
        Students.Clear();
        foreach (var student in students)
            Students.Add(student);

        RebuildClassFilters();
        FilteredStudents.Refresh();
        SelectedStudent = Students.FirstOrDefault();
        OnPropertyChanged(nameof(TotalStudentsCount));
        OnPropertyChanged(nameof(NewStudentsCount));
    }

    partial void OnSearchTextChanged(string value) => FilteredStudents.Refresh();
    partial void OnSelectedClassFilterChanged(string value) => FilteredStudents.Refresh();
    partial void OnSelectedStatisticsFilterChanged(string value) => FilteredStudents.Refresh();
    partial void OnSelectedRegistrationDateFilterChanged(string value) => FilteredStudents.Refresh();

    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = "";
        SelectedClassFilter = "Все классы";
        SelectedStatisticsFilter = "Любая активность";
        SelectedRegistrationDateFilter = "Любая дата";
    }

    [RelayCommand]
    private void SendEmailToSelected()
    {
        var selected = Students.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Выберите хотя бы одного ученика для рассылки.", "Массовая операция", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MessageBox.Show($"Подготовлена рассылка для {selected.Count} учеников.", "Массовая операция", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void AddToCourseSelected()
    {
        var selected = Students.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Выберите учеников для добавления на курс.", "Массовая операция", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MessageBox.Show($"Открыт мастер добавления на курс для {selected.Count} учеников.", "Массовая операция", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void OpenStudentProfile()
    {
        if (SelectedStudent == null)
        {
            MessageBox.Show("Сначала выберите ученика в таблице.", "Профиль ученика", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        ProfileRequested?.Invoke(SelectedStudent);
    }

    private bool FilterStudent(object obj)
    {
        if (obj is not StudentListItem student)
            return false;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.Trim();
            var byName = student.FullName.Contains(q, StringComparison.OrdinalIgnoreCase);
            var byEmail = student.Email.Contains(q, StringComparison.OrdinalIgnoreCase);
            var byPhone = student.Phone.Contains(q, StringComparison.OrdinalIgnoreCase);
            if (!byName && !byEmail && !byPhone)
                return false;
        }

        if (SelectedClassFilter != "Все классы" && !string.Equals(student.ClassName, SelectedClassFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        if (SelectedStatisticsFilter != "Любая активность" && !string.Equals(student.ActivityStatus, SelectedStatisticsFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!MatchRegistrationPeriod(student.RegisteredAt))
            return false;

        return true;
    }

    private bool MatchRegistrationPeriod(DateTime? registeredAt)
    {
        if (SelectedRegistrationDateFilter == "Любая дата")
            return true;

        if (!registeredAt.HasValue)
            return false;

        var date = registeredAt.Value.Date;
        var today = DateTime.Today;

        return SelectedRegistrationDateFilter switch
        {
            "Сегодня" => date == today,
            "За 7 дней" => date >= today.AddDays(-7),
            "За 30 дней" => date >= today.AddDays(-30),
            "Старше 30 дней" => date < today.AddDays(-30),
            _ => true
        };
    }

    private void RebuildClassFilters()
    {
        var current = SelectedClassFilter;
        ClassFilterOptions.Clear();
        ClassFilterOptions.Add("Все классы");

        foreach (var className in Students
                     .Select(x => x.ClassName)
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            ClassFilterOptions.Add(className);
        }

        SelectedClassFilter = ClassFilterOptions.Contains(current) ? current : "Все классы";
    }

    private void OnStudentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalStudentsCount));
        OnPropertyChanged(nameof(NewStudentsCount));
    }
}
