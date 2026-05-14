using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class PromoCodeEditViewModel : BaseViewModel
{
    private readonly AdminPromoCodesService _promo;
    private readonly AdminCoursesService _courses;
    private readonly AdminInstancesService _instances;
    private readonly int? _id;

    public PromoCodeEditViewModel(AdminPromoCodesService promo, AdminCoursesService courses, AdminInstancesService instances, int? promoCodeId)
    {
        _promo = promo;
        _courses = courses;
        _instances = instances;
        _id = promoCodeId;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public string Title => _id.HasValue ? "Редактирование промокода" : "Создание промокода";

    public ObservableCollection<IdTitleOption> DiscountTypeOptions { get; } = new();
    public ObservableCollection<IdTitleOption> CourseOptions { get; } = new();
    public ObservableCollection<IdTitleOption> InstanceOptions { get; } = new();

    private string _currentUsesLine = "";
    /// <summary>Только при редактировании: число записей в promo_code_usage.</summary>
    public string CurrentUsesLine { get => _currentUsesLine; set => SetProperty(ref _currentUsesLine, value); }

    private string _code = "";
    public string Code { get => _code; set => SetProperty(ref _code, value); }

    private IdTitleOption? _selectedDiscountType;
    public IdTitleOption? SelectedDiscountType { get => _selectedDiscountType; set => SetProperty(ref _selectedDiscountType, value); }

    private string _discountValueText = "0";
    public string DiscountValueText { get => _discountValueText; set => SetProperty(ref _discountValueText, value); }

    private string _startDateText = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string StartDateText { get => _startDateText; set => SetProperty(ref _startDateText, value); }

    private string _endDateText = "";
    public string EndDateText { get => _endDateText; set => SetProperty(ref _endDateText, value); }

    private string _maxUsesText = "";
    public string MaxUsesText { get => _maxUsesText; set => SetProperty(ref _maxUsesText, value); }

    private string _minOrderAmountText = "";
    public string MinOrderAmountText { get => _minOrderAmountText; set => SetProperty(ref _minOrderAmountText, value); }

    private string _maxDiscountAmountText = "";
    public string MaxDiscountAmountText { get => _maxDiscountAmountText; set => SetProperty(ref _maxDiscountAmountText, value); }

    private IdTitleOption? _selectedCourse;
    public IdTitleOption? SelectedCourse { get => _selectedCourse; set => SetProperty(ref _selectedCourse, value); }

    private IdTitleOption? _selectedInstance;
    public IdTitleOption? SelectedInstance { get => _selectedInstance; set => SetProperty(ref _selectedInstance, value); }

    private bool _isActive = true;
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        CurrentUsesLine = "";
        DiscountTypeOptions.Clear();
        DiscountTypeOptions.Add(new IdTitleOption(0, "Не выбрано"));
        foreach (var t in await _promo.GetDiscountTypesAsync(cancellationToken))
            DiscountTypeOptions.Add(new IdTitleOption(t.TypeId, t.TypeName));
        SelectedDiscountType = DiscountTypeOptions.FirstOrDefault();

        CourseOptions.Clear();
        CourseOptions.Add(new IdTitleOption(0, "Не ограничивать курсом"));
        foreach (var c in await _courses.GetCoursesAsync(null, null, null, null, null, cancellationToken))
            CourseOptions.Add(new IdTitleOption(c.CourseId, c.Title));
        SelectedCourse = CourseOptions.FirstOrDefault();

        InstanceOptions.Clear();
        InstanceOptions.Add(new IdTitleOption(0, "Не ограничивать потоком"));
        foreach (var i in await _instances.GetInstancesAsync(null, null, null, null, cancellationToken))
            InstanceOptions.Add(new IdTitleOption(i.InstanceId, i.Title));
        SelectedInstance = InstanceOptions.FirstOrDefault();

        if (_id.HasValue)
            await LoadAsync(cancellationToken);
    }

    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _promo.GetPromoCodeAsync(_id!.Value, cancellationToken);
            Code = dto.Code;
            SelectedDiscountType = DiscountTypeOptions.FirstOrDefault(x => x.Id == (dto.DiscountTypeId ?? 0)) ?? DiscountTypeOptions.FirstOrDefault();
            DiscountValueText = dto.DiscountValue.ToString(CultureInfo.InvariantCulture);
            StartDateText = dto.ValidFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            EndDateText = dto.ValidUntil?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
            MaxUsesText = dto.MaxUses?.ToString(CultureInfo.InvariantCulture) ?? "";
            MinOrderAmountText = dto.MinOrderAmount?.ToString(CultureInfo.InvariantCulture) ?? "";
            MaxDiscountAmountText = dto.MaxDiscountAmount?.ToString(CultureInfo.InvariantCulture) ?? "";
            SelectedCourse = CourseOptions.FirstOrDefault(x => x.Id == (dto.AppliesToCourseId ?? 0)) ?? CourseOptions.FirstOrDefault();
            SelectedInstance = InstanceOptions.FirstOrDefault(x => x.Id == (dto.AppliesToInstanceId ?? 0)) ?? InstanceOptions.FirstOrDefault();
            IsActive = dto.IsActive;
            CurrentUsesLine = $"Уже применён: {dto.CurrentUses} раз(а)";
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

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (string.IsNullOrWhiteSpace(Code))
                throw new ApiException(HttpStatusCode.BadRequest, "Укажите code");

            if (!TryParseDecimal(DiscountValueText, out var discountValue))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректное discount_value");

            if (!TryParseDate(StartDateText, out var validFrom))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректное start_date (ожидается yyyy-MM-dd)");

            DateOnly? validUntil = null;
            if (!string.IsNullOrWhiteSpace(EndDateText))
            {
                if (!TryParseDate(EndDateText, out var d))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректное end_date (ожидается yyyy-MM-dd)");
                validUntil = d;
            }

            int? maxUses = null;
            if (!string.IsNullOrWhiteSpace(MaxUsesText))
            {
                if (!int.TryParse(MaxUsesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mi))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректное max_uses");
                maxUses = mi;
            }

            decimal? minOrderAmount = null;
            if (!string.IsNullOrWhiteSpace(MinOrderAmountText))
            {
                if (!TryParseDecimal(MinOrderAmountText, out var m))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректное min_order_amount");
                minOrderAmount = m;
            }

            decimal? maxDiscountAmount = null;
            if (!string.IsNullOrWhiteSpace(MaxDiscountAmountText))
            {
                if (!TryParseDecimal(MaxDiscountAmountText, out var m))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректное max_discount_amount");
                maxDiscountAmount = m;
            }

            var dto = new AdminPromoCodeUpsertDto
            {
                Code = Code.Trim(),
                DiscountTypeId = SelectedDiscountType is { Id: > 0 } dt ? dt.Id : null,
                DiscountValue = discountValue,
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                MaxUses = maxUses,
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount,
                AppliesToCourseId = SelectedCourse is { Id: > 0 } c ? c.Id : null,
                AppliesToInstanceId = SelectedInstance is { Id: > 0 } i ? i.Id : null,
                IsActive = IsActive
            };

            if (_id.HasValue)
                await _promo.UpdatePromoCodeAsync(_id.Value, dto, cancellationToken);
            else
                await _promo.CreatePromoCodeAsync(dto, cancellationToken);

            Saved?.Invoke();
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

    private static bool TryParseDate(string text, out DateOnly date)
        => DateOnly.TryParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static bool TryParseDecimal(string text, out decimal value)
        => decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
}

