namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public class CombatStatsService : ICombatStatsService
{
    public CombatantStats ComputeAttackerStats(Character attacker, IAttackSource source)
    {
        var buffModifiers = AccumulateAttackBuffs(attacker);
        var abilityScore  = ResolveAttackerAbilityScore(attacker, source);

        var classBonus = source.AttackType == AttackType.Ranged
            ? attacker.RangedAttackBonus
            : 0;

        var twoHandedBonus = attacker.TwoHandedWeaponBonus;
        var shieldBonus = attacker.ShieldBonusDamage;
        var elvenRangerBonus = attacker.ElvenRangerDexBonus;

        return new CombatantStats
        {
            ClassAccuracyBase = attacker.StrikeRating,
            LevelScaling      = attacker.Level / 2,
            AttributeModifier = CalculateAbilityModifier(abilityScore),
            WeaponAttackBonus = source.AttackBonus + classBonus + twoHandedBonus + shieldBonus + elvenRangerBonus,
            SkillModifiers    = attacker.Feats.Sum(f => f.AttackBonus),
            BuffModifiers     = buffModifiers,
            RacialModifiers   = attacker.Race?.Feats.Sum(f => f.AttackBonus) ?? 0,
            ItemSetBonuses    = 0
        };
    }

    private static int AccumulateAttackBuffs(Character attacker)
    {
        var stackSum       = 0;
        var highestWinsMax = 0;
        var hasHighestWins = false;
        var noStackFirst   = 0;
        var hasNoStack     = false;
        var negativeSum    = 0;

        foreach (var e in attacker.ActiveStatusEffects)
        {
            var mod = e.AttackPowerModifier;
            if (mod == 0) continue;
            if (mod < 0) { negativeSum += mod; continue; }
            switch (e.StackRule)
            {
                case StackRule.Stack:
                    stackSum += mod;
                    break;
                case StackRule.HighestWins:
                    if (mod > highestWinsMax) { highestWinsMax = mod; hasHighestWins = true; }
                    break;
                case StackRule.NoStack:
                    if (!hasNoStack) { noStackFirst = mod; hasNoStack = true; }
                    break;
            }
        }

        return stackSum
            + (hasHighestWins ? highestWinsMax : 0)
            + (hasNoStack     ? noStackFirst   : 0)
            + negativeSum;
    }

    private static int ResolveAttackerAbilityScore(Character attacker, IAttackSource source)
    {
        if (source.UsesIntelligence) return attacker.Intelligence;
        return source.AttackType == AttackType.Ranged ? attacker.Dexterity : attacker.Strength;
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
        var maxDexBonus       = ComputeMaxDexBonus(defender.Equipment);
        if (maxDexBonus.HasValue)
            dexterityModifier = Math.Min(dexterityModifier, maxDexBonus.Value);

        // Per-source HighestWins: buffs from the same source don't stack — only the best applies.
        // Negative debuffs always stack regardless of source.
        var sourceBestBuffs = new Dictionary<string, int>();
        var negativeDebuffs = 0;
        foreach (var e in defender.ActiveStatusEffects)
        {
            var mod = e.DefensePowerModifier;
            if (mod == 0) continue;
            if (mod < 0) { negativeDebuffs += mod; continue; }
            if (e.Type != StatusEffectType.Buff) continue;
            var src = e.Source ?? string.Empty;
            if (!sourceBestBuffs.TryGetValue(src, out var best) || mod > best)
                sourceBestBuffs[src] = mod;
        }
        var positiveBuffTotal = sourceBestBuffs.Values.Sum();

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

    private static int? ComputeMaxDexBonus(ArmorSlots equipment)
    {
        var total = 0;
        var found = false;
        if (equipment.Head   is { } h) { total += h.MaxDexterityBonus; found = true; }
        if (equipment.Chest  is { } c) { total += c.MaxDexterityBonus; found = true; }
        if (equipment.Hands  is { } g) { total += g.MaxDexterityBonus; found = true; }
        if (equipment.Waist  is { } w) { total += w.MaxDexterityBonus; found = true; }
        if (equipment.Boots  is { } b) { total += b.MaxDexterityBonus; found = true; }
        if (equipment.Neck   is { } n) { total += n.MaxDexterityBonus; found = true; }
        if (equipment.Back   is { } k) { total += k.MaxDexterityBonus; found = true; }
        return found ? total : null;
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
        // Per-source HighestWins: buffs from the same source don't stack — only the best applies.
        var sourceBestBuffs = new Dictionary<string, int>();
        var negativeDebuffs = 0;
        foreach (var e in defender.ActiveStatusEffects)
        {
            var mod = e.DefensePowerModifier;
            if (mod == 0) continue;
            if (mod < 0) { negativeDebuffs += mod; continue; }
            if (e.Type != StatusEffectType.Buff) continue;
            var src = e.Source ?? string.Empty;
            if (!sourceBestBuffs.TryGetValue(src, out var best) || mod > best)
                sourceBestBuffs[src] = mod;
        }
        var positiveBuffTotal = sourceBestBuffs.Values.Sum();

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

    private static int CalculateAbilityModifier(int score) => (score - 10) / 2;
}
