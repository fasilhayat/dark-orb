namespace BattleArena.UnitTests.Services;

using Application.Models;

public class CombatResultTests
{
    [Fact]
    public void CombatId_NewInstance_IsNonEmptyGuid()
    {
        var result = new CombatResult();
        Assert.NotEqual(Guid.Empty, result.CombatId);
    }

    [Fact]
    public void CombatId_MultipleInstances_AreUnique()
    {
        var result1 = new CombatResult();
        var result2 = new CombatResult();
        Assert.NotEqual(result1.CombatId, result2.CombatId);
    }
}
