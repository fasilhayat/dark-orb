namespace BattleArena.Core.Entities;

// Pairs a character with the attack source they bring into combat.
// AttackSource may be null for spellcasters (CombatSimulator picks from MemorizedSpells).
public class PartyMember
{
    public Character Character { get; set; } = null!;
    public IAttackSource? AttackSource { get; set; }
}
