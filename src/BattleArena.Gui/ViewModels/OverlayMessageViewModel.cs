using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BattleArena.Gui.ViewModels;

public sealed class OverlayMessageViewModel : INotifyPropertyChanged
{
    private static readonly Random Rng = new();

    public string Text { get; }
    public double OffsetX { get; }
    public double OffsetY { get; }
    public string Color { get; }

    private double _opacity;
    public double Opacity
    {
        get => _opacity;
        set => SetField(ref _opacity, value);
    }

    private double _scale;
    public double Scale
    {
        get => _scale;
        set => SetField(ref _scale, value);
    }

    private double _trailOpacity1;
    public double TrailOpacity1
    {
        get => _trailOpacity1;
        set => SetField(ref _trailOpacity1, value);
    }

    private double _trailScale1;
    public double TrailScale1
    {
        get => _trailScale1;
        set => SetField(ref _trailScale1, value);
    }

    private double _trailOpacity2;
    public double TrailOpacity2
    {
        get => _trailOpacity2;
        set => SetField(ref _trailOpacity2, value);
    }

    private double _trailScale2;
    public double TrailScale2
    {
        get => _trailScale2;
        set => SetField(ref _trailScale2, value);
    }

    private readonly int _durationMs;
    private readonly double _endScale;
    private readonly double _startScale;

    public OverlayMessageViewModel(string text, string color)
    {
        Text = text;
        Color = color;

        lock (Rng)
        {
            OffsetX = Rng.Next(-200, 201);
            OffsetY = Rng.Next(-120, 121);
            _durationMs = Rng.Next(1400, 2400);
            _endScale = Rng.NextDouble() * 0.8 + 1.2;
            _startScale = Rng.NextDouble() * 0.3 + 0.6;
        }

        Opacity = 1.0;
        Scale = _startScale;
        TrailOpacity1 = 0.35;
        TrailScale1 = _startScale * 0.8;
        TrailOpacity2 = 0.15;
        TrailScale2 = _startScale * 0.56;
    }

    public void Animate(Action<OverlayMessageViewModel> onCompleted, CancellationToken ct)
    {
        const int intervalMs = 30;
        var steps = _durationMs / intervalMs;

        Task.Run(async () =>
        {
            for (var i = 0; i < steps; i++)
            {
                await Task.Delay(intervalMs, ct);
                if (ct.IsCancellationRequested) return;
                var t = (double)(i * intervalMs) / _durationMs;
                var eased = 1.0 - (1.0 - t) * (1.0 - t);
                var mainScale = _startScale + eased * (_endScale - _startScale);
                var mainOpacity = Math.Max(0, 1.0 - (eased - 0.10) / 0.90);
                Scale = mainScale;
                Opacity = mainOpacity;
                TrailOpacity1 = mainOpacity * 0.35;
                TrailScale1 = mainScale * 0.78;
                TrailOpacity2 = mainOpacity * 0.15;
                TrailScale2 = mainScale * 0.56;
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
