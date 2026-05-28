namespace BattleArena.UnitTests.Services;

using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

public class CharacterAttackResolverTests
{
    [Fact]
    public void Resolve_WeaponEquipped_ReturnsWeapon()
    {
        var weapon = new Weapon { Name = "Longsword", DamageDie = DieType.D8, DamageCount = 1 };
        var character = new Character
        {
            Equipment = new ArmorSlots { RightHand = weapon },
            MemorizedSpells = { new Spell { Name = "Fireball", AttackBonus = 5 } }
        };

        var result = CharacterAttackResolver.Resolve(character);

        Assert.Same(weapon, result);
    }

    [Fact]
    public void Resolve_NoWeaponHasSpells_ReturnsHighestAttackBonusSpell()
    {
        var best = new Spell { Name = "Fireball", AttackBonus = 5, DamageCount = 2 };
        var character = new Character
        {
            MemorizedSpells =
            {
                new Spell { Name = "Magic Missile", AttackBonus = 3, DamageCount = 1 },
                best,
                new Spell { Name = "Ray of Frost", AttackBonus = 4, DamageCount = 1 }
            }
        };

        var result = CharacterAttackResolver.Resolve(character);

        Assert.Same(best, result);
    }

    [Fact]
    public void Resolve_NoWeaponNoSpells_ReturnsUnarmedStrike()
    {
        var character = new Character();

        var result = CharacterAttackResolver.Resolve(character);

        Assert.Same(UnarmedStrike.Default, result);
    }
}
