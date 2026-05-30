namespace BattleArena.UnitTests.Services;

using BattleArena.Application.Models;
using BattleArena.Presentation;

public class CombatDisplayStateTests
{
    private static CharDisplayState Hero(string name, int hp = 100, int mana = 50) =>
        new() { Name = name, MaxHp = hp, Hp = hp, MaxMana = mana, Mana = mana, IsHero = true, IsAlive = true };

    private static CharDisplayState Enemy(string name, int hp = 80, int mana = 0) =>
        new() { Name = name, MaxHp = hp, Hp = hp, MaxMana = mana, Mana = mana, IsHero = false, IsAlive = true };

    private static CombatLayout Layout(IEnumerable<string> heroes, IEnumerable<string> enemies) =>
        CombatLayout.From(heroes, enemies, false);

    [Fact]
    public void ApplyEvent_TurnMeterGain_UpdatesTm()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "TurnMeterGain", ActorName = "Alice", TurnMeterAfter = 42
        });

        Assert.Equal(42, state.TryGet("Alice")!.Tm);
    }

    [Fact]
    public void ApplyEvent_TurnMeterGain_UnknownActor_NoException()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "TurnMeterGain", ActorName = "NOBODY", TurnMeterAfter = 50
        });
    }

    [Fact]
    public void ApplyEvent_TurnStart_UpdatesWeapon()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "TurnStart", ActorName = "Alice", AttackSourceName = "Longsword"
        });

        Assert.Equal("Longsword", state.TryGet("Alice")!.Weapon);
    }

    [Fact]
    public void ApplyEvent_TurnStart_NullAttackSource_DoesNotClearWeapon()
    {
        var hero = Hero("Alice");
        hero.Weapon = "Dagger";
        var state = new CombatDisplayState([hero], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "TurnStart", ActorName = "Alice", AttackSourceName = null
        });

        Assert.Equal("Dagger", state.TryGet("Alice")!.Weapon);
    }

    [Fact]
    public void ApplyEvent_TurnEnd_UpdatesTm()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "TurnEnd", ActorName = "Alice", TurnMeterAfter = 15
        });

        Assert.Equal(15, state.TryGet("Alice")!.Tm);
    }

    [Fact]
    public void ApplyEvent_Damage_UpdatesHp()
    {
        var state = new CombatDisplayState([Hero("Alice", 100)], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "Damage", ActorName = "Alice", TargetHpAfter = 65
        });

        Assert.Equal(65, state.TryGet("Alice")!.Hp);
    }

    [Fact]
    public void ApplyEvent_Death_SetsIsAliveToFalse()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry { EventType = "Death", ActorName = "Alice" });

        Assert.False(state.TryGet("Alice")!.IsAlive);
    }

    [Fact]
    public void ApplyEvent_KnockedOut_SetsIsAliveToFalse()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry { EventType = "KnockedOut", ActorName = "Alice" });

        Assert.False(state.TryGet("Alice")!.IsAlive);
    }

    [Fact]
    public void ApplyEvent_ManaDeduct_UpdatesMana()
    {
        var state = new CombatDisplayState([Hero("Alice", mana: 100)], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "ManaDeduct", ActorName = "Alice", ManaAfter = 70
        });

        Assert.Equal(70, state.TryGet("Alice")!.Mana);
    }

    [Fact]
    public void ApplyEvent_ManaRegen_UpdatesMana()
    {
        var state = new CombatDisplayState([Hero("Alice", mana: 50)], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "ManaRegen", ActorName = "Alice", ManaAfter = 60
        });

        Assert.Equal(60, state.TryGet("Alice")!.Mana);
    }

    [Fact]
    public void EnsurePet_NewPet_AddsToAll()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));

        state.EnsurePet("Fluffy", 30, true);

        var pet = state.TryGet("Fluffy");
        Assert.NotNull(pet);
        Assert.Equal(30, pet!.Hp);
        Assert.True(pet.IsHero);
    }

    [Fact]
    public void EnsurePet_ExistingPet_DoesNotOverwrite()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));
        state.EnsurePet("Fluffy", 30, true);
        state.TryGet("Fluffy")!.Hp = 10;

        state.EnsurePet("Fluffy", 30, true);

        Assert.Equal(10, state.TryGet("Fluffy")!.Hp);
    }

    [Fact]
    public void ApplyEvent_PetSummoned_RevivesExistingPetAtFullHp()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));
        state.EnsurePet("Fluffy", 30, true);
        state.TryGet("Fluffy")!.Hp = 5;
        state.TryGet("Fluffy")!.IsAlive = false;

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "PetSummoned", ActorName = "Alice", SummonedPetName = "Fluffy"
        });

        Assert.Equal(30, state.TryGet("Fluffy")!.Hp);
        Assert.True(state.TryGet("Fluffy")!.IsAlive);
    }

    [Fact]
    public void ApplyEvent_PetExpired_SetsDeadAndZeroHp()
    {
        var state = new CombatDisplayState([Hero("Alice")], Layout(["Alice"], []));
        state.EnsurePet("Fluffy", 30, true);

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "PetExpired", ActorName = "Alice", SummonedPetName = "Fluffy"
        });

        Assert.Equal(0, state.TryGet("Fluffy")!.Hp);
        Assert.False(state.TryGet("Fluffy")!.IsAlive);
    }

    [Fact]
    public void ApplyEvent_DoTTick_UsesTargetHpAfterWhenPresent()
    {
        // Simulator now stamps TargetHpAfter — display must use it as source of truth
        var state = new CombatDisplayState([Hero("Alice", 100)], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "DoTTick", ActorName = "Alice", DamageDealt = 12, TargetHpAfter = 88
        });

        Assert.Equal(88, state.TryGet("Alice")!.Hp);
    }

    [Fact]
    public void ApplyEvent_DoTTick_FallsBackToCalculationWhenNoTargetHpAfter()
    {
        // Backward-compat: old log entries without TargetHpAfter still work
        var state = new CombatDisplayState([Hero("Alice", 100)], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "DoTTick", ActorName = "Alice", DamageDealt = 12
        });

        Assert.Equal(88, state.TryGet("Alice")!.Hp);
    }

    [Fact]
    public void ApplyEvent_DoTTick_HpFlooredAtMinusTen_WhenFallingBack()
    {
        var hero = Hero("Alice", 100);
        hero.Hp = 3;
        var state = new CombatDisplayState([hero], Layout(["Alice"], []));

        state.ApplyEvent(new CombatLogEntry
        {
            EventType = "DoTTick", ActorName = "Alice", DamageDealt = 20
        });

        Assert.Equal(-10, state.TryGet("Alice")!.Hp);
    }

    [Fact]
    public void ApplyEvent_UnknownEventType_DoesNotThrow()
    {
        var state = new CombatDisplayState([Hero("Alice"), Enemy("Goblin")], Layout(["Alice"], ["Goblin"]));

        state.ApplyEvent(new CombatLogEntry { EventType = "SomeNewEventType", ActorName = "Alice" });
    }
}
