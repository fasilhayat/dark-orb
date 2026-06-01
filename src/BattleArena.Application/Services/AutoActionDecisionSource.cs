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
        int currentTick,
        CancellationToken ct)
    {
        var spells = actor.MemorizedSpells;
        if (spells.Count > 0)
        {
            var spell = spells[_dice.RollIndex(spells.Count)];
            if (spell.ManaCost <= 0 || actor.CurrentMana >= spell.ManaCost)
                return Task.FromResult<IAttackSource?>(spell);
            return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
        }

        if (defaultAttack is not null)
            return Task.FromResult<IAttackSource?>(defaultAttack);

        return Task.FromResult<IAttackSource?>(UnarmedStrike.Default);
    }
}
