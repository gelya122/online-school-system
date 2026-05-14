using System.Globalization;
using System.Linq;
using OnlineSchoolAPI.Dto;

namespace OnlineSchoolAPI.Services;

/// <summary>
/// Журнал действий по заявке хранится в <see cref="Models.TrialApplication.ManagerComment"/> строками с префиксом #log.
/// Пользовательский текст — все остальные строки (см. <see cref="SplitAudit"/>).
/// </summary>
public static class ApplicationAuditLog
{
    public const string Prefix = "#log";
    private const char Sep = '\u001e';

    public static (string UserNotes, List<string> AuditLines) SplitAudit(string? field)
    {
        var audit = new List<string>();
        if (string.IsNullOrEmpty(field))
            return ("", audit);

        var user = new List<string>();
        foreach (var line in field.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (line.StartsWith(Prefix, StringComparison.Ordinal))
                audit.Add(line);
            else
                user.Add(line);
        }

        return (string.Join("\n", user).TrimEnd(), audit);
    }

    public static string Merge(string userNotes, IEnumerable<string> auditLines)
    {
        var a = auditLines.ToList();
        if (a.Count == 0)
            return userNotes.TrimEnd();
        var u = userNotes.TrimEnd();
        return string.IsNullOrEmpty(u) ? string.Join("\n", a) : u + "\n" + string.Join("\n", a);
    }

    public static string Append(string? existing, int? employeeId, string kind, string text)
    {
        var (u, lines) = SplitAudit(existing);
        lines.Add(BuildLine(employeeId, kind, text));
        return Merge(u, lines);
    }

    private static string BuildLine(int? employeeId, string kind, string text)
    {
        var emp = employeeId?.ToString(CultureInfo.InvariantCulture) ?? "";
        var stamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        var safe = text.Replace("\r", "").Replace("\n", " ").Trim();
        return string.Concat(Prefix, Sep, stamp, Sep, emp, Sep, kind, Sep, safe);
    }

    public static IReadOnlyList<AdminApplicationCommentDto> ParseComments(string? field)
    {
        var (_, audit) = SplitAudit(field);
        var list = new List<AdminApplicationCommentDto>();
        var i = 1;
        foreach (var line in audit)
        {
            var dto = TryParseLine(line, i++);
            if (dto != null)
                list.Add(dto);
        }

        return list;
    }

    public static IReadOnlyList<AdminApplicationStatusHistoryRowDto> ParseStatusHistory(string? field)
    {
        var (_, audit) = SplitAudit(field);
        var list = new List<AdminApplicationStatusHistoryRowDto>();
        foreach (var line in audit)
        {
            if (!TryParseParts(line, out var at, out _, out var kind, out var payload))
                continue;
            if (!string.Equals(kind, "status", StringComparison.OrdinalIgnoreCase))
                continue;
            list.Add(new AdminApplicationStatusHistoryRowDto
            {
                CreatedAt = at,
                StatusName = payload,
                EmployeeName = null
            });
        }

        return list.OrderByDescending(x => x.CreatedAt).Take(50).ToList();
    }

    private static AdminApplicationCommentDto? TryParseLine(string line, int syntheticId)
    {
        if (!TryParseParts(line, out var at, out var emp, out var kind, out var payload))
            return null;
        if (string.Equals(kind, "status", StringComparison.OrdinalIgnoreCase))
            return null;
        if (string.Equals(kind, "convert", StringComparison.OrdinalIgnoreCase))
            return null;

        return new AdminApplicationCommentDto
        {
            CommentId = syntheticId,
            CommentText = string.IsNullOrEmpty(kind) ? payload : $"[{kind}] {payload}",
            EmployeeId = emp,
            EmployeeName = null,
            CreatedAt = at ?? DateTime.MinValue
        };
    }

    /// <summary>Из журнала после конвертации в студента (kind=convert, payload — числовой student_id).</summary>
    public static int? TryParseConvertedStudentId(string? field)
    {
        var (_, audit) = SplitAudit(field);
        for (var i = audit.Count - 1; i >= 0; i--)
        {
            if (!TryParseParts(audit[i], out _, out _, out var kind, out var payload))
                continue;
            if (!string.Equals(kind, "convert", StringComparison.OrdinalIgnoreCase))
                continue;
            if (int.TryParse(payload.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sid))
                return sid;
        }

        return null;
    }

    private static bool TryParseParts(string line, out DateTime? at, out int? empId, out string kind, out string payload)
    {
        at = null;
        empId = null;
        kind = "";
        payload = "";
        if (!line.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        var rest = line.Length > Prefix.Length ? line.AsSpan(Prefix.Length).ToString() : "";
        if (rest.Length == 0 || rest[0] != Sep)
            return false;

        var parts = rest.Split(Sep, StringSplitOptions.None);
        if (parts.Length < 5)
            return false;

        if (DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            at = dt;
        if (int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var eid))
            empId = eid;
        kind = parts[3];
        payload = parts.Length > 4 ? string.Join(Sep, parts.Skip(4)) : "";
        return true;
    }
}
