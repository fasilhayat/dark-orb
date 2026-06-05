namespace BattleArena.Presentation;

public sealed class VisualEventBus
{
    private readonly ManualResetEventSlim _incredibleWait = new(false);

    public event Action<VisualEvent>? NormalEventPublished;
    public event Action<VisualEvent>? IncredibleEventPublished;

    public void PublishNormal(VisualEvent visualEvent)
    {
        NormalEventPublished?.Invoke(visualEvent);
    }

    public void PublishIncredible(VisualEvent visualEvent)
    {
        _incredibleWait.Reset();
        IncredibleEventPublished?.Invoke(visualEvent);
        _incredibleWait.Wait();
    }

    public void SignalIncredibleComplete()
    {
        _incredibleWait.Set();
    }
}
