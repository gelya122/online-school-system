using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed partial class AuthApiService
{
    private const int StudentRoleId = 7;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public AuthApiService(HttpClient http)
    {
        _http = http;
    }

    public Uri? ApiBaseAddress => _http.BaseAddress;

    public string? ToAbsoluteUrl(string? relativeOrAbsolute)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            return null;
        if (Uri.TryCreate(relativeOrAbsolute, UriKind.Absolute, out var absolute))
            return absolute.ToString();
        if (_http.BaseAddress == null)
            return relativeOrAbsolute;
        return new Uri(_http.BaseAddress, relativeOrAbsolute.TrimStart('/')).ToString();
    }

    public async Task<LoginAdminResponse> LoginAdminAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync(
            "api/Registration/admin/login",
            new { email, password },
            cancellationToken);

        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginAdminResponse>(JsonOptions, cancellationToken);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ сервера.");
        return body;
    }

    public async Task<RegisterAdminResponse> RegisterAdminAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? patronymic,
        string phone,
        int roleId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync(
            "api/Registration/admin/register",
            new
            {
                email,
                password,
                firstName,
                lastName,
                patronymic,
                phone,
                roleId
            },
            cancellationToken);

        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<RegisterAdminResponse>(JsonOptions, cancellationToken);
        if (body == null)
            throw new AuthApiException(response.StatusCode, "Пустой ответ сервера.");
        return body;
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetStaffRolesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync("api/UserRoles", cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var list = await response.Content.ReadFromJsonAsync<List<UserRoleDto>>(JsonOptions, cancellationToken);
        if (list == null)
            return Array.Empty<UserRoleDto>();
        return list.Where(r => r.RoleId != StudentRoleId).ToList();
    }

    public async Task<AdminDataSnapshot> GetAdminDataSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var studentsTask = GetListAsync<ApiStudentDto>("api/Students", cancellationToken);
        var usersTask = GetListAsync<ApiUserDto>("api/Users", cancellationToken);
        var enrollmentsTask = GetListAsync<ApiEnrollmentDto>("api/Enrollments", cancellationToken);
        var instancesTask = GetListAsync<ApiCourseInstanceDto>("api/CourseInstances", cancellationToken);
        var coursesTask = GetListAsync<ApiCourseDto>("api/Courses", cancellationToken);
        var ordersTask = GetListAsync<ApiAppOrderDto>("api/AppOrders", cancellationToken);
        var paymentsTask = GetListAsync<ApiPaymentDto>("api/Payments", cancellationToken);
        var submissionsTask = GetListAsync<ApiSubmissionDto>("api/Submissions", cancellationToken);
        var progressesTask = GetListAsync<ApiStudentProgressDto>("api/StudentProgresses", cancellationToken);
        var assignmentsTask = GetListAsync<ApiAssignmentDto>("api/Assignments", cancellationToken);
        var trialsTask = GetListAsync<ApiTrialApplicationDto>("api/TrialApplications", cancellationToken);

        await Task.WhenAll(studentsTask, usersTask, enrollmentsTask, instancesTask, coursesTask, ordersTask, paymentsTask,
            submissionsTask, progressesTask, assignmentsTask, trialsTask);

        var students = studentsTask.Result;
        var users = usersTask.Result;
        var enrollments = enrollmentsTask.Result;
        var instances = instancesTask.Result;
        var courses = coursesTask.Result;
        var orders = ordersTask.Result;
        var payments = paymentsTask.Result;
        var submissions = submissionsTask.Result;
        var progresses = progressesTask.Result;
        var assignments = assignmentsTask.Result;
        var trials = trialsTask.Result;

        var userById = users.ToDictionary(x => x.UserId);
        var enrollmentById = enrollments.ToDictionary(x => x.EnrollmentId);
        var instanceById = instances.ToDictionary(x => x.InstanceId);
        var courseById = courses.ToDictionary(x => x.CourseId);
        var assignmentById = assignments.ToDictionary(x => x.AssignmentId);

        var progressesByEnrollment = progresses
            .GroupBy(x => x.EnrollmentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var progressesByStudentId = progresses
            .Where(p => enrollmentById.ContainsKey(p.EnrollmentId))
            .GroupBy(p => enrollmentById[p.EnrollmentId].StudentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var studentIdByEnrollment = enrollments.ToDictionary(e => e.EnrollmentId, e => e.StudentId);
        var submissionsByStudentId = submissions
            .Where(s => s.EnrollmentId.HasValue && studentIdByEnrollment.ContainsKey(s.EnrollmentId.Value))
            .GroupBy(s => studentIdByEnrollment[s.EnrollmentId!.Value])
            .Where(g => g.Key > 0)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ordersByStudent = orders.GroupBy(x => x.StudentId).ToDictionary(g => g.Key, g => g.ToList());
        var paymentsByOrder = payments.GroupBy(x => x.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        var studentItems = new List<StudentListItem>(students.Count);
        var now = DateTime.UtcNow;
        var last30 = now.AddDays(-30);

        foreach (var student in students)
        {
            userById.TryGetValue(student.UserId, out var user);
            var studentEnrollments = enrollments.Where(e => e.StudentId == student.StudentId).ToList();
            var studentProgresses = progressesByStudentId.GetValueOrDefault(student.StudentId, []);
            var studentSubmissions = submissionsByStudentId.GetValueOrDefault(student.StudentId, []);
            var studentOrders = ordersByStudent.GetValueOrDefault(student.StudentId, []);

            var coursesProgress = BuildCourseProgress(studentEnrollments, progressesByEnrollment, instanceById, courseById);
            var paymentHistory = BuildPaymentHistory(studentOrders, paymentsByOrder);
            var homework = BuildHomeworkSubmissions(studentSubmissions, progresses, enrollmentById, assignmentById, instanceById, courseById);
            var activity = BuildActivityStats(studentProgresses, studentSubmissions);

            var activityEventsLast30 = studentProgresses.Count(x => (x.LastAccessed ?? x.CreatedAt) >= last30) +
                                       studentSubmissions.Count(x => (x.SubmittedAt ?? x.CreatedAt) >= last30);

            var activityStatus = activityEventsLast30 switch
            {
                >= 20 => "Высокая активность",
                >= 8 => "Средняя активность",
                _ => "Низкая активность"
            };

            var item = new StudentListItem
            {
                StudentId = student.StudentId,
                FullName = $"{student.FirstName} {student.LastName}".Trim(),
                Email = user?.Email ?? "—",
                Phone = string.IsNullOrWhiteSpace(student.Phone) ? "—" : student.Phone,
                ClassName = student.ClassNumber > 0 ? $"{student.ClassNumber} класс" : "Без класса",
                RegisteredAt = student.CreatedAt ?? user?.CreatedAt,
                ActivityStatus = activityStatus
            };

            item.Courses.AddRange(coursesProgress);
            item.PaymentHistory.AddRange(paymentHistory);
            item.HomeworkSubmissions.AddRange(homework);
            item.ActivityStats.AddRange(activity);
            studentItems.Add(item);
        }

        var activeCourses = courses.Count(x => x.IsActive == true);
        var newTrials = trials.Count(x => x.CreatedAt.HasValue && x.CreatedAt.Value >= DateTime.UtcNow.AddDays(-7));
        var ordersInPayment = orders.Count(x => !x.PaidAt.HasValue);
        var homeworkPending = submissions.Count(x => !x.Score.HasValue);

        return new AdminDataSnapshot
        {
            Students = studentItems,
            ActiveCoursesCount = activeCourses,
            NewTrialApplicationsCount = newTrials,
            OrdersInPaymentCount = ordersInPayment,
            HomeworkPendingReviewCount = homeworkPending
        };
    }

    private async Task<List<T>> GetListAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(path, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var data = await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken);
        return data ?? [];
    }

    private static List<StudentCourseProgress> BuildCourseProgress(
        List<ApiEnrollmentDto> enrollments,
        Dictionary<int, List<ApiStudentProgressDto>> progressesByEnrollment,
        Dictionary<int, ApiCourseInstanceDto> instanceById,
        Dictionary<int, ApiCourseDto> courseById)
    {
        var result = new List<StudentCourseProgress>();

        foreach (var enrollment in enrollments)
        {
            var progressItems = progressesByEnrollment.GetValueOrDefault(enrollment.EnrollmentId, []);
            var total = progressItems.Count;
            var completed = progressItems.Count(x => x.IsCompleted == true);
            var percent = total == 0 ? 0 : (int)Math.Round((double)completed * 100 / total);

            var title = "Курс";
            if (instanceById.TryGetValue(enrollment.InstanceId, out var instance))
            {
                if (courseById.TryGetValue(instance.CourseId, out var course))
                    title = course.Title;
                else
                    title = instance.InstanceName;
            }

            var status = percent switch
            {
                >= 100 => "Завершён",
                0 => "Старт",
                _ => "В процессе"
            };

            result.Add(new StudentCourseProgress
            {
                CourseTitle = title,
                ProgressPercent = percent,
                Status = status
            });
        }

        return result;
    }

    private static List<StudentPaymentRecord> BuildPaymentHistory(
        List<ApiAppOrderDto> orders,
        Dictionary<int, List<ApiPaymentDto>> paymentsByOrder)
    {
        var result = new List<StudentPaymentRecord>();

        foreach (var order in orders)
        {
            var orderPayments = paymentsByOrder.GetValueOrDefault(order.OrderId, []);
            if (orderPayments.Count == 0)
            {
                result.Add(new StudentPaymentRecord
                {
                    Date = order.PaidAt ?? order.CreatedAt,
                    Amount = order.FinalAmount,
                    Status = order.PaidAt.HasValue ? "Оплачено" : "Ожидает оплату",
                    Method = order.MethodName ?? "Не указан"
                });
                continue;
            }

            foreach (var payment in orderPayments)
            {
                result.Add(new StudentPaymentRecord
                {
                    Date = payment.PaidAt ?? payment.CreatedAt,
                    Amount = payment.Amount,
                    Status = payment.PaymentStatusId == 2 ? "Оплачено" : "В обработке",
                    Method = payment.MethodName ?? "Не указан"
                });
            }
        }

        return result.OrderByDescending(x => x.Date).ToList();
    }

    private static List<StudentHomeworkSubmission> BuildHomeworkSubmissions(
        List<ApiSubmissionDto> submissions,
        List<ApiStudentProgressDto> allProgress,
        Dictionary<int, ApiEnrollmentDto> enrollmentById,
        Dictionary<int, ApiAssignmentDto> assignmentById,
        Dictionary<int, ApiCourseInstanceDto> instanceById,
        Dictionary<int, ApiCourseDto> courseById)
    {
        var result = new List<StudentHomeworkSubmission>();

        foreach (var submission in submissions)
        {
            var homeworkTitle = assignmentById.TryGetValue(submission.AssignmentId, out var assignment)
                ? assignment.Title
                : $"Задание #{submission.AssignmentId}";

            var courseTitle = "Курс";
            if (submission.EnrollmentId.HasValue &&
                enrollmentById.TryGetValue(submission.EnrollmentId.Value, out var enrollment) &&
                instanceById.TryGetValue(enrollment.InstanceId, out var instance) &&
                courseById.TryGetValue(instance.CourseId, out var course))
            {
                courseTitle = course.Title;
            }

            result.Add(new StudentHomeworkSubmission
            {
                CourseTitle = courseTitle,
                HomeworkTitle = homeworkTitle,
                SubmittedAt = submission.SubmittedAt ?? submission.CreatedAt,
                Grade = submission.Score?.ToString() ?? "Без оценки"
            });
        }

        return result.OrderByDescending(x => x.SubmittedAt).ToList();
    }

    private static List<StudentActivityStat> BuildActivityStats(
        List<ApiStudentProgressDto> progresses,
        List<ApiSubmissionDto> submissions)
    {
        var totalLessons = progresses.Count;
        var visitedLessons = progresses.Count(x => x.LastAccessed.HasValue || x.IsCompleted == true);
        var doneHomework = submissions.Count(x => x.SubmittedAt.HasValue || x.CreatedAt.HasValue);

        var watchSeconds = progresses.Sum(x => x.WatchTimeSeconds ?? 0);
        var days = progresses
            .Select(x => x.LastAccessed?.Date ?? x.CreatedAt?.Date)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .Count();
        var avgMinutes = days == 0 ? 0 : Math.Round((watchSeconds / 60.0) / days, 1);

        return
        [
            new StudentActivityStat { Metric = "Посещено уроков", Value = $"{visitedLessons} из {Math.Max(totalLessons, visitedLessons)}" },
            new StudentActivityStat { Metric = "Среднее время в платформе", Value = $"{avgMinutes:0.#} мин/день" },
            new StudentActivityStat { Metric = "Выполнено ДЗ", Value = doneHomework.ToString() }
        ];
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var text = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (text.StartsWith('"') && text.EndsWith('"') && text.Length >= 2)
            text = text[1..^1].Replace("\\\"", "\"", StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(text))
            text = response.ReasonPhrase ?? response.StatusCode.ToString();

        throw new AuthApiException(response.StatusCode, text);
    }

    private sealed class ApiStudentDto
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? Phone { get; set; }
        public int ClassNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    private sealed class ApiUserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
    }

    private sealed class ApiEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int InstanceId { get; set; }
    }

    private sealed class ApiCourseInstanceDto
    {
        public int InstanceId { get; set; }
        public int CourseId { get; set; }
        public string InstanceName { get; set; } = "";
    }

    private sealed class ApiCourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public bool? IsActive { get; set; }
    }

    private sealed class ApiAppOrderDto
    {
        public int OrderId { get; set; }
        public int StudentId { get; set; }
        public decimal FinalAmount { get; set; }
        public string? MethodName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    private sealed class ApiPaymentDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int? PaymentStatusId { get; set; }
        public string? MethodName { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    private sealed class ApiSubmissionDto
    {
        public int? EnrollmentId { get; set; }
        public int AssignmentId { get; set; }
        public int? Score { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    private sealed class ApiStudentProgressDto
    {
        public int ProgressId { get; set; }
        public int EnrollmentId { get; set; }
        public bool? IsCompleted { get; set; }
        public int? WatchTimeSeconds { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    private sealed class ApiAssignmentDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = "";
    }

    private sealed class ApiTrialApplicationDto
    {
        public DateTime? CreatedAt { get; set; }
    }
}
