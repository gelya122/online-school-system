using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using online_school_admin.Models.Admin;
using online_school_admin.Services;
using online_school_admin.Views;

namespace online_school_admin.ViewModels;

public sealed class CourseTemplateRow
{
    public CourseTemplateRow(CourseTemplateDto dto, string subjectName, string examName)
    {
        Dto = dto;
        SubjectName = subjectName;
        ExamName = examName;
    }

    public CourseTemplateDto Dto { get; }
    public string SubjectName { get; }
    public string ExamName { get; }
}

public sealed class CourseInstanceRow
{
    public CourseInstanceRow(CourseInstanceDto instance, string courseTitle)
    {
        Instance = instance;
        CourseTitle = courseTitle;
    }

    public CourseInstanceDto Instance { get; }
    public string CourseTitle { get; }
}

public partial class CourseManagementViewModel : ObservableObject
{
    private readonly AuthApiService _api;

    public ObservableCollection<CourseTemplateRow> Templates { get; } = new();
    public ICollectionView FilteredTemplates { get; }

    public ObservableCollection<CourseInstanceRow> Instances { get; } = new();
    public ICollectionView FilteredInstances { get; }

    public ObservableCollection<string> SubjectFilterOptions { get; } = ["Все предметы"];
    public ObservableCollection<string> TrackFilterOptions { get; } = ["Все программы", "ОГЭ (8–9)", "ЕГЭ (10–11)"];
    public ObservableCollection<string> StatusFilterOptions { get; } = ["Любой статус", "Активен", "Скрыт"];

    [ObservableProperty] private string _selectedSubjectFilter = "Все предметы";
    [ObservableProperty] private string _selectedTrackFilter = "Все программы";
    [ObservableProperty] private string _selectedStatusFilter = "Любой статус";
    [ObservableProperty] private string _instanceSearch = "";
    [ObservableProperty] private CourseTemplateRow? _selectedTemplate;
    [ObservableProperty] private CourseInstanceRow? _selectedInstance;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    private readonly Dictionary<int, string> _subjectNames = new();
    private readonly Dictionary<int, string> _examNames = new();
    private readonly Dictionary<int, int> _courseSubject = new();
    private readonly Dictionary<int, int?> _courseExam = new();

    public CourseManagementViewModel(AuthApiService api)
    {
        _api = api;
        FilteredTemplates = CollectionViewSource.GetDefaultView(Templates);
        FilteredTemplates.Filter = FilterTemplate;
        FilteredInstances = CollectionViewSource.GetDefaultView(Instances);
        FilteredInstances.Filter = FilterInstance;
    }

    partial void OnSelectedSubjectFilterChanged(string value) => FilteredTemplates.Refresh();
    partial void OnSelectedTrackFilterChanged(string value) => FilteredTemplates.Refresh();
    partial void OnSelectedStatusFilterChanged(string value) => FilteredTemplates.Refresh();
    partial void OnInstanceSearchChanged(string value) => FilteredInstances.Refresh();

    [RelayCommand]
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var courses = await _api.GetCoursesAsync(cancellationToken);
            var subjects = await _api.GetSubjectsAsync(cancellationToken);
            var exams = await _api.GetExamsAsync(cancellationToken);
            var instances = await _api.GetCourseInstancesAsync(cancellationToken);

            _subjectNames.Clear();
            foreach (var s in subjects)
                _subjectNames[s.SubjectId] = s.SubjectName;

            _examNames.Clear();
            foreach (var e in exams)
                _examNames[e.ExamId] = e.ExamName;

            _courseSubject.Clear();
            _courseExam.Clear();
            foreach (var c in courses)
            {
                if (c.SubjectId is { } sid)
                    _courseSubject[c.CourseId] = sid;
                _courseExam[c.CourseId] = c.ExamId;
            }

            RebuildSubjectFilterOptions();
            Templates.Clear();
            foreach (var c in courses.OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase))
            {
                var subj = c.SubjectId is { } s && _subjectNames.TryGetValue(s, out var sn) ? sn : "—";
                var ex = c.ExamId is { } eid && _examNames.TryGetValue(eid, out var en) ? en : "—";
                Templates.Add(new CourseTemplateRow(c, subj, ex));
            }

            var courseTitles = courses.ToDictionary(x => x.CourseId, x => x.Title);
            Instances.Clear();
            foreach (var i in instances.OrderByDescending(x => x.StartDate))
            {
                var title = courseTitles.GetValueOrDefault(i.CourseId, $"Курс #{i.CourseId}");
                Instances.Add(new CourseInstanceRow(i, title));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Курсы", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            IsLoading = false;
            FilteredTemplates.Refresh();
            FilteredInstances.Refresh();
        }
    }

    [RelayCommand]
    private void OpenTemplateEditor()
    {
        if (SelectedTemplate == null)
        {
            MessageBox.Show("Выберите шаблон курса в списке.", "Курсы", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var w = new CourseEditorWindow(_api, SelectedTemplate.Dto.CourseId) { Owner = Application.Current.MainWindow };
        w.ShowDialog();
        _ = ReloadAsync();
    }

    [RelayCommand]
    private async Task CreateTemplateAsync()
    {
        var categories = await _api.GetCourseCategoriesAsync();
        var first = categories.FirstOrDefault();
        if (first == null)
        {
            MessageBox.Show("В базе нет категорий курсов (CourseCategories). Сначала добавьте категорию через API или сидер.", "Курсы", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var created = await _api.CreateCourseAsync(new CreateCourseRequest
            {
                Title = "Новый курс-шаблон",
                CategoryId = first.CategoryId,
                Price = 0,
                IsActive = false
            });

            var w = new CourseEditorWindow(_api, created.CourseId) { Owner = Application.Current.MainWindow };
            w.ShowDialog();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Курсы", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenInstanceEditor()
    {
        if (SelectedInstance == null)
        {
            MessageBox.Show("Выберите поток в нижней таблице.", "Потоки", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var w = new CourseInstanceWindow(_api, SelectedInstance.Instance.InstanceId) { Owner = Application.Current.MainWindow };
        w.ShowDialog();
        _ = ReloadAsync();
    }

    [RelayCommand]
    private async Task CreateInstanceAsync()
    {
        if (SelectedTemplate == null)
        {
            MessageBox.Show("Выберите шаблон курса в верхней таблице, чтобы создать для него поток.", "Потоки", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var created = await _api.CreateCourseInstanceAsync(new CreateCourseInstanceRequest
            {
                CourseId = SelectedTemplate.Dto.CourseId,
                InstanceName = $"{SelectedTemplate.Dto.Title} — поток {today:yyyy-MM}",
                StartDate = today,
                EndDate = today.AddMonths(3),
                IsActive = true
            });

            var w = new CourseInstanceWindow(_api, created.InstanceId) { Owner = Application.Current.MainWindow };
            w.ShowDialog();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Потоки", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RebuildSubjectFilterOptions()
    {
        var cur = SelectedSubjectFilter;
        SubjectFilterOptions.Clear();
        SubjectFilterOptions.Add("Все предметы");
        foreach (var name in _subjectNames.Values.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            SubjectFilterOptions.Add(name);
        SelectedSubjectFilter = SubjectFilterOptions.Contains(cur) ? cur : "Все предметы";
    }

    private bool FilterTemplate(object obj)
    {
        if (obj is not CourseTemplateRow row)
            return false;

        if (SelectedSubjectFilter != "Все предметы")
        {
            var sid = row.Dto.SubjectId;
            if (!sid.HasValue || !_subjectNames.TryGetValue(sid.Value, out var sn) || sn != SelectedSubjectFilter)
                return false;
        }

        if (SelectedTrackFilter != "Все программы")
        {
            var examId = row.Dto.ExamId;
            var examName = examId is { } e && _examNames.TryGetValue(e, out var en) ? en : "";
            var oge = examName.Contains("ОГЭ", StringComparison.OrdinalIgnoreCase) ||
                      examName.Contains("огэ", StringComparison.OrdinalIgnoreCase);
            var ege = examName.Contains("ЕГЭ", StringComparison.OrdinalIgnoreCase) ||
                      examName.Contains("егэ", StringComparison.OrdinalIgnoreCase);

            if (SelectedTrackFilter.StartsWith("ОГЭ", StringComparison.Ordinal) && !oge)
                return false;
            if (SelectedTrackFilter.StartsWith("ЕГЭ", StringComparison.Ordinal) && !ege)
                return false;
        }

        if (SelectedStatusFilter != "Любой статус")
        {
            var active = row.Dto.IsActive == true;
            if (SelectedStatusFilter == "Активен" && !active)
                return false;
            if (SelectedStatusFilter == "Скрыт" && active)
                return false;
        }

        return true;
    }

    private bool FilterInstance(object obj)
    {
        if (obj is not CourseInstanceRow row)
            return false;
        if (string.IsNullOrWhiteSpace(InstanceSearch))
            return true;
        var q = InstanceSearch.Trim();
        return row.CourseTitle.Contains(q, StringComparison.OrdinalIgnoreCase)
               || row.Instance.InstanceName.Contains(q, StringComparison.OrdinalIgnoreCase);
    }
}
