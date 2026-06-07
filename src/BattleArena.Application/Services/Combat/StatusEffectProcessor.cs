namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Handles status effect application, ticking, and expiration.
/// </summary>
internal class StatusEffectProcessor
{
    private readonly IStatusEffectService _statusEffectService;
    private readonly IDiceService _dice;
    private readonly CombatLogger _logger;

    public StatusEffectProcessor(IStatusEffectService statusEffectService, IDiceService dice, CombatLogger logger)
    {
        _statusEffectService = statusEffectService;
        _dice = dice;
        _logger = logger;
    }

    public async Task ProcessActorLeechAsync(
        int tick, CombatantState actorState, CombatantState casterState, 
        StatusEffect effect, Func<CombatLogEntry, Task> notify)
    {
        if (effect.LeechPerTurn <= 0) return;
        
        var casterName = casterState.Character.Name;
        var resourceType = effect.LeechResourceType ?? "HP";
        var leechAmount = effect.LeechPerTurn;

        if (resourceType == "HP")
        {
            var targetHpBefore = actorState.Character.CurrentHitPoints;
            actorState.Character.CurrentHitPoints -= leechAmount;

            var casterHpBefore = casterState.Character.CurrentHitPoints;
            casterState.Character.CurrentHitPoints = Math.Min(
                casterState.Character.MaxHitPoints,
                casterHpBefore + leechAmount);

            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = actorState.Character.Name,
                EventType          = "LeechTick",
                LeechAmount        = leechAmount,
                LeechCasterName    = casterName,
                LeechResourceType  = "HP",
                LeechTargetAfter   = actorState.Character.CurrentHitPoints,
                LeechCasterAfter   = casterState.Character.CurrentHitPoints,
                StatusEffectName   = effect.Name,
                Message            = $"{actorState.Character.Name} loses {leechAmount} HP to {casterName}'s {effect.Name}.  {casterName} gains {leechAmount} HP."
            });
        }
        else if (resourceType == "Mana")
        {
            var targetManaBefore = actorState.Character.CurrentMana;
            actorState.Character.CurrentMana = Math.Max(0, targetManaBefore - leechAmount);
            var actualDrain = targetManaBefore - actorState.Character.CurrentMana;

            var casterManaBefore = casterState.Character.CurrentMana;
            casterState.Character.CurrentMana = Math.Min(
                casterState.Character.MaxMana,
                casterManaBefore + actualDrain);

            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = actorState.Character.Name,
                EventType          = "LeechTick",
                LeechAmount        = actualDrain,
                LeechCasterName    = casterName,
                LeechResourceType  = "Mana",
                LeechTargetAfter   = actorState.Character.CurrentMana,
                LeechCasterAfter   = casterState.Character.CurrentMana,
                StatusEffectName   = effect.Name,
                Message            = $"{actorState.Character.Name} loses {actualDrain} mana to {casterName}'s {effect.Name}.  {casterName} gains {actualDrain} mana."
            });
        }
    }

    public async Task<bool> ProcessActorDoTAsync(
        int tick, CombatantState actorState, StatusEffect effect, 
        Func<CombatLogEntry, Task> notify)
    {
        if (effect.DamagePerTurn <= 0) return false;
        
        var hpBefore = actorState.Character.CurrentHitPoints;
        actorState.Character.CurrentHitPoints -= effect.DamagePerTurn;
        
        await notify(new CombatLogEntry
        {
            Tick         = tick,
            ActorName    = actorState.Character.Name,
            EventType    = "DoTDamage",
            DamageDealt  = effect.DamagePerTurn,
            TargetHpBefore = hpBefore,
            TargetHpAfter = actorState.Character.CurrentHitPoints,
            StatusEffectName = effect.Name,
            Message      = $"{effect.Name}: {actorState.Character.Name} takes {effect.DamagePerTurn} damage. " +
                          $"HP: {hpBefore} → {actorState.Character.CurrentHitPoints}"
        });
        
        if (actorState.Character.CurrentHitPoints <= 0)
        {
            await notify(_logger.BuildDefeatEntry(tick, actorState.Character));
            return true; // Character defeated
        }
        
        return false;
    }

    public async Task ProcessActorHoTAsync(
        int tick, CombatantState actorState, StatusEffect effect, 
        Func<CombatLogEntry, Task> notify)
    {
        if (effect.HealingPerTurn <= 0) return;
        
        var hpBefore = actorState.Character.CurrentHitPoints;
        var actualHeal = Math.Min(effect.HealingPerTurn, 
            actorState.Character.MaxHitPoints - actorState.Character.CurrentHitPoints);
        actorState.Character.CurrentHitPoints += actualHeal;
        
        await notify(new CombatLogEntry
        {
            Tick         = tick,
            ActorName    = actorState.Character.Name,
            EventType    = "HoTHealing",
            TargetHpBefore = hpBefore,
            TargetHpAfter = actorState.Character.CurrentHitPoints,
            StatusEffectName = effect.Name,
            Message      = $"{effect.Name}: {actorState.Character.Name} heals {actualHeal}. " +
                          $"HP: {hpBefore} → {actorState.Character.CurrentHitPoints}"
        });
    }

    public async Task ProcessSelfBuffsAsync(
        int tick, Character caster, Spell spell, Func<CombatLogEntry, Task> notify)
    {
        foreach (var template in spell.OnHitEffects)
        {
            if (template.Target != EffectTarget.Caster) continue;
            
            var effect = new StatusEffect
            {
                Name                 = template.Name,
                Type                 = template.Type,
                Duration             = template.Duration,
                StackRule            = template.StackRule,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                MovementModifier     = template.MovementModifier,
                ManaRegenModifier    = template.ManaRegenModifier,
                Source               = spell.Name,
                ResistanceType       = template.ResistanceType,
                ApplicationChance    = template.ApplicationChance
            };
            
            _statusEffectService.Apply(caster, effect);
            
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = caster.Name,
                EventType        = "EffectApplied",
                StatusEffectName = effect.Name,
                AttackSourceName = spell.Name,
                Message          = $"{caster.Name} gains {effect.Name} from {spell.Name}!"
            });
        }
    }

    public async Task TryApplyEffectAsync(
        int tick, StatusEffect effect, Character target, string sourceName,
        Func<CombatLogEntry, Task> notify)
    {
        var resistance = target.ComputeResistance(effect.ResistanceType);
        var result = _statusEffectService.TryApply(target, effect, resistance, _dice);
        
        if (result.Applied)
        
        if (result.Applied)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "EffectApplied",
                StatusEffectName = effect.Name,
                AttackSourceName = sourceName,
                Message          = $"{target.Name} is affected by {effect.Name}!"
            });
        }
        else if (result.WasResisted)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "EffectResisted",
                StatusEffectName = effect.Name,
                AttackSourceName = sourceName,
                Message          = $"{target.Name} resists {effect.Name}!"
            });
        }
    }

    public async Task ProcessOnHitEffectsAsync(
        int tick, IAttackSource source, Character target, string sourceName,
        Func<CombatLogEntry, Task> notify)
    {
        if (source is not Spell spell) return;
        
        foreach (var template in spell.OnHitEffects)
        {
            if (template.Target != EffectTarget.Target) continue;
            
            var effect = new StatusEffect
            {
                Name                 = template.Name,
                Type                 = template.Type,
                Duration             = template.Duration,
                StackRule            = template.StackRule,
                DamagePerTurn        = template.DamagePerTurn,
                HealingPerTurn       = template.HealingPerTurn,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                MovementModifier     = template.MovementModifier,
                LeechPerTurn         = template.LeechPerTurn,
                LeechResourceType    = template.LeechResourceType,
                Source               = sourceName,
                ResistanceType       = template.ResistanceType,
                ApplicationChance    = template.ApplicationChance
            };
            
            await TryApplyEffectAsync(tick, effect, target, sourceName, notify);
        }
    }

    public async Task TryApplyElementalDoTAsync(
        int tick, Character target, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.ElementalType == ElementalType.None) return;
        
        var effect = CreateElementalDoT(spell.ElementalType, spell.Name);
        if (effect != null)
            await TryApplyEffectAsync(tick, effect, target, spell.Name, notify);
    }

    public async Task ExpireSummonedPetsAsync(
        int tick, int currentRound, List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        var expiredPets = states.Where(s => s.IsSummoned && s.SummonExpiryRound == currentRound).ToList();
        
        foreach (var pet in expiredPets)
        {
            pet.Character.CurrentHitPoints = 0;
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = pet.Character.Name,
                EventType = "SummonExpired",
                Message   = $"{pet.Character.Name} vanishes as the summoning expires."
            });
        }
    }

    public async Task ApplyFumblePenaltyAsync(
        int tick, CombatantState actorState, AttackResult result,
        Func<CombatLogEntry, Task> notify)
    {
        if (!result.IsFumble) return;
        
        var penaltyName = result.IsTotalReversal ? "Total Reversal Penalty" : "Fumble Penalty";
        _statusEffectService.Apply(actorState.Character, new StatusEffect
        {
            Name                = penaltyName,
            Type                = StatusEffectType.Debuff,
            AttackPowerModifier = result.AttackPowerPenalty,
            Duration            = 1,
            StackRule           = StackRule.NoStack,
            Source              = "Fumble"
        });
        
        if (result.IsTotalReversal) return;
        
        await notify(new CombatLogEntry
        {
            Tick      = tick,
            ActorName = actorState.Character.Name,
            EventType = "FumblePenalty",
            Message   = $"[FUMBLE] {actorState.Character.Name} fumbles! -2 AttackPower applied for next turn."
        });
    }

    private static StatusEffect? CreateElementalDoT(ElementalType type, string sourceName) =>
        type switch
        {
            ElementalType.Fire => new StatusEffect
            {
                Name = "Burning",
                Type = StatusEffectType.DamageOverTime,
                DamagePerTurn = 5,
                Duration = 3,
                Source = sourceName,
                ResistanceType = ResistanceType.Fire,
                ApplicationChance = 30
            },
            ElementalType.Ice => new StatusEffect
            {
                Name = "Freezing",
                Type = StatusEffectType.Debuff,
                MovementModifier = -10,
                Duration = 2,
                Source = sourceName,
                ResistanceType = ResistanceType.Cold,
                ApplicationChance = 25
            },
            ElementalType.Lightning => new StatusEffect
            {
                Name = "Shocked",
                Type = StatusEffectType.Shock,
                Duration = 1,
                Source = sourceName,
                ResistanceType = ResistanceType.Lightning,
                ApplicationChance = 20
            },
            ElementalType.Poison => new StatusEffect
            {
                Name = "Poisoned",
                Type = StatusEffectType.DamageOverTime,
                DamagePerTurn = 3,
                Duration = 5,
                Source = sourceName,
                ResistanceType = ResistanceType.Poison,
                ApplicationChance = 40
            },
            _ => null
        };
}