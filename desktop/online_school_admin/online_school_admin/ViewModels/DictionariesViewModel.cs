using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class DictionariesViewModel : BaseViewModel
{
    private readonly AdminDictionariesService _dict;

    public DictionariesViewModel(AdminDictionariesService dict)
    {
        _dict = dict;
        RefreshRegistryCommand = new RelayCommand(async _ => await LoadRegistryAsync(), _ => !IsBusy);
        RefreshRowsCommand = new RelayCommand(async _ => await LoadRowsAsync(), _ => !IsBusy && SelectedRegistry != null);
        NewCommand = new RelayCommand(_ => ClearForm(), _ => !IsBusy && SelectedRegistry != null);
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy && SelectedRegistry != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && SelectedRegistry != null && SelectedRow != null);
        DeactivateCommand = new RelayCommand(async _ => await DeactivateAsync(), _ => !IsBusy && SelectedRegistry != null && SelectedRow != null && SelectedRegistry.SupportsDeactivate);
    }

    public RelayCommand RefreshRegistryCommand { get; }
    public RelayCommand RefreshRowsCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand DeactivateCommand { get; }

    public ObservableCollection<AdminDictionaryRegistryItemDto> Registry { get; } = new();
    public ObservableCollection<DictGridRow> Rows { get; } = new();

    private AdminDictionaryRegistryItemDto? _selectedRegistry;
    public AdminDictionaryRegistryItemDto? SelectedRegistry
    {
        get => _selectedRegistry;
        set
        {
            if (SetProperty(ref _selectedRegistry, value))
            {
                _ = LoadRowsAsync(CancellationToken.None);
                RefreshRowsCommand.RaiseCanExecuteChanged();
                NewCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ShowCourseCategoryFields));
                OnPropertyChanged(nameof(ShowIsActive));
                OnPropertyChanged(nameof(ShowDeactivate));
            }
        }
    }

    private DictGridRow? _selectedRow;
    public DictGridRow? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (SetProperty(ref _selectedRow, value))
            {
                if (value != null)
                {
                    NameText = value.Name;
                    DescriptionText = value.Description ?? "";
                    IsActive = value.IsActive ?? true;
                    SubjectIdText = value.SubjectId?.ToString(CultureInfo.InvariantCulture) ?? "";
                    ExamIdText = value.ExamId?.ToString(CultureInfo.InvariantCulture) ?? "";
                }
                DeleteCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string NameText { get => _nameText; set => SetProperty(ref _nameText, value); }
    private string _nameText = "";

    public string DescriptionText { get => _descriptionText; set => SetProperty(ref _descriptionText, value); }
    private string _descriptionText = "";

    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    private bool _isActive = true;

    public string SubjectIdText { get => _subjectIdText; set => SetProperty(ref _subjectIdText, value); }
    private string _subjectIdText = "";

    public string ExamIdText { get => _examIdText; set => SetProperty(ref _examIdText, value); }
    private string _examIdText = "";

    public bool ShowCourseCategoryFields => SelectedRegistry?.Code == "course-categories";
    public bool ShowIsActive => SelectedRegistry?.Code is "subjects" or "exams" or "payment-methods" or "discount-types";
    public bool ShowDeactivate => SelectedRegistry?.SupportsDeactivate == true;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshRegistryCommand.RaiseCanExecuteChanged();
                RefreshRowsCommand.RaiseCanExecuteChanged();
                NewCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await LoadRegistryAsync(cancellationToken);
        if (Registry.Count > 0)
            SelectedRegistry = Registry[0];
    }

    private async Task LoadRegistryAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _dict.GetRegistryAsync(cancellationToken);
            Registry.Clear();
            foreach (var x in list)
                Registry.Add(x);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private void ClearForm()
    {
        SelectedRow = null;
        NameText = "";
        DescriptionText = "";
        IsActive = true;
        SubjectIdText = "";
        ExamIdText = "";
    }

    private async Task LoadRowsAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedRegistry == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            Rows.Clear();
            var code = SelectedRegistry.Code;
            switch (code)
            {
                case "user-roles":
                    foreach (var x in await _dict.GetUserRolesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.RoleId, Name = x.RoleName, Description = x.Description });
                    break;
                case "application-statuses":
                    foreach (var x in await _dict.GetApplicationStatusesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.Id, Name = x.Name, Description = x.Description });
                    break;
                case "assignment-types":
                    foreach (var x in await _dict.GetAssignmentTypesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.TypeId, Name = x.TypeName, Description = x.Description });
                    break;
                case "submission-statuses":
                    foreach (var x in await _dict.GetSubmissionStatusesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.Id, Name = x.Name, Description = x.Description });
                    break;
                case "enrollment-statuses":
                    foreach (var x in await _dict.GetEnrollmentStatusesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.Id, Name = x.Name, Description = x.Description });
                    break;
                case "order-statuses":
                    foreach (var x in await _dict.GetOrderStatusesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.Id, Name = x.Name, Description = x.Description });
                    break;
                case "payment-statuses":
                    foreach (var x in await _dict.GetPaymentStatusesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.Id, Name = x.Name, Description = x.Description });
                    break;
                case "payment-methods":
                    foreach (var x in await _dict.GetPaymentMethodsAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.MethodId, Name = x.MethodName, Description = x.Description, IsActive = x.IsActive });
                    break;
                case "discount-types":
                    foreach (var x in await _dict.GetDiscountTypesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.TypeId, Name = x.TypeName, Description = x.Description, IsActive = x.IsActive });
                    break;
                case "lesson-types":
                    foreach (var x in await _dict.GetLessonTypesAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.TypeId, Name = x.TypeName, Description = x.Description });
                    break;
                case "subjects":
                    foreach (var x in await _dict.GetSubjectsAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.SubjectId, Name = x.SubjectName, Description = x.Description, IsActive = x.IsActive });
                    break;
                case "exams":
                    foreach (var x in await _dict.GetExamsAsync(cancellationToken))
                        Rows.Add(new DictGridRow { Id = x.ExamId, Name = x.ExamName, Description = x.Description, IsActive = x.IsActive });
                    break;
                case "course-categories":
                    foreach (var x in await _dict.GetCourseCategoriesAsync(cancellationToken))
                        Rows.Add(new DictGridRow
                        {
                            Id = x.CategoryId,
                            Name = x.CategoryName,
                            Description = x.Description,
                            SubjectId = x.SubjectId,
                            ExamId = x.ExamId
                        });
                    break;
            }
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task SaveAsync()
    {
        if (SelectedRegistry == null) return;
        if (string.IsNullOrWhiteSpace(NameText)) { Error = "Укажите название."; return; }
        Error = null;
        IsBusy = true;
        try
        {
            var code = SelectedRegistry.Code;
            var desc = string.IsNullOrWhiteSpace(DescriptionText) ? null : DescriptionText.Trim();
            if (SelectedRow == null)
                await CreateAsync(code, desc);
            else
                await UpdateAsync(code, SelectedRow.Id, desc);
            ClearForm();
            await LoadRowsAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task CreateAsync(string code, string? desc)
    {
        var dto = new AdminDictNameDescUpsertDto { Name = NameText.Trim(), Description = desc };
        switch (code)
        {
            case "user-roles": await _dict.CreateUserRoleAsync(dto); break;
            case "application-statuses": await _dict.CreateApplicationStatusAsync(dto); break;
            case "assignment-types": await _dict.CreateAssignmentTypeAsync(dto); break;
            case "submission-statuses": await _dict.CreateSubmissionStatusAsync(dto); break;
            case "enrollment-statuses": await _dict.CreateEnrollmentStatusAsync(dto); break;
            case "order-statuses": await _dict.CreateOrderStatusAsync(dto); break;
            case "payment-statuses": await _dict.CreatePaymentStatusAsync(dto); break;
            case "payment-methods":
                await _dict.CreatePaymentMethodAsync(new AdminPaymentMethodUpsertDto
                {
                    MethodName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "discount-types":
                await _dict.CreateDiscountTypeAsync(new AdminDiscountTypeUpsertDto
                {
                    TypeName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "lesson-types": await _dict.CreateLessonTypeAsync(dto); break;
            case "subjects":
                await _dict.CreateSubjectAsync(new AdminSubjectUpsertDto
                {
                    SubjectName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "exams":
                await _dict.CreateExamAsync(new AdminExamUpsertDto
                {
                    ExamName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "course-categories":
                await _dict.CreateCourseCategoryAsync(new AdminCourseCategoryUpsertDto
                {
                    CategoryName = NameText.Trim(),
                    Description = desc,
                    SubjectId = ParseOptionalInt(SubjectIdText),
                    ExamId = ParseOptionalInt(ExamIdText)
                });
                break;
            default: throw new ApiException(HttpStatusCode.BadRequest, "Неизвестный справочник");
        }
    }

    private async Task UpdateAsync(string code, int id, string? desc)
    {
        var dto = new AdminDictNameDescUpsertDto { Name = NameText.Trim(), Description = desc };
        switch (code)
        {
            case "user-roles": await _dict.UpdateUserRoleAsync(id, dto); break;
            case "application-statuses": await _dict.UpdateApplicationStatusAsync(id, dto); break;
            case "assignment-types": await _dict.UpdateAssignmentTypeAsync(id, dto); break;
            case "submission-statuses": await _dict.UpdateSubmissionStatusAsync(id, dto); break;
            case "enrollment-statuses": await _dict.UpdateEnrollmentStatusAsync(id, dto); break;
            case "order-statuses": await _dict.UpdateOrderStatusAsync(id, dto); break;
            case "payment-statuses": await _dict.UpdatePaymentStatusAsync(id, dto); break;
            case "payment-methods":
                await _dict.UpdatePaymentMethodAsync(id, new AdminPaymentMethodUpsertDto
                {
                    MethodName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "discount-types":
                await _dict.UpdateDiscountTypeAsync(id, new AdminDiscountTypeUpsertDto
                {
                    TypeName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "lesson-types": await _dict.UpdateLessonTypeAsync(id, dto); break;
            case "subjects":
                await _dict.UpdateSubjectAsync(id, new AdminSubjectUpsertDto
                {
                    SubjectName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "exams":
                await _dict.UpdateExamAsync(id, new AdminExamUpsertDto
                {
                    ExamName = NameText.Trim(),
                    Description = desc,
                    IsActive = IsActive
                });
                break;
            case "course-categories":
                await _dict.UpdateCourseCategoryAsync(id, new AdminCourseCategoryUpsertDto
                {
                    CategoryName = NameText.Trim(),
                    Description = desc,
                    SubjectId = ParseOptionalInt(SubjectIdText),
                    ExamId = ParseOptionalInt(ExamIdText)
                });
                break;
            default: throw new ApiException(HttpStatusCode.BadRequest, "Неизвестный справочник");
        }
    }

    private static int? ParseOptionalInt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private async Task DeleteAsync()
    {
        if (SelectedRegistry == null || SelectedRow == null) return;
        if (!UserDialogs.Confirm("Удалить запись?", "Справочники")) return;
        Error = null;
        IsBusy = true;
        try
        {
            var id = SelectedRow.Id;
            switch (SelectedRegistry.Code)
            {
                case "user-roles": await _dict.DeleteUserRoleAsync(id); break;
                case "application-statuses": await _dict.DeleteApplicationStatusAsync(id); break;
                case "assignment-types": await _dict.DeleteAssignmentTypeAsync(id); break;
                case "submission-statuses": await _dict.DeleteSubmissionStatusAsync(id); break;
                case "enrollment-statuses": await _dict.DeleteEnrollmentStatusAsync(id); break;
                case "order-statuses": await _dict.DeleteOrderStatusAsync(id); break;
                case "payment-statuses": await _dict.DeletePaymentStatusAsync(id); break;
                case "payment-methods": await _dict.DeletePaymentMethodAsync(id); break;
                case "discount-types": await _dict.DeleteDiscountTypeAsync(id); break;
                case "lesson-types": await _dict.DeleteLessonTypeAsync(id); break;
                case "subjects": await _dict.DeleteSubjectAsync(id); break;
                case "exams": await _dict.DeleteExamAsync(id); break;
                case "course-categories": await _dict.DeleteCourseCategoryAsync(id); break;
            }
            ClearForm();
            await LoadRowsAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task DeactivateAsync()
    {
        if (SelectedRegistry == null || SelectedRow == null || !SelectedRegistry.SupportsDeactivate) return;
        Error = null;
        IsBusy = true;
        try
        {
            var id = SelectedRow.Id;
            switch (SelectedRegistry.Code)
            {
                case "payment-methods": await _dict.PatchPaymentMethodActiveAsync(id, false); break;
                case "discount-types": await _dict.PatchDiscountTypeActiveAsync(id, false); break;
                case "subjects": await _dict.PatchSubjectActiveAsync(id, false); break;
                case "exams": await _dict.PatchExamActiveAsync(id, false); break;
            }
            await LoadRowsAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
