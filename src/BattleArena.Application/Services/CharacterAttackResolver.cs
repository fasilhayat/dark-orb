namespace BattleArena.Application.Services;

using Core.Entities;

public static class CharacterAttackResolver
{
    public static IAttackSource Resolve(Character character)
    {
        if (character.Equipment.RightHand is { } weapon)
            return weapon;
        var castable = character.MemorizedSpells.Where(s => character.CanCast(s)).ToList();
        if (castable.Count > 0)
            return castable
                .OrderByDescending(s => s.AttackBonus)
                .ThenByDescending(s => s.DamageCount)
                .First();
        return UnarmedStrike.Default;
    }
}
