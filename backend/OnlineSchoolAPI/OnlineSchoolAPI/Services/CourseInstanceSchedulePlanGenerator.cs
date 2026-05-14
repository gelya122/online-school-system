using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Services;

/// <summary>
/// Генерация строк <see cref="CourseSchedulePlan"/> по правилам JSON или по lessons_per_week (legacy).
/// </summary>
public static class CourseInstanceSchedulePlanGenerator
{
    public static async Task<(List<CourseSchedulePlan>? Plans, string? Error)> TryBuildPlansAsync(
        OnlineSchoolDbContext context,
        int instanceId,
        int courseId,
        DateOnly startDate,
        DateOnly? endDate,
        string? scheduleRulesJson,
        int? lessonsPerWeek,
        TimeOnly? defaultOpenTime,
        CancellationToken cancellationToken)
    {
        var lessonIds = await context.Lessons
            .AsNoTracking()
            .Where(l => l.Module.CourseId == courseId && l.DeletedAt == null && l.Module.DeletedAt == null)
            .OrderBy(l => l.Module.ModuleOrder)
            .ThenBy(l => l.LessonOrder)
            .Select(l => l.LessonId)
            .ToListAsync(cancellationToken);

        if (lessonIds.Count == 0)
            return (null, "В курсе нет уроков.");

        var defaultTime = defaultOpenTime ?? new TimeOnly(9, 0);
        var plans = TryCreatePlansFromScheduleRulesJson(instanceId, startDate, endDate, scheduleRulesJson, lessonIds, out var rulesError);
        if (plans != null)
            return (plans, null);

        var lpw = lessonsPerWeek ?? 0;
        if (lpw <= 0)
            return (null, string.IsNullOrWhiteSpace(rulesError)
                ? "Укажите lessons_per_week или корректный schedule_rules_json на потоке."
                : rulesError);

        return (BuildPlansLegacy(instanceId, startDate, lessonIds, lpw, defaultTime), null);
    }

    public static List<CourseSchedulePlan>? TryCreatePlansFromScheduleRulesJson(
        int instanceId,
        DateOnly startDate,
        DateOnly? endDate,
        string? scheduleRulesJson,
        IReadOnlyList<int> lessonIds,
        out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(scheduleRulesJson))
            return null;

        if (!TryParseWeeklyRules(scheduleRulesJson, out var weekly, out error) || weekly.Count == 0)
            return null;

        var end = endDate ?? startDate.AddDays(Math.Min(730, Math.Max(lessonIds.Count * 14, 90)));
        var slots = BuildOrderedSlots(startDate, end, weekly);
        if (slots.Count < lessonIds.Count)
        {
            error =
                $"По правилам расписания найдено слотов: {slots.Count}, уроков: {lessonIds.Count}. Расширьте период потока (end_date) или добавьте правила.";
            return null;
        }

        var now = DateTime.UtcNow;
        var plans = new List<CourseSchedulePlan>();
        for (var i = 0; i < lessonIds.Count; i++)
        {
            var (day, time) = slots[i];
            var offset = day.DayNumber - startDate.DayNumber;
            if (offset < 0) offset = 0;
            var scheduledAt = DateTime.SpecifyKind(day.ToDateTime(time), DateTimeKind.Utc);
            plans.Add(new CourseSchedulePlan
            {
                InstanceId = instanceId,
                LessonId = lessonIds[i],
                ReleaseDayOffset = offset,
                ReleaseTime = time,
                CreatedAt = now,
                IsPublished = true,
                LessonOrder = i + 1,
                ScheduledAt = scheduledAt
            });
        }

        return plans;
    }

    public static List<CourseSchedulePlan> BuildPlansLegacy(
        int instanceId,
        DateOnly startDate,
        IReadOnlyList<int> lessonIds,
        int lessonsPerWeek,
        TimeOnly defaultTime)
    {
        var step = 7.0 / lessonsPerWeek;
        var plans = new List<CourseSchedulePlan>();
        var lastOffset = -1;
        var now = DateTime.UtcNow;
        for (var i = 0; i < lessonIds.Count; i++)
        {
            var raw = (int)Math.Floor(i * step);
            if (raw < lastOffset) raw = lastOffset;
            if (raw == lastOffset) raw = lastOffset + 1;
            lastOffset = raw;

            var scheduledAt = DateTime.SpecifyKind(startDate.AddDays(raw).ToDateTime(defaultTime), DateTimeKind.Utc);
            plans.Add(new CourseSchedulePlan
            {
                InstanceId = instanceId,
                LessonId = lessonIds[i],
                ReleaseDayOffset = raw,
                ReleaseTime = defaultTime,
                CreatedAt = now,
                IsPublished = true,
                LessonOrder = i + 1,
                ScheduledAt = scheduledAt
            });
        }

        return plans;
    }

    private sealed class ScheduleRuleJsonItem
    {
        [JsonPropertyName("dayOfWeek")]
        public int DayOfWeek { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }

    private static bool TryParseWeeklyRules(string json, out List<(DayOfWeek Dow, TimeOnly Time)> weekly, out string? error)
    {
        weekly = [];
        error = null;
        try
        {
            var items = JsonSerializer.Deserialize<List<ScheduleRuleJsonItem>>(json.Trim(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (items == null || items.Count == 0)
                return false;

            foreach (var it in items)
            {
                if (it.DayOfWeek is < 1 or > 7)
                {
                    error = "dayOfWeek в schedule_rules_json должен быть от 1 (понедельник) до 7 (воскресенье).";
                    weekly = [];
                    return false;
                }

                if (string.IsNullOrWhiteSpace(it.Time) || !TimeOnly.TryParse(it.Time.Trim(), out var tm))
                {
                    error = "Каждое правило должно содержать корректное поле time (например, 18:00).";
                    weekly = [];
                    return false;
                }

                weekly.Add((UserIsoDayToDayOfWeek(it.DayOfWeek), tm));
            }

            return weekly.Count > 0;
        }
        catch (JsonException ex)
        {
            error = "Не удалось разобрать schedule_rules_json: " + ex.Message;
            weekly = [];
            return false;
        }
    }

    private static DayOfWeek UserIsoDayToDayOfWeek(int d) =>
        d == 7 ? DayOfWeek.Sunday : (DayOfWeek)d;

    private static List<(DateOnly Day, TimeOnly Time)> BuildOrderedSlots(
        DateOnly start,
        DateOnly endInclusive,
        IReadOnlyList<(DayOfWeek Dow, TimeOnly Time)> weeklyRules)
    {
        var distinct = weeklyRules
            .GroupBy(x => (x.Dow, x.Time))
            .Select(g => g.Key)
            .ToList();

        var list = new List<(DateOnly, TimeOnly)>();
        for (var d = start; d <= endInclusive; d = d.AddDays(1))
        {
            foreach (var (dow, tm) in distinct.OrderBy(x => x.Time))
            {
                if (d.DayOfWeek == dow)
                    list.Add((d, tm));
            }
        }

        list.Sort((a, b) =>
        {
            var c = a.Item1.CompareTo(b.Item1);
            return c != 0 ? c : a.Item2.CompareTo(b.Item2);
        });
        return list;
    }
}
