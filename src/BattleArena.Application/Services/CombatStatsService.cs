namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public class CombatStatsService : ICombatStatsService
{
    public CombatantStats ComputeAttackerStats(Character attacker, IAttackSource source)
    {
        var attackEffects = attacker.ActiveStatusEffects.Where(e => e.AttackPowerModifier != 0).ToList();
        var positiveBuffs = attackEffects.Where(e => e.AttackPowerModifier > 0).ToList();
        var negativeBuffs = attackEffects.Where(e => e.AttackPowerModifier < 0).Sum(e => e.AttackPowerModifier);
        var abilityScore = source.UsesIntelligence
            ? attacker.Intelligence
            : source.AttackType == AttackType.Ranged ? attacker.Dexterity : attacker.Strength;

        return new CombatantStats
        {
            ClassAccuracyBase = attacker.StrikeRating,
            LevelScaling = attacker.Level,
            AttributeModifier = CalculateAbilityModifier(abilityScore),
            WeaponAttackBonus = source.AttackBonus,
            SkillModifiers = attacker.Feats.Sum(f => f.AttackBonus),
            BuffModifiers =
                ApplyBuffStacking(positiveBuffs.Where(e => e.StackRule == StackRule.Stack), e => e.AttackPowerModifier, StackRule.Stack) +
                ApplyBuffStacking(positiveBuffs.Where(e => e.StackRule == StackRule.HighestWins), e => e.AttackPowerModifier, StackRule.HighestWins) +
                ApplyBuffStacking(positiveBuffs.Where(e => e.StackRule == StackRule.NoStack), e => e.AttackPowerModifier, StackRule.NoStack) +
                negativeBuffs,
            RacialModifiers = attacker.Race?.Feats.Sum(f => f.AttackBonus) ?? 0,
            ItemSetBonuses = 0
        };
    }

    public CombatantStats ComputeDefenderStats(Character defender, IAttackSource? source = null)
    {
        var isSpell = source?.AttackType == AttackType.Spell;

        if (isSpell)
            return ComputeSpellDefenderStats(defender);

        return ComputePhysicalDefenderStats(defender);
    }

    /// <summary>
    /// Physical defense: AC + DEX modifier (capped by armor) + shield + buffs + racial + level.
    /// Used for Melee and Ranged attacks and for character-sheet display (source = null).
    /// </summary>
    private CombatantStats ComputePhysicalDefenderStats(Character defender)
    {
        var dexterityModifier = CalculateAbilityModifier(defender.Dexterity);
        var armorPieces = new[]
        {
            defender.Equipment.Head,
            defender.Equipment.Chest,
            defender.Equipment.Hands,
            defender.Equipment.Waist,
            defender.Equipment.Boots,
            defender.Equipment.Neck,
            defender.Equipment.Back
        }.Where(a => a is not null).ToList();

        if (armorPieces.Count > 0)
        {
            var maxDexterityBonus = armorPieces.Sum(a => a!.MaxDexterityBonus);
            dexterityModifier = Math.Min(dexterityModifier, maxDexterityBonus);
        }

        var defenseEffects = defender.ActiveStatusEffects.Where(e => e.DefensePowerModifier != 0).ToList();
        var positiveBuffs = defenseEffects.Where(e => e.Type == StatusEffectType.Buff && e.DefensePowerModifier > 0);
        var positiveBuffTotal = positiveBuffs
            .GroupBy(e => e.Source)
            .Sum(group => ApplyBuffStacking(group, e => e.DefensePowerModifier, StackRule.HighestWins));
        var negativeDebuffs = defenseEffects.Where(e => e.DefensePowerModifier < 0).Sum(e => e.DefensePowerModifier);

        return new CombatantStats
        {
            EffectiveAC            = defender.Equipment.TotalArmorClass,
            DexterityModifier      = dexterityModifier,
            ShieldBonus            = defender.Equipment.Shield?.DefenseBonus ?? 0,
            DefensiveBuffs         = positiveBuffTotal + negativeDebuffs,
            DefenseRacialModifiers = (defender.Race?.Feats.Sum(f => f.DefenseBonus) ?? 0) + defender.Feats.Sum(f => f.DefenseBonus),
            DefenseItemSetBonuses  = 0,
            LevelDefenseBonus      = defender.Level,
            MagicResistanceBonus   = 0
        };
    }

    /// <summary>
    /// Spell defense: Wisdom modifier + magic resistance (converted to d20 scale) + buffs + racial + level.
    /// Armor and shields do not protect against spells; wisdom and innate magic resistance do.
    /// </summary>
    private CombatantStats ComputeSpellDefenderStats(Character defender)
    {
        var wisdomModifier = CalculateAbilityModifier(defender.Wisdom);

        // Magic resistance (0–95 %) converted to d20 bonus scale (÷5 → 0–19).
        var magicResistanceBonus = defender.ComputeResistance(ResistanceType.Magic) / 5;

        // Protective spell buffs still apply (ward spells, etc.)
        var defenseEffects = defender.ActiveStatusEffects.Where(e => e.DefensePowerModifier != 0).ToList();
        var positiveBuffs  = defenseEffects.Where(e => e.Type == StatusEffectType.Buff && e.DefensePowerModifier > 0);
        var positiveBuffTotal = positiveBuffs
            .GroupBy(e => e.Source)
            .Sum(group => ApplyBuffStacking(group, e => e.DefensePowerModifier, StackRule.HighestWins));
        var negativeDebuffs = defenseEffects.Where(e => e.DefensePowerModifier < 0).Sum(e => e.DefensePowerModifier);

        return new CombatantStats
        {
            EffectiveAC            = 0,            // armor irrelevant vs spells
            DexterityModifier      = wisdomModifier,
            ShieldBonus            = 0,            // shields irrelevant vs spells
            DefensiveBuffs         = positiveBuffTotal + negativeDebuffs,
            DefenseRacialModifiers = (defender.Race?.Feats.Sum(f => f.DefenseBonus) ?? 0) + defender.Feats.Sum(f => f.DefenseBonus),
            DefenseItemSetBonuses  = 0,
            LevelDefenseBonus      = defender.Level,
            MagicResistanceBonus   = magicResistanceBonus
        };
    }

    private static int ApplyBuffStacking(IEnumerable<StatusEffect> effects, Func<StatusEffect, int> selector, StackRule rule)
    {
        var effectList = effects.ToList();
        if (effectList.Count == 0)
            return 0;

        if (effectList.All(e => selector(e) < 0))
            return effectList.Sum(selector);

        return rule switch
        {
            StackRule.Stack => effectList.Sum(selector),
            StackRule.HighestWins => effectList.Max(selector),
            StackRule.NoStack => selector(effectList[0]),
            _ => 0
        };
    }

    private static int CalculateAbilityModifier(int score) => (score - 10) / 2;
}
