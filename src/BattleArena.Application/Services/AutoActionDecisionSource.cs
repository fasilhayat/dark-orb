namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;

public class AutoActionDecisionSource : IActionDecisionSource
{
    private const double HealThreshold = 0.7;

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

    public Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct)
    {
        var spells = actor.MemorizedSpells.Where(s => actor.CanCast(s) && IsUsefulLeech(s, actor)).ToList();

        if (spells.Count > 0)
        {
            var healSpells = spells
                .Where(s => s.IsHealing && (s.ManaCost <= 0 || actor.CurrentMana >= s.ManaCost))
                .ToList();

            var dmgSpells = spells
                .Where(s => !s.IsHealing && (s.ManaCost <= 0 || actor.CurrentMana >= s.ManaCost))
                .ToList();

            // Priority 1: Heal only if an ally is below the HP threshold
            if (healSpells.Count > 0)
            {
                var anyNeedHeal = allies.Any(a => (double)a.CurrentHitPoints / a.MaxHitPoints < HealThreshold);
                if (anyNeedHeal)
                    return Task.FromResult<IAttackSource?>(healSpells[_dice.RollIndex(healSpells.Count)]);
            }

            // Priority 2: Use damage spells on enemies
            if (dmgSpells.Count > 0)
                return Task.FromResult<IAttackSource?>(dmgSpells[_dice.RollIndex(dmgSpells.Count)]);
        }

        // Priority 3: Use default weapon
        if (defaultAttack is not null)
            return Task.FromResult<IAttackSource?>(defaultAttack);

        // Priority 3b: Check equipped weapon directly (handles case where
        // PartyMember.AttackSource is null for spellcasters — see Demo.GetAttackSource)
        if (actor.Equipment.RightHand is { } weapon)
            return Task.FromResult<IAttackSource?>(weapon);

        // Priority 4: Unarmed strike
        return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
    }
}
