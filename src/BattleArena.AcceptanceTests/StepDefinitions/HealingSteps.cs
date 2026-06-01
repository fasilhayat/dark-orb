namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

[Binding]
public class HealingSteps
{
    private readonly Dictionary<string, Character> _combatants = new();
    private readonly Dictionary<string, Weapon> _weapons = new();
    private readonly List<Character> _heroPartyMembers = new();
    private readonly List<Character> _enemyPartyMembers = new();
    private CombatResult _combatResult = null!;
    private readonly ICombatSimulator _combatSimulator;

    public HealingSteps()
    {
        var dice = new DiceService();
        var combatStats = new CombatStatsService();
        var combat = new CombatService(dice, combatStats);
        var turnmeter = new TurnmeterService();
        var statusEffect = new StatusEffectService();
        _combatSimulator = new CombatSimulator(combat, turnmeter, statusEffect, dice);
    }

    [Given(@"a (\w+) ""([^""]+)"" with (\d+) HP and (\d+) mana")]
    public void GivenCharacterWithSimpleStats(string className, string name, int hp, int mana)
    {
        var character = new Character
        {
            Name = name,
            Level = 5,
            Strength = 14,
            Dexterity = 10,
            Intelligence = 14,
            StrikeRating = 14,
            TurnSpeed = 8,
            MaxHitPoints = hp,
            CurrentHitPoints = hp,
            MaxMana = mana,
            CurrentMana = mana,
            ClassName = className is "Priest" or "Cleric" ? "Priest" : "Fighter",
            ClassId = className is "Priest" or "Cleric" ? 4 : 8
        };
        _combatants[name] = character;
        _heroPartyMembers.Add(character);
    }

    [Given(@"(?:a|an) (\w+) ""([^""]+)"" with (\d+) HP and (\d+) mana who wields a (.+)")]
    public void GivenEnemyWithWeapon(string className, string name, int hp, int mana, string weaponName)
    {
        var isEnemy = className is "Orc" or "Goblin" or "Skeleton" or "Bandit";
        var character = new Character
        {
            Name = name,
            Level = isEnemy ? 3 : 5,
            Strength = isEnemy ? 16 : 14,
            Dexterity = isEnemy ? 8 : 10,
            Intelligence = 10,
            StrikeRating = isEnemy ? 16 : 14,
            TurnSpeed = isEnemy ? 6 : 8,
            MaxHitPoints = hp,
            CurrentHitPoints = hp,
            MaxMana = mana,
            CurrentMana = mana
        };
        _combatants[name] = character;

        var weapon = new Weapon
        {
            Name = weaponName,
            DamageDie = weaponName == "Longsword" ? DieType.D8 : DieType.D8,
            DamageCount = 1,
            DamageType = DamageType.Slashing,
            AttackType = AttackType.Melee,
            AttackBonus = 2
        };
        _weapons[name] = weapon;
        character.Equipment.RightHand = weapon;

        if (isEnemy)
            _enemyPartyMembers.Add(character);
        else
            _heroPartyMembers.Add(character);
    }

    [Given(@"""([^""]+)"" has (\d+) hit points remaining")]
    public void GivenCharacterHasRemainingHP(string name, int hp)
    {
        if (_combatants.TryGetValue(name, out var c))
            c.CurrentHitPoints = hp;
    }

    [Given(@"""([^""]+)"" has memorized spells?: ""([^""]+)""$")]
    public void GivenCharacterHasOneSpell(string name, string spell1)
    {
        _combatants[name].MemorizedSpells = new List<Spell> { CreateSpell(spell1) };
    }

    [Given(@"""([^""]+)"" has memorized spells?: ""([^""]+)"" and ""([^""]+)""")]
    public void GivenCharacterHasTwoSpells(string name, string spell1, string spell2)
    {
        _combatants[name].MemorizedSpells = new List<Spell> { CreateSpell(spell1), CreateSpell(spell2) };
    }

    [When(@"the healing combat is simulated with a maximum of (\d+) ticks")]
    public void WhenHealingCombatIsSimulated(int maxTicks)
    {
        var hero = _heroPartyMembers[0];
        var enemy = _enemyPartyMembers[0];

        _combatResult = _combatSimulator.Simulate(
            hero, _weapons.GetValueOrDefault(hero.Name),
            enemy, _weapons.GetValueOrDefault(enemy.Name),
            maxTicks);
    }

    [When(@"the healing party combat is simulated with a maximum of (\d+) ticks")]
    public void WhenHealingPartyCombatIsSimulated(int maxTicks)
    {
        var heroParty = new Party();
        foreach (var c in _heroPartyMembers)
        {
            heroParty.Members.Add(new PartyMember
            {
                Character = c,
                AttackSource = _weapons.GetValueOrDefault(c.Name)
            });
        }

        var enemyParty = new Party();
        foreach (var c in _enemyPartyMembers)
        {
            enemyParty.Members.Add(new PartyMember
            {
                Character = c,
                AttackSource = _weapons.GetValueOrDefault(c.Name)
            });
        }

        _combatResult = _combatSimulator.Simulate(heroParty, enemyParty, maxTicks);
    }

    [Then(@"the heal event appears in the log")]
    public void ThenHealEventAppearsInLog()
    {
        Assert.Contains(_combatResult.Log, e => e.EventType == "Healed");
    }

    // ── Helpers ────────────────────────────────────────────────

    private static Spell CreateSpell(string name) => name switch
    {
        "Heal" => new Spell
        {
            Name = "Heal",
            School = SpellSchool.Healing,
            DamageDie = DieType.D8,
            DamageCount = 2,
            FlatDamageBonus = 4,
            ManaCost = 25,
            TurnMeterCost = 80,
            SpellLevel = 2,
            DamageType = DamageType.Holy,
            AttackType = AttackType.Ranged
        },
        "Mass Heal" => new Spell
        {
            Name = "Mass Heal",
            School = SpellSchool.Healing,
            DamageDie = DieType.D6,
            DamageCount = 3,
            FlatDamageBonus = 6,
            ManaCost = 45,
            TurnMeterCost = 100,
            SpellLevel = 4,
            DamageType = DamageType.Holy,
            AttackType = AttackType.Ranged
        },
        "Smite" => new Spell
        {
            Name = "Smite",
            School = SpellSchool.Evocation,
            DamageDie = DieType.D8,
            DamageCount = 2,
            ManaCost = 35,
            TurnMeterCost = 80,
            SpellLevel = 2,
            DamageType = DamageType.Holy,
            AttackBonus = 2,
            AttackType = AttackType.Ranged
        },
        _ => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown spell: {name}")
    };
}
