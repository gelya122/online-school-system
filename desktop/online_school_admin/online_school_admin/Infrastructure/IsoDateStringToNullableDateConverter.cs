using System.Globalization;
using System.Windows.Data;

namespace online_school_admin.Infrastructure;

/// <summary>Связка полей VM (строка yyyy-MM-dd) с <see cref="System.Windows.Controls.DatePicker"/>.</summary>
public sealed class IsoDateStringToNullableDateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return null;
        if (!DateOnly.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return null;
        return d.ToDateTime(TimeOnly.MinValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "";
        if (value is not DateTime dt)
            return "";
        return DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
