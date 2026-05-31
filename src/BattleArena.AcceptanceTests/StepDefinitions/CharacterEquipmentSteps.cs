namespace BattleArena.ReqnrollTests.StepDefinitions;

using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

[Binding]
public class CharacterEquipmentSteps
{
    private Character _character = new();

    // ── Character setup ────────────────────────────────────────────────────────

    [Given(@"a character with strength (\d+) and dexterity (\d+)")]
    public void GivenACharacterWithStrengthAndDexterity(int strength, int dexterity)
    {
        _character = new Character
        {
            Strength = strength,
            Dexterity = dexterity
        };
    }

    [Given(@"the character wears ""([^""]+)"" with armor class (\d+) and mitigation (\d+)")]
    public void GivenTheCharacterWearsArmor(string armorName, int armorClass, int mitigation)
    {
        _character.Equipment.Chest = new Armor
        {
            Name = armorName,
            ArmorClass = armorClass,
            Mitigation = mitigation,
            MaxDexterityBonus = 10
        };
    }

    [Given(@"the character also wears a ""([^""]+)"" with armor class (\d+) and mitigation (\d+)")]
    public void GivenTheCharacterAlsoWearsArmor(string armorName, int armorClass, int mitigation)
    {
        AddArmorPiece(armorName, armorClass, mitigation);
    }

    [Given(@"the character also wears ""([^""]+)"" with armor class (\d+) and mitigation (\d+)")]
    public void GivenTheCharacterAlsoWearsArmorWithoutArticle(string armorName, int armorClass, int mitigation)
    {
        AddArmorPiece(armorName, armorClass, mitigation);
    }

    private void AddArmorPiece(string armorName, int armorClass, int mitigation)
    {
        if (_character.Equipment.Head is null)
        {
            _character.Equipment.Head = new Armor
            {
                Name = armorName,
                ArmorClass = armorClass,
                Mitigation = mitigation,
                MaxDexterityBonus = 10
            };
        }
        else if (_character.Equipment.Boots is null)
        {
            _character.Equipment.Boots = new Armor
            {
                Name = armorName,
                ArmorClass = armorClass,
                Mitigation = mitigation,
                MaxDexterityBonus = 10
            };
        }
    }

    [Given(@"the character wields a ""([^""]+)"" in their right hand dealing (\d+)d(\d+) (\w+) damage with attack bonus (\d+)")]
    public void GivenTheCharacterWieldsAWeapon(string weaponName, int dieCount, int dieSides, string damageTypeName, int attackBonus)
    {
        _character.Equipment.RightHand = new Weapon
        {
            Name = weaponName,
            DamageDie = ParseDieType(dieSides),
            DamageCount = dieCount,
            DamageType = ParseDamageType(damageTypeName),
            AttackType = AttackType.Melee,
            AttackBonus = attackBonus
        };
    }

    [Given(@"the character wears a shield with defense bonus (\d+)")]
    public void GivenTheCharacterWearsAShield(int defenseBonus)
    {
        _character.Equipment.Shield = new Shield
        {
            Name = "Shield",
            DefenseBonus = defenseBonus
        };
    }

    // ── Assertions ─────────────────────────────────────────────────────────────

    [Then(@"the character's unarmed strike should be named ""([^""]+)""")]
    public void ThenUnarmedStrikeShouldBeNamed(string expectedName)
    {
        Assert.Equal(expectedName, UnarmedStrike.Default.Name);
    }

    [Then(@"the unarmed strike should deal (\d+)d(\d+) (\w+) damage")]
    public void ThenUnarmedStrikeShouldDealDamage(int dieCount, int dieSides, string damageTypeName)
    {
        Assert.Equal(dieCount, UnarmedStrike.Default.DamageCount);
        Assert.Equal(ParseDieType(dieSides), UnarmedStrike.Default.DamageDie);
        Assert.Equal(ParseDamageType(damageTypeName), UnarmedStrike.Default.DamageType);
    }

    [Then(@"the unarmed strike should be a melee attack")]
    public void ThenUnarmedStrikeShouldBeMelee()
    {
        Assert.Equal(AttackType.Melee, UnarmedStrike.Default.AttackType);
    }

    [Then(@"the unarmed strike should have (\d+) attack bonus")]
    public void ThenUnarmedStrikeShouldHaveAttackBonus(int expected)
    {
        Assert.Equal(expected, UnarmedStrike.Default.AttackBonus);
    }

    [Then(@"the character's total armor class should be (\d+)")]
    public void ThenTotalArmorClassShouldBe(int expected)
    {
        Assert.Equal(expected, _character.Equipment.TotalArmorClass);
    }

    [Then(@"the character's total mitigation should be (\d+)")]
    public void ThenTotalMitigationShouldBe(int expected)
    {
        Assert.Equal(expected, _character.Equipment.TotalMitigation);
    }

    [Then(@"the character should have no attack source")]
    public void ThenCharacterShouldHaveNoAttackSource()
    {
        Assert.Null(_character.Equipment.RightHand);
    }

    [Then(@"the character should have an attack source named ""([^""]+)""")]
    public void ThenCharacterShouldHaveAttackSourceNamed(string expectedName)
    {
        Assert.NotNull(_character.Equipment.RightHand);
        Assert.Equal(expectedName, _character.Equipment.RightHand.Name);
    }

    [Then(@"the attack source should deal (\d+)d(\d+) damage")]
    public void ThenAttackSourceShouldDealDamage(int dieCount, int dieSides)
    {
        Assert.NotNull(_character.Equipment.RightHand);
        Assert.Equal(dieCount, _character.Equipment.RightHand.DamageCount);
        Assert.Equal(ParseDieType(dieSides), _character.Equipment.RightHand.DamageDie);
    }

    [Then(@"the character's shield defense bonus should be (\d+)")]
    public void ThenShieldDefenseBonusShouldBe(int expected)
    {
        Assert.NotNull(_character.Equipment.Shield);
        Assert.Equal(expected, _character.Equipment.Shield.DefenseBonus);
    }

    [Then(@"the character's right hand weapon should be ""([^""]+)""")]
    public void ThenRightHandWeaponShouldBe(string expectedName)
    {
        Assert.NotNull(_character.Equipment.RightHand);
        Assert.Equal(expectedName, _character.Equipment.RightHand.Name);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static DieType ParseDieType(int sides) => sides switch
    {
        4   => DieType.D4,
        6   => DieType.D6,
        8   => DieType.D8,
        10  => DieType.D10,
        12  => DieType.D12,
        20  => DieType.D20,
        100 => DieType.D100,
        _   => throw new ArgumentOutOfRangeException(nameof(sides), $"Unknown die size: d{sides}")
    };

    private static DamageType ParseDamageType(string name) => name switch
    {
        "Slashing"     => DamageType.Slashing,
        "Piercing"     => DamageType.Piercing,
        "Bludgeoning"  => DamageType.Bludgeoning,
        "Fire"         => DamageType.Fire,
        "Ice"          => DamageType.Ice,
        "Lightning"    => DamageType.Lightning,
        "Poison"       => DamageType.Poison,
        "Shadow"       => DamageType.Shadow,
        "Holy"         => DamageType.Holy,
        "Acid"         => DamageType.Acid,
        _              => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown damage type: {name}")
    };
}
