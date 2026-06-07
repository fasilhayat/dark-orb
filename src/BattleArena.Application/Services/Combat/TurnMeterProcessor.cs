namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;

/// <summary>
/// Handles turn meter calculations, updates, and mana regeneration.
/// </summary>
public class TurnMeterProcessor
{
    private readonly ITurnmeterService _turnmeterService;
    private readonly CombatLogger _logger;

    public TurnMeterProcessor(ITurnmeterService turnmeterService, CombatLogger logger)
    {
        _turnmeterService = turnmeterService;
        _logger = logger;
    }

    public async Task ProcessTickMeterAndManaAsync(
        int tick, List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        // Process turn meter gains for all living combatants
        foreach (var s in states.Where(s => s.Character.IsAlive))
        {
            s.SnapshotMeter();
            s.Meter = _turnmeterService.Tick(s.Character, s.Meter);
            if (s.Meter.CurrentValue > s.PrevMeter)
                await notify(_logger.BuildTurnMeterGainEntry(tick, s));
        }

        // Process mana regeneration
        foreach (var s in states.Where(s => s.Character.IsAlive))
        {
            var regenBase = s.Character.ManaRegenPerTick;
            var regenBonus = s.Character.ActiveStatusEffects
                .Where(e => e.ManaRegenModifier > 0)
                .Sum(e => e.ManaRegenModifier);
            var totalRegen = regenBase + regenBonus;
            
            if (totalRegen <= 0) continue;
            
            var manaBefore = s.Character.CurrentMana;
            s.Character.CurrentMana = Math.Min(s.Character.CurrentMana + totalRegen, s.Character.MaxMana);
            
            if (s.Character.CurrentMana > manaBefore)
                await notify(_logger.BuildManaRegenEntry(tick, s.Character.Name, manaBefore, s.Character.CurrentMana, totalRegen));
        }
    }

    public async Task ApplyDefenderTmBoostAsync(
        int tick, CombatantState actorState, Character target, AttackResult result,
        Dictionary<Character, CombatantState> stateMap, Func<CombatLogEntry, Task> notify)
    {
        if (!result.IsPerfectParry && !result.IsTotalReversal) return;
        
        var defenderState = stateMap[target];
        var tmBefore = defenderState.Meter.CurrentValue;
        defenderState.Meter.CurrentValue += result.DefenderTmBonus;
        
        var eventType = result.IsTotalReversal ? "TotalReversal" : "PerfectParry";
        var msg = result.IsTotalReversal
            ? $"[TOTAL REVERSAL] {target.Name} capitalises on {actorState.Character.Name}'s fumble! +{result.DefenderTmBonus} TM. ({tmBefore} -> {defenderState.Meter.CurrentValue})"
            : $"[PERFECT PARRY] {target.Name} deflects {actorState.Character.Name}'s attack! +{result.DefenderTmBonus} TM. ({tmBefore} -> {defenderState.Meter.CurrentValue})";
        
        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = target.Name,
            TargetName      = actorState.Character.Name,
            EventType       = eventType,
            TurnMeterBefore = tmBefore,
            TurnMeterAfter  = defenderState.Meter.CurrentValue,
            Message         = msg
        });
    }

    public CombatLogEntry BuildAfterTurnEntry(CombatantState state, int tick, int tmCost = TurnmeterState.TurnThreshold)
    {
        var before = state.Meter.CurrentValue;
        state.Meter = _turnmeterService.AfterTurn(state.Meter, tmCost);
        return _logger.BuildAfterTurnEntry(tick, state.Character.Name, before, state.Meter.CurrentValue, tmCost);
    }
}