namespace BattleArena.Gui.Services;

using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

public sealed class ForcedSpellDecisionSource : IActionDecisionSource
{
    private readonly Spell _spell;
    private bool _returned;

    public ForcedSpellDecisionSource(Spell spell)
    {
        _spell = spell;
    }

    public Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct,
        EngagementRange engagementRange = EngagementRange.Melee)
    {
        if (_returned)
        {
            return Task.FromResult(defaultAttack);
        }

        _returned = true;
        return Task.FromResult<IAttackSource?>(_spell);
    }
}
