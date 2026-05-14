using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace online_school_admin.Infrastructure;

public static class CsvExporter
{
    /// <summary>
    /// Экспорт коллекции в CSV (UTF-8 с BOM — Excel в Windows открывает кириллицу корректно).
    /// </summary>
    public static bool PromptSaveAndExport<T>(IEnumerable<T> rows, string suggestedFileName, params string[] propertyNames)
    {
        var dlg = new SaveFileDialog
        {
            Title = "Экспорт таблицы",
            Filter = "CSV (*.csv)|*.csv|Все файлы|*.*",
            FileName = suggestedFileName,
            AddExtension = true,
            DefaultExt = ".csv"
        };
        if (dlg.ShowDialog() != true)
            return false;

        File.WriteAllText(dlg.FileName, BuildCsv(rows, propertyNames), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return true;
    }

    public static string BuildCsv<T>(IEnumerable<T> rows, params string[] propertyNames)
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && propertyNames.Contains(p.Name, StringComparer.Ordinal))
            .OrderBy(p => Array.IndexOf(propertyNames, p.Name))
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", propertyNames.Select(Escape)));

        foreach (var row in rows)
        {
            var cells = props.Select(p => Escape(ToCsvCell(p.GetValue(row))));
            sb.AppendLine(string.Join(";", cells));
        }

        return sb.ToString();
    }

    private static string ToCsvCell(object? value)
    {
        if (value == null) return "";
        if (value is IFormattable f)
            return f.ToString(null, CultureInfo.InvariantCulture) ?? "";
        return value.ToString() ?? "";
    }

    private static string Escape(string? s)
    {
        s ??= "";
        var needsQuote = s.Contains(';') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        s = s.Replace("\"", "\"\"", StringComparison.Ordinal);
        return needsQuote ? $"\"{s}\"" : s;
    }
}
