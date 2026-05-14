using System.IO;
using online_school_admin.Models;
using online_school_admin.Models.Admin;

namespace online_school_admin.Services;

public sealed class AdminCoursesService
{
    private readonly ApiClient _api;

    public AdminCoursesService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminCourseCategoryDictDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminCourseCategoryDictDto>>("api/admin/dictionaries/course-categories", cancellationToken);

    public Task<List<AdminSubjectDictDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSubjectDictDto>>("api/admin/dictionaries/subjects", cancellationToken);

    public Task<List<AdminExamDictDto>> GetExamsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminExamDictDto>>("api/admin/dictionaries/exams", cancellationToken);

    public Task<List<LessonTypeDto>> GetLessonTypesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<LessonTypeDto>>("api/LessonTypes", cancellationToken);

    public Task<List<AssignmentTypeDto>> GetAssignmentTypesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AssignmentTypeDto>>("api/AssignmentTypes", cancellationToken);

    public async Task<string> UploadCourseCoverAsync(int courseId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var resp = await _api.PostMultipartAsync<CoverUploadResponseDto>($"api/Courses/{courseId}/cover", fileStream, "file", fileName,
            contentType: null, cancellationToken: cancellationToken);
        return resp.AvatarUrl ?? "";
    }

    public async Task<string> UploadLessonVideoAsync(int lessonId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        await using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, cancellationToken);
        return await UploadLessonVideoAsync(lessonId, ms.ToArray(), fileName, cancellationToken);
    }

    public async Task<string> UploadLessonVideoAsync(int lessonId, byte[] fileBytes, string fileName, CancellationToken cancellationToken = default)
    {
        var resp = await _api.PostMultipartBytesAsync<CoverUploadResponseDto>($"api/Lessons/{lessonId}/video", fileBytes, "file", fileName,
            contentType: null, cancellationToken: cancellationToken);
        return resp.AvatarUrl ?? "";
    }

    /// <summary>Файл на сервер и запись в lesson_material (multipart, поле file).</summary>
    public Task<AdminLessonMaterialRowDto> UploadLessonMaterialFileAsync(int lessonId, byte[] fileBytes, string fileName, CancellationToken cancellationToken = default)
        => _api.PostMultipartBytesAsync<AdminLessonMaterialRowDto>($"api/Lessons/{lessonId}/material", fileBytes, "file", fileName,
            contentType: null, cancellationToken: cancellationToken);

    public Task<List<AdminCourseListRowDto>> GetCoursesAsync(
        string? search = null,
        int? categoryId = null,
        int? subjectId = null,
        int? examId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (categoryId.HasValue) qs.Add($"categoryId={categoryId.Value}");
        if (subjectId.HasValue) qs.Add($"subjectId={subjectId.Value}");
        if (examId.HasValue) qs.Add($"examId={examId.Value}");
        if (isActive.HasValue) qs.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/admin/courses";
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return _api.GetAsync<List<AdminCourseListRowDto>>(url, cancellationToken);
    }

    public Task<AdminCourseDetailsDto> GetCourseAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminCourseDetailsDto>($"api/admin/courses/{id}", cancellationToken);

    public Task<AdminCourseDetailsDto> CreateAsync(AdminCourseUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminCourseUpsertDto, AdminCourseDetailsDto>("api/admin/courses", dto, cancellationToken);

    public Task UpdateAsync(int id, AdminCourseUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/courses/{id}", dto, cancellationToken);

    public Task PublishAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/courses/{id}/publish", cancellationToken);

    public Task HideAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/courses/{id}/hide", cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/courses/{id}", cancellationToken);

    // structure
    public Task<List<AdminCourseModuleRowDto>> GetModulesAsync(int courseId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminCourseModuleRowDto>>($"api/admin/courses/{courseId}/modules", cancellationToken);

    public Task<AdminCourseModuleRowDto> CreateModuleAsync(int courseId, AdminCourseModuleUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminCourseModuleUpsertDto, AdminCourseModuleRowDto>($"api/admin/courses/{courseId}/modules", dto, cancellationToken);

    public Task UpdateModuleAsync(int moduleId, AdminCourseModuleUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/course-modules/{moduleId}", dto, cancellationToken);

    public Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/course-modules/{moduleId}", cancellationToken);

    public Task ReorderModulesAsync(AdminReorderRequestDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync("api/admin/course-modules/reorder", dto, cancellationToken);

    public Task<List<AdminLessonRowDto>> GetLessonsAsync(int moduleId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminLessonRowDto>>($"api/admin/course-modules/{moduleId}/lessons", cancellationToken);

    public Task<AdminLessonRowDto> CreateLessonAsync(int moduleId, AdminLessonCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminLessonCreateDto, AdminLessonRowDto>($"api/admin/course-modules/{moduleId}/lessons", dto, cancellationToken);

    public Task UpdateLessonAsync(int lessonId, AdminLessonUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/lessons/{lessonId}", dto, cancellationToken);

    public Task DeleteLessonAsync(int lessonId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/lessons/{lessonId}", cancellationToken);

    public Task ReorderLessonsAsync(AdminReorderRequestDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync("api/admin/lessons/reorder", dto, cancellationToken);

    public Task<List<AdminLessonMaterialRowDto>> GetMaterialsAsync(int lessonId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminLessonMaterialRowDto>>($"api/admin/lessons/{lessonId}/materials", cancellationToken);

    public Task<AdminLessonMaterialRowDto> CreateMaterialAsync(int lessonId, AdminLessonMaterialCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminLessonMaterialCreateDto, AdminLessonMaterialRowDto>($"api/admin/lessons/{lessonId}/materials", dto, cancellationToken);

    public Task UpdateMaterialAsync(int materialId, AdminLessonMaterialUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/lesson-materials/{materialId}", dto, cancellationToken);

    public Task DeleteMaterialAsync(int materialId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/lesson-materials/{materialId}", cancellationToken);

    private sealed class CoverUploadResponseDto
    {
        public string? AvatarUrl { get; set; }
    }

    // homeworks (lesson -> homeworks -> tasks -> answers)
    public Task<List<AdminHomeworkRowDto>> GetHomeworksAsync(int lessonId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminHomeworkRowDto>>($"api/admin/lessons/{lessonId}/homeworks", cancellationToken);

    public Task<AdminHomeworkRowDto> CreateHomeworkAsync(int lessonId, AdminHomeworkUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminHomeworkUpsertDto, AdminHomeworkRowDto>($"api/admin/lessons/{lessonId}/homeworks", dto, cancellationToken);

    public Task UpdateHomeworkAsync(int homeworkId, AdminHomeworkUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/homeworks/{homeworkId}", dto, cancellationToken);

    public Task DeleteHomeworkAsync(int homeworkId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/homeworks/{homeworkId}", cancellationToken);

    public Task<List<AdminHomeworkTaskRowDto>> GetHomeworkTasksAsync(int homeworkId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminHomeworkTaskRowDto>>($"api/admin/homeworks/{homeworkId}/tasks", cancellationToken);

    public Task<AdminHomeworkTaskRowDto> CreateHomeworkTaskAsync(int homeworkId, AdminHomeworkTaskUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminHomeworkTaskUpsertDto, AdminHomeworkTaskRowDto>($"api/admin/homeworks/{homeworkId}/tasks", dto, cancellationToken);

    public Task UpdateHomeworkTaskAsync(int taskId, AdminHomeworkTaskUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/homework-tasks/{taskId}", dto, cancellationToken);

    public Task DeleteHomeworkTaskAsync(int taskId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/homework-tasks/{taskId}", cancellationToken);

    public Task ReorderHomeworkTasksAsync(AdminReorderRequestDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync("api/admin/homework-tasks/reorder", dto, cancellationToken);

    public Task<List<AdminHomeworkTaskAnswerRowDto>> GetHomeworkTaskAnswersAsync(int taskId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminHomeworkTaskAnswerRowDto>>($"api/admin/homework-tasks/{taskId}/answers", cancellationToken);

    public Task<AdminHomeworkTaskAnswerRowDto> CreateHomeworkTaskAnswerAsync(int taskId, AdminHomeworkTaskAnswerUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminHomeworkTaskAnswerUpsertDto, AdminHomeworkTaskAnswerRowDto>($"api/admin/homework-tasks/{taskId}/answers", dto, cancellationToken);

    public Task UpdateHomeworkTaskAnswerAsync(int answerId, AdminHomeworkTaskAnswerUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/homework-task-answers/{answerId}", dto, cancellationToken);

    public Task DeleteHomeworkTaskAnswerAsync(int answerId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/homework-task-answers/{answerId}", cancellationToken);
}

