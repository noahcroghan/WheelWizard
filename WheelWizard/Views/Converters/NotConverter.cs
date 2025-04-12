using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace WheelWizard.Views.Converters;

public class NotConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return AvaloniaProperty.UnsetValue; // Or return false/true based on desired default
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return AvaloniaProperty.UnsetValue;
    }
}
