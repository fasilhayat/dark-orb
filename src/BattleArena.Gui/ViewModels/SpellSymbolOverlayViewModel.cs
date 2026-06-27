namespace BattleArena.Gui.ViewModels;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public sealed class SpellSymbolOverlayViewModel : INotifyPropertyChanged
{
    public string Symbol { get; }
    public string Color { get; }
    public string FontFamily { get; }

    private double _opacity = 1.0;
    public double Opacity
    {
        get => _opacity;
        set => SetField(ref _opacity, value);
    }

    private double _scale = 0.3;
    public double Scale
    {
        get => _scale;
        set => SetField(ref _scale, value);
    }

    private const int DurationMs = 1200;
    private const int IntervalMs = 30;

    public SpellSymbolOverlayViewModel(string symbol, string color, string fontFamily)
    {
        Symbol = symbol;
        Color = color;
        FontFamily = fontFamily;
    }

    public void Animate(Action<SpellSymbolOverlayViewModel> onCompleted, CancellationToken ct)
    {
        var steps = DurationMs / IntervalMs;

        Task.Run(async () =>
        {
            for (var i = 0; i < steps; i++)
            {
                await Task.Delay(IntervalMs, ct);
                if (ct.IsCancellationRequested) return;
                var t = (double)(i * IntervalMs) / DurationMs;
                var eased = 1.0 - (1.0 - t) * (1.0 - t);
                Scale = 0.3 + eased * 2.2;
                Opacity = Math.Max(0, 1.0 - t * 1.05);
            }
            if (!ct.IsCancellationRequested)
                onCompleted(this);
        }, ct);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
