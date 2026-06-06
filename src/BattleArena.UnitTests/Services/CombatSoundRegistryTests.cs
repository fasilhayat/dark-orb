namespace BattleArena.UnitTests.Services;

using BattleArena.Presentation;

public class CombatSoundRegistryTests
{
    [Fact]
    public void AllKnownSoundIds_ContainsAllEffectSoundIds()
    {
        var all = CombatSoundRegistry.AllKnownSoundIds.ToList();
        Assert.Contains("BurnTick", all);
        Assert.Contains("PoisonTick", all);
        Assert.Contains("BleedTick", all);
        Assert.Contains("FrostTick", all);
        Assert.Contains("ShockTick", all);
    }

    [Fact]
    public void AllKnownSoundIds_ContainsAllEventSoundIds()
    {
        var all = CombatSoundRegistry.AllKnownSoundIds.ToList();
        Assert.Contains("PerfectParry", all);
        Assert.Contains("PerfectDodge", all);
        Assert.Contains("CounterAttack", all);
        Assert.Contains("CriticalHit", all);
        Assert.Contains("Fumble", all);
        Assert.Contains("KillingBlow", all);
        Assert.Contains("Resurrection", all);
    }

    [Fact]
    public void AllKnownSoundIds_ReturnsDistinctIds()
    {
        var all = CombatSoundRegistry.AllKnownSoundIds.ToList();
        Assert.Equal(all.Distinct().Count(), all.Count);
    }

    [Fact]
    public void GetEffectSoundId_KnownEffect_ReturnsExpectedId()
    {
        Assert.Equal("BurnTick", CombatSoundRegistry.GetEffectSoundId("Burning"));
        Assert.Equal("BurnTick", CombatSoundRegistry.GetEffectSoundId("Ignite"));
        Assert.Equal("PoisonTick", CombatSoundRegistry.GetEffectSoundId("Poisoned"));
        Assert.Equal("BleedTick", CombatSoundRegistry.GetEffectSoundId("Bleeding"));
        Assert.Equal("FrostTick", CombatSoundRegistry.GetEffectSoundId("Frozen"));
        Assert.Equal("FrostTick", CombatSoundRegistry.GetEffectSoundId("Freeze"));
        Assert.Equal("ShockTick", CombatSoundRegistry.GetEffectSoundId("Shocked"));
    }

    [Fact]
    public void GetEffectSoundId_UnknownEffect_ReturnsEmpty()
    {
        Assert.Empty(CombatSoundRegistry.GetEffectSoundId("UnknownEffect"));
        Assert.Empty(CombatSoundRegistry.GetEffectSoundId("Stun"));
        Assert.Empty(CombatSoundRegistry.GetEffectSoundId(""));
    }

    [Fact]
    public void GetEventSoundId_KnownEvent_ReturnsExpectedId()
    {
        Assert.Equal("PerfectParry", CombatSoundRegistry.GetEventSoundId("PerfectParry"));
        Assert.Equal("PerfectDodge", CombatSoundRegistry.GetEventSoundId("PerfectDodge"));
        Assert.Equal("CounterAttack", CombatSoundRegistry.GetEventSoundId("CounterAttack"));
        Assert.Equal("CriticalHit", CombatSoundRegistry.GetEventSoundId("DevastatingStrike"));
        Assert.Equal("Fumble", CombatSoundRegistry.GetEventSoundId("TotalReversal"));
        Assert.Equal("Fumble", CombatSoundRegistry.GetEventSoundId("FumblePenalty"));
        Assert.Equal("KillingBlow", CombatSoundRegistry.GetEventSoundId("KillingBlow"));
        Assert.Equal("KillingBlow", CombatSoundRegistry.GetEventSoundId("Death"));
        Assert.Equal("Resurrection", CombatSoundRegistry.GetEventSoundId("Resurrection"));
    }

    [Fact]
    public void GetEventSoundId_UnknownEvent_ReturnsEmpty()
    {
        Assert.Empty(CombatSoundRegistry.GetEventSoundId("UnknownEvent"));
        Assert.Empty(CombatSoundRegistry.GetEventSoundId("Attack"));
        Assert.Empty(CombatSoundRegistry.GetEventSoundId(""));
    }

    [Fact]
    public void GetCriticalHitSoundId_ReturnsCriticalHit()
    {
        Assert.Equal("CriticalHit", CombatSoundRegistry.GetCriticalHitSoundId());
    }
}
