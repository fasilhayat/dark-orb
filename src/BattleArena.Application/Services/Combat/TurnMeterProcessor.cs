namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Handles turn meter calculations, updates, and mana regeneration.
/// </summary>
internal class TurnMeterProcessor
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
        foreach (var s in states)
        {
            if (!s.Character.IsAlive) continue;

            s.SnapshotMeter();
            s.Meter = _turnmeterService.Tick(s.Character, s.Meter);
            await notify(_logger.BuildTurnMeterGainEntry(tick, s.Character.Name, s.PrevMeter, s.Meter.CurrentValue, s.Meter.IsReady, s.Meter.IsActive));

            if (s.Character.MaxMana > 0 && s.Character.CurrentMana < s.Character.EffectiveMaxMana)
            {
                var regen = s.Character.ManaRegenPerTick;

                var leech = s.Character.ActiveStatusEffects
                    .FirstOrDefault(e => e.Type == StatusEffectType.Leech
                        && e.LeechPerTurn > 0
                        && e.LeechResourceType == "Mana"
                        && !string.IsNullOrEmpty(e.CasterName));

                if (leech is not null)
                {
                    var redirectAmount = Math.Min(regen, leech.LeechPerTurn);
                    var leechCaster = states.FirstOrDefault(cs => cs.Character.Name == leech.CasterName);
                    if (leechCaster?.Character.IsAlive == true)
                    {
                        var casterManaBefore = leechCaster.Character.CurrentMana;
                        leechCaster.Character.CurrentMana = Math.Min(
                            leechCaster.Character.EffectiveMaxMana,
                            casterManaBefore + redirectAmount);

                        await notify(new CombatLogEntry
                        {
                            Tick = tick, ActorName = s.Character.Name,
                            EventType = "LeechTick", LeechAmount = redirectAmount,
                            LeechCasterName = leech.CasterName,
                            LeechResourceType = "Mana",
                            LeechTargetAfter = s.Character.CurrentMana,
                            LeechCasterAfter = leechCaster.Character.CurrentMana,
                            StatusEffectName = leech.Name,
                            Message = $"{s.Character.Name}'s mana regen ({regen}) redirected to {leech.CasterName} by {leech.Name}.  {leech.CasterName} gains {redirectAmount} mana."
                        });
                        continue;
                    }
                }

                var manaBefore = s.Character.CurrentMana;
                s.Character.CurrentMana = Math.Min(s.Character.EffectiveMaxMana, manaBefore + regen);
                await notify(new CombatLogEntry
                {
                    Tick = tick, ActorName = s.Character.Name,
                    EventType = "ManaRegen", ManaRegen = regen,
                    ManaAfter = s.Character.CurrentMana,
                    Message = $"{s.Character.Name} regens {regen} mana  ({manaBefore} -> {s.Character.CurrentMana})"
                });
            }

            if (s.QueuedSpell is not null)
                s.QueuedSpell.RemainingCost -= _turnmeterService.ComputeGainPerTick(s.Character);
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