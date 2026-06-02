using System;
using System.Collections.Generic;
using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Services;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.Gui;

/// <summary>
/// Wraps <see cref="DiceService"/> and records every meaningful roll as a
/// <c>CombatLogEntry</c> (EventType = "ApiCall") so the GUI combat log can
/// show inline dice results — mirroring the Demo's <c>ApiDiceService</c>
/// pattern but without HTTP calls.
/// </summary>
internal sealed class LoggingDiceService : IDiceService
{
    private readonly DiceService _inner = new();

    public int Seed => _inner.Seed;

    public int CurrentTick
    {
        get => _inner.CurrentTick;
        set => _inner.CurrentTick = value;
    }

    public List<CombatLogEntry> DiceLog { get; } = new();

    private void Log(string label, int result)
    {
        DiceLog.Add(new CombatLogEntry
        {
            Tick      = CurrentTick,
            EventType = "ApiCall",
            ActorName = "dice",
            Message   = $"{label} → {result}"
        });
    }

    public int Roll(DieType dieType)
    {
        var result = _inner.Roll(dieType);
        Log(dieType.ToString().ToLower(), result);
        return result;
    }

    public int Roll(int count, int sides)
    {
        var result = _inner.Roll(count, sides);
        Log($"{count}d{sides}", result);
        return result;
    }

    public int RollWithAdvantage(DieType dieType)
    {
        var result = _inner.RollWithAdvantage(dieType);
        Log($"{dieType.ToString().ToLower()} (adv)", result);
        return result;
    }

    public int RollWithDisadvantage(DieType dieType)
    {
        var result = _inner.RollWithDisadvantage(dieType);
        Log($"{dieType.ToString().ToLower()} (dis)", result);
        return result;
    }

    public int RollIndex(int maxExclusive) => _inner.RollIndex(maxExclusive);
}
