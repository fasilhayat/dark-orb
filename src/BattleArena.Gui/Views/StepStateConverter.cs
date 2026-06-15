namespace BattleArena.Gui.Views;

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

internal sealed class StepStateConverter : IValueConverter
{
    public string Mode { get; set; } = "Bg";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int creationStep || parameter is not string stepStr) return Brushes.White;
        if (!int.TryParse(stepStr, out var stepIndex)) return Brushes.White;

        if (stepIndex < creationStep)
            return Mode switch
            {
                "Bg" => new SolidColorBrush(Color.Parse("#1a6b3c")),
                "Border" => new SolidColorBrush(Color.Parse("#2a9a5a")),
                "Fg" => new SolidColorBrush(Colors.White),
                "Label" => new SolidColorBrush(Color.Parse("#2a9a5a")),
                "Text" => "✓",
                _ => Brushes.White
            };

        if (stepIndex == creationStep)
            return Mode switch
            {
                "Bg" => new SolidColorBrush(Color.Parse("#00bfff")),
                "Border" => new SolidColorBrush(Color.Parse("#00dfff")),
                "Fg" => new SolidColorBrush(Colors.White),
                "Label" => new SolidColorBrush(Color.Parse("#00bfff")),
                "Text" => (stepIndex + 1).ToString(),
                _ => Brushes.White
            };

        return Mode switch
        {
            "Bg" => new SolidColorBrush(Color.Parse("#1a1a2e")),
            "Border" => new SolidColorBrush(Color.Parse("#444")),
            "Fg" => new SolidColorBrush(Color.Parse("#555")),
            "Label" => new SolidColorBrush(Color.Parse("#444")),
            "Text" => (stepIndex + 1).ToString(),
            _ => Brushes.White
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}