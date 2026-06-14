namespace BattleArena.UnitTests.Diagnostics;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Xunit;
using Xunit.Abstractions;

public class CombatBenchmarkTests(ITestOutputHelper out_)
{
    private const int Trials = 1000;

    // ── Service stack (all real, no mocks) ────────────────────────────────────
    private static CombatSimulator BuildSim()
    {
        var dice = new DiceService();
        return new(new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice);
    }

    private static readonly string LogPath =
        Path.Combine(AppContext.BaseDirectory, "benchmark-results.txt");

    // ── Spell factory ─────────────────────────────────────────────────────────
    private static void ResetCombatant(Character c)
    {
        c.CurrentHitPoints = c.MaxHitPoints;
        c.CurrentMana = c.MaxMana;
        c.ActiveStatusEffects.Clear();
        c.RemainingCasts = c.MaxCastsPerCombat;
    }

    private static Spell MakeSpell(string name, DieType die, int count, int bonus,
        DamageType dmgType, SpellSchool school = SpellSchool.Stormcraft,
        int manaCost = 0, int spellLevel = 1, List<StatusEffect>? onHit = null)
    {
        var s = new Spell
        {
            Name = name, DamageDie = die, DamageCount = count,
            AttackBonus = bonus, DamageType = dmgType, School = school,
            ManaCost = manaCost, SpellLevel = spellLevel, AttackType = AttackType.Spell,
        };
        if (onHit is not null)
            s.OnHitEffects.AddRange(onHit);
        return s;
    }

    // ── Roster characters ─────────────────────────────────────────────────────

    // High Priestess Luna — lvl 14 Priest
    private static Character MakeLuna()
    {
        var luna = new Character
        {
            Name = "High Priestess Luna", Level = 14,
            Strength = 12, Dexterity = 10, Stamina = 16,
            Intelligence = 14, Wisdom = 20, Charisma = 18,
            ClassName = "Priest", StrikeRating = 18, TurnSpeed = 6,
            MaxHitPoints = 80, CurrentHitPoints = 80,
            MaxMana = 120, CurrentMana = 120, RemainingCasts = 20,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Plate Armor", ArmorClass = 18, Mitigation = 5 },
                RightHand = new Weapon
                {
                    Name = "Great Mace", DamageDie = DieType.D8, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 2
                },
            },
            MemorizedSpells =
            [
                MakeSpell("Smite", DieType.D8, 2, 2, DamageType.Holy, SpellSchool.Deity, 35, 2),
                MakeSpell("Flame Strike", DieType.D8, 3, 3, DamageType.Fire, SpellSchool.Deity, 30, 5),
                MakeSpell("Heal", DieType.D8, 2, 0, DamageType.Healing, SpellSchool.Deity, 25, 6),
                MakeSpell("Mass Heal", DieType.D6, 3, 0, DamageType.Healing, SpellSchool.Deity, 50, 4),
                MakeSpell("Bless", DieType.D4, 0, 0, DamageType.Holy, SpellSchool.Deity, 10, 1),
                MakeSpell("Chasten", DieType.D4, 1, 0, DamageType.Holy, SpellSchool.Deity, 10, 1),
                MakeSpell("Holy Nova", DieType.D8, 3, 3, DamageType.Holy, SpellSchool.Deity, 55, 5),
            ],
        };
        luna.CurrentHitPoints = luna.MaxHitPoints;
        luna.CurrentMana = luna.MaxMana;
        return luna;
    }

    // Vaelith Moonveil — lvl 9 Fighter
    private static Character MakeVaelith()
    {
        var vaelith = new Character
        {
            Name = "Vaelith Moonveil", Level = 9,
            Strength = 16, Dexterity = 18, Stamina = 14,
            Intelligence = 10, Wisdom = 12, Charisma = 13,
            ClassName = "Fighter", StrikeRating = 17, TurnSpeed = 10,
            MaxHitPoints = 68, CurrentHitPoints = 68,
            MaxMana = 0, CurrentMana = 0,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Mithril Chain", ArmorClass = 14, Mitigation = 2,
                    MaxDexterityBonus = 6, Resistances = [new ResistanceBonus(ResistanceType.Magic, 5)] },
                RightHand = new Weapon
                {
                    Name = "Long Sword", DamageDie = DieType.D8, DamageCount = 1,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2
                },
            },
        };
        vaelith.CurrentHitPoints = vaelith.MaxHitPoints;
        return vaelith;
    }

    // Target Golem — lvl 14 Fighter with spells
    private static Character MakeGolem()
    {
        var golem = new Character
        {
            Name = "Target Golem", Level = 14,
            Strength = 18, Dexterity = 10, Stamina = 18,
            Intelligence = 16, Wisdom = 10, Charisma = 8,
            ClassName = "Fighter", StrikeRating = 16, TurnSpeed = 6,
            MaxHitPoints = 400, CurrentHitPoints = 400,
            MaxMana = 150, CurrentMana = 150, RemainingCasts = 20,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Plate Armor", ArmorClass = 18, Mitigation = 5 },
                RightHand = new Weapon
                {
                    Name = "Long Sword", DamageDie = DieType.D8, DamageCount = 1,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2
                },
            },
            MemorizedSpells =
            [
                MakeSpell("Fireball", DieType.D6, 3, 2, DamageType.Fire, SpellSchool.Stormcraft, 50, 3,
                    onHit: [new() { Name = "Burning", Type = StatusEffectType.DamageOverTime, Duration = 2, ApplicationChance = 60, DamagePerTurn = 4, ResistanceType = ResistanceType.Fire }]),
                MakeSpell("Ice Bolt", DieType.D8, 2, 2, DamageType.Ice, SpellSchool.Stormcraft, 35, 2,
                    onHit: [new() { Name = "Frozen", Type = StatusEffectType.Stun, Duration = 2, ApplicationChance = 70, ResistanceType = ResistanceType.Cold }]),
                MakeSpell("Shock", DieType.D6, 2, 2, DamageType.Lightning, SpellSchool.Stormcraft, 20, 2,
                    onHit: [new() { Name = "Shocked", Type = StatusEffectType.Shock, Duration = 1, ApplicationChance = 50, ResistanceType = ResistanceType.Magic }]),
                MakeSpell("Static Shock", DieType.D6, 1, 2, DamageType.Lightning, SpellSchool.Stormcraft, 30, 2,
                    onHit: [new() { Name = "Electrified", Type = StatusEffectType.Shock, Duration = 2, ApplicationChance = 100, ResistanceType = ResistanceType.Magic }]),
                MakeSpell("Smite", DieType.D8, 2, 2, DamageType.Holy, SpellSchool.Deity, 35, 2),
                MakeSpell("Heal", DieType.D8, 2, 0, DamageType.Healing, SpellSchool.Deity, 25, 6),
                MakeSpell("Mass Heal", DieType.D6, 3, 0, DamageType.Healing, SpellSchool.Deity, 50, 4),
                MakeSpell("Mind Siphon", DieType.D4, 1, 0, DamageType.Shadow, SpellSchool.Umbramancy, 30, 3,
                    onHit: [new() { Name = "Leech", Type = StatusEffectType.Leech, Duration = 3, ApplicationChance = 80, LeechPerTurn = 6, LeechResourceType = "Mana", ResistanceType = ResistanceType.Shadow }]),
            ],
        };
        golem.CurrentHitPoints = golem.MaxHitPoints;
        golem.CurrentMana = golem.MaxMana;
        return golem;
    }

    // Practice Dummy — lvl 10, no armor, no spells, high HP
    private static Character MakeDummy()
    {
        var dummy = new Character
        {
            Name = "Practice Dummy", Level = 10,
            Strength = 1, Dexterity = 1, Stamina = 1,
            Intelligence = 1, Wisdom = 1, Charisma = 1,
            ClassName = "Fighter", StrikeRating = 1, TurnSpeed = 4,
            MaxHitPoints = 500, CurrentHitPoints = 500,
            MaxMana = 999, CurrentMana = 999, RemainingCasts = 0,
            Equipment = new ArmorSlots(),
        };
        dummy.CurrentHitPoints = dummy.MaxHitPoints;
        return dummy;
    }

    // ── Run N trials and collect stats ────────────────────────────────────────

    private record BenchmarkResult(
        string Label, int TotalTrials,
        int HeroWins, int EnemyWins,
        double AvgTicks,
        double AvgDamagePerCombat, double AvgDamagePerHit,
        double HitRate, double CritRate, double FumbleRate,
        int TotalSpellCasts)
    {
        public double HeroWinRate => (double)HeroWins / TotalTrials * 100;
        public double EnemyWinRate => (double)EnemyWins / TotalTrials * 100;
    }

    private BenchmarkResult RunBenchmark(string label, Character hero, Character enemy, int trials = Trials)
    {
        var sim = BuildSim();
        var heroWins = 0;
        var enemyWins = 0;
        var totalTicks = 0L;
        var totalDamage = 0L;
        var totalHits = 0;
        var totalCrits = 0;
        var totalFumbles = 0;
        var totalAttacks = 0;
        var totalSpellCasts = 0;

        for (var i = 0; i < trials; i++)
        {
            ResetCombatant(hero);
            ResetCombatant(enemy);

            var party1 = Party.Solo(hero, hero.Equipment.RightHand as IAttackSource);
            var party2 = Party.Solo(enemy, enemy.Equipment.RightHand as IAttackSource);

            var result = sim.Simulate(party1, party2, 500);
            totalTicks += result.TotalTicks;

            if (result.WinningParty == party1) heroWins++;
            else if (result.WinningParty == party2) enemyWins++;

            foreach (var e in result.Log)
            {
                if (e.DamageDealt > 0)
                {
                    totalDamage += e.DamageDealt.Value;
                    totalAttacks++;
                }
                if (e.IsCritical == true) totalCrits++;
                if (e.IsFumble == true) totalFumbles++;
                if (e.IsHit == true) totalHits++;
                if (e.EventType == "TurnStart" && e.IsSpell == true) totalSpellCasts++;
            }
        }

        return new BenchmarkResult(
            label, trials,
            heroWins, enemyWins,
            (double)totalTicks / trials,
            (double)totalDamage / trials,
            totalHits > 0 ? (double)totalDamage / totalHits : 0,
            totalAttacks > 0 ? (double)totalHits / totalAttacks * 100 : 0,
            totalAttacks > 0 ? (double)totalCrits / totalAttacks * 100 : 0,
            totalAttacks > 0 ? (double)totalFumbles / totalAttacks * 100 : 0,
            totalSpellCasts);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Benchmark_LunaVsVaelith()
    {
        var luna = MakeLuna();
        var vaelith = MakeVaelith();
        var result = RunBenchmark("Luna (lvl 14) vs Vaelith (lvl 9)", luna, vaelith);

        var lines = new[]
        {
            "",
            $"╔═════════════════════════════════════════════════════════════════════════╗",
            $"║  BENCHMARK: {result.Label,-40}                                          ║",
            $"╠═════════════════════════════════════════════════════════════════════════╣",
            $"║  Trials:       {result.TotalTrials,6}                                   ║",
            $"║  Luna wins:    {result.HeroWins,6} ({result.HeroWinRate,5:F1}%)         ║",
            $"║  Vaelith wins: {result.EnemyWins,6} ({result.EnemyWinRate,5:F1}%)       ║",
            $"║  Avg ticks:    {result.AvgTicks,6:F1}                                   ║",
            $"║  Avg dmg/combat: {result.AvgDamagePerCombat,6:F0}                       ║",
            $"║  Hit rate:     {result.HitRate,6:F1}%                                   ║",
            $"║  Crit rate:    {result.CritRate,6:F1}%                                  ║",
            $"║  Fumble rate:  {result.FumbleRate,6:F1}%                                ║",
            $"║  Spell casts:  {result.TotalSpellCasts,6}                               ║",
            $"╚═════════════════════════════════════════════════════════════════════════╝",
        };
        foreach (var l in lines) out_.WriteLine(l);
        try { File.AppendAllLines(LogPath, lines); } catch { }

        // Regression: Luna should dominate a 5-level-lower opponent
        Assert.True(result.HeroWinRate >= 75,
            $"REGRESSION: Luna win rate {result.HeroWinRate:F1}% is below 75% threshold");
        if (result.HeroWinRate < 90)
            out_.WriteLine($"  ⚠ Luna win rate {result.HeroWinRate:F1}% is below target 90% — balance may need tuning");
    }

    [Fact]
    public void Benchmark_GolemVsDummy()
    {
        var golem = MakeGolem();
        var dummy = MakeDummy();
        var result = RunBenchmark("Golem (lvl 14) vs Dummy (lvl 10)", golem, dummy);

        out_.WriteLine("");
        out_.WriteLine($"╔══════════════════════════════════════════════════════════════════════╗");
        out_.WriteLine($"║  BENCHMARK: {result.Label,-40}                                       ║");
        out_.WriteLine($"╠══════════════════════════════════════════════════════════════════════╣");
        out_.WriteLine($"║  Trials:       {result.TotalTrials,6}                                ║");
        out_.WriteLine($"║  Golem wins:   {result.HeroWins,6} ({result.HeroWinRate,5:F1}%)      ║");
        out_.WriteLine($"║  Dummy wins:   {result.EnemyWins,6} ({result.EnemyWinRate,5:F1}%)    ║");
        out_.WriteLine($"║  Avg ticks:    {result.AvgTicks,6:F1}                                ║");
        out_.WriteLine($"║  Avg dmg/combat: {result.AvgDamagePerCombat,6:F0}                    ║");
        out_.WriteLine($"║  Avg dmg/hit:  {result.AvgDamagePerHit,6:F1}                         ║");
        out_.WriteLine($"║  Hit rate:     {result.HitRate,6:F1}%                                ║");
        out_.WriteLine($"║  Crit rate:    {result.CritRate,6:F1}%                               ║");
        out_.WriteLine($"║  Fumble rate:  {result.FumbleRate,6:F1}%                             ║");
        out_.WriteLine($"║  Spell casts:  {result.TotalSpellCasts,6}                            ║");
        out_.WriteLine($"╚══════════════════════════════════════════════════════════════════════╝");

        // Regression: Golem should defeat a defenseless dummy
        Assert.True(result.HeroWinRate >= 80,
            $"REGRESSION: Golem win rate {result.HeroWinRate:F1}% is below 80%");
        if (result.HeroWinRate < 95)
            out_.WriteLine($"  ⚠ Golem win rate {result.HeroWinRate:F1}% is below 95% — damage may need tuning");
    }

    [Fact]
    public void Benchmark_AllMatchups()
    {
        var trials = 200; // fewer trials per matchup to keep test fast

        var matchups = new (string Label, int HeroLevel, int EnemyLevel, Func<Character> HeroFactory, Func<Character> EnemyFactory)[]
        {
            ("Ser Garrick (lvl 12 Paladin)",        12, 11, MakeSerGarrick,   MakeLordAethor),
            ("Kaela (lvl 10 Barbarian)",            10, 12, MakeKaela,        MakeGreta),
            ("Elira (lvl 8 Tempest)",                8,  8, MakeElira,        MakeFinnick),
            ("Lysander (lvl 7 Bard)",                7,  6, MakeLysander,     MakeMerchantVex),
            ("Old Man Kael (lvl 8 Priest)",          8, 20, MakeOldManKael,   MakeElderTreant),
            ("Infernal Commander (lvl 18 Knight)",  18, 14, MakeInfernal,     MakeGolem),
        };

        out_.WriteLine("");
        out_.WriteLine("======= COMPREHENSIVE BALANCE REPORT =======");
        out_.WriteLine("Matchup                        Wins  Rate% Ticks  Dmg Spells  OK?");

        var issues = 0;
        foreach (var (label, hl, el, hf, ef) in matchups)
        {
            var (hero, enemy) = (hf(), ef());
            var r = RunBenchmark(label, hero, enemy, trials);
            var higherWon = (hl >= el && r.HeroWinRate >= 50) || (el >= hl && r.HeroWinRate <= 50);
            var ok = higherWon || r.HeroWinRate == 50; // 50% is a draw, acceptable
            if (!ok) issues++;
            out_.WriteLine($"  {r.Label,-45} {r.HeroWins,6} {r.HeroWinRate,6:F1}% {r.AvgTicks,6:F0} {r.AvgDamagePerCombat,5:F0} {r.TotalSpellCasts,5} {(ok ? "✓" : "✗")}");
        }

        out_.WriteLine("==========================================");
        Assert.True(issues == 0, $"{issues} matchup(s) had the lower-level character winning more often than the higher-level character");
    }

    // ── Additional character factories ───────────────────────────

    private static Character MakeSerGarrick()
    {
        var c = new Character
        {
            Name = "Ser Garrick Dawnshield", Level = 12,
            Strength = 18, Dexterity = 11, Stamina = 15,
            Intelligence = 11, Wisdom = 13, Charisma = 17,
            ClassName = "Paladin", StrikeRating = 13, TurnSpeed = 8,
            MaxHitPoints = 96, CurrentHitPoints = 96,
            MaxMana = 60, CurrentMana = 60, RemainingCasts = 20,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Plate Armor", ArmorClass = 18, Mitigation = 5 },
                RightHand = new Weapon { Name = "War Hammer", DamageDie = DieType.D10, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 2 },
            },
            MemorizedSpells =
            [
                new() { Name = "Smite", DamageDie = DieType.D8, DamageCount = 2, AttackBonus = 2, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 35, SpellLevel = 2, AttackType = AttackType.Spell },
                new() { Name = "Heal", DamageDie = DieType.D8, DamageCount = 2, AttackBonus = 0, DamageType = DamageType.Healing, School = SpellSchool.Deity, ManaCost = 25, SpellLevel = 6, AttackType = AttackType.Spell },
            ],
        };
        c.CurrentHitPoints = c.MaxHitPoints; c.CurrentMana = c.MaxMana; return c;
    }

    private static Character MakeLordAethor()
    {
        var c = new Character
        {
            Name = "Lord Aethor Valeborn", Level = 11,
            Strength = 17, Dexterity = 13, Stamina = 16,
            Intelligence = 12, Wisdom = 13, Charisma = 14,
            ClassName = "Knight", StrikeRating = 14, TurnSpeed = 7,
            MaxHitPoints = 88, CurrentHitPoints = 88,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Plate Armor", ArmorClass = 18, Mitigation = 5 },
                RightHand = new Weapon { Name = "Great Sword", DamageDie = DieType.D10, DamageCount = 1,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; return c;
    }

    private static Character MakeKaela()
    {
        var c = new Character
        {
            Name = "Kaela Vornskald", Level = 10,
            Strength = 19, Dexterity = 15, Stamina = 17,
            Intelligence = 9, Wisdom = 11, Charisma = 13,
            ClassName = "Barbarian", StrikeRating = 15, TurnSpeed = 6,
            MaxHitPoints = 100, CurrentHitPoints = 100,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Hide Armor", ArmorClass = 12, Mitigation = 2 },
                RightHand = new Weapon { Name = "Great Sword", DamageDie = DieType.D10, DamageCount = 1,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; return c;
    }

    private static Character MakeGreta()
    {
        var c = new Character
        {
            Name = "Greta Ironhand", Level = 12,
            Strength = 18, Dexterity = 13, Stamina = 17,
            Intelligence = 11, Wisdom = 11, Charisma = 12,
            ClassName = "Fighter", StrikeRating = 15, TurnSpeed = 6,
            MaxHitPoints = 90, CurrentHitPoints = 90,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Chain Mail", ArmorClass = 16, Mitigation = 3 },
                RightHand = new Weapon { Name = "War Hammer", DamageDie = DieType.D10, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 2 },
                LeftHand = new Weapon { Name = "Mace", DamageDie = DieType.D6, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; return c;
    }

    private static Character MakeElira()
    {
        var c = new Character
        {
            Name = "Elira Vane", Level = 8,
            Strength = 14, Dexterity = 13, Stamina = 16,
            Intelligence = 13, Wisdom = 18, Charisma = 15,
            ClassName = "Tempest", StrikeRating = 17, TurnSpeed = 8,
            MaxHitPoints = 56, CurrentHitPoints = 56,
            MaxMana = 80, CurrentMana = 80, RemainingCasts = 20,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Chain Mail", ArmorClass = 16, Mitigation = 3 },
                RightHand = new Weapon { Name = "Mace", DamageDie = DieType.D6, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
            MemorizedSpells =
            [
                new() { Name = "Chasten", DamageDie = DieType.D4, DamageCount = 1, AttackBonus = 0, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 10, SpellLevel = 1, AttackType = AttackType.Spell },
                new() { Name = "Heal", DamageDie = DieType.D8, DamageCount = 2, AttackBonus = 0, DamageType = DamageType.Healing, School = SpellSchool.Deity, ManaCost = 25, SpellLevel = 6, AttackType = AttackType.Spell },
                new() { Name = "Turn Undead", DamageDie = DieType.D6, DamageCount = 2, AttackBonus = 2, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 25, SpellLevel = 2, AttackType = AttackType.Spell },
                new() { Name = "Smite", DamageDie = DieType.D8, DamageCount = 2, AttackBonus = 2, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 35, SpellLevel = 2, AttackType = AttackType.Spell },
            ],
        };
        c.CurrentHitPoints = c.MaxHitPoints; c.CurrentMana = c.MaxMana; return c;
    }

    private static Character MakeFinnick()
    {
        var c = new Character
        {
            Name = "Finnick Bramblefoot", Level = 8,
            Strength = 8, Dexterity = 20, Stamina = 13,
            Intelligence = 14, Wisdom = 11, Charisma = 16,
            ClassName = "Rogue", StrikeRating = 17, TurnSpeed = 12,
            MaxHitPoints = 40, CurrentHitPoints = 40,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Studded Leather", ArmorClass = 12, Mitigation = 1, MaxDexterityBonus = 6 },
                RightHand = new Weapon { Name = "Dagger", DamageDie = DieType.D4, DamageCount = 1,
                    DamageType = DamageType.Piercing, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; return c;
    }

    private static Character MakeLysander()
    {
        var c = new Character
        {
            Name = "Lysander the Bard", Level = 7,
            Strength = 8, Dexterity = 18, Stamina = 13,
            Intelligence = 14, Wisdom = 11, Charisma = 18,
            ClassName = "Bard", StrikeRating = 17, TurnSpeed = 10,
            MaxHitPoints = 35, CurrentHitPoints = 35,
            MaxMana = 50, CurrentMana = 50, RemainingCasts = 20,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Studded Leather", ArmorClass = 12, Mitigation = 1, MaxDexterityBonus = 6 },
                RightHand = new Weapon { Name = "Short Sword", DamageDie = DieType.D6, DamageCount = 1,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1 },
                LeftHand = new Weapon { Name = "Dagger", DamageDie = DieType.D4, DamageCount = 1,
                    DamageType = DamageType.Piercing, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
            MemorizedSpells =
            [
                new() { Name = "Mind Game", DamageDie = DieType.D4, DamageCount = 1, AttackBonus = 0, DamageType = DamageType.Shadow, School = SpellSchool.Umbramancy, ManaCost = 25, SpellLevel = 2, AttackType = AttackType.Spell },
                new() { Name = "Charm Person", DamageDie = DieType.D4, DamageCount = 0, AttackBonus = 0, DamageType = DamageType.Psychic, School = SpellSchool.Mirage, ManaCost = 30, SpellLevel = 2, AttackType = AttackType.Spell },
            ],
        };
        c.CurrentHitPoints = c.MaxHitPoints; c.CurrentMana = c.MaxMana; return c;
    }

    private static Character MakeMerchantVex()
    {
        var c = new Character
        {
            Name = "Merchant Vex", Level = 6,
            Strength = 8, Dexterity = 17, Stamina = 12,
            Intelligence = 14, Wisdom = 10, Charisma = 16,
            ClassName = "Rogue", StrikeRating = 16, TurnSpeed = 10,
            MaxHitPoints = 32, CurrentHitPoints = 32,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Padded Armor", ArmorClass = 11, Mitigation = 1, MaxDexterityBonus = 6 },
                RightHand = new Weapon { Name = "Dagger", DamageDie = DieType.D4, DamageCount = 1,
                    DamageType = DamageType.Piercing, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; return c;
    }

    private static Character MakeOldManKael()
    {
        var c = new Character
        {
            Name = "Old Man Kael", Level = 8,
            Strength = 9, Dexterity = 8, Stamina = 11,
            Intelligence = 15, Wisdom = 19, Charisma = 14,
            ClassName = "Priest", StrikeRating = 17, TurnSpeed = 6,
            MaxHitPoints = 40, CurrentHitPoints = 40,
            MaxMana = 60, CurrentMana = 60, RemainingCasts = 20,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Padded Armor", ArmorClass = 11, Mitigation = 1 },
                RightHand = new Weapon { Name = "Mace", DamageDie = DieType.D6, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; c.CurrentMana = c.MaxMana; return c;
    }

    private static Character MakeElderTreant()
    {
        var c = new Character
        {
            Name = "Elder Treant", Level = 20,
            Strength = 14, Dexterity = 12, Stamina = 16,
            Intelligence = 16, Wisdom = 20, Charisma = 15,
            ClassName = "Druid", StrikeRating = 18, TurnSpeed = 7,
            MaxHitPoints = 140, CurrentHitPoints = 140,
            MaxMana = 120, CurrentMana = 120, RemainingCasts = 30,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Padded Armor", ArmorClass = 11, Mitigation = 1 },
                RightHand = new Weapon { Name = "Mace", DamageDie = DieType.D6, DamageCount = 1,
                    DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 1 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; c.CurrentMana = c.MaxMana; return c;
    }

    private static Character MakeInfernal()
    {
        var c = new Character
        {
            Name = "Infernal Commander Maleth", Level = 18,
            Strength = 20, Dexterity = 14, Stamina = 18,
            Intelligence = 16, Wisdom = 14, Charisma = 16,
            ClassName = "Knight", StrikeRating = 19, TurnSpeed = 7,
            MaxHitPoints = 160, CurrentHitPoints = 160,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Plate Armor", ArmorClass = 18, Mitigation = 5 },
                RightHand = new Weapon { Name = "Great Sword", DamageDie = DieType.D10, DamageCount = 1,
                    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 3 },
            },
        };
        c.CurrentHitPoints = c.MaxHitPoints; return c;
    }
}
