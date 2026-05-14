using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public enum StructureRightPanelKind
{
    Block,
    Lesson
}

public sealed class CourseStructureViewModel : BaseViewModel
{
    private readonly AdminCoursesService _courses;
    private readonly int _courseId;
    private bool _isReadOnly;
    private bool _suppressSelectedModuleLessons;
    private bool _suppressAutoLessonSelect;

    public CourseStructureViewModel(AdminCoursesService courses, int courseId, bool isReadOnly = false)
    {
        _courses = courses;
        _courseId = courseId;
        _isReadOnly = isReadOnly;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);

        AddModuleCommand = new RelayCommand(_ => PrepareNewModule(), _ => !IsBusy && !_isReadOnly);
        SaveModuleCommand = new RelayCommand(async _ => await SaveModuleAsync(), _ => !IsBusy && !_isReadOnly && (SelectedModule != null || IsNewModuleDraft));
        DeleteModuleCommand = new RelayCommand(async _ => await DeleteModuleAsync(), _ => !IsBusy && !_isReadOnly && SelectedModule != null && !IsNewModuleDraft);
        MoveModuleUpCommand = new RelayCommand(async _ => await MoveModuleAsync(-1), _ => !IsBusy && !_isReadOnly && !IsNewModuleDraft && CanMoveModule(-1));
        MoveModuleDownCommand = new RelayCommand(async _ => await MoveModuleAsync(1), _ => !IsBusy && !_isReadOnly && !IsNewModuleDraft && CanMoveModule(1));

        AddLessonCommand = new RelayCommand(_ => PrepareNewLesson(), _ => !IsBusy && !_isReadOnly);
        SaveLessonCommand = new RelayCommand(async _ => await SaveLessonAsync(), _ => !IsBusy && !_isReadOnly && SelectedModule != null && (SelectedLesson != null || IsNewLessonDraft));
        DeleteLessonCommand = new RelayCommand(async _ => await DeleteLessonAsync(), _ => !IsBusy && !_isReadOnly && SelectedLesson != null && !IsNewLessonDraft);
        MoveLessonUpCommand = new RelayCommand(async _ => await MoveLessonAsync(-1), _ => !IsBusy && !_isReadOnly && !IsNewLessonDraft && CanMoveLesson(-1));
        MoveLessonDownCommand = new RelayCommand(async _ => await MoveLessonAsync(1), _ => !IsBusy && !_isReadOnly && !IsNewLessonDraft && CanMoveLesson(1));
        OpenLessonCommand = new RelayCommand(async _ => await OpenLessonAsync(), _ => !IsBusy && SelectedLesson != null && !IsNewLessonDraft);

        AddMaterialCommand = new RelayCommand(async _ => await AddMaterialFromFileAsync(), _ => !IsBusy && !_isReadOnly && SelectedModule != null && (SelectedLesson != null || IsNewLessonDraft));
        AddMaterialFromUrlCommand = new RelayCommand(async _ => await AddMaterialFromUrlAsync(), _ => !IsBusy && !_isReadOnly && SelectedModule != null && (SelectedLesson != null || IsNewLessonDraft));
        SaveMaterialCommand = new RelayCommand(async _ => await SaveMaterialAsync(), _ => !IsBusy && !_isReadOnly && SelectedMaterial != null);
        DeleteMaterialCommand = new RelayCommand(async _ => await DeleteMaterialAsync(), _ => !IsBusy && !_isReadOnly && SelectedMaterial != null);
        UploadLessonVideoCommand = new RelayCommand(async _ => await UploadLessonVideoAsync(), _ => !IsBusy && !_isReadOnly && SelectedModule != null && (SelectedLesson != null || IsNewLessonDraft));
    }

    public int CourseId => _courseId;

    public bool IsReadOnly
    {
        get => _isReadOnly;
        private set
        {
            if (!SetProperty(ref _isReadOnly, value))
                return;
            RaiseAllCanExecuteChanged();
        }
    }

    public void SetReadOnly(bool readOnly)
    {
        IsReadOnly = readOnly;
        OnPropertyChanged(nameof(LessonOrderFieldIsReadOnly));
    }

    public ObservableCollection<CourseModuleTreeGroup> ModuleGroups { get; } = new();

    private object? _selectedTreeItem;
    public object? SelectedTreeItem
    {
        get => _selectedTreeItem;
        set
        {
            if (!SetProperty(ref _selectedTreeItem, value))
                return;
            _ = ApplyTreeSelectionAsync(value);
        }
    }

    public string StructureSummary =>
        $"Всего: {ModuleGroups.Count} блок., {ModuleGroups.Sum(g => g.LessonItems.Count)} урок.";

    private StructureRightPanelKind _activeRightPanel = StructureRightPanelKind.Block;

    public StructureRightPanelKind ActiveRightPanel
    {
        get => _activeRightPanel;
        private set
        {
            if (!SetProperty(ref _activeRightPanel, value))
                return;
            OnPropertyChanged(nameof(ShowBlockEditor));
            OnPropertyChanged(nameof(ShowLessonEditor));
            RaiseAllCanExecuteChanged();
        }
    }

    public bool ShowBlockEditor => ActiveRightPanel == StructureRightPanelKind.Block;

    public bool ShowLessonEditor => ActiveRightPanel == StructureRightPanelKind.Lesson;

    private bool _isNewModuleDraft;
    public bool IsNewModuleDraft
    {
        get => _isNewModuleDraft;
        private set
        {
            if (!SetProperty(ref _isNewModuleDraft, value))
                return;
            RaiseAllCanExecuteChanged();
        }
    }

    private bool _isNewLessonDraft;
    public bool IsNewLessonDraft
    {
        get => _isNewLessonDraft;
        private set
        {
            if (!SetProperty(ref _isNewLessonDraft, value))
                return;
            RaiseAllCanExecuteChanged();
            OnPropertyChanged(nameof(LessonOrderFieldIsReadOnly));
        }
    }

    /// <summary>При добавлении урока порядок в блоке задаётся автоматически и не редактируется.</summary>
    public bool LessonOrderFieldIsReadOnly => IsReadOnly || IsNewLessonDraft;

    private void ClearDrafts()
    {
        IsNewModuleDraft = false;
        IsNewLessonDraft = false;
        RaiseAllCanExecuteChanged();
    }

    private void PrepareNewModule()
    {
        ClearDrafts();

        ActiveRightPanel = StructureRightPanelKind.Block;

        _suppressAutoLessonSelect = true;
        SelectedModule = null;
        _suppressAutoLessonSelect = false;

        IsNewModuleDraft = true;

        ModuleTitle = "";
        ModuleDescription = "";
        ModuleOrder = Modules.Count == 0 ? 1 : Modules.Max(x => x.ModuleOrder) + 1;
        ModuleIsActive = true;
        RaiseAllCanExecuteChanged();
    }

    private void PrepareNewLesson()
    {
        if (Modules.Count == 0)
        {
            MessageBox.Show("Сначала создайте хотя бы один блок курса.", "Урок", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedTreeItem is not CourseModuleTreeGroup g)
        {
            MessageBox.Show(
                "Выделите в дереве блок курса (строка с номером блока), в который нужно добавить урок, затем нажмите «+ Добавить урок».",
                "Урок",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var mod = Modules.FirstOrDefault(m => m.ModuleId == g.Module.ModuleId);
        if (mod == null)
            return;

        _suppressAutoLessonSelect = true;
        SelectedModule = mod;
        _suppressAutoLessonSelect = false;

        IsNewModuleDraft = false;
        IsNewLessonDraft = true;
        ActiveRightPanel = StructureRightPanelKind.Lesson;

        SelectedLesson = null;
        LessonTitle = "";
        LessonContent = null;
        LessonTypeId = LessonTypes.FirstOrDefault(x => x.Id > 0)?.Id ?? 1;
        VideoUrl = null;
        DurationMinutes = null;
        LessonOrder = ComputeNextLessonOrder();
        LessonIsActive = true;

        RaiseAllCanExecuteChanged();
    }

    /// <summary>
    /// Номера порядка уроков в блоке: из панели справа и из дерева слева.
    /// Нужно, чтобы не полагаться только на <see cref="Lessons"/> — при смене блока список
    /// сначала очищается, и до завершения загрузки API он пуст, из‑за чего новый урок
    /// ошибочно получал порядок 1 и конфликтовал с уже существующим уроком.
    /// </summary>
    private IEnumerable<int> LessonOrdersInModule(int moduleId)
    {
        foreach (var o in Lessons.Where(l => l.ModuleId == moduleId).Select(l => l.LessonOrder))
            yield return o;
        var g = ModuleGroups.FirstOrDefault(x => x.Module.ModuleId == moduleId);
        if (g == null)
            yield break;
        foreach (var o in g.LessonItems.Select(n => n.Lesson.LessonOrder))
            yield return o;
    }

    private int ComputeNextLessonOrder()
    {
        if (SelectedModule == null)
            return 1;
        var distinct = LessonOrdersInModule(SelectedModule.ModuleId).Distinct().ToList();
        return distinct.Count == 0 ? 1 : distinct.Max() + 1;
    }

    /// <summary>Порядок для создаваемого урока: из UI или следующий свободный; при занятости — сдвиг вверх.</summary>
    private int ResolveNewLessonPersistOrder(int lessonOrderFromUi)
    {
        if (SelectedModule == null)
            return 1;
        var desired = lessonOrderFromUi > 0 ? lessonOrderFromUi : ComputeNextLessonOrder();
        var occupied = new HashSet<int>(LessonOrdersInModule(SelectedModule.ModuleId));
        var o = desired;
        while (occupied.Contains(o))
            o++;
        return o;
    }

    private async Task ApplyTreeSelectionAsync(object? value)
    {
        switch (value)
        {
            case CourseModuleTreeGroup g:
                ClearDrafts();
                ActiveRightPanel = StructureRightPanelKind.Block;
                _suppressAutoLessonSelect = true;
                SelectedModule = Modules.FirstOrDefault(m => m.ModuleId == g.Module.ModuleId);
                _suppressAutoLessonSelect = false;
                return;
            case LessonTreeNode n:
                ClearDrafts();
                ActiveRightPanel = StructureRightPanelKind.Lesson;
                var le = n.Lesson;
                var mod = Modules.FirstOrDefault(m => m.ModuleId == le.ModuleId);
                if (mod == null) return;
                _suppressSelectedModuleLessons = true;
                SelectedModule = mod;
                _suppressSelectedModuleLessons = false;
                await LoadLessonsAsync(CancellationToken.None, le.LessonId);
                return;
        }
    }

    private async Task BuildModuleGroupsAsync(CancellationToken cancellationToken = default)
    {
        ModuleGroups.Clear();
        foreach (var m in Modules.OrderBy(x => x.ModuleOrder).ThenBy(x => x.ModuleId))
        {
            var g = new CourseModuleTreeGroup(m);
            try
            {
                var list = await _courses.GetLessonsAsync(m.ModuleId, cancellationToken);
                foreach (var l in list.OrderBy(x => x.LessonOrder).ThenBy(x => x.LessonId))
                    g.LessonItems.Add(new LessonTreeNode { ModuleOrder = m.ModuleOrder, Lesson = l });
            }
            catch
            {
                /* дерево не ломаем */
            }

            ModuleGroups.Add(g);
        }

        OnPropertyChanged(nameof(StructureSummary));
    }

    private async Task RefreshTreeLessonsForModuleAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        var g = ModuleGroups.FirstOrDefault(x => x.Module.ModuleId == moduleId);
        if (g == null)
            return;
        g.LessonItems.Clear();
        try
        {
            foreach (var l in (await _courses.GetLessonsAsync(moduleId, cancellationToken)).OrderBy(x => x.LessonOrder))
                g.LessonItems.Add(new LessonTreeNode { ModuleOrder = g.Module.ModuleOrder, Lesson = l });
        }
        catch
        {
            /* ignore */
        }

        g.RaiseHeaderChanged();
        OnPropertyChanged(nameof(StructureSummary));
    }

    public RelayCommand RefreshCommand { get; }

    public RelayCommand AddModuleCommand { get; }
    public RelayCommand SaveModuleCommand { get; }
    public RelayCommand DeleteModuleCommand { get; }
    public RelayCommand MoveModuleUpCommand { get; }
    public RelayCommand MoveModuleDownCommand { get; }

    public RelayCommand AddLessonCommand { get; }
    public RelayCommand SaveLessonCommand { get; }
    public RelayCommand DeleteLessonCommand { get; }
    public RelayCommand MoveLessonUpCommand { get; }
    public RelayCommand MoveLessonDownCommand { get; }
    public RelayCommand OpenLessonCommand { get; }

    public RelayCommand AddMaterialCommand { get; }
    public RelayCommand AddMaterialFromUrlCommand { get; }
    public RelayCommand SaveMaterialCommand { get; }
    public RelayCommand DeleteMaterialCommand { get; }
    public RelayCommand UploadLessonVideoCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                RaiseAllCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public ObservableCollection<AdminCourseModuleRowDto> Modules { get; } = new();
    public ObservableCollection<AdminLessonRowDto> Lessons { get; } = new();
    public ObservableCollection<AdminLessonMaterialRowDto> Materials { get; } = new();
    public ObservableCollection<IdTitleOption> LessonTypes { get; } = new();

    private AdminCourseModuleRowDto? _selectedModule;
    public AdminCourseModuleRowDto? SelectedModule
    {
        get => _selectedModule;
        set
        {
            var suppressAutoLesson = _suppressAutoLessonSelect;
            if (!SetProperty(ref _selectedModule, value))
            {
                RaiseAllCanExecuteChanged();
                return;
            }

            LoadModuleToEditor();
            if (!_suppressSelectedModuleLessons)
                _ = LoadLessonsAsync(CancellationToken.None, selectLessonId: null, selectFirstWhenNoId: !suppressAutoLesson);

            RaiseAllCanExecuteChanged();
        }
    }

    private AdminLessonRowDto? _selectedLesson;
    public AdminLessonRowDto? SelectedLesson
    {
        get => _selectedLesson;
        set
        {
            if (!SetProperty(ref _selectedLesson, value))
                return;

            LoadLessonToEditor();
            _ = LoadMaterialsAsync();
            RaiseAllCanExecuteChanged();
        }
    }

    private AdminLessonMaterialRowDto? _selectedMaterial;
    public AdminLessonMaterialRowDto? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (!SetProperty(ref _selectedMaterial, value))
                return;
            LoadMaterialToEditor();
            RaiseAllCanExecuteChanged();
        }
    }

    // module editor
    private string _moduleTitle = "";
    public string ModuleTitle { get => _moduleTitle; set => SetProperty(ref _moduleTitle, value); }

    private string? _moduleDescription;
    public string? ModuleDescription { get => _moduleDescription; set => SetProperty(ref _moduleDescription, value); }

    private int _moduleOrder;
    public int ModuleOrder { get => _moduleOrder; set => SetProperty(ref _moduleOrder, value); }

    private bool _moduleIsActive = true;
    public bool ModuleIsActive { get => _moduleIsActive; set => SetProperty(ref _moduleIsActive, value); }

    // lesson editor
    private string _lessonTitle = "";
    public string LessonTitle { get => _lessonTitle; set => SetProperty(ref _lessonTitle, value); }

    private string? _lessonContent;
    public string? LessonContent { get => _lessonContent; set => SetProperty(ref _lessonContent, value); }

    private int _lessonTypeId = 1;
    public int LessonTypeId { get => _lessonTypeId; set => SetProperty(ref _lessonTypeId, value); }

    private string? _videoUrl;
    public string? VideoUrl { get => _videoUrl; set => SetProperty(ref _videoUrl, value); }

    private int? _durationMinutes;
    public int? DurationMinutes { get => _durationMinutes; set => SetProperty(ref _durationMinutes, value); }

    private int _lessonOrder;
    public int LessonOrder { get => _lessonOrder; set => SetProperty(ref _lessonOrder, value); }

    private bool _lessonIsActive = true;
    public bool LessonIsActive { get => _lessonIsActive; set => SetProperty(ref _lessonIsActive, value); }

    // material editor
    private string _materialName = "";
    public string MaterialName { get => _materialName; set => SetProperty(ref _materialName, value); }

    private string _materialUrl = "";
    public string MaterialUrl { get => _materialUrl; set => SetProperty(ref _materialUrl, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            LessonTypes.Clear();
            LessonTypes.Add(new IdTitleOption(0, "Тип урока"));
            foreach (var t in await _courses.GetLessonTypesAsync(cancellationToken))
                LessonTypes.Add(new IdTitleOption(t.TypeId, t.TypeName));

            var modules = await _courses.GetModulesAsync(_courseId, cancellationToken);
            Modules.Clear();
            foreach (var m in modules.OrderBy(x => x.ModuleOrder).ThenBy(x => x.ModuleId))
                Modules.Add(m);

            await BuildModuleGroupsAsync(cancellationToken);

            ClearDrafts();
            ActiveRightPanel = StructureRightPanelKind.Block;
            _suppressAutoLessonSelect = true;
            SelectedModule = Modules.FirstOrDefault();
            _suppressAutoLessonSelect = false;
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadLessonsAsync(CancellationToken cancellationToken = default, int? selectLessonId = null, bool selectFirstWhenNoId = true)
    {
        Lessons.Clear();
        Materials.Clear();
        SelectedLesson = null;
        SelectedMaterial = null;

        if (SelectedModule == null)
            return;

        try
        {
            var list = await _courses.GetLessonsAsync(SelectedModule.ModuleId, cancellationToken);
            foreach (var l in list.OrderBy(x => x.LessonOrder).ThenBy(x => x.LessonId))
                Lessons.Add(l);
            SelectedLesson = selectLessonId.HasValue
                ? Lessons.FirstOrDefault(l => l.LessonId == selectLessonId.Value)
                : (selectFirstWhenNoId ? Lessons.FirstOrDefault() : null);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
    }

    private async Task LoadMaterialsAsync(CancellationToken cancellationToken = default)
    {
        Materials.Clear();
        SelectedMaterial = null;

        if (SelectedLesson == null)
            return;

        try
        {
            var list = await _courses.GetMaterialsAsync(SelectedLesson.LessonId, cancellationToken);
            foreach (var m in list.OrderByDescending(x => x.UploadedAt).ThenBy(x => x.MaterialId))
                Materials.Add(m);
            SelectedMaterial = Materials.FirstOrDefault();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
    }

    private async Task SaveModuleAsync()
    {
        var title = (ModuleTitle ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Укажите название блока.", "Блок", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            if (IsNewModuleDraft)
            {
                var order = ModuleOrder > 0 ? ModuleOrder : (Modules.Count == 0 ? 1 : Modules.Max(x => x.ModuleOrder) + 1);
                await _courses.CreateModuleAsync(_courseId, new AdminCourseModuleUpsertDto
                {
                    Title = title,
                    Description = string.IsNullOrWhiteSpace(ModuleDescription) ? null : ModuleDescription,
                    ModuleOrder = order,
                    IsActive = ModuleIsActive
                });
                IsNewModuleDraft = false;
                await LoadAsync();
                return;
            }

            if (SelectedModule == null)
            {
                MessageBox.Show(
                    "Не выбран блок для сохранения. Выберите блок в дереве слева или нажмите «+ Добавить блок».",
                    "Блок",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            await _courses.UpdateModuleAsync(SelectedModule.ModuleId, new AdminCourseModuleUpsertDto
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(ModuleDescription) ? null : ModuleDescription,
                ModuleOrder = ModuleOrder,
                IsActive = ModuleIsActive
            });

            SelectedModule.Title = title;
            SelectedModule.Description = string.IsNullOrWhiteSpace(ModuleDescription) ? null : ModuleDescription;
            SelectedModule.ModuleOrder = ModuleOrder;
            SelectedModule.IsActive = ModuleIsActive;

            await LoadAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteModuleAsync()
    {
        if (SelectedModule == null) return;

        if (MessageBox.Show("Скрыть блок? (можно вернуть, включив IsActive)", "Блок", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.DeleteModuleAsync(SelectedModule.ModuleId);
            await LoadAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanMoveModule(int delta)
    {
        if (SelectedModule == null) return false;
        var idx = Modules.IndexOf(SelectedModule);
        var newIdx = idx + delta;
        return idx >= 0 && newIdx >= 0 && newIdx < Modules.Count;
    }

    private async Task MoveModuleAsync(int delta)
    {
        if (!CanMoveModule(delta) || SelectedModule == null) return;
        var idx = Modules.IndexOf(SelectedModule);
        var newIdx = idx + delta;

        var tmp = Modules[newIdx];
        Modules[newIdx] = Modules[idx];
        Modules[idx] = tmp;

        // normalize orders to 1..N
        for (var i = 0; i < Modules.Count; i++)
            Modules[i].ModuleOrder = i + 1;

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.ReorderModulesAsync(new AdminReorderRequestDto
            {
                Items = Modules.Select(m => new AdminReorderItemDto { Id = m.ModuleId, Order = m.ModuleOrder }).ToList()
            });
            await LoadAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Создаёт урок на сервере из черновика (нужен перед загрузкой видео/материалов).</summary>
    private async Task<bool> PersistLessonDraftIfNeededAsync(CancellationToken cancellationToken = default)
    {
        if (!IsNewLessonDraft)
            return SelectedLesson != null;

        if (SelectedModule == null)
            return false;

        var title = (LessonTitle ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Укажите название урока.", "Урок", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (LessonTypeId <= 0)
        {
            MessageBox.Show("Выберите тип урока в списке.", "Урок", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var nextOrder = ResolveNewLessonPersistOrder(LessonOrder);
        var created = await _courses.CreateLessonAsync(SelectedModule.ModuleId, new AdminLessonCreateDto
        {
            Title = title,
            LessonTypeId = LessonTypeId,
            Content = string.IsNullOrWhiteSpace(LessonContent) ? null : LessonContent,
            VideoUrl = string.IsNullOrWhiteSpace(VideoUrl) ? null : VideoUrl,
            DurationMinutes = DurationMinutes,
            LessonOrder = nextOrder,
            IsActive = LessonIsActive
        }, cancellationToken);

        IsNewLessonDraft = false;
        await RefreshTreeLessonsForModuleAsync(SelectedModule.ModuleId, cancellationToken);
        await LoadLessonsAsync(cancellationToken, created.LessonId);
        RaiseAllCanExecuteChanged();
        return SelectedLesson != null;
    }

    private async Task SaveLessonAsync()
    {
        if (SelectedModule == null)
            return;

        var title = (LessonTitle ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Укажите название урока.", "Урок", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            if (IsNewLessonDraft)
            {
                if (LessonTypeId <= 0)
                {
                    MessageBox.Show("Выберите тип урока в списке.", "Урок", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!await PersistLessonDraftIfNeededAsync())
                    return;
                return;
            }

            if (SelectedLesson == null)
                return;

            await _courses.UpdateLessonAsync(SelectedLesson.LessonId, new AdminLessonUpdateDto
            {
                Title = title,
                LessonTypeId = LessonTypeId,
                Content = string.IsNullOrWhiteSpace(LessonContent) ? null : LessonContent,
                VideoUrl = string.IsNullOrWhiteSpace(VideoUrl) ? null : VideoUrl,
                DurationMinutes = DurationMinutes,
                LessonOrder = LessonOrder,
                IsActive = LessonIsActive
            });
            var selLessonId = SelectedLesson.LessonId;
            await LoadLessonsAsync(cancellationToken: default, selLessonId);
            if (SelectedModule != null)
                await RefreshTreeLessonsForModuleAsync(SelectedModule.ModuleId);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteLessonAsync()
    {
        if (SelectedLesson == null) return;

        if (MessageBox.Show("Скрыть урок? (можно вернуть, включив IsActive)", "Урок", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            var mid = SelectedModule!.ModuleId;
            await _courses.DeleteLessonAsync(SelectedLesson.LessonId);
            await LoadLessonsAsync();
            await RefreshTreeLessonsForModuleAsync(mid);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanMoveLesson(int delta)
    {
        if (SelectedLesson == null) return false;
        var idx = Lessons.IndexOf(SelectedLesson);
        var newIdx = idx + delta;
        return idx >= 0 && newIdx >= 0 && newIdx < Lessons.Count;
    }

    private async Task MoveLessonAsync(int delta)
    {
        if (!CanMoveLesson(delta) || SelectedLesson == null) return;
        var idx = Lessons.IndexOf(SelectedLesson);
        var newIdx = idx + delta;

        var tmp = Lessons[newIdx];
        Lessons[newIdx] = Lessons[idx];
        Lessons[idx] = tmp;

        for (var i = 0; i < Lessons.Count; i++)
            Lessons[i].LessonOrder = i + 1;

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.ReorderLessonsAsync(new AdminReorderRequestDto
            {
                Items = Lessons.Select(l => new AdminReorderItemDto { Id = l.LessonId, Order = l.LessonOrder }).ToList()
            });
            var lid = SelectedLesson?.LessonId;
            await LoadLessonsAsync(cancellationToken: default, lid);
            if (SelectedModule != null)
                await RefreshTreeLessonsForModuleAsync(SelectedModule.ModuleId);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddMaterialFromFileAsync()
    {
        if (SelectedModule == null)
            return;

        var dlg = new OpenFileDialog
        {
            Filter = "Файлы|*.pdf;*.doc;*.docx;*.ppt;*.pptx;*.xlsx;*.zip;*.rar;*.7z;*.txt;*.png;*.jpg;*.jpeg;*.webp|Все файлы|*.*"
        };
        if (dlg.ShowDialog() != true)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            if (SelectedLesson == null || IsNewLessonDraft)
            {
                if (!await PersistLessonDraftIfNeededAsync())
                    return;
            }

            if (SelectedLesson == null)
                return;

            var lessonId = SelectedLesson.LessonId;
            var fileName = Path.GetFileName(dlg.FileName);
            var bytes = await File.ReadAllBytesAsync(dlg.FileName);
            var created = await _courses.UploadLessonMaterialFileAsync(lessonId, bytes, fileName);
            Materials.Insert(0, created);
            SelectedMaterial = created;
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddMaterialFromUrlAsync()
    {
        if (SelectedModule == null)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            if (SelectedLesson == null || IsNewLessonDraft)
            {
                if (!await PersistLessonDraftIfNeededAsync())
                    return;
            }

            if (SelectedLesson == null)
                return;

            await AddMaterialCoreAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddMaterialCoreAsync()
    {
        if (SelectedLesson == null) return;

        var name = (MaterialName ?? "").Trim();
        var url = (MaterialUrl ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("Укажите название и ссылку.", "Материал", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var created = await _courses.CreateMaterialAsync(SelectedLesson.LessonId, new AdminLessonMaterialCreateDto
            {
                FileName = name,
                FileUrl = url
            });
            Materials.Insert(0, created);
            SelectedMaterial = created;
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
    }

    private async Task SaveMaterialAsync()
    {
        if (SelectedMaterial == null) return;

        var name = (MaterialName ?? "").Trim();
        var url = (MaterialUrl ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("Укажите название и ссылку.", "Материал", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.UpdateMaterialAsync(SelectedMaterial.MaterialId, new AdminLessonMaterialUpdateDto
            {
                FileName = name,
                FileUrl = url
            });
            await LoadMaterialsAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteMaterialAsync()
    {
        if (SelectedMaterial == null) return;

        if (MessageBox.Show("Удалить материал?", "Материал", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _courses.DeleteMaterialAsync(SelectedMaterial.MaterialId);
            await LoadMaterialsAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadModuleToEditor()
    {
        if (SelectedModule == null)
        {
            ModuleTitle = "";
            ModuleDescription = null;
            ModuleOrder = 0;
            ModuleIsActive = true;
            return;
        }

        ModuleTitle = SelectedModule.Title;
        ModuleDescription = SelectedModule.Description;
        ModuleOrder = SelectedModule.ModuleOrder;
        ModuleIsActive = SelectedModule.IsActive;
    }

    private void LoadLessonToEditor()
    {
        if (SelectedLesson == null)
        {
            LessonTitle = "";
            LessonContent = null;
            LessonTypeId = LessonTypes.FirstOrDefault(x => x.Id > 0)?.Id ?? 1;
            VideoUrl = null;
            DurationMinutes = null;
            LessonOrder = 0;
            LessonIsActive = true;
            return;
        }

        LessonTitle = SelectedLesson.Title;
        LessonContent = SelectedLesson.Content;
        LessonTypeId = SelectedLesson.LessonTypeId;
        VideoUrl = SelectedLesson.VideoUrl;
        DurationMinutes = SelectedLesson.DurationMinutes;
        LessonOrder = SelectedLesson.LessonOrder;
        LessonIsActive = SelectedLesson.IsActive;
    }

    private void LoadMaterialToEditor()
    {
        if (SelectedMaterial == null)
        {
            MaterialName = "";
            MaterialUrl = "";
            return;
        }

        MaterialName = SelectedMaterial.FileName;
        MaterialUrl = SelectedMaterial.FileUrl;
    }

    private void RaiseAllCanExecuteChanged()
    {
        AddModuleCommand.RaiseCanExecuteChanged();
        SaveModuleCommand.RaiseCanExecuteChanged();
        DeleteModuleCommand.RaiseCanExecuteChanged();
        MoveModuleUpCommand.RaiseCanExecuteChanged();
        MoveModuleDownCommand.RaiseCanExecuteChanged();

        AddLessonCommand.RaiseCanExecuteChanged();
        SaveLessonCommand.RaiseCanExecuteChanged();
        DeleteLessonCommand.RaiseCanExecuteChanged();
        MoveLessonUpCommand.RaiseCanExecuteChanged();
        MoveLessonDownCommand.RaiseCanExecuteChanged();
        OpenLessonCommand.RaiseCanExecuteChanged();

        AddMaterialCommand.RaiseCanExecuteChanged();
        AddMaterialFromUrlCommand.RaiseCanExecuteChanged();
        SaveMaterialCommand.RaiseCanExecuteChanged();
        DeleteMaterialCommand.RaiseCanExecuteChanged();
        UploadLessonVideoCommand.RaiseCanExecuteChanged();
    }

    private async Task UploadLessonVideoAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedModule == null)
            return;

        var dlg = new OpenFileDialog { Filter = "Видео|*.mp4;*.webm;*.mov;*.mkv|Все файлы|*.*" };
        if (dlg.ShowDialog() != true)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            if (SelectedLesson == null || IsNewLessonDraft)
            {
                if (!await PersistLessonDraftIfNeededAsync(cancellationToken))
                    return;
            }

            if (SelectedLesson == null)
                return;

            var lessonId = SelectedLesson.LessonId;
            var videoBytes = await File.ReadAllBytesAsync(dlg.FileName, cancellationToken);
            var url = await _courses.UploadLessonVideoAsync(lessonId, videoBytes, Path.GetFileName(dlg.FileName), cancellationToken);
            VideoUrl = url;
            await _courses.UpdateLessonAsync(lessonId, new AdminLessonUpdateDto
            {
                Title = (LessonTitle ?? "").Trim(),
                LessonTypeId = LessonTypeId,
                Content = string.IsNullOrWhiteSpace(LessonContent) ? null : LessonContent,
                VideoUrl = url,
                DurationMinutes = DurationMinutes,
                LessonOrder = LessonOrder,
                IsActive = LessonIsActive
            }, cancellationToken);
            await LoadLessonsAsync(cancellationToken, lessonId);
            if (SelectedModule != null)
                await RefreshTreeLessonsForModuleAsync(SelectedModule.ModuleId);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex is ApiException api ? api.Message : "Не удалось связаться с сервером.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenLessonAsync()
    {
        if (SelectedLesson == null) return;

        // ДЗ в отдельном окне редактируем всегда: _isReadOnly относится к дереву/панели структуры, а не к окну «Урок → ДЗ».
        var window = new online_school_admin.Views.LessonDetailsWindow
        {
            Owner = Application.Current?.MainWindow,
            DataContext = new LessonDetailsViewModel(_courses, SelectedLesson, homeworkReadOnly: false)
        };

        // pre-load homeworks for better UX
        await ((LessonDetailsViewModel)window.DataContext).Homework.LoadAsync();
        window.ShowDialog();
    }
}

