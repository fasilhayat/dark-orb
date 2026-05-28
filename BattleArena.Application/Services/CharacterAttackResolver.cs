namespace BattleArena.Application.Services;

using Core.Entities;

public static class CharacterAttackResolver
{
    public static IAttackSource Resolve(Character character)
    {
        if (character.Equipment.RightHand is { } weapon)
            return weapon;
        if (character.MemorizedSpells.Count > 0)
            return character.MemorizedSpells
                .OrderByDescending(s => s.AttackBonus)
                .ThenByDescending(s => s.DamageCount)
                .First();
        return UnarmedStrike.Default;
    }
}
