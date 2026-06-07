namespace BattleArena.Application.Models.Combat;

using Core.Entities;

/// <summary>
/// Tracks a spell being charged over multiple ticks during combat.
/// </summary>
public class QueuedSpellInfo
{
    public Spell     Spell         { get; }
    public Character Target        { get; set; }
    public int       RemainingCost { get; set; }

    public QueuedSpellInfo(Spell spell, Character target, int remainingCost)
    {
        Spell         = spell;
        Target        = target;
        RemainingCost = remainingCost;
    }
}