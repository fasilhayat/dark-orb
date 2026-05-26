namespace BattleArena.Demo;

using Core.Entities;
using Core.Entities.Enums;

// Character, weapon and spell data definitions used by the demo battles.
static partial class Demo
{
    // ── Hero weapons ──────────────────────────────────────────────────────────────
    private static readonly Weapon Longsword = new()
    {
        Name = "Longsword", DamageDie = DieType.D8, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2
    };
    private static readonly Weapon BattleAxe = new()
    {
        Name = "Battle Axe", DamageDie = DieType.D8, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1
    };

    // ── Hero spells ───────────────────────────────────────────────────────────────
    private static readonly Spell Fireball = new()
    {
        Name = "Fireball", Description = "A blazing orb of fire",
        School = SpellSchool.Evocation, DamageDie = DieType.D6, DamageCount = 3,
        DamageType = DamageType.Fire, AttackBonus = 2, SpellLevel = 3
    };
    private static readonly Spell IceBolt = new()
    {
        Name = "Ice Bolt", Description = "A shard of magical ice",
        School = SpellSchool.Evocation, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Cold, AttackBonus = 2, SpellLevel = 2
    };
    private static readonly Spell LightningStrike = new()
    {
        Name = "Lightning Strike", Description = "A bolt of crackling lightning",
        School = SpellSchool.Evocation, DamageDie = DieType.D10, DamageCount = 2,
        DamageType = DamageType.Lightning, AttackBonus = 3, SpellLevel = 4
    };

    // ── Hero characters ──────────────────────────────────────────────────────────
    private static readonly Character Theron = new()
    {
        Name = "Theron", Level = 5, Strength = 18, Dexterity = 12, Intelligence = 10,
        ClassId = 8, StrikeRating = 14, TurnSpeed = 10, MaxHitPoints = 50, CurrentHitPoints = 50,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Chain Mail", ArmorClass = 5, Mitigation = 2, MaxDexterityBonus = 6 },
            RightHand = Longsword
        }
    };
    private static readonly Character Gruk = new()
    {
        Name = "Gruk", Level = 3, Strength = 16, Dexterity = 8, Intelligence = 8,
        ClassId = 1, StrikeRating = 16, TurnSpeed = 6, MaxHitPoints = 35, CurrentHitPoints = 35,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Leather Armor", ArmorClass = 7, Mitigation = 1, MaxDexterityBonus = 6 },
            RightHand = BattleAxe
        }
    };
    private static readonly Character Lyra = new()
    {
        Name = "Lyra", Level = 5, Strength = 8, Dexterity = 14, Intelligence = 18,
        ClassId = 5, StrikeRating = 13, TurnSpeed = 8, MaxHitPoints = 30, CurrentHitPoints = 30,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Mage Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 }
        },
        MemorizedSpells = [Fireball, IceBolt, LightningStrike]
    };

    // ── Enemy weapons ─────────────────────────────────────────────────────────────
    private static readonly Weapon OrcAxe = new()
    {
        Name = "Orcish Axe", DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1
    };
    private static readonly Weapon GoblinDagger = new()
    {
        Name = "Poisoned Dagger", DamageDie = DieType.D4, DamageCount = 2,
        DamageType = DamageType.Piercing, AttackType = AttackType.Melee, AttackBonus = 3
    };

    // ── Enemy spells ──────────────────────────────────────────────────────────────
    private static readonly Spell ShadowBolt = new()
    {
        Name = "Shadow Bolt", Description = "A bolt of shadow energy",
        School = SpellSchool.Other, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Cold, AttackBonus = 2, SpellLevel = 2
    };
    private static readonly Spell SoulDrain = new()
    {
        Name = "Soul Drain", Description = "Saps the life force of a target",
        School = SpellSchool.Other, DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Fire, AttackBonus = 1, SpellLevel = 2
    };

    // ── Enemy characters ──────────────────────────────────────────────────────────
    internal static readonly Character Krag = new()
    {
        Name = "Krag", Level = 4, Strength = 17, Dexterity = 9, Intelligence = 6,
        ClassId = 1, StrikeRating = 15, TurnSpeed = 7, MaxHitPoints = 45, CurrentHitPoints = 45,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Orcish Hide", ArmorClass = 6, Mitigation = 2, MaxDexterityBonus = 4 },
            RightHand = OrcAxe
        }
    };
    internal static readonly Character Skrix = new()
    {
        Name = "Skrix", Level = 2, Strength = 9, Dexterity = 16, Intelligence = 10,
        ClassId = 9, StrikeRating = 12, TurnSpeed = 12, MaxHitPoints = 20, CurrentHitPoints = 20,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Worn Leather", ArmorClass = 8, Mitigation = 0, MaxDexterityBonus = 6 },
            RightHand = GoblinDagger
        }
    };
    internal static readonly Character Mordak = new()
    {
        Name = "Mordak", Level = 3, Strength = 7, Dexterity = 12, Intelligence = 16,
        ClassId = 5, StrikeRating = 14, TurnSpeed = 9, MaxHitPoints = 25, CurrentHitPoints = 25,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Dark Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 }
        },
        MemorizedSpells = [ShadowBolt, SoulDrain]
    };

    // ── Lookup table initialization ──────────────────────────────────────────────

    private static void InitializeData()
    {
        AllHeroes = new()
        {
            ['T'] = Theron,
            ['G'] = Gruk,
            ['L'] = Lyra
        };
        AttackMap = new()
        {
            [Theron.Name] = Longsword,
            [Gruk.Name] = BattleAxe,
            [Lyra.Name] = null,
            [Krag.Name] = OrcAxe,
            [Skrix.Name] = GoblinDagger,
            [Mordak.Name] = null
        };
    }
}
