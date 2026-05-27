namespace BattleArena.Application.Models;

public class TurnmeterState
{
    public int CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int CurrentValue { get; set; }

    // True when meter has reached the action threshold — character may act this tick.
    public bool IsReady => CurrentValue >= 100;

    // True while this character is currently resolving their turn.
    // Set by CombatSimulator at TurnStart, cleared at TurnEnd.
    public bool IsActive { get; set; }

    // Kept for compatibility with existing callers.
    public bool CanTakeTurn => IsReady;
    public bool HasDualAction => CurrentValue >= 200;
}
