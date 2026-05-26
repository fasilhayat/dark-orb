namespace BattleArena.Application.Models;

public class TurnmeterState
{
    public int CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public bool CanTakeTurn => CurrentValue >= 100;
    public bool HasDualAction => CurrentValue >= 200;
}
