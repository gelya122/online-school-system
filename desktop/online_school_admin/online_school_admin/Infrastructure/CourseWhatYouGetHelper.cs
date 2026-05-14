using System.Text.Json;

namespace online_school_admin.Infrastructure;

public static class CourseWhatYouGetHelper
{
    public static void Parse(string? raw, out List<string> benefits, out List<string> outcomes)
    {
        benefits = [];
        outcomes = [];
        if (string.IsNullOrWhiteSpace(raw))
            return;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.TryGetProperty("benefits", out var b) && b.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in b.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                        benefits.Add(item.GetString() ?? "");
                }
            }

            if (root.TryGetProperty("outcomes", out var o) && o.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in o.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                        outcomes.Add(item.GetString() ?? "");
                }
            }

            if (benefits.Count > 0 || outcomes.Count > 0)
                return;
        }
        catch (JsonException)
        {
            // legacy plain text
        }

        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            benefits.Add(line);
    }

    public static string Serialize(IReadOnlyList<string> benefits, IReadOnlyList<string> outcomes)
    {
        return JsonSerializer.Serialize(new { v = 1, benefits, outcomes });
    }
}
