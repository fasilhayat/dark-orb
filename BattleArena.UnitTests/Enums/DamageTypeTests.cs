namespace BattleArena.UnitTests.Enums;

using Core.Entities.Enums;

public class DamageTypeTests
{
    /// <summary>
    /// Every damage type in the seed data (arena_data.damage_type) must exist
    /// in the C# <see cref="DamageType"/> enum. If this test fails, add the
    /// missing value to <c>Core/Entities/Enums/DamageType.cs</c>.
    /// </summary>
    public static TheoryData<string> SeedDamageTypes => new()
    {
        "Bludgeoning",
        "Piercing",
        "Slashing",
        "Poison",
        "Fire",
        "Ice",
        "Lightning",
        "Shadow",
        "Holy",
        "Acid",
        "Psychic"
    };

    [Theory]
    [MemberData(nameof(SeedDamageTypes))]
    public void Parse_SeedDamageType_DoesNotThrow(string damageTypeName)
    {
        var parsed = Enum.Parse<DamageType>(damageTypeName);

        Assert.Equal(damageTypeName, parsed.ToString());
    }
}
