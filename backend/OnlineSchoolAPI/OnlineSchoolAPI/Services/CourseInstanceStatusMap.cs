using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Services;

/// <summary>Соответствие кода статуса потока (course_instance_status.code) и status_id из script5.</summary>
public static class CourseInstanceStatusMap
{
    /// <summary>Значение по умолчанию для нового потока: planned.</summary>
    public const int DefaultStatusId = 1;

    private static readonly Dictionary<string, int> CodeToId = new(StringComparer.OrdinalIgnoreCase)
    {
        ["planned"] = 1,
        ["enrollment_open"] = 2,
        ["enrollment_closed"] = 3,
        ["active"] = 4,
        ["completed"] = 5,
        ["cancelled"] = 6,
        ["paused"] = 7,
        // ранее использовавшиеся строки в API / десктопе
        ["draft"] = 1,
        ["archived"] = 6,
    };

    public static int ResolveStatusId(string? code)
    {
        var c = (code ?? "").Trim();
        if (string.IsNullOrEmpty(c))
            return DefaultStatusId;
        if (CodeToId.TryGetValue(c, out var id))
            return id;
        throw new ArgumentException($"Неизвестный код статуса потока: «{code}». Допустимы: {string.Join(", ", CodeToId.Keys)}.");
    }

    /// <summary>Нормализация query-параметра фильтра (старые значения → новые коды).</summary>
    public static string NormalizeFilterCode(string status)
    {
        var s = status.Trim().ToLowerInvariant();
        return s switch
        {
            "draft" => "planned",
            "archived" => "cancelled",
            _ => s
        };
    }
}
