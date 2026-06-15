# Task — Dispose AvaloniaSoundPlayer Resources

Project: Dark Orb

Priority: Medium

Type: Bug / Resource Leak

Status: Draft

---

## Objective

Fix resource leaks in `AvaloniaSoundPlayer` and `VisualEventBus` by properly disposing `IDisposable` resources.

---

## Current State

### 1. `AvaloniaSoundPlayer` — `SoundPlayer` instances never disposed

`AvaloniaSoundPlayer.cs:13` stores cached `SoundPlayer` objects (implements `IDisposable`) in a dictionary:

```csharp
private readonly Dictionary<string, SoundPlayer> _players = new();
```

`SoundPlayer` is created at line 40 (`new SoundPlayer(path)`) and loaded at line 41 (`player.Load()`), but **never disposed**. Over the lifetime of the application, each unique `.wav` file creates a `SoundPlayer` that holds unmanaged resources (file handles, audio buffers) indefinitely.

The class itself does not implement `IDisposable`, so even if the owner (`MainWindow`) is garbage collected, the `SoundPlayer` instances are not deterministically cleaned up.

### 2. `VisualEventBus` — `ManualResetEventSlim` never disposed

`VisualEventBus.cs:5`:

```csharp
private readonly ManualResetEventSlim _incredibleWait = new(false);
```

`ManualResetEventSlim` implements `IDisposable` (uses kernel-level wait handle). The event bus is never disposed, so the wait handle lives for the process lifetime.

### 3. Event handler subscriptions never unsubscribed

In `MainWindow.axaml.cs:57`:

```csharp
_vm.CombatLog.CollectionChanged += (_, _) => { /* scroll to bottom */ };
```

This lambda is never unsubscribed. Since the VM and Window share the same lifetime, this is **low risk** — but it prevents the VM from being GC'd independently during testing.

In `MainWindowViewModel.cs:801,803`:

```csharp
EffectBars.CollectionChanged += ...
EffectNames.CollectionChanged += ...
```

Never unsubscribed. Same low-risk scenario.

In `AvaloniaCombatPresenter.cs:458,542` — timer `Tick` lambdas captured in closures, stored in dictionaries. When `StopFlickerTimer` stops the timer, the lambda reference persists until the dictionary entry is removed.

---

## Proposed Solution

### 4a. Make `AvaloniaSoundPlayer : IDisposable`

```csharp
internal sealed class AvaloniaSoundPlayer : ISoundPlayer, IDisposable
{
    private readonly Dictionary<string, SoundPlayer> _players = new();

    public void Dispose()
    {
        foreach (var player in _players.Values)
            player?.Dispose();
        _players.Clear();
    }
}
```

### 4b. Make `ISoundPlayer : IDisposable` or add `Shutdown()` method

Choose one:
- **Option A**: Add `IDisposable` to `ISoundPlayer` — requires all implementations to dispose
- **Option B**: Add `void Shutdown()` to `ISoundPlayer` — explicit lifecycle method, less invasive

### 4c. Dispose `AvaloniaSoundPlayer` in `MainWindow`

In `MainWindow` constructor or `Closing` event:

```csharp
protected override void OnClosing(WindowClosingEventArgs e)
{
    (_soundPlayer as IDisposable)?.Dispose();
    base.OnClosing(e);
}
```

### 4d. Make `VisualEventBus : IDisposable`

```csharp
public sealed class VisualEventBus : IDisposable
{
    private readonly ManualResetEventSlim _incredibleWait = new(false);

    public void Dispose()
    {
        _incredibleWait.Dispose();
    }
}
```

### 4e. Unsubscribe event handlers

- Add unsubscribe for `CollectionChanged` in `MainWindow.axaml.cs:57` (or suppress with a comment noting shared-lifetime guarantee)
- Consider adding unsubscribe on `PropertyChanged` in world views

---

## Files to Modify

| File | Change |
|------|--------|
| `BattleArena.Gui/Presenters/AvaloniaSoundPlayer.cs` | Add `IDisposable`, dispose `_players` |
| `BattleArena.Application/Interfaces/ISoundPlayer.cs` | Add `IDisposable` or `Shutdown()` |
| `BattleArena.Gui/Views/MainWindow.axaml.cs` | Dispose `_soundPlayer` on close |
| `BattleArena.Presentation/VisualEventBus.cs` | Add `IDisposable`, dispose `_incredibleWait` |
| `BattleArena.Gui/Views/MainWindow.axaml.cs:57` | Add unsubscribe or comment documenting shared-lifetime |
| `BattleArena.Gui/ViewModels/MainWindowViewModel.cs:801,803` | Add unsubscribe or comment |

---

## Acceptance Criteria

- [ ] `AvaloniaSoundPlayer` disposes all cached `SoundPlayer` instances on shutdown
- [ ] `VisualEventBus` disposes its `ManualResetSettEventSlim`
- [ ] No change to combat simulation or playback behavior
- [ ] All 719 tests pass
- [ ] Sound playback continues to work identically during combat
