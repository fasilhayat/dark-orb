namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;

public class AutoActionDecisionSource : IActionDecisionSource
{
    private const double EmergencyHp = 0.25;
    private const double TacticalHp = 0.40;
    private const double ManaLow = 0.25;

    private static readonly double[] DieAverages = new double[]
    {
        0, 0, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5
    };

    private static readonly Dictionary<DamageType, ElementalType> DamageToElement = new()
    {
        [DamageType.Fire] = ElementalType.Fire,
        [DamageType.Ice] = ElementalType.Ice,
        [DamageType.Lightning] = ElementalType.Lightning,
        [DamageType.Acid] = ElementalType.Poison,
    };

    public AutoActionDecisionSource() { }

    public AutoActionDecisionSource(IDiceService _) { }

    public Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct,
        EngagementRange engagementRange = EngagementRange.Melee)
    {
        var weapon = actor.Equipment.RightHand;
        var healSpells = FilteredSpells(actor, s => s.IsHealing);
        var dmgSpells = FilteredSpells(actor, s => !s.IsHealing);
        var enemyCount = enemies.Count(e => e.IsAlive);

        // ── STAGE 1: Emergency — self below 25% HP ─────────────────
        if (actor.CurrentHitPoints < actor.MaxHitPoints * EmergencyHp)
        {
            var selfHeal = healSpells.FirstOrDefault();
            if (selfHeal is not null)
                return Task.FromResult<IAttackSource?>(selfHeal);

            var defense = dmgSpells.FirstOrDefault(s => IsDefensive(s));
            if (defense is not null)
                return Task.FromResult<IAttackSource?>(defense);
        }

        // ── STAGE 2: Tactical — ally below 40% HP ──────────────────
        if (allies.Any(a => a.IsAlive && (double)a.CurrentHitPoints / a.MaxHitPoints < TacticalHp))
        {
            var groupHeal = healSpells.FirstOrDefault(s => s.IsGroupHeal);
            if (groupHeal is not null && allies.Count(a => a.IsAlive) >= 2)
                return Task.FromResult<IAttackSource?>(groupHeal);

            var anyHeal = healSpells.FirstOrDefault();
            if (anyHeal is not null)
                return Task.FromResult<IAttackSource?>(anyHeal);
        }

        // ── STAGE 3: Mana low — conserve ───────────────────────────
        if (actor.EffectiveMaxMana > 0 && (double)actor.CurrentMana / actor.EffectiveMaxMana < ManaLow)
        {
            if (weapon is not null)
                return Task.FromResult<IAttackSource?>(weapon);

            var cheapSpell = dmgSpells
                .OrderBy(s => s.ManaCost)
                .FirstOrDefault();
            if (cheapSpell is not null)
                return Task.FromResult<IAttackSource?>(cheapSpell);
        }

        // ── STAGE 4: Offensive — pick a random damage spell ──────
        if (dmgSpells.Count > 0)
        {
            var scored = dmgSpells
                .Select(s => (Spell: s, Score: ScoreDamageSpell(actor, s, enemyCount)))
                .OrderByDescending(x => x.Score)
                .ToList();

            // Consider only spells within 80% of the top score, then pick randomly
            var topScore = scored[0].Score;
            var candidates = scored.Where(x => x.Score >= topScore * 0.8).ToList();
            var picked = candidates[Random.Shared.Next(candidates.Count)];
            return Task.FromResult<IAttackSource?>(picked.Spell);
        }

        // ── STAGE 5: Weapon fallback ───────────────────────────────
        if (weapon is not null)
            return Task.FromResult<IAttackSource?>(weapon);

        if (defaultAttack is not null)
            return Task.FromResult<IAttackSource?>(defaultAttack);

        return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
    }

    private List<Spell> FilteredSpells(Character actor, Func<Spell, bool> predicate)
    {
        return actor.MemorizedSpells
            .Where(s => actor.CanCast(s))
            .Where(s => s.ManaCost <= 0 || actor.CurrentMana >= s.ManaCost)
            .Where(s => IsUsefulLeech(s, actor))
            .Where(s => s.SummonedPet is not null || s.DamageCount > 0 || s.OnHitEffects.Count > 0 || s.IsHealing)
            .Where(predicate)
            .ToList();
    }

    private static bool IsUsefulLeech(Spell spell, Character actor)
    {
        foreach (var eff in spell.OnHitEffects)
        {
            if (eff.Type != StatusEffectType.Leech || eff.LeechPerTurn <= 0)
                continue;
            if (eff.LeechResourceType == "Mana" && actor.CurrentMana >= actor.EffectiveMaxMana)
                return false;
            if (eff.LeechResourceType == "HP" && actor.CurrentHitPoints >= actor.MaxHitPoints)
                return false;
        }
        return true;
    }

    private static bool IsDefensive(Spell spell)
    {
        var name = spell.Name.ToLowerInvariant();
        return name is "shield" or "blink" or "mirror image" or "invisibility"
            or "barkskin" or "stoneskin" or "armor";
    }

    private static double ScoreDamageSpell(Character actor, Spell spell, int enemyCount)
    {
        var score = 0.0;

        // Base damage
        var avgDice = spell.DamageCount * AverageDie(spell.DamageDie);
        score += avgDice * 2;

        // Level bonus (spell level reflects power)
        score += spell.SpellLevel * 1.5;

        // Intelligence/Wisdom bonus
        var spellStat = spell.UsesIntelligence ? actor.Intelligence : actor.Wisdom;
        var statMod = (spellStat - 10) / 2;
        score += statMod * 1.5;

        // Flat damage bonus
        score += spell.FlatDamageBonus;

        // Elemental damage bonus
        if (DamageToElement.ContainsKey(spell.DamageType))
            score += 2;

        // On-hit effects add value
        foreach (var eff in spell.OnHitEffects)
        {
            if (eff.Type == StatusEffectType.DamageOverTime && eff.DamagePerTurn > 0)
                score += eff.DamagePerTurn * eff.Duration * 0.5;
            if (eff.Type == StatusEffectType.Shock)
                score += 3;
            if (eff.LeechPerTurn > 0)
                score += eff.LeechPerTurn * 0.5;
        }

        // AOE bonus when multiple enemies
        if (enemyCount >= 2 && spell.DamageCount > 1)
            score *= 1.3;

        // Pet-summoning spells get a baseline score
        if (spell.SummonedPet is not null)
            score = Math.Max(score, 8);

        return score;
    }

    private static double AverageDie(DieType die) =>
        die >= 0 && (int)die < DieAverages.Length ? DieAverages[(int)die] : (int)die / 2.0 + 0.5;
}
