namespace BattleArena.Application.Models;

public class ActionEconomy
{
    public bool HasPrimaryAction { get; set; } = true;
    public bool HasBonusAction { get; set; } = true;
    public bool HasMovementPhase { get; set; } = true;

    public void UsePrimaryAction() => HasPrimaryAction = false;
    public void UseBonusAction() => HasBonusAction = false;
    public void UseMovementPhase() => HasMovementPhase = false;
    public bool HasAnyActionRemaining => HasPrimaryAction || HasBonusAction || HasMovementPhase;
}
