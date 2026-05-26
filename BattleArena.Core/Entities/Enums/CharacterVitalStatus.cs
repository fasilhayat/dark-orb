namespace BattleArena.Core.Entities.Enums;

// Represents the vital state of a character based on their current HP.
//   Alive      = HP > 0         : fighting normally
//   KnockedOut = HP 0 to -9     : unconscious but not yet dead
//   Dead       = HP -10 or lower: permanently slain
public enum CharacterVitalStatus
{
    Alive,
    KnockedOut,
    Dead
}
