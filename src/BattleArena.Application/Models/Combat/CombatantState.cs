namespace BattleArena.Application.Models.Combat;

using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Tracks per-combatant state during a combat simulation run.
/// </summary>
internal class CombatantState
{
    public Character         Character         { get; }
    public IAttackSource?    AttackSource      { get; }
    public int               PartyIndex        { get; }   // 0 = hero party, 1 = enemy party
    public TurnmeterState    Meter             { get; set; }
    public int               PrevMeter         { get; set; }  // value before this tick's gain
    public QueuedSpellInfo?  QueuedSpell       { get; set; }
    public Character?        SummonedBy        { get; set; }
    public int               SummonExpiryRound { get; set; }
    public bool              IsSummoned        => SummonedBy is not null;
    /// <summary>Attacks remaining this turn for multi-attack support.</summary>
    public int               AttacksRemaining  { get; set; }

    /// <summary>
    /// Distance to the current target. Defaults to <see cref="EngagementRange.Melee"/>.
    /// Will be set by the distance system once position tracking is implemented,
    /// enabling ranged-at-distance bonuses and melee-out-of-reach penalties.
    /// </summary>
    public EngagementRange EngagementRange { get; set; } = EngagementRange.Melee;

    public CombatantState(Character character, IAttackSource? attackSource, int partyIndex)
    {
        Character    = character;
        AttackSource = attackSource;
        PartyIndex   = partyIndex;
        Meter        = new TurnmeterState { CharacterId = character.Id, CharacterName = character.Name };
    }

    // Called at the start of each tick before Tick() is applied.
    public void SnapshotMeter() => PrevMeter = Meter.CurrentValue;
}