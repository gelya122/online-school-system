namespace OnlineSchoolAPI.Services;

public static class RoleNameMapper
{
    /// <summary>
    /// Приводит произвольное имя роли из БД к каноническому виду: admin/manager/teacher.
    /// Если роль не распознана — возвращает нормализованную строку (lower/trim).
    /// </summary>
    public static string Canonicalize(string? roleName)
    {
        var r = (roleName ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(r))
            return "";

        // admin
        if (r is "admin" or "administrator" or "админ" or "администратор" or "superadmin" or "super-admin")
            return "admin";
        if (r.Contains("admin", StringComparison.OrdinalIgnoreCase) || r.Contains("админ", StringComparison.OrdinalIgnoreCase))
            return "admin";

        // manager
        if (r is "manager" or "менеджер" or "координатор")
            return "manager";
        if (r.Contains("manager", StringComparison.OrdinalIgnoreCase) || r.Contains("менедж", StringComparison.OrdinalIgnoreCase))
            return "manager";

        // teacher
        if (r is "teacher" or "преподаватель" or "учитель" or "тренер")
            return "teacher";
        if (r.Contains("teach", StringComparison.OrdinalIgnoreCase)
            || r.Contains("препод", StringComparison.OrdinalIgnoreCase)
            || r.Contains("учител", StringComparison.OrdinalIgnoreCase))
            return "teacher";

        return r;
    }
}

