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
        int tick, CombatantState actorState,
        List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var leechEffect in actorState.Character.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.Leech && e.LeechPerTurn > 0)
            .OrderBy(e => e.ResolutionPriority)
            .ToList())
        {
            var casterName = leechEffect.CasterName;
            var casterState = states.FirstOrDefault(s => s.Character.Name == casterName);
            if (casterState is null || !casterState.Character.IsAlive) continue;

            var resourceType = leechEffect.LeechResourceType ?? "HP";
            var leechAmount = leechEffect.LeechPerTurn;

            if (resourceType == "HP")
            {
                var targetBefore = actorState.Character.CurrentHitPoints;
                actorState.Character.CurrentHitPoints -= leechAmount;

                var casterBefore = casterState.Character.CurrentHitPoints;
                casterState.Character.CurrentHitPoints = Math.Min(
                    casterState.Character.MaxHitPoints,
                    casterBefore + leechAmount);

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
                    StatusEffectName   = leechEffect.Name,
                    EffectDuration     = leechEffect.Duration,
                    EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == leechEffect.Name),
                    Message            = $"{actorState.Character.Name} loses {leechAmount} HP to {casterName}'s {leechEffect.Name}.  {casterName} gains {leechAmount} HP."
                });
            }
            else if (resourceType == "Mana")
            {
                var targetBefore = actorState.Character.CurrentMana;
                actorState.Character.CurrentMana = Math.Max(0, targetBefore - leechAmount);
                var actualDrain = targetBefore - actorState.Character.CurrentMana;

                var casterBefore = casterState.Character.CurrentMana;
                casterState.Character.CurrentMana = Math.Min(
                    casterState.Character.MaxMana,
                    casterBefore + actualDrain);

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
                    StatusEffectName   = leechEffect.Name,
                    EffectDuration     = leechEffect.Duration,
                    EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == leechEffect.Name),
                    Message            = $"{actorState.Character.Name} loses {actualDrain} mana to {casterName}'s {leechEffect.Name}.  {casterName} gains {actualDrain} mana."
                });
            }
        }
    }

    // Per-effect overload (for refactored callers that loop individually).
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
                EffectDuration     = effect.Duration,
                EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == effect.Name),
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
                EffectDuration     = effect.Duration,
                EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == effect.Name),
                Message            = $"{actorState.Character.Name} loses {actualDrain} mana to {casterName}'s {effect.Name}.  {casterName} gains {actualDrain} mana."
            });
        }
    }

    public async Task<bool> ProcessActorDoTAsync(
        int tick, CombatantState actorState,
        Func<CombatLogEntry, Task> notify)
    {
        var defeated = false;
        foreach (var dotEffect in actorState.Character.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.DamageOverTime && e.DamagePerTurn > 0)
            .OrderBy(e => e.ResolutionPriority)
            .ToList())
        {
            var dotDmg = dotEffect.DamagePerTurn;
            actorState.Character.CurrentHitPoints -= dotDmg;

            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = actorState.Character.Name,
                EventType          = "DoTTick",
                DamageDealt        = dotDmg,
                TargetHpAfter      = actorState.Character.CurrentHitPoints,
                StatusEffectName   = dotEffect.Name,
                EffectDuration     = dotEffect.Duration,
                EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == dotEffect.Name),
                Message            = $"{actorState.Character.Name} suffers {dotDmg} {dotEffect.Name} damage."
            });

            if (!defeated && actorState.Character.CurrentHitPoints <= 0)
            {
                await notify(_logger.BuildDefeatEntry(tick, actorState.Character));
                defeated = true;
            }

            if (defeated) break;
        }
        return defeated;
    }

    // Per-effect overload (for refactored callers that loop individually).
    public async Task<bool> ProcessActorDoTAsync(
        int tick, CombatantState actorState, StatusEffect effect,
        Func<CombatLogEntry, Task> notify)
    {
        if (effect.DamagePerTurn <= 0) return false;

        actorState.Character.CurrentHitPoints -= effect.DamagePerTurn;

        await notify(new CombatLogEntry
        {
            Tick               = tick,
            ActorName          = actorState.Character.Name,
            EventType          = "DoTTick",
            DamageDealt        = effect.DamagePerTurn,
            TargetHpAfter      = actorState.Character.CurrentHitPoints,
            StatusEffectName   = effect.Name,
            EffectDuration     = effect.Duration,
            EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == effect.Name),
            Message            = $"{actorState.Character.Name} suffers {effect.DamagePerTurn} {effect.Name} damage."
        });

        if (actorState.Character.CurrentHitPoints <= 0)
        {
            await notify(_logger.BuildDefeatEntry(tick, actorState.Character));
            return true;
        }

        return false;
    }

    public async Task ProcessActorHoTAsync(
        int tick, CombatantState actorState,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var hotEffect in actorState.Character.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.HealOverTime && e.HealingPerTurn > 0)
            .OrderBy(e => e.ResolutionPriority)
            .ToList())
        {
            var hotName = hotEffect.Name;
            var hotHeal = hotEffect.HealingPerTurn;
            var hpBefore = actorState.Character.CurrentHitPoints;
            actorState.Character.CurrentHitPoints = Math.Min(
                actorState.Character.MaxHitPoints,
                hpBefore + hotHeal);

            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = actorState.Character.Name,
                EventType          = "HoTTick",
                DamageDealt        = hotHeal,
                TargetHpBefore     = hpBefore,
                TargetHpAfter      = actorState.Character.CurrentHitPoints,
                StatusEffectName   = hotName,
                EffectDuration     = hotEffect.Duration,
                EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == hotEffect.Name),
                Message            = $"{actorState.Character.Name} recovers {hotHeal} HP from {hotName}.  HP: {hpBefore} -> {actorState.Character.CurrentHitPoints}"
            });
        }
    }

    // Per-effect overload (for refactored callers that loop individually).
    public async Task ProcessActorHoTAsync(
        int tick, CombatantState actorState, StatusEffect effect,
        Func<CombatLogEntry, Task> notify)
    {
        if (effect.HealingPerTurn <= 0) return;
        var hotName = effect.Name;
        var hotHeal = effect.HealingPerTurn;
        var hpBefore = actorState.Character.CurrentHitPoints;
        actorState.Character.CurrentHitPoints = Math.Min(
            actorState.Character.MaxHitPoints,
            hpBefore + hotHeal);

        await notify(new CombatLogEntry
        {
            Tick               = tick,
            ActorName          = actorState.Character.Name,
            EventType          = "HoTTick",
            DamageDealt        = hotHeal,
            TargetHpBefore     = hpBefore,
            TargetHpAfter      = actorState.Character.CurrentHitPoints,
            StatusEffectName   = hotName,
            EffectDuration     = effect.Duration,
            EffectStacks       = actorState.Character.ActiveStatusEffects.Count(e => e.Name == effect.Name),
            Message            = $"{actorState.Character.Name} recovers {hotHeal} HP from {hotName}.  HP: {hpBefore} -> {actorState.Character.CurrentHitPoints}"
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
                Target               = template.Target,
                ResistanceType       = template.ResistanceType,
                ResistanceBonuses    = template.ResistanceBonuses,
                Duration             = template.Duration,
                DamagePerTurn        = template.DamagePerTurn,
                HealingPerTurn       = template.HealingPerTurn,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                TurnMeterModifier    = template.TurnMeterModifier,
                ManaRegenModifier    = template.ManaRegenModifier,
                StackRule            = template.StackRule,
                ApplicationChance    = template.ApplicationChance,
                Source               = spell.Name,
                LeechPerTurn         = template.LeechPerTurn,
                LeechResourceType    = template.LeechResourceType ?? "HP",
                CasterName           = template.Type == StatusEffectType.Leech ? caster.Name : string.Empty
            };

            _statusEffectService.Apply(caster, effect);
            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = caster.Name,
                EventType          = "EffectApplied",
                StatusEffectName   = effect.Name,
                AttackSourceName   = spell.Name,
                IsBuff             = true,
                EffectDuration     = effect.Duration,
                EffectMaxDuration  = effect.Duration,
                EffectStacks       = caster.ActiveStatusEffects.Count(e => e.Name == effect.Name),
                Message            = $"{caster.Name} gains {effect.Name} from {spell.Name}!"
            });
        }
    }

    public async Task ProcessPartyBuffsAsync(
        int tick, Character caster, Spell spell, List<Character> partyMembers,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var template in spell.OnHitEffects)
        {
            if (template.Target != EffectTarget.Party) continue;

            var targets = template.Type == StatusEffectType.Debuff
                ? new List<Character> { caster }
                : partyMembers.Where(m => m.IsAlive).ToList();

            foreach (var target in targets)
            {
                var effect = new StatusEffect
                {
                    Name                 = template.Name,
                    Type                 = template.Type,
                    Target               = template.Target,
                    ResistanceType       = template.ResistanceType,
                    ResistanceBonuses    = template.ResistanceBonuses,
                    Duration             = template.Duration,
                    DamagePerTurn        = template.DamagePerTurn,
                    HealingPerTurn       = template.HealingPerTurn,
                    AttackPowerModifier  = template.AttackPowerModifier,
                    DefensePowerModifier = template.DefensePowerModifier,
                    TurnMeterModifier    = template.TurnMeterModifier,
                    ManaRegenModifier    = template.ManaRegenModifier,
                    StackRule            = template.StackRule,
                    ApplicationChance    = template.ApplicationChance,
                    Source               = spell.Name,
                    LeechPerTurn         = template.LeechPerTurn,
                    LeechResourceType    = template.LeechResourceType ?? "HP",
                    CasterName           = template.Type == StatusEffectType.Leech ? caster.Name : string.Empty
                };

                _statusEffectService.Apply(target, effect);
                await notify(new CombatLogEntry
                {
                    Tick               = tick,
                    ActorName          = target.Name,
                    EventType          = "EffectApplied",
                    StatusEffectName   = effect.Name,
                    AttackSourceName   = spell.Name,
                    IsBuff             = effect.Type == StatusEffectType.Buff,
                    EffectDuration     = effect.Duration,
                    EffectMaxDuration  = effect.Duration,
                    EffectStacks       = target.ActiveStatusEffects.Count(e => e.Name == effect.Name),
                    Message            = $"{target.Name} gains {effect.Name} from {spell.Name}!"
                });
            }
        }
    }

    public async Task TryApplyEffectAsync(
        int tick, Character target, StatusEffect effect,
        Func<CombatLogEntry, Task> notify)
    {
        var resistance = target.ComputeResistance(effect.ResistanceType);
        var appResult = _statusEffectService.TryApply(target, effect, resistance, _dice);

        if (appResult.Applied)
        {
            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = target.Name,
                EventType          = "EffectApplied",
                StatusEffectName   = effect.Name,
                EffectDuration     = effect.Duration,
                EffectMaxDuration  = effect.Duration,
                EffectStacks       = target.ActiveStatusEffects.Count(e => e.Name == effect.Name),
                Message            = $"{target.Name} is afflicted with {effect.Name}!"
            });
        }
        else if (appResult.WasResisted)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "EffectResisted",
                StatusEffectName = effect.Name,
                ResistRoll       = appResult.Roll,
                ResistThreshold  = appResult.TotalResistance,
                Message          = $"{target.Name} resists {effect.Name}! (rolled {appResult.Roll} vs {appResult.TotalResistance} resistance)"
            });
        }
    }

    // Per-effect overload (for refactored callers that loop individually).
    public async Task TryApplyEffectAsync(
        int tick, StatusEffect effect, Character target, string sourceName,
        Func<CombatLogEntry, Task> notify)
    {
        var resistance = target.ComputeResistance(effect.ResistanceType);
        var result = _statusEffectService.TryApply(target, effect, resistance, _dice);

        if (result.Applied)
        {
            await notify(new CombatLogEntry
            {
                Tick               = tick,
                ActorName          = target.Name,
                EventType          = "EffectApplied",
                StatusEffectName   = effect.Name,
                AttackSourceName   = sourceName,
                EffectDuration     = effect.Duration,
                EffectMaxDuration  = effect.Duration,
                EffectStacks       = target.ActiveStatusEffects.Count(e => e.Name == effect.Name),
                Message            = $"{target.Name} is afflicted with {effect.Name}!"
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
                ResistRoll       = result.Roll,
                ResistThreshold  = result.TotalResistance,
                AttackSourceName = sourceName,
                Message          = $"{target.Name} resists {effect.Name}! (rolled {result.Roll} vs {result.TotalResistance} resistance)"
            });
        }
    }

    public async Task ProcessOnHitEffectsAsync(
        int tick, Character attacker, Character target, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.OnHitEffects.Count == 0 && spell.ElementalType == ElementalType.None) return;

        foreach (var template in spell.OnHitEffects)
        {
            if (template.Target != EffectTarget.Target) continue;

            var actualTarget = target;
            if (TryGetReflectChance(target, out var reflectChance)
                && _dice.Roll(DieType.D100) <= reflectChance)
            {
                actualTarget = attacker;
                await notify(new CombatLogEntry
                {
                    Tick = tick, ActorName = target.Name, EventType = "EffectReflected",
                    StatusEffectName = template.Name, TargetName = attacker.Name,
                    Message = $"{target.Name}'s reflective shield reflects {template.Name} back to {attacker.Name}!"
                });
            }

            var dmgPerTurn = template.DamagePerTurn;
            if (dmgPerTurn <= 0 && template.DoTDamageCount > 0)
                for (var i = 0; i < template.DoTDamageCount; i++)
                    dmgPerTurn += _dice.Roll(template.DoTDamageDie);

            var effect = new StatusEffect
            {
                Name = template.Name, Type = template.Type, Target = template.Target,
                ResistanceType = template.ResistanceType, ResistanceBonuses = template.ResistanceBonuses,
                Duration = template.Duration, DamagePerTurn = dmgPerTurn,
                HealingPerTurn = template.HealingPerTurn,
                AttackPowerModifier = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                TurnMeterModifier = template.TurnMeterModifier, StackRule = template.StackRule,
                ApplicationChance = template.ApplicationChance, Source = spell.Name,
                LeechPerTurn = template.LeechPerTurn,
                LeechResourceType = template.LeechResourceType ?? "HP",
                CasterName = template.Type == StatusEffectType.Leech ? attacker.Name : string.Empty
            };

            await TryApplyEffectAsync(tick, actualTarget, effect, notify);
        }

        if (spell.ElementalType != ElementalType.None)
        {
            var dotName = GetElementalDoTName(spell.ElementalType);
            var hasMatchingEffect = dotName is not null &&
                spell.OnHitEffects.Any(e => e.Name == dotName);
            if (!hasMatchingEffect)
                await TryApplyElementalDoTAsync(tick, target, spell, notify);
        }
    }

    // Per-effect overload (for refactored callers that use IAttackSource).
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
                Name = template.Name, Type = template.Type,
                Duration = template.Duration, StackRule = template.StackRule,
                DamagePerTurn = template.DamagePerTurn, HealingPerTurn = template.HealingPerTurn,
                AttackPowerModifier = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                MovementModifier = template.MovementModifier,
                LeechPerTurn = template.LeechPerTurn, LeechResourceType = template.LeechResourceType,
                Source = sourceName, ResistanceType = template.ResistanceType,
                ApplicationChance = template.ApplicationChance
            };
            await TryApplyEffectAsync(tick, effect, target, sourceName, notify);
        }
    }

    private static bool TryGetReflectChance(Character character, out int chance)
    {
        chance = 0;
        foreach (var effect in character.ActiveStatusEffects)
        {
            if (effect.ReflectChance > 0 && effect.ReflectChance > chance)
                chance = effect.ReflectChance;
        }
        return chance > 0;
    }

    private int RollDie(DieType die) => _dice.Roll(die);

    private static string? GetElementalDoTName(ElementalType type) => type switch
    {
        ElementalType.Fire => "Burning",
        ElementalType.Ice => "Chilled",
        ElementalType.Lightning => "Electrified",
        ElementalType.Poison => "Poisoned",
        _ => null
    };

    public async Task TryApplyElementalDoTAsync(
        int tick, Character target, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        var dot = CreateElementalDoT(spell.ElementalType, spell.Name);
        if (dot is null) return;

        var dmgPerTurn = dot.DamagePerTurn;
        if (dmgPerTurn <= 0 && dot.DoTDamageCount > 0)
            for (var i = 0; i < dot.DoTDamageCount; i++)
                dmgPerTurn += RollDie(dot.DoTDamageDie);
        dot.DamagePerTurn = dmgPerTurn;

        await TryApplyEffectAsync(tick, target, dot, notify);
    }

    private static StatusEffect? CreateElementalDoT(ElementalType type, string sourceName)
    {
        return type switch
        {
            ElementalType.Fire => new StatusEffect
            {
                Name = "Burning", Type = StatusEffectType.DamageOverTime,
                DamagePerTurn = 0, DoTDamageCount = 1, DoTDamageDie = DieType.D6,
                Duration = 3, ApplicationChance = 60,
                ResistanceType = ResistanceType.Fire,
                Source = sourceName
            },
            ElementalType.Ice => new StatusEffect
            {
                Name = "Chilled", Type = StatusEffectType.DamageOverTime,
                DamagePerTurn = 0, DoTDamageCount = 1, DoTDamageDie = DieType.D4,
                Duration = 2, ApplicationChance = 50,
                ResistanceType = ResistanceType.Cold,
                Source = sourceName
            },
            ElementalType.Lightning => new StatusEffect
            {
                Name = "Shocked", Type = StatusEffectType.DamageOverTime,
                DamagePerTurn = 0, DoTDamageCount = 1, DoTDamageDie = DieType.D8,
                Duration = 2, ApplicationChance = 40,
                ResistanceType = ResistanceType.Lightning,
                Source = sourceName
            },
            ElementalType.Poison => new StatusEffect
            {
                Name = "Poisoned", Type = StatusEffectType.DamageOverTime,
                DamagePerTurn = 0, DoTDamageCount = 1, DoTDamageDie = DieType.D4,
                Duration = 3, ApplicationChance = 70,
                ResistanceType = ResistanceType.Poison,
                Source = sourceName
            },
            _ => null
        };
    }

    public static async Task NotifyExpiredEffectsAsync(
        int tick, Character character,
        IReadOnlyList<string> expired,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var name in expired)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = character.Name,
                EventType        = "EffectExpired",
                StatusEffectName = name,
                Message          = $"{name} has worn off {character.Name}."
            });
        }
    }

    public async Task ExpireSummonedPetsAsync(
        int tick, int currentRound, List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states)
        {
            if (!s.IsSummoned || !s.Character.IsAlive || s.SummonExpiryRound <= 0 || s.SummonExpiryRound > currentRound)
                continue;
            s.Character.CurrentHitPoints = -999;
            await notify(new CombatLogEntry
            {
                Tick = tick,
                EventType = "PetExpired",
                ActorName = s.Character.Name,
                SummonedPetName = s.Character.Name,
                RoundNumber = currentRound,
                Message = $"{s.Character.Name} fades away as the summoning ends."
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
}