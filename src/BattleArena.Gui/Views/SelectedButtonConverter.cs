using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BattleArena.Gui.Views;

internal sealed class SelectedButtonConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return new SolidColorBrush(Color.Parse("#1a1a2e"));
        var item = values[0]?.ToString();
        var selected = values[1]?.ToString();

        if (string.Equals(item, selected, StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.Parse("#00bfff"));

        var defaultColor = parameter?.ToString();
        return new SolidColorBrush(Color.Parse(string.IsNullOrEmpty(defaultColor) ? "#1a1a2e" : defaultColor));
    }
}
