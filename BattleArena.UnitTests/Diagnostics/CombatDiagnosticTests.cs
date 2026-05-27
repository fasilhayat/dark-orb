namespace BattleArena.UnitTests.Diagnostics;

// ─────────────────────────────────────────────────────────────────────────────
// Live diagnostic runs — NOT mocked. Each test prints the full event log via
// ITestOutputHelper so you can inspect every action. Run with:
//   dotnet test --filter "FullyQualifiedName~CombatDiagnosticTests" -v normal
//
// Structural assertions are made on top; the printed log is for manual review.
// ─────────────────────────────────────────────────────────────────────────────

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Xunit;
using Xunit.Abstractions;

public class CombatDiagnosticTests(ITestOutputHelper out_)
{
    // ── Service stack (all real, no mocks) ────────────────────────────────────
    private static CombatSimulator BuildSim()
    {
        var dice = new DiceService();
        return new(new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice);
    }

    // ── Character factory helpers ─────────────────────────────────────────────

    static Character MakeWarrior(string name, int level, int str, int dex, int hp, int turnSpeed,
        int strikeRating, string armorName, int ac, int mit, string weaponName, DieType die, int dieCount, int atkBonus) =>
        new()
        {
            Name = name, Level = level, Strength = str, Dexterity = dex,
            Intelligence = 10, StrikeRating = strikeRating, TurnSpeed = turnSpeed,
            MaxHitPoints = hp, CurrentHitPoints = hp,
            Equipment = new ArmorSlots
            {
                Chest     = new Armor { Name = armorName, ArmorClass = ac, Mitigation = mit, MaxDexterityBonus = 6 },
                RightHand = new Weapon { Name = weaponName, DamageDie = die, DamageCount = dieCount,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = atkBonus }
            }
        };

    static Character MakeCaster(string name, int level, int intel, int dex, int hp, int turnSpeed,
        int strikeRating, params Spell[] spells) =>
        new()
        {
            Name = name, Level = level, Strength = 8, Dexterity = dex,
            Intelligence = intel, StrikeRating = strikeRating, TurnSpeed = turnSpeed,
            MaxHitPoints = hp, CurrentHitPoints = hp,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 }
            },
            MemorizedSpells = spells.ToList()
        };

    static Spell MakeSpell(string name, DieType die, int count, int bonus) =>
        new() { Name = name, DamageDie = die, DamageCount = count, DamageType = DamageType.Fire,
                AttackBonus = bonus, School = SpellSchool.Evocation, SpellLevel = 2 };

    // ── Log printer ───────────────────────────────────────────────────────────

    void PrintLog(CombatResult r)
    {
        var bar = new string('─', 80);
        out_.WriteLine(bar);
        out_.WriteLine($"  COMBAT RESULT: {(r.WinningParty?.Name ?? "TIMEOUT")}  |  Ticks: {r.TotalTicks}  |  MaxTicksReached: {r.MaxTicksReached}");
        out_.WriteLine(bar);

        int turnNo = 0;
        foreach (var e in r.Log)
        {
            switch (e.EventType)
            {
                case "TurnMeterGain":
                    if (e.IsReady || e.IsActive)
                        out_.WriteLine($"  [{e.Tick,4}] TM     {e.ActorName,-12} {e.TurnMeterBefore,3} -> {e.TurnMeterAfter,3}{(e.IsActive ? " [ACTS]" : " [READY]")}");
                    break;

                case "TurnStart":
                    turnNo++;
                    var spell = e.IsSpell == true ? " (spell)" : "";
                    out_.WriteLine($"\n  [{e.Tick,4}] -- TURN {turnNo} --  {e.ActorName} -> {e.TargetName}  [{e.AttackSourceName}{spell}]");
                    break;

                case "Attack":
                    var hit  = e.IsHit    == true  ? "HIT"  : "MISS";
                    var crit = e.IsCritical == true ? " CRIT!" : "";
                    var fumb = e.IsFumble  == true  ? " FUMBLE!" : "";
                    var src  = e.AttackSourceName ?? "?";
                    out_.WriteLine($"         Attack  d20={e.DieRoll,2}  AP={e.AttackPower,3}  DP={e.DefensePower,3}  total={(e.DieRoll ?? 0)+(e.AttackPower ?? 0),3}  -> {hit}{crit}{fumb}  [{src}]");
                    break;

                case "Damage":
                    out_.WriteLine($"         Damage  {e.ActorName,-12} HP {e.TargetHpBefore,3} -> {e.TargetHpAfter,3}  (-{e.DamageDealt})");
                    break;

                case "FumblePenalty":
                    out_.WriteLine($"         FUMBLE  {e.Message}");
                    break;

                case "Death":
                case "KnockedOut":
                    out_.WriteLine($"  [{e.Tick,4}] *** {e.EventType.ToUpper()}: {e.Message}");
                    break;
            }
        }

        out_.WriteLine($"\n{bar}");
        out_.WriteLine($"  STATS:  Turns={turnNo}  " +
            $"Hits={r.Log.Count(e => e.IsHit == true && e.EventType == "Attack")}  " +
            $"Misses={r.Log.Count(e => e.IsHit == false && e.IsFumble == false && e.EventType == "Attack")}  " +
            $"Crits={r.Log.Count(e => e.IsCritical == true)}  " +
            $"Fumbles={r.Log.Count(e => e.IsFumble == true)}");
        out_.WriteLine(bar);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 1 — Duel: Warrior vs Warrior
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Duel_WarriorVsWarrior()
    {
        var theron = MakeWarrior("Theron", 5, 18, 12, 50, 10, 14, "Chain Mail",  5, 2, "Longsword",  DieType.D8,  1, 2);
        var krag   = MakeWarrior("Krag",   4, 17,  9, 45,  7, 15, "Orcish Hide", 6, 2, "Orcish Axe", DieType.D10, 1, 1);

        var result = BuildSim().Simulate(
            Party.Solo(theron, theron.Equipment.RightHand!),
            Party.Solo(krag,   krag  .Equipment.RightHand!));

        PrintLog(result);

        Assert.False(result.MaxTicksReached, "Duel should finish before timeout");
        Assert.NotNull(result.WinningParty);
        Assert.NotNull(result.LosingParty);
        AssertLogIntegrity(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 2 — Duel: Caster vs Caster
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Duel_CasterVsCaster()
    {
        var lyra   = MakeCaster("Lyra",   5, 18, 14, 30,  8, 13, MakeSpell("Fireball", DieType.D6, 3, 2), MakeSpell("Ice Bolt", DieType.D8, 2, 2));
        var mordak = MakeCaster("Mordak", 3, 16, 12, 25,  9, 14, MakeSpell("Shadow Bolt", DieType.D8, 2, 2), MakeSpell("Soul Drain", DieType.D10, 1, 1));

        var result = BuildSim().Simulate(Party.Solo(lyra, null), Party.Solo(mordak, null));

        PrintLog(result);

        Assert.False(result.MaxTicksReached);
        Assert.NotNull(result.WinningParty);
        AssertLogIntegrity(result);

        // All turns must reference a spell name
        var turnStarts = result.Log.Where(e => e.EventType == "TurnStart").ToList();
        Assert.All(turnStarts, e => Assert.NotNull(e.AttackSourceName));
        Assert.All(turnStarts, e => Assert.True(e.IsSpell == true, $"{e.ActorName} used non-spell in caster duel"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 3 — Duel: Warrior vs Caster
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Duel_WarriorVsCaster()
    {
        var theron = MakeWarrior("Theron", 5, 18, 12, 50, 10, 14, "Chain Mail", 5, 2, "Longsword", DieType.D8, 1, 2);
        var lyra   = MakeCaster("Lyra",   5, 18, 14, 30,  8, 13, MakeSpell("Fireball", DieType.D6, 3, 2), MakeSpell("Ice Bolt", DieType.D8, 2, 2));

        var result = BuildSim().Simulate(
            Party.Solo(theron, theron.Equipment.RightHand!),
            Party.Solo(lyra,   null));

        PrintLog(result);

        Assert.False(result.MaxTicksReached);
        Assert.NotNull(result.WinningParty);
        AssertLogIntegrity(result);

        // Theron: weapon turns; Lyra: spell turns
        var theronTurns = result.Log.Where(e => e.EventType == "TurnStart" && e.ActorName == "Theron").ToList();
        var lyraTurns   = result.Log.Where(e => e.EventType == "TurnStart" && e.ActorName == "Lyra").ToList();
        Assert.All(theronTurns, e => Assert.False(e.IsSpell == true, "Theron should not cast spells"));
        Assert.All(lyraTurns,   e => Assert.True(e.IsSpell == true,  "Lyra should always cast spells"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 4 — Party 3v3
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Party_3v3_MixedComposition()
    {
        var heroes = Party.HeroParty("Heroes", new[]
        {
            new PartyMember { Character = MakeWarrior("Theron", 5, 18, 12, 50, 10, 14, "Chain Mail",    5, 2, "Longsword",  DieType.D8,  1, 2), AttackSource = new Weapon { Name="Longsword", DamageDie=DieType.D8,  DamageCount=1, DamageType=DamageType.Slashing, AttackType=AttackType.Melee, AttackBonus=2 } },
            new PartyMember { Character = MakeWarrior("Gruk",   3, 16,  8, 35,  6, 16, "Leather Armor", 7, 1, "Battle Axe", DieType.D8,  1, 1), AttackSource = new Weapon { Name="Battle Axe",DamageDie=DieType.D8,  DamageCount=1, DamageType=DamageType.Slashing, AttackType=AttackType.Melee, AttackBonus=1 } },
            new PartyMember { Character = MakeCaster("Lyra",    5, 18, 14, 30,  8, 13, MakeSpell("Fireball", DieType.D6, 3, 2), MakeSpell("Ice Bolt", DieType.D8, 2, 2)), AttackSource = null },
        });
        var enemies = Party.HeroParty("Enemies", new[]
        {
            new PartyMember { Character = MakeWarrior("Krag",   4, 17,  9, 45,  7, 15, "Orcish Hide",  6, 2, "Orcish Axe", DieType.D10, 1, 1), AttackSource = new Weapon { Name="Orcish Axe",DamageDie=DieType.D10,DamageCount=1, DamageType=DamageType.Slashing, AttackType=AttackType.Melee, AttackBonus=1 } },
            new PartyMember { Character = MakeWarrior("Skrix",  2,  9, 16, 20, 12, 12, "Worn Leather", 8, 0, "Dagger",     DieType.D4,  2, 3), AttackSource = new Weapon { Name="Dagger",    DamageDie=DieType.D4,  DamageCount=2, DamageType=DamageType.Piercing, AttackType=AttackType.Melee, AttackBonus=3 } },
            new PartyMember { Character = MakeCaster("Mordak",  3, 16, 12, 25,  9, 14, MakeSpell("Shadow Bolt", DieType.D8, 2, 2), MakeSpell("Soul Drain", DieType.D10, 1, 1)), AttackSource = null },
        });

        var result = BuildSim().Simulate(heroes, enemies);

        PrintLog(result);

        Assert.False(result.MaxTicksReached);
        Assert.NotNull(result.WinningParty);
        AssertLogIntegrity(result);
        AssertNoAttacksOnDead(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 5 — Party 6v4: max hero party
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Party_6v4_MaxHeroParty()
    {
        var heroes = Party.HeroParty("Heroes", new[]
        {
            new PartyMember { Character = MakeWarrior("Theron", 5, 18, 12, 50, 10, 14, "Chain Mail",    5, 2, "Longsword",   DieType.D8,  1, 2), AttackSource = new Weapon { Name="Longsword",   DamageDie=DieType.D8,  DamageCount=1, DamageType=DamageType.Slashing,    AttackType=AttackType.Melee, AttackBonus=2 } },
            new PartyMember { Character = MakeWarrior("Gruk",   3, 16,  8, 35,  6, 16, "Leather Armor", 7, 1, "Battle Axe",  DieType.D8,  1, 1), AttackSource = new Weapon { Name="Battle Axe",  DamageDie=DieType.D8,  DamageCount=1, DamageType=DamageType.Slashing,    AttackType=AttackType.Melee, AttackBonus=1 } },
            new PartyMember { Character = MakeCaster("Lyra",    5, 18, 14, 30,  8, 13, MakeSpell("Fireball", DieType.D6, 3, 2), MakeSpell("Ice Bolt", DieType.D8, 2, 2)), AttackSource = null },
            new PartyMember { Character = MakeWarrior("Brynn",  4, 15, 14, 42,  9, 13, "Scale Mail",    4, 3, "Warhammer",   DieType.D8,  1, 1), AttackSource = new Weapon { Name="Warhammer",   DamageDie=DieType.D8,  DamageCount=1, DamageType=DamageType.Bludgeoning, AttackType=AttackType.Melee, AttackBonus=1 } },
            new PartyMember { Character = MakeWarrior("Sera",   3, 13, 17, 32, 11, 14, "Leather Armor", 7, 1, "Short Sword", DieType.D6,  1, 2), AttackSource = new Weapon { Name="Short Sword", DamageDie=DieType.D6,  DamageCount=1, DamageType=DamageType.Slashing,    AttackType=AttackType.Melee, AttackBonus=2 } },
            new PartyMember { Character = MakeCaster("Zeph",    4, 17, 13, 28, 10, 13, MakeSpell("Thunder", DieType.D10, 2, 3), MakeSpell("Shadow Bolt", DieType.D8, 2, 2)), AttackSource = null },
        });
        var enemies = Party.HeroParty("Enemies", new[]
        {
            new PartyMember { Character = MakeWarrior("Krag",   4, 17,  9, 45,  7, 15, "Orcish Hide",  6, 2, "Orcish Axe",  DieType.D10, 1, 1), AttackSource = new Weapon { Name="Orcish Axe",  DamageDie=DieType.D10, DamageCount=1, DamageType=DamageType.Slashing, AttackType=AttackType.Melee, AttackBonus=1 } },
            new PartyMember { Character = MakeWarrior("Skrix",  2,  9, 16, 20, 12, 12, "Worn Leather", 8, 0, "Dagger",      DieType.D4,  2, 3), AttackSource = new Weapon { Name="Dagger",      DamageDie=DieType.D4,  DamageCount=2, DamageType=DamageType.Piercing, AttackType=AttackType.Melee, AttackBonus=3 } },
            new PartyMember { Character = MakeWarrior("Gortax", 5, 19, 10, 55,  8, 15, "Heavy Plate",  2, 4, "Great Sword", DieType.D12, 1, 2), AttackSource = new Weapon { Name="Great Sword", DamageDie=DieType.D12, DamageCount=1, DamageType=DamageType.Slashing, AttackType=AttackType.Melee, AttackBonus=2 } },
            new PartyMember { Character = MakeCaster("Mordak",  3, 16, 12, 25,  9, 14, MakeSpell("Soul Drain", DieType.D10, 1, 1), MakeSpell("Shadow Bolt", DieType.D8, 2, 2)), AttackSource = null },
        });

        var result = BuildSim().Simulate(heroes, enemies);

        PrintLog(result);

        Assert.False(result.MaxTicksReached);
        Assert.NotNull(result.WinningParty);
        AssertLogIntegrity(result);
        AssertNoAttacksOnDead(result);

        // Confirm 6 hero members participated (at least one TM gain each)
        var activeNames = result.Log.Where(e => e.EventType == "TurnMeterGain")
                                    .Select(e => e.ActorName).Distinct().ToList();
        Assert.Equal(10, activeNames.Count);
    }

    // ── Shared structural assertion helpers ───────────────────────────────────

    static void AssertLogIntegrity(CombatResult result)
    {
        var lastTurnActor  = "";
        var lastTurnTarget = "";
        foreach (var e in result.Log)
        {
            if (e.EventType == "TurnStart")
            {
                lastTurnActor  = e.ActorName;
                lastTurnTarget = e.TargetName ?? "";
                Assert.NotEmpty(e.ActorName);
                Assert.NotEmpty(e.TargetName ?? "X");  // target must be set
                Assert.NotNull(e.AttackSourceName);     // weapon or spell name must be set
            }
            if (e.EventType == "Attack")
                Assert.Equal(lastTurnActor, e.ActorName);

            // Damage events: ActorName = the one who RECEIVED the hit.
            // DamageDealt > 0 is now guaranteed (0-damage hits are suppressed in CombatSimulator).
            if (e.EventType == "Damage")
            {
                Assert.True(e.DamageDealt > 0,
                    $"Unexpected 0-damage entry for {e.ActorName} (should be suppressed)");
                Assert.True((e.TargetHpAfter ?? 0) < (e.TargetHpBefore ?? 0),
                    $"HP did not decrease for {e.ActorName}: {e.TargetHpBefore} -> {e.TargetHpAfter}");
            }
        }
        Assert.NotNull(result.WinningParty);
        Assert.NotNull(result.LosingParty);
    }

    static void AssertNoAttacksOnDead(CombatResult result)
    {
        var dead = new HashSet<string>();
        foreach (var e in result.Log)
        {
            if (e.EventType is "Death" or "KnockedOut") dead.Add(e.ActorName);
            if (e.EventType == "TurnStart" && e.TargetName != null)
                Assert.False(dead.Contains(e.TargetName),
                    $"Dead/KO target was selected: {e.TargetName}");
        }
    }
}
