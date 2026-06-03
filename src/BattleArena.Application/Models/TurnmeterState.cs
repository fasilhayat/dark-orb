namespace BattleArena.Application.Models;

public class TurnmeterState
{
    /// <summary>Base threshold for one full turn — 100 = 100 %. All spell TM costs are relative to this.</summary>
    public const int TurnThreshold = 100;

    /// <summary>A character with this much TM has a dual action available.</summary>
    public const int DualActionThreshold = 200;

    public int CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int CurrentValue { get; set; }

    public bool IsReady => CurrentValue >= TurnThreshold;
    public bool IsActive { get; set; }
    public bool CanTakeTurn => IsReady;
    public bool HasDualAction => CurrentValue >= DualActionThreshold;
}
