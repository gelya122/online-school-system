using System.Globalization;
using System.Windows.Data;

namespace online_school_admin.Infrastructure;

public sealed class AssignmentKindToRuConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var k = (value as string ?? "").Trim().ToLowerInvariant();
        return k switch
        {
            "teacher" => "Преподаватель",
            "coordinator" => "Координатор",
            _ => value ?? ""
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
