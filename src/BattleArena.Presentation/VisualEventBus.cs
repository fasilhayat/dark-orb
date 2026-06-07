namespace BattleArena.Presentation;

public sealed class VisualEventBus
{
    private readonly ManualResetEventSlim _incredibleWait = new(false);

    public event Action<VisualEvent>? NormalEventPublished;
    public event Action<VisualEvent>? MajorEventPublished;
    public event Action<VisualEvent>? IncredibleEventPublished;
    public event Action<SoundEvent>? SoundRequested;

    public void PublishNormal(VisualEvent visualEvent)
    {
        NormalEventPublished?.Invoke(visualEvent);
    }

    public void PublishMajor(VisualEvent visualEvent)
    {
        MajorEventPublished?.Invoke(visualEvent);
    }

    public void PublishIncredible(VisualEvent visualEvent)
    {
        _incredibleWait.Reset();
        IncredibleEventPublished?.Invoke(visualEvent);
        _incredibleWait.Wait();
    }

    public void PublishSound(SoundEvent soundEvent)
    {
        SoundRequested?.Invoke(soundEvent);
    }

    public void SignalIncredibleComplete()
    {
        _incredibleWait.Set();
    }
}
