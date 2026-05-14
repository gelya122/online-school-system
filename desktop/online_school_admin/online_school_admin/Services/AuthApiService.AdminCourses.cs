using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using online_school_admin.Models.Admin;

namespace online_school_admin.Services;

public sealed partial class AuthApiService
{
    public Task<List<CourseTemplateDto>> GetCoursesAsync(CancellationToken ct = default) =>
        GetListAsync<CourseTemplateDto>("api/Courses", ct);

    public async Task<CourseTemplateDto?> GetCourseAsync(int courseId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/Courses/{courseId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<CourseTemplateDto>(JsonOptions, ct);
    }

    public async Task<CourseTemplateDto> CreateCourseAsync(CreateCourseRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/Courses", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<CourseTemplateDto>(JsonOptions, ct);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ при создании курса.");
        return body;
    }

    public async Task UpdateCourseAsync(int courseId, UpdateCourseRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/Courses/{courseId}", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<string> UploadCourseCoverAsync(int courseId, Stream fileStream, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        using var response = await _http.PostAsync($"api/Courses/{courseId}/cover", content, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<CoverUploadResponseDto>(JsonOptions, ct);
        return body?.AvatarUrl ?? "";
    }

    public Task<List<CourseModuleDto>> GetCourseModulesAsync(CancellationToken ct = default) =>
        GetListAsync<CourseModuleDto>("api/CourseModules", ct);

    public async Task<CourseModuleDto> CreateCourseModuleAsync(CreateCourseModuleRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/CourseModules", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<CourseModuleDto>(JsonOptions, ct);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ.");
        return body;
    }

    public async Task UpdateCourseModuleAsync(int id, UpdateCourseModuleRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/CourseModules/{id}", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task DeleteCourseModuleAsync(int id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/CourseModules/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public Task<List<LessonDto>> GetLessonsAsync(CancellationToken ct = default) =>
        GetListAsync<LessonDto>("api/Lessons", ct);

    public async Task<LessonDto> CreateLessonAsync(CreateLessonRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/Lessons", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<LessonDto>(JsonOptions, ct);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ.");
        return body;
    }

    public async Task UpdateLessonAsync(int id, UpdateLessonRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/Lessons/{id}", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task DeleteLessonAsync(int id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/Lessons/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<string> UploadLessonVideoAsync(int lessonId, Stream fileStream, string fileName, CancellationToken ct = default)
    {
        await using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, ct);
        return await UploadLessonVideoAsync(lessonId, ms.ToArray(), fileName, ct);
    }

    public async Task<string> UploadLessonVideoAsync(int lessonId, byte[] fileBytes, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileBytes), "file", fileName);
        using var response = await _http.PostAsync($"api/Lessons/{lessonId}/video", content, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<CoverUploadResponseDto>(JsonOptions, ct);
        return body?.AvatarUrl ?? "";
    }

    public async Task<LessonMaterialDto> UploadLessonMaterialAsync(int lessonId, Stream fileStream, string fileName, CancellationToken ct = default)
    {
        await using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, ct);
        return await UploadLessonMaterialAsync(lessonId, ms.ToArray(), fileName, ct);
    }

    public async Task<LessonMaterialDto> UploadLessonMaterialAsync(int lessonId, byte[] fileBytes, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileBytes), "file", fileName);
        using var response = await _http.PostAsync($"api/Lessons/{lessonId}/material", content, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<LessonMaterialDto>(JsonOptions, ct);
        return body ?? throw new AuthApiException(response.StatusCode, "Пустой ответ.");
    }

    public Task<List<LessonMaterialDto>> GetLessonMaterialsAsync(CancellationToken ct = default) =>
        GetListAsync<LessonMaterialDto>("api/LessonMaterials", ct);

    public async Task DeleteLessonMaterialAsync(int id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/LessonMaterials/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public Task<List<AssignmentDto>> GetAssignmentsAsync(CancellationToken ct = default) =>
        GetListAsync<AssignmentDto>("api/Assignments", ct);

    public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/Assignments", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<AssignmentDto>(JsonOptions, ct);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ.");
        return body;
    }

    public async Task UpdateAssignmentAsync(int id, UpdateAssignmentRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/Assignments/{id}", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task DeleteAssignmentAsync(int id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/Assignments/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public Task<List<ReviewDto>> GetReviewsAsync(CancellationToken ct = default) =>
        GetListAsync<ReviewDto>("api/Reviews", ct);

    public async Task UpdateReviewAsync(int id, UpdateReviewRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/Reviews/{id}", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public Task<List<CourseInstanceDto>> GetCourseInstancesAsync(CancellationToken ct = default) =>
        GetListAsync<CourseInstanceDto>("api/CourseInstances", ct);

    public async Task<CourseInstanceDto> CreateCourseInstanceAsync(CreateCourseInstanceRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/CourseInstances", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var body = await response.Content.ReadFromJsonAsync<CourseInstanceDto>(JsonOptions, ct);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ.");
        return body;
    }

    public async Task UpdateCourseInstanceAsync(int id, UpdateCourseInstanceRequest dto, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/CourseInstances/{id}", dto, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public Task<List<CourseCategoryDto>> GetCourseCategoriesAsync(CancellationToken ct = default) =>
        GetListAsync<CourseCategoryDto>("api/CourseCategories", ct);

    public Task<List<SubjectDto>> GetSubjectsAsync(CancellationToken ct = default) =>
        GetListAsync<SubjectDto>("api/Subjects", ct);

    public Task<List<ExamDto>> GetExamsAsync(CancellationToken ct = default) =>
        GetListAsync<ExamDto>("api/Exams", ct);

    public Task<List<LessonTypeDto>> GetLessonTypesAsync(CancellationToken ct = default) =>
        GetListAsync<LessonTypeDto>("api/LessonTypes", ct);

    public Task<List<AssignmentTypeDto>> GetAssignmentTypesAsync(CancellationToken ct = default) =>
        GetListAsync<AssignmentTypeDto>("api/AssignmentTypes", ct);

    public Task<List<EnrollmentDto>> GetEnrollmentsAsync(CancellationToken ct = default) =>
        GetListAsync<EnrollmentDto>("api/Enrollments", ct);

    private sealed class CoverUploadResponseDto
    {
        public string AvatarUrl { get; set; } = "";
    }
}
