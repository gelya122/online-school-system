using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Win32;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class CourseDetailsViewModel : BaseViewModel
{
    private readonly AdminCoursesService _courses;
    private readonly AdminInstancesService _instances;

    private int? _courseId;
    private CourseDetailsPageMode _pageMode;
    private string? _pendingCoverPath;

    /// <summary>Снимок is_active из БД: меняется только через список курсов (Опубликовать), не с этой формы.</summary>
    private bool _catalogIsActive;

    public CourseDetailsViewModel(
        AdminCoursesService courses,
        AdminInstancesService instances,
        int? courseId,
        CourseDetailsPageMode pageMode)
    {
        _courses = courses;
        _instances = instances;
        _courseId = courseId;
        _pageMode = pageMode;

        if (_courseId.HasValue && _pageMode != CourseDetailsPageMode.Create)
            Structure = new CourseStructureViewModel(_courses, _courseId.Value, pageMode == CourseDetailsPageMode.View);

        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy && HasPersistedCourse);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy && PageMode != CourseDetailsPageMode.View);
        CancelCommand = new RelayCommand(_ => NavigateBackRequested?.Invoke(), _ => !IsBusy);
        PickCoverCommand = new RelayCommand(_ => PickCover(), _ => !IsBusy && PageMode != CourseDetailsPageMode.View);
        SwitchToEditCommand = new RelayCommand(_ => SwitchToEdit(), _ => !IsBusy && PageMode == CourseDetailsPageMode.View && HasPersistedCourse);
        CreateInstanceCommand = new RelayCommand(_ => CreateInstanceRequested?.Invoke(_courseId ?? 0),
            _ => !IsBusy && HasPersistedCourse && PageMode != CourseDetailsPageMode.View && Details?.IsActive == true);

        OpenStreamCommand = new RelayCommand(p =>
        {
            if (p is int id)
                OpenStreamRequested?.Invoke(id);
        }, _ => !IsBusy);
    }

    public event Action? NavigateBackRequested;
    public event Action<int>? CreateInstanceRequested;
    public event Action<int>? OpenStreamRequested;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PickCoverCommand { get; }
    public RelayCommand SwitchToEditCommand { get; }
    public RelayCommand CreateInstanceCommand { get; }
    public RelayCommand OpenStreamCommand { get; }

    public bool HasPersistedCourse => _courseId.HasValue;

    public CourseDetailsPageMode PageMode
    {
        get => _pageMode;
        private set
        {
            if (!SetProperty(ref _pageMode, value))
                return;
            OnPropertyChanged(nameof(IsViewMode));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(IsCreateMode));
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(BreadcrumbText));
            OnPropertyChanged(nameof(ShowSaveToolbar));
            OnPropertyChanged(nameof(ShowEditToolbarButton));
            OnPropertyChanged(nameof(ShowMainEditorFields));
            OnPropertyChanged(nameof(ShowCreateInstanceButton));
            SaveCommand.RaiseCanExecuteChanged();
            SwitchToEditCommand.RaiseCanExecuteChanged();
            PickCoverCommand.RaiseCanExecuteChanged();
            CreateInstanceCommand.RaiseCanExecuteChanged();
            RefreshCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsViewMode => PageMode == CourseDetailsPageMode.View;
    public bool IsEditMode => PageMode == CourseDetailsPageMode.Edit;
    public bool IsCreateMode => PageMode == CourseDetailsPageMode.Create;

    public bool ShowSaveToolbar => PageMode != CourseDetailsPageMode.View;
    public bool ShowEditToolbarButton => PageMode == CourseDetailsPageMode.View && HasPersistedCourse;

    public bool ShowMainEditorFields => PageMode != CourseDetailsPageMode.View;

    public bool ShowCreateInstanceButton =>
        HasPersistedCourse && PageMode != CourseDetailsPageMode.View && (Details?.IsActive ?? false);

    public bool ShowDeferredTabsPlaceholder => !HasPersistedCourse;

    public string PageTitle => PageMode switch
    {
        CourseDetailsPageMode.Create => "Новый курс",
        CourseDetailsPageMode.Edit => "Редактирование курса",
        CourseDetailsPageMode.View => "Просмотр курса",
        _ => "Курс"
    };

    public string BreadcrumbText => PageMode switch
    {
        CourseDetailsPageMode.Create => "Курсы → Новый курс",
        CourseDetailsPageMode.Edit => $"Курсы → {Title.Trim()} → Редактирование",
        CourseDetailsPageMode.View => $"Курсы → {Title.Trim()} → Просмотр",
        _ => "Курсы"
    };

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                PickCoverCommand.RaiseCanExecuteChanged();
                SwitchToEditCommand.RaiseCanExecuteChanged();
                CreateInstanceCommand.RaiseCanExecuteChanged();
                OpenStreamCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminCourseDetailsDto? _details;
    public AdminCourseDetailsDto? Details
    {
        get => _details;
        private set
        {
            if (!SetProperty(ref _details, value))
                return;
            OnPropertyChanged(nameof(ShowCreateInstanceButton));
            CreateInstanceCommand.RaiseCanExecuteChanged();
        }
    }

    public CourseStructureViewModel? Structure { get; private set; }

    public ObservableCollection<AdminCourseInstanceListRowDto> CourseStreams { get; } = new();

    public ObservableCollection<AdminCourseCategoryDictDto> Categories { get; } = new();

    private AdminCourseCategoryDictDto? _selectedCategory;
    public AdminCourseCategoryDictDto? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private string _title = "";
    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                OnPropertyChanged(nameof(BreadcrumbText));
            }
        }
    }

    private string? _shortDescription;
    public string? ShortDescription { get => _shortDescription; set => SetProperty(ref _shortDescription, value); }

    private string? _description;
    public string? Description { get => _description; set => SetProperty(ref _description, value); }

    private string? _coverImgUrl;
    public string? CoverImgUrl { get => _coverImgUrl; set => SetProperty(ref _coverImgUrl, value); }

    public string? CoverFileName =>
        string.IsNullOrWhiteSpace(_pendingCoverPath) ? null : Path.GetFileName(_pendingCoverPath);

    private decimal _price;
    public decimal Price { get => _price; set => SetProperty(ref _price, value); }

    private decimal? _discountPrice;
    public decimal? DiscountPrice { get => _discountPrice; set => SetProperty(ref _discountPrice, value); }

    private int? _totalHours;
    public int? TotalHours { get => _totalHours; set => SetProperty(ref _totalHours, value); }

    private string? _whatYouGet;
    public string? WhatYouGet { get => _whatYouGet; set => SetProperty(ref _whatYouGet, value); }

    /// <summary>Только отображение: публикация из списка курсов.</summary>
    public string CatalogActivityHint =>
        !HasPersistedCourse
            ? "После сохранения черновика опубликовать курс можно в списке «Курсы»."
            : (_catalogIsActive ? "Курс опубликован в каталоге (is_active). Снять или опубликовать — в списке курсов." : "Курс скрыт в каталоге. Опубликовать — в списке курсов.");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Categories.Clear();
        foreach (var c in await _courses.GetCategoriesAsync(cancellationToken))
            Categories.Add(c);
        SelectedCategory ??= Categories.FirstOrDefault();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;

        if (!HasPersistedCourse)
        {
            Title = "";
            ShortDescription = null;
            Description = null;
            CoverImgUrl = null;
            Price = 0;
            DiscountPrice = null;
            TotalHours = null;
            WhatYouGet = null;
            _catalogIsActive = false;
            Details = null;
            CourseStreams.Clear();
            OnPropertyChanged(nameof(CatalogActivityHint));
            OnPropertyChanged(nameof(BreadcrumbText));
            return;
        }

        IsBusy = true;
        try
        {
            var d = await _courses.GetCourseAsync(_courseId!.Value, cancellationToken);
            Details = d;
            ApplyDetailsToEditors(d);
            _catalogIsActive = d.IsActive;
            OnPropertyChanged(nameof(CatalogActivityHint));

            if (Structure != null)
                await Structure.LoadAsync(cancellationToken);

            CourseStreams.Clear();
            foreach (var row in await _instances.GetInstancesAsync(null, _courseId.Value, null, null, cancellationToken))
                CourseStreams.Add(row);

            CreateInstanceCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(BreadcrumbText));
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

    private void ApplyDetailsToEditors(AdminCourseDetailsDto d)
    {
        Title = d.Title;
        ShortDescription = d.ShortDescription;
        Description = d.Description;
        CoverImgUrl = d.CoverImgUrl;
        Price = d.Price;
        DiscountPrice = d.DiscountPrice;
        TotalHours = d.TotalHours;
        WhatYouGet = d.WhatYouGet;
        SelectedCategory = Categories.FirstOrDefault(x => x.CategoryId == d.CategoryId) ?? Categories.FirstOrDefault();
        OnPropertyChanged(nameof(BreadcrumbText));
    }

    private void SwitchToEdit()
    {
        PageMode = CourseDetailsPageMode.Edit;
        Structure?.SetReadOnly(false);
        OnPropertyChanged(nameof(ShowDeferredTabsPlaceholder));
        OnPropertyChanged(nameof(ShowMainEditorFields));
        OnPropertyChanged(nameof(ShowSaveToolbar));
        OnPropertyChanged(nameof(ShowEditToolbarButton));
        OnPropertyChanged(nameof(ShowCreateInstanceButton));
    }

    private void PickCover()
    {
        var dlg = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.webp" };
        if (dlg.ShowDialog() != true)
            return;
        _pendingCoverPath = dlg.FileName;
        OnPropertyChanged(nameof(CoverFileName));
    }

    private AdminCourseUpsertDto BuildUpsertDto()
    {
        if (SelectedCategory == null)
            throw new InvalidOperationException("Категория не выбрана.");

        return new AdminCourseUpsertDto
        {
            Title = Title.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(ShortDescription) ? null : ShortDescription.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            CategoryId = SelectedCategory.CategoryId,
            CoverImgUrl = string.IsNullOrWhiteSpace(CoverImgUrl) ? null : CoverImgUrl.Trim(),
            Price = Price,
            DiscountPrice = DiscountPrice,
            TotalHours = TotalHours,
            WhatYouGet = string.IsNullOrWhiteSpace(WhatYouGet) ? null : WhatYouGet.Trim(),
            IsActive = HasPersistedCourse ? _catalogIsActive : false
        };
    }

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedCategory == null)
        {
            Error = "Выберите категорию.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            Error = "Укажите название курса.";
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            var dto = BuildUpsertDto();

            if (!HasPersistedCourse)
            {
                dto.IsActive = false;
                var created = await _courses.CreateAsync(dto, cancellationToken);
                _courseId = created.CourseId;
                _catalogIsActive = created.IsActive;
                Details = created;
                ApplyDetailsToEditors(created);

                if (!string.IsNullOrWhiteSpace(_pendingCoverPath) && File.Exists(_pendingCoverPath))
                {
                    await using var fs = File.OpenRead(_pendingCoverPath);
                    var url = await _courses.UploadCourseCoverAsync(_courseId.Value, fs, Path.GetFileName(_pendingCoverPath), cancellationToken);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        CoverImgUrl = url;
                        await _courses.UpdateAsync(_courseId.Value, BuildUpsertDto(), cancellationToken);
                    }
                }

                _pendingCoverPath = null;
                OnPropertyChanged(nameof(CoverFileName));

                Structure = new CourseStructureViewModel(_courses, _courseId.Value, false);
                OnPropertyChanged(nameof(Structure));
                OnPropertyChanged(nameof(HasPersistedCourse));
                OnPropertyChanged(nameof(ShowDeferredTabsPlaceholder));
                OnPropertyChanged(nameof(CatalogActivityHint));
                OnPropertyChanged(nameof(BreadcrumbText));

                PageMode = CourseDetailsPageMode.Edit;
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(ShowSaveToolbar));
                OnPropertyChanged(nameof(ShowEditToolbarButton));

                await Structure.LoadAsync(cancellationToken);

                CourseStreams.Clear();
                foreach (var row in await _instances.GetInstancesAsync(null, _courseId.Value, null, null, cancellationToken))
                    CourseStreams.Add(row);

                UserDialogs.Info("Курс сохранён как черновик. Опубликовать его можно в списке «Курсы».", "Курсы");
            }
            else
            {
                await _courses.UpdateAsync(_courseId!.Value, dto, cancellationToken);

                if (!string.IsNullOrWhiteSpace(_pendingCoverPath) && File.Exists(_pendingCoverPath))
                {
                    await using var fs = File.OpenRead(_pendingCoverPath);
                    var url = await _courses.UploadCourseCoverAsync(_courseId.Value, fs, Path.GetFileName(_pendingCoverPath), cancellationToken);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        CoverImgUrl = url;
                        await _courses.UpdateAsync(_courseId.Value, BuildUpsertDto(), cancellationToken);
                    }
                }

                _pendingCoverPath = null;
                OnPropertyChanged(nameof(CoverFileName));

                await LoadAsync(cancellationToken);
                UserDialogs.Info("Изменения сохранены.", "Курсы");
            }

            CreateInstanceCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
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
}
