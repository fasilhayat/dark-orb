namespace BattleArena.UnitTests.Services;

using Core.Entities;
using Core.Entities.Enums;
using Xunit;

// Tests for the HP-based vital status system.
//   HP > 0      → Alive
//   HP 0 to -9  → KnockedOut (unconscious but not dead)
//   HP -10 or lower → Dead
public class CharacterVitalStatusTests
{
    // ── IsAlive ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public void IsAlive_WhenHpAboveZero_ReturnsTrue(int hp)
    {
        var ch = new Character { MaxHitPoints = 50, CurrentHitPoints = hp };
        Assert.True(ch.IsAlive);
        Assert.False(ch.IsKnockedOut);
        Assert.False(ch.IsDead);
        Assert.Equal(CharacterVitalStatus.Alive, ch.VitalStatus);
    }

    // ── KnockedOut range: 0 to -9 ─────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-9)]
    public void IsKnockedOut_WhenHpZeroToMinusNine_ReturnsTrue(int hp)
    {
        var ch = new Character { MaxHitPoints = 50, CurrentHitPoints = hp };
        Assert.False(ch.IsAlive);
        Assert.True(ch.IsKnockedOut);
        Assert.False(ch.IsDead);
        Assert.Equal(CharacterVitalStatus.KnockedOut, ch.VitalStatus);
    }

    // ── Dead threshold: -10 or lower ──────────────────────────────────────────

    [Theory]
    [InlineData(-10)]
    [InlineData(-11)]
    [InlineData(-25)]
    [InlineData(-100)]
    public void IsDead_WhenHpMinusTenOrLower_ReturnsTrue(int hp)
    {
        var ch = new Character { MaxHitPoints = 50, CurrentHitPoints = hp };
        Assert.False(ch.IsAlive);
        Assert.False(ch.IsKnockedOut);
        Assert.True(ch.IsDead);
        Assert.Equal(CharacterVitalStatus.Dead, ch.VitalStatus);
    }

    // ── Boundary: -9 is KO, -10 is Dead ──────────────────────────────────────

    [Fact]
    public void AtMinusNine_IsKnockedOut_NotDead()
    {
        var ch = new Character { MaxHitPoints = 20, CurrentHitPoints = -9 };
        Assert.Equal(CharacterVitalStatus.KnockedOut, ch.VitalStatus);
    }

    [Fact]
    public void AtMinusTen_IsDead_NotKnockedOut()
    {
        var ch = new Character { MaxHitPoints = 20, CurrentHitPoints = -10 };
        Assert.Equal(CharacterVitalStatus.Dead, ch.VitalStatus);
    }
}
