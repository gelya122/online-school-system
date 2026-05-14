using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminDictionariesService
{
    private readonly ApiClient _api;

    public AdminDictionariesService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminDictionaryRegistryItemDto>> GetRegistryAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminDictionaryRegistryItemDto>>("api/admin/dictionaries/registry", cancellationToken);

    public Task<List<AdminUserRoleDictDto>> GetUserRolesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminUserRoleDictDto>>("api/admin/dictionaries/user-roles", cancellationToken);

    public Task CreateUserRoleAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/user-roles", dto, cancellationToken);

    public Task UpdateUserRoleAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/user-roles/{id}", dto, cancellationToken);

    public Task DeleteUserRoleAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/user-roles/{id}", cancellationToken);

    public Task<List<AdminSimpleStatusDictDto>> GetApplicationStatusesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSimpleStatusDictDto>>("api/admin/dictionaries/application-statuses", cancellationToken);

    public Task CreateApplicationStatusAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/application-statuses", dto, cancellationToken);

    public Task UpdateApplicationStatusAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/application-statuses/{id}", dto, cancellationToken);

    public Task DeleteApplicationStatusAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/application-statuses/{id}", cancellationToken);

    public Task<List<AdminSimpleStatusDictDto>> GetSubmissionStatusesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSimpleStatusDictDto>>("api/admin/dictionaries/submission-statuses", cancellationToken);

    public Task CreateSubmissionStatusAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/submission-statuses", dto, cancellationToken);

    public Task UpdateSubmissionStatusAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/submission-statuses/{id}", dto, cancellationToken);

    public Task DeleteSubmissionStatusAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/submission-statuses/{id}", cancellationToken);

    public Task<List<AdminSimpleStatusDictDto>> GetEnrollmentStatusesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSimpleStatusDictDto>>("api/admin/dictionaries/enrollment-statuses", cancellationToken);

    public Task CreateEnrollmentStatusAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/enrollment-statuses", dto, cancellationToken);

    public Task UpdateEnrollmentStatusAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/enrollment-statuses/{id}", dto, cancellationToken);

    public Task DeleteEnrollmentStatusAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/enrollment-statuses/{id}", cancellationToken);

    public Task<List<AdminSimpleStatusDictDto>> GetOrderStatusesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSimpleStatusDictDto>>("api/admin/dictionaries/order-statuses", cancellationToken);

    public Task CreateOrderStatusAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/order-statuses", dto, cancellationToken);

    public Task UpdateOrderStatusAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/order-statuses/{id}", dto, cancellationToken);

    public Task DeleteOrderStatusAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/order-statuses/{id}", cancellationToken);

    public Task<List<AdminSimpleStatusDictDto>> GetPaymentStatusesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSimpleStatusDictDto>>("api/admin/dictionaries/payment-statuses", cancellationToken);

    public Task CreatePaymentStatusAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/payment-statuses", dto, cancellationToken);

    public Task UpdatePaymentStatusAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/payment-statuses/{id}", dto, cancellationToken);

    public Task DeletePaymentStatusAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/payment-statuses/{id}", cancellationToken);

    public Task<List<AdminAssignmentTypeDictDto>> GetAssignmentTypesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminAssignmentTypeDictDto>>("api/admin/dictionaries/assignment-types", cancellationToken);

    public Task CreateAssignmentTypeAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/assignment-types", dto, cancellationToken);

    public Task UpdateAssignmentTypeAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/assignment-types/{id}", dto, cancellationToken);

    public Task DeleteAssignmentTypeAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/assignment-types/{id}", cancellationToken);

    public Task<List<AdminLessonTypeDictDto>> GetLessonTypesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminLessonTypeDictDto>>("api/admin/dictionaries/lesson-types", cancellationToken);

    public Task CreateLessonTypeAsync(AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/lesson-types", dto, cancellationToken);

    public Task UpdateLessonTypeAsync(int id, AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/lesson-types/{id}", dto, cancellationToken);

    public Task DeleteLessonTypeAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/lesson-types/{id}", cancellationToken);

    public Task<List<AdminPaymentMethodDictDto>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminPaymentMethodDictDto>>("api/admin/dictionaries/payment-methods", cancellationToken);

    public Task CreatePaymentMethodAsync(AdminPaymentMethodUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/payment-methods", dto, cancellationToken);

    public Task UpdatePaymentMethodAsync(int id, AdminPaymentMethodUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/payment-methods/{id}", dto, cancellationToken);

    public Task PatchPaymentMethodActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/dictionaries/payment-methods/{id}/active", new AdminDictActiveDto { IsActive = isActive }, cancellationToken);

    public Task DeletePaymentMethodAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/payment-methods/{id}", cancellationToken);

    public Task<List<AdminDiscountTypeDictDto>> GetDiscountTypesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminDiscountTypeDictDto>>("api/admin/dictionaries/discount-types", cancellationToken);

    public Task CreateDiscountTypeAsync(AdminDiscountTypeUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/discount-types", dto, cancellationToken);

    public Task UpdateDiscountTypeAsync(int id, AdminDiscountTypeUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/discount-types/{id}", dto, cancellationToken);

    public Task PatchDiscountTypeActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/dictionaries/discount-types/{id}/active", new AdminDictActiveDto { IsActive = isActive }, cancellationToken);

    public Task DeleteDiscountTypeAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/discount-types/{id}", cancellationToken);

    public Task<List<AdminSubjectDictDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSubjectDictDto>>("api/admin/dictionaries/subjects", cancellationToken);

    public Task CreateSubjectAsync(AdminSubjectUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/subjects", dto, cancellationToken);

    public Task UpdateSubjectAsync(int id, AdminSubjectUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/subjects/{id}", dto, cancellationToken);

    public Task PatchSubjectActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/dictionaries/subjects/{id}/active", new AdminDictActiveDto { IsActive = isActive }, cancellationToken);

    public Task DeleteSubjectAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/subjects/{id}", cancellationToken);

    public Task<List<AdminExamDictDto>> GetExamsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminExamDictDto>>("api/admin/dictionaries/exams", cancellationToken);

    public Task CreateExamAsync(AdminExamUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/exams", dto, cancellationToken);

    public Task UpdateExamAsync(int id, AdminExamUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/exams/{id}", dto, cancellationToken);

    public Task PatchExamActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/dictionaries/exams/{id}/active", new AdminDictActiveDto { IsActive = isActive }, cancellationToken);

    public Task DeleteExamAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/exams/{id}", cancellationToken);

    public Task<List<AdminCourseCategoryDictDto>> GetCourseCategoriesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminCourseCategoryDictDto>>("api/admin/dictionaries/course-categories", cancellationToken);

    public Task CreateCourseCategoryAsync(AdminCourseCategoryUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/dictionaries/course-categories", dto, cancellationToken);

    public Task UpdateCourseCategoryAsync(int id, AdminCourseCategoryUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/dictionaries/course-categories/{id}", dto, cancellationToken);

    public Task DeleteCourseCategoryAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/dictionaries/course-categories/{id}", cancellationToken);
}
