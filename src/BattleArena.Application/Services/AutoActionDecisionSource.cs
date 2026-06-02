namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;

public class AutoActionDecisionSource : IActionDecisionSource
{
    private readonly IDiceService _dice;

    public AutoActionDecisionSource(IDiceService dice)
    {
        _dice = dice;
    }

    public Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct)
    {
        var spells = actor.MemorizedSpells;
        if (spells.Count > 0)
        {
            var healSpells = spells.Where(s => s.IsHealing).ToList();
            var dmgSpells = spells.Where(s => !s.IsHealing).ToList();

            var anyInjuredAlly = allies.Any(a => a.CurrentHitPoints < a.MaxHitPoints);
            var anyLowAlly = allies.Any(a => a.CurrentHitPoints <= a.MaxHitPoints / 2);

            List<Spell> candidates;
            if (!anyInjuredAlly && healSpells.Count > 0)
            {
                // Everyone at full HP — exclude healing spells
                candidates = dmgSpells.Count > 0 ? dmgSpells : healSpells;
            }
            else if (anyLowAlly && healSpells.Count > 0 && dmgSpells.Count > 0)
            {
                // Someone critically injured — prefer healing (70% chance)
                candidates = _dice.RollIndex(10) < 7 ? healSpells : dmgSpells;
            }
            else
            {
                // Mix of all affordable spells
                candidates = spells;
            }

            var affordable = candidates
                .Where(s => s.ManaCost <= 0 || actor.CurrentMana >= s.ManaCost)
                .ToList();

            if (affordable.Count > 0)
                return Task.FromResult<IAttackSource?>(affordable[_dice.RollIndex(affordable.Count)]);

            return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
        }

        if (defaultAttack is not null)
            return Task.FromResult<IAttackSource?>(defaultAttack);

        return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
    }
}
