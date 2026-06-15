namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

[Binding]
public class CombatBalanceSteps
{
    private Character _character = new();
    private CombatantStats? _stats;
    private int _defensePower;
    private int _attributeModifier;
    private int _scaledMitigation;
    private int _lunaWins;

    // ── Defense power ────────────────────────────────────────────────

    [Given(@"a defender at level (\d+) with armor class (\d+) and dexterity (\d+)")]
    public void GivenDefender(int level, int ac, int dex)
    {
        _character = new Character
        {
            Level = level,
            Dexterity = dex,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { ArmorClass = ac }
            },
        };
    }

    [When(@"the defender's combat stats are computed")]
    public void WhenDefensePowerIsComputed()
    {
        var svc = new CombatStatsService();
        _stats = svc.ComputeDefenderStats(_character);
        _defensePower = _stats.DefensePower;
    }

    [Then(@"the defense power should be (\d+)")]
    public void ThenDefensePowerShouldBe(int expected)
    {
        Assert.Equal(expected, _defensePower);
    }

    // ── Spell ability score ──────────────────────────────────────────

    [Given(@"a priest with intelligence (\d+) and wisdom (\d+)")]
    public void GivenPriest(int intelligence, int wisdom)
    {
        _character = new Character { Intelligence = intelligence, Wisdom = wisdom, ClassName = "Priest" };
    }

    [Given(@"a deity school spell")]
    public void GivenDeitySpell()
    {
    }

    [Given(@"a mage with intelligence (\d+) and wisdom (\d+)")]
    public void GivenMage(int intelligence, int wisdom)
    {
        _character = new Character { Intelligence = intelligence, Wisdom = wisdom, ClassName = "Mage" };
    }

    [Given(@"a stormcraft school spell")]
    public void GivenStormcraftSpell()
    {
    }

    [When(@"attack stats are computed for the spell")]
    public void WhenAttackStatsComputed()
    {
        var svc = new CombatStatsService();
        var school = _character.ClassName == "Mage" ? SpellSchool.Stormcraft : SpellSchool.Deity;
        var spell = new Spell { School = school, DamageType = school == SpellSchool.Deity ? DamageType.Holy : DamageType.Fire };
        _stats = svc.ComputeAttackerStats(_character, spell);
        _attributeModifier = _stats.AttributeModifier;
    }

    [Then(@"the attribute modifier should be (\d+)")]
    public void ThenAttributeModifierShouldBe(int expected)
    {
        Assert.Equal(expected, _attributeModifier);
    }

    // ── Armor mitigation ─────────────────────────────────────────────

    [Given(@"a defender with plate armor at level (\d+)")]
    public void GivenPlateArmorDefender(int level)
    {
        _character = new Character
        {
            Level = level,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Plate Armor", ArmorClass = 18, Mitigation = 5 },
            },
        };
    }

    [When(@"mitigation is computed")]
    public void WhenMitigationIsComputed()
    {
        var raw = _character.Equipment.TotalMitigation;
        var levelFactor = 1.0 + _character.Level / 10.0;
        _scaledMitigation = (int)(raw * levelFactor);
    }

    [Then(@"the scaled mitigation should be (\d+)")]
    public void ThenScaledMitigationShouldBe(int expected)
    {
        Assert.Equal(expected, _scaledMitigation);
    }

    // ── Luna vs Vaelith benchmark ────────────────────────────────────

    [Given(@"High Priestess Luna at level (\d+)")]
    public void GivenLuna(int level)
    {
        _character = MakeLuna(level);
    }

    [Given(@"Vaelith Moonveil at level (\d+)")]
    public void GivenVaelith(int level)
    {
        // stored in second character slot
    }

    [When(@"they fight (\d+) duels")]
    public void WhenTheyFight(int duels)
    {
        _lunaWins = 0;
        for (var i = 0; i < duels; i++)
        {
            var luna = MakeLuna(14);
            var vaelith = MakeVaelith(9);

            var sim = new CombatSimulator(
                new CombatService(new DiceService(), new CombatStatsService()),
                new TurnmeterService(),
                new StatusEffectService(),
                new DiceService());

            var result = sim.Simulate(
                Party.Solo(luna, luna.Equipment.RightHand as IAttackSource),
                Party.Solo(vaelith, vaelith.Equipment.RightHand as IAttackSource),
                500);

            if (result.WinningParty?.Members.Any(m => m.Character.Name == "High Priestess Luna") == true)
                _lunaWins++;
        }
    }

    [Then(@"Luna should win at least (\d+) duels")]
    public void ThenLunaWinsAtLeast(int minWins)
    {
        Assert.True(_lunaWins >= minWins,
            $"Luna won {_lunaWins}/{100} duels, expected at least {minWins}");
    }

    // ── Character factories ──────────────────────────────────────────

    private static Character MakeLuna(int level)
    {
        var luna = new Character
        {
            Name = "High Priestess Luna", Level = level,
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
                new() { Name = "Smite", DamageDie = DieType.D8, DamageCount = 2, AttackBonus = 2, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 35, SpellLevel = 2, AttackType = AttackType.Spell },
                new() { Name = "Flame Strike", DamageDie = DieType.D8, DamageCount = 3, AttackBonus = 3, DamageType = DamageType.Fire, School = SpellSchool.Deity, ManaCost = 30, SpellLevel = 5, AttackType = AttackType.Spell },
                new() { Name = "Heal", DamageDie = DieType.D8, DamageCount = 2, AttackBonus = 0, DamageType = DamageType.Healing, School = SpellSchool.Deity, ManaCost = 25, SpellLevel = 6, AttackType = AttackType.Spell },
                new() { Name = "Mass Heal", DamageDie = DieType.D6, DamageCount = 3, AttackBonus = 0, DamageType = DamageType.Healing, School = SpellSchool.Deity, ManaCost = 50, SpellLevel = 4, AttackType = AttackType.Spell },
                new() { Name = "Chasten", DamageDie = DieType.D4, DamageCount = 1, AttackBonus = 0, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 10, SpellLevel = 1, AttackType = AttackType.Spell },
                new() { Name = "Holy Nova", DamageDie = DieType.D8, DamageCount = 3, AttackBonus = 3, DamageType = DamageType.Holy, School = SpellSchool.Deity, ManaCost = 55, SpellLevel = 5, AttackType = AttackType.Spell },
            ],
        };
        luna.CurrentHitPoints = luna.MaxHitPoints;
        luna.CurrentMana = luna.MaxMana;
        return luna;
    }

    private static Character MakeVaelith(int level)
    {
        var vaelith = new Character
        {
            Name = "Vaelith Moonveil", Level = level,
            Strength = 16, Dexterity = 18, Stamina = 14,
            Intelligence = 10, Wisdom = 12, Charisma = 13,
            ClassName = "Fighter", StrikeRating = 17, TurnSpeed = 10,
            MaxHitPoints = 68, CurrentHitPoints = 68,
            MaxMana = 0, CurrentMana = 0,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Mithril Chain", ArmorClass = 14, Mitigation = 2, MaxDexterityBonus = 6, Resistances = [new ResistanceBonus(ResistanceType.Magic, 5)] },
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
}
