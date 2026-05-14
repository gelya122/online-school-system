using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using online_school_admin.Infrastructure;
using online_school_admin.Models.Admin;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public partial class CourseEditorViewModel : ObservableObject
{
    private readonly AuthApiService _api;
    public int CourseId { get; }

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string? _shortDescription;
    [ObservableProperty] private string? _fullDescription;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private decimal? _discountPrice;
    [ObservableProperty] private int? _totalHours;
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private string _benefitsText = "";
    [ObservableProperty] private string _outcomesText = "";
    [ObservableProperty] private string? _coverImgUrl;
    [ObservableProperty] private string? _coverPreviewUrl;
    [ObservableProperty] private int _selectedCategoryId;

    [ObservableProperty] private int _statsEnrolled;
    [ObservableProperty] private int _statsCompleted;
    [ObservableProperty] private double? _statsAvgRating;
    [ObservableProperty] private int _statsReviewCount;

    public ObservableCollection<CourseCategoryDto> Categories { get; } = new();
    public ObservableCollection<LessonTypeDto> LessonTypes { get; } = new();
    public ObservableCollection<AssignmentTypeDto> AssignmentTypes { get; } = new();

    public ObservableCollection<CourseStructureNode> StructureRoots { get; } = new();

    [ObservableProperty] private CourseStructureNode? _selectedStructureNode;

    [ObservableProperty] private string _lessonTitleEdit = "";
    [ObservableProperty] private string? _lessonContent;
    [ObservableProperty] private string? _lessonVideoUrl;
    [ObservableProperty] private int? _lessonDurationMinutes;
    [ObservableProperty] private int _lessonOrder;
    [ObservableProperty] private int _lessonTypeId;
    [ObservableProperty] private bool _lessonFreePreview;

    public ObservableCollection<LessonMaterialDto> LessonMaterials { get; } = new();

    public ObservableCollection<AssignmentFlatRow> AssignmentsFlat { get; } = new();

    public ObservableCollection<ReviewDto> Reviews { get; } = new();

    [ObservableProperty] private bool _isBusy;

    private readonly Dictionary<int, LessonDto> _lessonById = new();

    public CourseEditorViewModel(AuthApiService api, int courseId)
    {
        _api = api;
        CourseId = courseId;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            var course = await _api.GetCourseAsync(CourseId, cancellationToken)
                         ?? throw new InvalidOperationException("Курс не найден.");

            Title = course.Title;
            ShortDescription = course.ShortDescription;
            FullDescription = course.Description;
            Price = course.Price;
            DiscountPrice = course.DiscountPrice;
            TotalHours = course.TotalHours;
            IsActive = course.IsActive == true;
            CoverImgUrl = course.CoverImgUrl;
            CoverPreviewUrl = _api.ToAbsoluteUrl(course.CoverImgUrl);
            SelectedCategoryId = course.CategoryId;

            CourseWhatYouGetHelper.Parse(course.WhatYouGet, out var ben, out var outc);
            BenefitsText = string.Join(Environment.NewLine, ben);
            OutcomesText = string.Join(Environment.NewLine, outc);

            Categories.Clear();
            foreach (var c in await _api.GetCourseCategoriesAsync(cancellationToken))
                Categories.Add(c);

            LessonTypes.Clear();
            foreach (var t in await _api.GetLessonTypesAsync(cancellationToken))
                LessonTypes.Add(t);

            AssignmentTypes.Clear();
            foreach (var t in await _api.GetAssignmentTypesAsync(cancellationToken))
                AssignmentTypes.Add(t);

            if (LessonTypes.Count > 0 && LessonTypeId == 0)
                LessonTypeId = LessonTypes[0].TypeId;

            await LoadStatsAsync(course, cancellationToken);
            await ReloadStructureAsync(cancellationToken);
            await ReloadAssignmentsFlatAsync(cancellationToken);
            await ReloadReviewsAsync(cancellationToken);

            if (SelectedStructureNode != null)
                await SyncLessonPanelAsync(SelectedStructureNode);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadStatsAsync(CourseTemplateDto course, CancellationToken cancellationToken)
    {
        StatsAvgRating = course.ReviewAverage;
        StatsReviewCount = course.ReviewCount ?? 0;

        var instances = (await _api.GetCourseInstancesAsync(cancellationToken)).Where(i => i.CourseId == CourseId).ToList();
        var ids = instances.Select(i => i.InstanceId).ToHashSet();
        var enrollments = (await _api.GetEnrollmentsAsync(cancellationToken)).Where(e => ids.Contains(e.InstanceId)).ToList();

        StatsEnrolled = enrollments.Count;
        StatsCompleted = enrollments.Count(e => e.CompletedAt.HasValue);
    }

    partial void OnSelectedStructureNodeChanged(CourseStructureNode? value)
    {
        AddHomeworkCommand.NotifyCanExecuteChanged();
        _ = SyncLessonPanelAsync(value);
    }

    partial void OnIsBusyChanged(bool value) => AddHomeworkCommand.NotifyCanExecuteChanged();

    private async Task SyncLessonPanelAsync(CourseStructureNode? node)
    {
        if (node?.Kind != "Lesson")
        {
            LessonMaterials.Clear();
            return;
        }

        if (!_lessonById.TryGetValue(node.Id, out var lesson))
            return;

        LessonTitleEdit = lesson.Title;
        LessonContent = lesson.Content;
        LessonVideoUrl = lesson.VideoUrl;
        LessonDurationMinutes = lesson.DurationMinutes;
        LessonOrder = lesson.LessonOrder;
        LessonTypeId = lesson.LessonTypeId;
        LessonFreePreview = lesson.IsFreePreview == true;

        LessonMaterials.Clear();
        var allMat = await _api.GetLessonMaterialsAsync(CancellationToken.None);
        foreach (var m in allMat.Where(x => x.LessonId == lesson.LessonId).OrderBy(x => x.MaterialId))
            LessonMaterials.Add(m);
    }

    [RelayCommand]
    private async Task SaveBasicAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            var whatYouGet = CourseWhatYouGetHelper.Serialize(
                BenefitsText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                OutcomesText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            await _api.UpdateCourseAsync(CourseId, new UpdateCourseRequest
            {
                Title = Title,
                ShortDescription = ShortDescription,
                Description = FullDescription,
                CategoryId = SelectedCategoryId,
                Price = Price,
                DiscountPrice = DiscountPrice,
                TotalHours = TotalHours,
                WhatYouGet = whatYouGet,
                IsActive = IsActive
            }, cancellationToken);

            MessageBox.Show("Шаблон курса сохранён.", "Курс", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Курс", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UploadCoverAsync(CancellationToken cancellationToken = default)
    {
        var dlg = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.webp" };
        if (dlg.ShowDialog() != true)
            return;

        IsBusy = true;
        try
        {
            await using var fs = File.OpenRead(dlg.FileName);
            var url = await _api.UploadCourseCoverAsync(CourseId, fs, Path.GetFileName(dlg.FileName), cancellationToken);
            CoverImgUrl = url;
            CoverPreviewUrl = _api.ToAbsoluteUrl(url);
            await _api.UpdateCourseAsync(CourseId, new UpdateCourseRequest { CoverImgUrl = url }, cancellationToken);
            MessageBox.Show("Обложка загружена.", "Курс", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Курс", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReloadStructureAsync(CancellationToken cancellationToken = default)
    {
        StructureRoots.Clear();
        _lessonById.Clear();

        var modules = (await _api.GetCourseModulesAsync(cancellationToken))
            .Where(m => m.CourseId == CourseId)
            .OrderBy(m => m.ModuleOrder)
            .ToList();

        var lessons = (await _api.GetLessonsAsync(cancellationToken)).ToList();
        var assignments = (await _api.GetAssignmentsAsync(cancellationToken)).ToList();

        foreach (var lesson in lessons)
            _lessonById[lesson.LessonId] = lesson;

        foreach (var mod in modules)
        {
            var mNode = new CourseStructureNode
            {
                Kind = "Module",
                Id = mod.ModuleId,
                Title = mod.Title,
                ParentModuleId = null,
                ParentLessonId = null
            };
            foreach (var les in lessons.Where(l => l.ModuleId == mod.ModuleId).OrderBy(l => l.LessonOrder))
            {
                var lNode = new CourseStructureNode
                {
                    Kind = "Lesson",
                    Id = les.LessonId,
                    ParentModuleId = mod.ModuleId,
                    ParentLessonId = null,
                    Title = les.Title
                };
                foreach (var a in assignments.Where(x => x.LessonId == les.LessonId).OrderBy(x => x.AssignmentId))
                {
                    lNode.Children.Add(new CourseStructureNode
                    {
                        Kind = "Hw",
                        Id = a.AssignmentId,
                        ParentLessonId = les.LessonId,
                        ParentModuleId = mod.ModuleId,
                        Title = a.Title
                    });
                }

                mNode.Children.Add(lNode);
            }

            StructureRoots.Add(mNode);
        }
    }

    [RelayCommand]
    private async Task AddModuleAsync(CancellationToken cancellationToken = default)
    {
        var modules = (await _api.GetCourseModulesAsync(cancellationToken)).Where(m => m.CourseId == CourseId).ToList();
        var next = modules.Count == 0 ? 1 : modules.Max(x => x.ModuleOrder) + 1;

        var name = $"Блок {next}";

        await _api.CreateCourseModuleAsync(new CreateCourseModuleRequest
        {
            CourseId = CourseId,
            Title = name,
            ModuleOrder = next,
            Description = null
        }, cancellationToken);

        await ReloadStructureAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task AddLessonAsync(CancellationToken cancellationToken = default)
    {
        var modId = SelectedStructureNode?.Kind == "Module"
            ? SelectedStructureNode.Id
            : SelectedStructureNode?.ParentModuleId;

        if (modId is not int moduleId)
        {
            MessageBox.Show("Выберите блок в дереве или элемент с контекстом блока (урок/ДЗ).", "Структура",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var lessons = (await _api.GetLessonsAsync(cancellationToken)).Where(l => l.ModuleId == moduleId).ToList();
        var next = lessons.Count == 0 ? 1 : lessons.Max(x => x.LessonOrder) + 1;

        var lt = LessonTypes.FirstOrDefault()?.TypeId ?? 1;
        var title = $"Урок {next}";

        await _api.CreateLessonAsync(new CreateLessonRequest
        {
            ModuleId = moduleId,
            Title = title,
            LessonTypeId = lt,
            LessonOrder = next,
            Content = null,
            VideoUrl = null,
            DurationMinutes = null,
            IsFreePreview = false
        }, cancellationToken);

        await ReloadStructureAsync(cancellationToken);
        await ReloadAssignmentsFlatAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanAddHomework))]
    private async Task AddHomeworkAsync(CancellationToken cancellationToken = default)
    {
        var lessonId = GetSelectedLessonIdForHomework();

        if (lessonId is not int lid)
        {
            MessageBox.Show("Выберите урок в дереве (или ДЗ под уроком).", "ДЗ", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (AssignmentsFlat.Any(r => r.Dto.LessonId == lid))
        {
            MessageBox.Show("У этого урока уже есть домашнее задание. Допускается только одно ДЗ на урок.", "ДЗ",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var title = "Домашнее задание";

        await _api.CreateAssignmentAsync(new CreateAssignmentRequest
        {
            LessonId = lid,
            Title = title,
            MaxScore = 10,
            DueDaysAfterLesson = 7,
            Description = null
        }, cancellationToken);

        await ReloadStructureAsync(cancellationToken);
        await ReloadAssignmentsFlatAsync(cancellationToken);
    }

    private int? GetSelectedLessonIdForHomework() =>
        SelectedStructureNode switch
        {
            { Kind: "Lesson" } s => s.Id,
            { Kind: "Hw" } h => h.ParentLessonId,
            _ => null
        };

    private bool CanAddHomework()
    {
        if (IsBusy) return false;
        var lid = GetSelectedLessonIdForHomework();
        if (lid is null) return true;
        return !AssignmentsFlat.Any(r => r.Dto.LessonId == lid.Value);
    }

    [RelayCommand]
    private async Task DeleteStructureNodeAsync(CancellationToken cancellationToken = default)
    {
        var n = SelectedStructureNode;
        if (n == null)
            return;

        if (MessageBox.Show($"Удалить «{n.Title}»?", "Подтверждение", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        if (n.Kind == "Module")
            await _api.DeleteCourseModuleAsync(n.Id, cancellationToken);
        else if (n.Kind == "Lesson")
            await _api.DeleteLessonAsync(n.Id, cancellationToken);
        else
            await _api.DeleteAssignmentAsync(n.Id, cancellationToken);

        await ReloadStructureAsync(cancellationToken);
        await ReloadAssignmentsFlatAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task SaveLessonAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedStructureNode?.Kind != "Lesson")
        {
            MessageBox.Show("Выберите урок в дереве.", "Урок", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsBusy = true;
        try
        {
            await _api.UpdateLessonAsync(SelectedStructureNode.Id, new UpdateLessonRequest
            {
                Title = LessonTitleEdit,
                Content = LessonContent,
                VideoUrl = LessonVideoUrl,
                DurationMinutes = LessonDurationMinutes,
                LessonOrder = LessonOrder,
                LessonTypeId = LessonTypeId,
                IsFreePreview = LessonFreePreview
            }, cancellationToken);

            MessageBox.Show("Урок сохранён.", "Урок", MessageBoxButton.OK, MessageBoxImage.Information);
            await ReloadStructureAsync(cancellationToken);
            await ReloadAssignmentsFlatAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UploadLessonVideoAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedStructureNode?.Kind != "Lesson")
            return;

        var dlg = new OpenFileDialog { Filter = "Видео|*.mp4;*.webm;*.mov;*.mkv|Все файлы|*.*" };
        if (dlg.ShowDialog() != true)
            return;

        IsBusy = true;
        try
        {
            var videoBytes = await File.ReadAllBytesAsync(dlg.FileName, cancellationToken);
            var url = await _api.UploadLessonVideoAsync(SelectedStructureNode.Id, videoBytes,
                Path.GetFileName(dlg.FileName), cancellationToken);
            LessonVideoUrl = url;
            await _api.UpdateLessonAsync(SelectedStructureNode.Id, new UpdateLessonRequest { VideoUrl = url },
                cancellationToken);
            MessageBox.Show("Видео загружено.", "Урок", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Урок", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UploadLessonMaterialAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedStructureNode?.Kind != "Lesson")
            return;

        var dlg = new OpenFileDialog { Filter = "Файлы|*.*" };
        if (dlg.ShowDialog() != true)
            return;

        IsBusy = true;
        try
        {
            var bytes = await File.ReadAllBytesAsync(dlg.FileName, cancellationToken);
            var mat = await _api.UploadLessonMaterialAsync(SelectedStructureNode.Id, bytes,
                Path.GetFileName(dlg.FileName), cancellationToken);
            LessonMaterials.Add(mat);
            MessageBox.Show("Файл добавлен к уроку.", "Материалы", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Материалы", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMaterialAsync(LessonMaterialDto? mat, CancellationToken cancellationToken = default)
    {
        if (mat == null)
            return;
        await _api.DeleteLessonMaterialAsync(mat.MaterialId, cancellationToken);
        LessonMaterials.Remove(mat);
    }

    private async Task ReloadAssignmentsFlatAsync(CancellationToken cancellationToken)
    {
        AssignmentsFlat.Clear();
        var moduleIds = (await _api.GetCourseModulesAsync(cancellationToken)).Where(m => m.CourseId == CourseId)
            .Select(m => m.ModuleId).ToHashSet();
        var lessons = (await _api.GetLessonsAsync(cancellationToken)).Where(l => moduleIds.Contains(l.ModuleId)).ToList();
        var lessonTitle = lessons.ToDictionary(x => x.LessonId, x => x.Title);
        var ids = lessonTitle.Keys.ToHashSet();

        foreach (var a in (await _api.GetAssignmentsAsync(cancellationToken)).Where(x => ids.Contains(x.LessonId))
                     .OrderBy(x => x.LessonId).ThenBy(x => x.AssignmentId))
            AssignmentsFlat.Add(new AssignmentFlatRow(a, lessonTitle[a.LessonId]));

        AddHomeworkCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task SaveAssignmentRowAsync(AssignmentFlatRow? row, CancellationToken cancellationToken = default)
    {
        if (row == null)
            return;

        await _api.UpdateAssignmentAsync(row.Dto.AssignmentId, new UpdateAssignmentRequest
        {
            Title = row.Dto.Title,
            Description = row.Dto.Description,
            MaxScore = row.Dto.MaxScore,
            DueDaysAfterLesson = row.Dto.DueDaysAfterLesson
        }, cancellationToken);

        MessageBox.Show("Задание сохранено.", "ДЗ", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task ReloadReviewsAsync(CancellationToken cancellationToken = default)
    {
        Reviews.Clear();
        var all = await _api.GetReviewsAsync(cancellationToken);
        foreach (var r in all.Where(x => x.CourseId == CourseId).OrderByDescending(x => x.CreatedAt))
            Reviews.Add(r);
    }

    [RelayCommand]
    private async Task ToggleReviewPublishAsync(ReviewDto? review, CancellationToken cancellationToken = default)
    {
        if (review == null)
            return;

        await _api.UpdateReviewAsync(review.ReviewId,
            new UpdateReviewRequest { IsPublished = !(review.IsPublished == true) }, cancellationToken);
        await ReloadReviewsAsync(cancellationToken);
        var course = await _api.GetCourseAsync(CourseId, cancellationToken);
        if (course != null)
            await LoadStatsAsync(course, cancellationToken);
    }
}
