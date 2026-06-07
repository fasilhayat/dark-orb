namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;

public class AutoActionDecisionSource : IActionDecisionSource
{
    private const double HealThreshold = 0.4;

    private readonly IDiceService _dice;

    public AutoActionDecisionSource(IDiceService dice)
    {
        _dice = dice;
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

    private static bool IsEffectiveDamageSpell(Spell spell)
    {
        // Allow spells that summon pets even with zero damage dice
        if (spell.SummonedPet is not null)
            return true;
        // Skip spells with zero damage dice and no useful on-hit effects
        if (spell.DamageCount <= 0 && spell.OnHitEffects.Count == 0)
            return false;
        return true;
    }

    public Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct)
    {
        var weapon = actor.Equipment.RightHand;

        var spells = actor.MemorizedSpells
            .Where(s => actor.CanCast(s) && IsUsefulLeech(s, actor) && IsEffectiveDamageSpell(s))
            .ToList();

        if (spells.Count > 0)
        {
            var healSpells = spells
                .Where(s => s.IsHealing && (s.ManaCost <= 0 || actor.CurrentMana >= s.ManaCost))
                .ToList();

            var dmgSpells = spells
                .Where(s => !s.IsHealing && (s.ManaCost <= 0 || actor.CurrentMana >= s.ManaCost))
                .ToList();

            // Priority 1: Heal only if critically injured
            if (healSpells.Count > 0)
            {
                var anyNeedHeal = allies.Any(a => (double)a.CurrentHitPoints / a.MaxHitPoints < HealThreshold);
                if (anyNeedHeal)
                {
                    // Prefer single-target heal over group heal when only one ally needs it
                    var singleTarget = healSpells.Where(s => !s.IsGroupHeal).ToList();
                    if (singleTarget.Count > 0)
                        return Task.FromResult<IAttackSource?>(singleTarget[_dice.RollIndex(singleTarget.Count)]);
                    return Task.FromResult<IAttackSource?>(healSpells[_dice.RollIndex(healSpells.Count)]);
                }
            }

            // Priority 2: Use damage spells when affordable
            if (dmgSpells.Count > 0)
                return Task.FromResult<IAttackSource?>(dmgSpells[_dice.RollIndex(dmgSpells.Count)]);
        }

        // Priority 3: Use weapon when no affordable spells remain
        if (weapon is not null)
            return Task.FromResult<IAttackSource?>(weapon);

        // Priority 4: Use default attack (may be null for spellcasters)
        if (defaultAttack is not null)
            return Task.FromResult<IAttackSource?>(defaultAttack);

        // Priority 5: Unarmed strike
        return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
    }
}
