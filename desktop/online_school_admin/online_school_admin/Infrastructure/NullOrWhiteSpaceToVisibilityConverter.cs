using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace online_school_admin.Infrastructure;

/// <summary>Пустая строка → Visible, иначе Collapsed.</summary>
public sealed class NullOrWhiteSpaceToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value as string;
        return string.IsNullOrWhiteSpace(s) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

