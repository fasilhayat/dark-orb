using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BattleArena.Gui.Views;

internal sealed class BoolToStepConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is true;
        return parameter?.ToString() switch
        {
            "Bg" => new SolidColorBrush(Color.Parse(isActive ? "#00bfff" : "#2a2a3e")),
            "Border" => new SolidColorBrush(Color.Parse(isActive ? "#00bfff" : "#555")),
            "Fg" => new SolidColorBrush(isActive ? Colors.White : Color.Parse("#888")),
            "Label" => new SolidColorBrush(Color.Parse(isActive ? "#00bfff" : "#666")),
            _ => Brushes.White,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
