namespace BattleArena.Gui.Views;

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

internal sealed class BoolToAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
