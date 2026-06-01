namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Core.Models;

public class CombatService : ICombatService
{
    private readonly IDiceService _dice;
    private readonly ICombatStatsService _combatStats;
    private readonly IReadOnlyList<ICombatModifier> _attackRollMods;
    private readonly IReadOnlyList<ICombatModifier> _damageCalcMods;
    private readonly IReadOnlyList<ICombatModifier> _healingMods;

    public CombatService(IDiceService dice, ICombatStatsService combatStats,
        IEnumerable<ICombatModifier> modifiers = default!)
    {
        _dice           = dice;
        _combatStats    = combatStats;
        var ordered     = (modifiers ?? []).OrderBy(m => m.Priority).ToList();
        _attackRollMods = ordered.Where(m => m.Phase == CombatPhase.AttackRoll).ToList();
        _damageCalcMods = ordered.Where(m => m.Phase == CombatPhase.DamageCalculation).ToList();
        _healingMods    = ordered.Where(m => m.Phase == CombatPhase.Healing).ToList();
    }

    public int CalculateAbilityModifier(int score)
    {
        return (score - 10) / 2;
    }

    public DamageRollResult RollDamage(IAttackSource source)
    {
        var result = _dice.Roll(source.DamageDie);
        return new DamageRollResult
        {
            DieType = source.DamageDie,
            Result  = result
        };
    }

    public AttackResult ResolveAttack(Character attacker, Character defender, IAttackSource source,
        EngagementRange range = EngagementRange.Melee,
        TerrainType terrain = TerrainType.Plains)
    {
        var attackerStats = _combatStats.ComputeAttackerStats(attacker, source);
        var defenderStats = _combatStats.ComputeDefenderStats(defender, source);

        // Run AttackRoll-phase modifiers through the pipeline.
        var ctx = new CombatModifierContext
        {
            Attacker         = attacker,
            Defender         = defender,
            Source           = source,
            Range            = range,
            Terrain          = terrain,
            BaseAttackPower  = attackerStats.AttackPower,
            BaseDefensePower = defenderStats.DefensePower
        };
        foreach (var mod in _attackRollMods)
            mod.Apply(ctx);

        var effectiveAP = attackerStats.AttackPower + ctx.AttackPowerDelta;
        var effectiveDP = defenderStats.DefensePower + ctx.DefensePowerDelta;

        var attackRoll  = _dice.Roll(DieType.D20);
        var defenseRoll = _dice.Roll(DieType.D20);

        // ── Priority 1 ─── TotalReversal (atk=1 AND def=20) ──────────────────
        if (attackRoll == 1 && defenseRoll == 20)
        {
            return new AttackResult
            {
                HitRoll           = attackRoll,
                DefenseRoll       = defenseRoll,
                IsHit             = false,
                IsFumble          = true,
                IsTotalReversal   = true,
                AttackPowerPenalty = -4,
                DefenderTmBonus   = ComputeDefenderTmBoost(source.AttackType, range, isTotalReversal: true),
                Damage            = 0,
                DamageDie         = source.DamageDie,
                WeaponName        = source.Name,
                AttackPower       = effectiveAP,
                DefensePower      = effectiveDP
            };
        }

        // ── Priority 2 ─── DevastatingStrike (atk=20 AND def=1) ──────────────
        if (attackRoll == 20 && defenseRoll == 1)
        {
            var dc = ResolveDamage(attacker, defender, source, isCritical: false, range, terrain);
            var devastatingDamage = Math.Max(0,
                (int)(dc.BaseDamage * 3 * dc.TypeMultiplier) - dc.ArmorMitigation + dc.ElementalModifiers);
            return new AttackResult
            {
                HitRoll             = attackRoll,
                DefenseRoll         = defenseRoll,
                IsHit               = true,
                IsDevastatingStrike = true,
                Damage              = devastatingDamage,
                DamageDie           = source.DamageDie,
                WeaponName          = source.Name,
                AttackPower         = effectiveAP,
                DefensePower        = effectiveDP,
                DamageContext       = dc
            };
        }

        // ── Priority 3 ─── Perfect Parry vs Critical (both roll 20) ───────────
        if (attackRoll == 20 && defenseRoll == 20)
        {
            return new AttackResult
            {
                HitRoll         = attackRoll,
                DefenseRoll     = defenseRoll,
                IsHit           = false,
                IsPerfectParry  = true,
                DefenderTmBonus = ComputeDefenderTmBoost(source.AttackType, range, isTotalReversal: false),
                Damage          = 0,
                DamageDie       = source.DamageDie,
                WeaponName      = source.Name,
                AttackPower     = effectiveAP,
                DefensePower    = effectiveDP
            };
        }

        // ── Priority 4 ─── Fumble (atk=1, def != 20) ─────────────────────────
        if (attackRoll == 1)
        {
            return new AttackResult
            {
                HitRoll            = attackRoll,
                DefenseRoll        = defenseRoll,
                IsHit              = false,
                IsFumble           = true,
                AttackPowerPenalty = -2,
                Damage             = 0,
                DamageDie          = source.DamageDie,
                WeaponName         = source.Name,
                AttackPower        = effectiveAP,
                DefensePower       = effectiveDP
            };
        }

        // ── Priority 5 ─── Critical hit (atk=20, def != 1 or 20) ─────────────
        if (attackRoll == 20)
        {
            var dc = ResolveDamage(attacker, defender, source, isCritical: true, range, terrain);
            return new AttackResult
            {
                HitRoll       = attackRoll,
                DefenseRoll   = defenseRoll,
                IsHit         = true,
                IsCriticalHit = true,
                Damage        = dc.FinalDamage,
                DamageDie     = source.DamageDie,
                WeaponName    = source.Name,
                AttackPower   = effectiveAP,
                DefensePower  = effectiveDP,
                DamageContext = dc
            };
        }

        // ── Priority 6 ─── Perfect Parry (def=20, atk=2–19) ──────────────────
        if (defenseRoll == 20)
        {
            return new AttackResult
            {
                HitRoll         = attackRoll,
                DefenseRoll     = defenseRoll,
                IsHit           = false,
                IsPerfectParry  = true,
                DefenderTmBonus = ComputeDefenderTmBoost(source.AttackType, range, isTotalReversal: false),
                Damage          = 0,
                DamageDie       = source.DamageDie,
                WeaponName      = source.Name,
                AttackPower     = effectiveAP,
                DefensePower    = effectiveDP
            };
        }

        // ── Priority 7 ─── Normal opposed roll (both 2–19) ───────────────────
        var isHit         = (attackRoll + effectiveAP) >= (defenseRoll + effectiveDP);
        var damageContext = isHit ? ResolveDamage(attacker, defender, source, isCritical: false, range, terrain) : null;

        return new AttackResult
        {
            HitRoll       = attackRoll,
            DefenseRoll   = defenseRoll,
            IsHit         = isHit,
            Damage        = damageContext?.FinalDamage ?? 0,
            DamageDie     = source.DamageDie,
            WeaponName    = source.Name,
            AttackPower   = effectiveAP,
            DefensePower  = effectiveDP,
            DamageContext = damageContext
        };
    }

    public DamageContext ResolveDamage(Character attacker, Character defender, IAttackSource source,
        bool isCritical = false,
        EngagementRange range = EngagementRange.Melee,
        TerrainType terrain = TerrainType.Plains)
    {
        // Run DamageCalculation-phase modifiers through the pipeline.
        var ctx = new CombatModifierContext
        {
            Attacker         = attacker,
            Defender         = defender,
            Source           = source,
            Range            = range,
            Terrain          = terrain,
            BaseAttackPower  = 0,
            BaseDefensePower = 0
        };
        foreach (var mod in _damageCalcMods)
            mod.Apply(ctx);

        var abilityScore = source.UsesIntelligence
            ? attacker.Intelligence
            : source.AttackType == AttackType.Ranged ? attacker.Dexterity : attacker.Strength;
        var attributeModifier = CalculateAbilityModifier(abilityScore);
        var weaponDiceRoll    = RollAttackDamageTotal(source);
            var levelScaling = attacker.Level / 2;
        var baseDamage        = weaponDiceRoll + attributeModifier + source.FlatDamageBonus + levelScaling;
        var typeMultiplier    = defender.Vulnerabilities.Contains(source.DamageType) ? 1.5f : 1.0f;

        // Apply damage modifiers from the pipeline.
        var scaledBase = isCritical ? baseDamage * 2 : baseDamage;
        scaledBase = (int)(scaledBase * ctx.DamageMultiplier);

        var finalDamage = Math.Max(0,
            (int)(scaledBase * typeMultiplier) - defender.Equipment.TotalMitigation + source.ElementalDamage + ctx.DamageDelta);

        return new DamageContext
        {
            WeaponDiceRoll    = weaponDiceRoll,
            AttributeModifier = attributeModifier,
            FlatBonuses       = source.FlatDamageBonus,
            LevelScaling      = levelScaling,
            BaseDamage        = baseDamage,
            TypeMultiplier    = typeMultiplier,
            ArmorMitigation   = defender.Equipment.TotalMitigation,
            ElementalModifiers = source.ElementalDamage,
            FinalDamage       = finalDamage
        };
    }

    public int ResolveHealing(Character healer, Character target, Spell spell,
        TerrainType terrain = TerrainType.Plains)
    {
        // Run Healing-phase modifiers through the pipeline.
        var ctx = new CombatModifierContext
        {
            Attacker         = healer,
            Defender         = target,
            Source           = spell,
            Range            = EngagementRange.Melee,
            Terrain          = terrain,
            BaseAttackPower  = 0,
            BaseDefensePower = 0
        };
        foreach (var mod in _healingMods)
            mod.Apply(ctx);

        var abilityMod = CalculateAbilityModifier(healer.Intelligence);
        var totalDice  = 0;
        for (var i = 0; i < spell.DamageCount; i++)
            totalDice += _dice.Roll(spell.DamageDie);

        var baseHeal = totalDice + abilityMod + spell.FlatDamageBonus + ctx.HealingPowerDelta;
        var final    = (int)(baseHeal * ctx.HealingMultiplier);

        return Math.Max(1, final);
    }

    /// <summary>
    /// Turn-meter boost awarded to the defender on PerfectParry or TotalReversal.
    /// </summary>
    private static int ComputeDefenderTmBoost(AttackType attackType, EngagementRange range, bool isTotalReversal)
    {
        var baseBoost = isTotalReversal ? 30 : 20;
        if (attackType == AttackType.Ranged && range != EngagementRange.Melee)
            return baseBoost / 2;
        return baseBoost;
    }

    private int RollAttackDamageTotal(IAttackSource source)
    {
        var total = 0;
        for (var i = 0; i < source.DamageCount; i++)
            total += _dice.Roll(source.DamageDie);
        return total;
    }
}
