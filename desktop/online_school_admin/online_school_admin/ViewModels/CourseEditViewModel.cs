using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using Microsoft.Win32;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class CourseEditViewModel : BaseViewModel
{
    private readonly AdminCoursesService _courses;
    private readonly int? _courseId;
    private string? _pendingCoverPath;

    public CourseEditViewModel(AdminCoursesService courses, int? courseId = null)
    {
        _courses = courses;
        _courseId = courseId;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        PickCoverCommand = new RelayCommand(_ => PickCover(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public bool IsCreate => !_courseId.HasValue;

    public event Action<int>? Saved;
    public event Action? CancelRequested;

    public RelayCommand SaveCommand { get; }
    public RelayCommand PickCoverCommand { get; }
    public RelayCommand CancelCommand { get; }

    public ObservableCollection<AdminCourseCategoryDictDto> Categories { get; } = new();
    public AdminCourseCategoryDictDto? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
    private AdminCourseCategoryDictDto? _selectedCategory;

    public string Title { get => _title; set => SetProperty(ref _title, value); }
    private string _title = "";

    public string? ShortDescription { get => _shortDescription; set => SetProperty(ref _shortDescription, value); }
    private string? _shortDescription;

    public string? Description { get => _description; set => SetProperty(ref _description, value); }
    private string? _description;

    public string? CoverImgUrl { get => _coverImgUrl; set => SetProperty(ref _coverImgUrl, value); }
    private string? _coverImgUrl;

    public string? CoverFileName => string.IsNullOrWhiteSpace(_pendingCoverPath) ? null : Path.GetFileName(_pendingCoverPath);

    public decimal Price { get => _price; set => SetProperty(ref _price, value); }
    private decimal _price;

    public decimal? DiscountPrice { get => _discountPrice; set => SetProperty(ref _discountPrice, value); }
    private decimal? _discountPrice;

    public int? TotalHours { get => _totalHours; set => SetProperty(ref _totalHours, value); }
    private int? _totalHours;

    public string? WhatYouGet { get => _whatYouGet; set => SetProperty(ref _whatYouGet, value); }
    private string? _whatYouGet;

    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    private bool _isActive = true;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
                PickCoverCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }
    private bool _isBusy;

    public string? Error { get => _error; set => SetProperty(ref _error, value); }
    private string? _error;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Categories.Clear();
        foreach (var c in await _courses.GetCategoriesAsync(cancellationToken))
            Categories.Add(c);
        SelectedCategory ??= Categories.FirstOrDefault();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!_courseId.HasValue) return;
        Error = null;
        IsBusy = true;
        try
        {
            var d = await _courses.GetCourseAsync(_courseId.Value, cancellationToken);
            Title = d.Title;
            ShortDescription = d.ShortDescription;
            Description = d.Description;
            CoverImgUrl = d.CoverImgUrl;
            Price = d.Price;
            DiscountPrice = d.DiscountPrice;
            TotalHours = d.TotalHours;
            WhatYouGet = d.WhatYouGet;
            IsActive = d.IsActive;
            SelectedCategory = Categories.FirstOrDefault(x => x.CategoryId == d.CategoryId) ?? Categories.FirstOrDefault();
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
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

        IsBusy = true;
        try
        {
            var dto = new AdminCourseUpsertDto
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
                IsActive = IsActive
            };

            if (IsCreate)
            {
                var created = await _courses.CreateAsync(dto, cancellationToken);
                var id = created.CourseId;

                if (!string.IsNullOrWhiteSpace(_pendingCoverPath) && File.Exists(_pendingCoverPath))
                {
                    await using var fs = File.OpenRead(_pendingCoverPath);
                    var url = await _courses.UploadCourseCoverAsync(id, fs, Path.GetFileName(_pendingCoverPath), cancellationToken);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        CoverImgUrl = url;
                        await _courses.UpdateAsync(id, new AdminCourseUpsertDto { CoverImgUrl = url }, cancellationToken);
                    }
                }

                Saved?.Invoke(id);
            }
            else
            {
                await _courses.UpdateAsync(_courseId!.Value, dto, cancellationToken);
                Saved?.Invoke(_courseId!.Value);
            }
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

    private void PickCover()
    {
        var dlg = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.webp" };
        if (dlg.ShowDialog() != true)
            return;

        _pendingCoverPath = dlg.FileName;
        OnPropertyChanged(nameof(CoverFileName));
    }
}

