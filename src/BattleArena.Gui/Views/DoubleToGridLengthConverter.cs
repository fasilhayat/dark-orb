namespace BattleArena.Gui.Views;

using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

internal sealed class DoubleToGridLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
            return new GridLength(Math.Max(0, d), GridUnitType.Star);
        return new GridLength(1, GridUnitType.Star);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}