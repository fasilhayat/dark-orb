using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BattleArena.Gui.Views;

internal sealed class DoubleToWidthConverter : IValueConverter
{
    public static readonly DoubleToWidthConverter Instance = new();

    private const double MaxWidth = 200;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double fraction)
            return Math.Max(2, fraction * MaxWidth);
        return 2.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
