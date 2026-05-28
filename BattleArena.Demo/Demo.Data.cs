namespace BattleArena.Demo;

using Core.Entities;
using Core.Entities.Enums;

// Character, weapon and spell data definitions used by the demo battles.
static partial class Demo
{
    // ── Racial templates with resistance feats ─────────────────────────────────
    private static readonly Race OrcRace = new()
    {
        Name = "Orc",
        Feats = []
    };

    private static readonly Race ElfRace = new()
    {
        Name = "Elf",
        Feats =
        [
            new Feat
            {
                Name = "Magic Resistance",
                Description = "Elves have innate advantage on saving throws against magical effects.",
                Resistances = [new ResistanceBonus(ResistanceType.Magic, 25)]
            }
        ]
    };

    private static readonly Race HumanRace = new()
    {
        Name = "Human",
        Feats = []
    };

    private static readonly Race DarkMageRace = new()
    {
        Name = "Undead",
        Feats =
        [
            new Feat
            {
                Name = "Dark Arcana",
                Description = "A dark mage has studied the arcane arts deeply, providing resistance to magical manipulation.",
                Resistances = [new ResistanceBonus(ResistanceType.Magic, 20)]
            }
        ]
    };

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
        DamageType = DamageType.Fire, AttackBonus = 2, SpellLevel = 3, TurnMeterCost = 90,
        OnHitEffects =
        [
            new StatusEffect { Name = "Burning",   Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Fire, Duration = 3, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 30, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell IceBolt = new()
    {
        Name = "Ice Bolt", Description = "A shard of magical ice",
        School = SpellSchool.Evocation, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Ice, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80,
        OnHitEffects =
        [
            new StatusEffect { Name = "Freezing",  Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Cold, Duration = 2, DoTDamageCount = 1, DoTDamageDie = DieType.D6, ApplicationChance = 25, StackRule = StackRule.HighestWins },
            new StatusEffect { Name = "Slippery",  Type = StatusEffectType.Debuff,         ResistanceType = ResistanceType.Magic, Duration = 2, TurnMeterModifier = -3, ApplicationChance = 30, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell LightningStrike = new()
    {
        Name = "Lightning Strike", Description = "A bolt of crackling lightning",
        School = SpellSchool.Evocation, DamageDie = DieType.D10, DamageCount = 2,
        DamageType = DamageType.Lightning, AttackBonus = 3, SpellLevel = 4, TurnMeterCost = 100,
        OnHitEffects =
        [
            new StatusEffect { Name = "Shocked",   Type = StatusEffectType.Debuff,         ResistanceType = ResistanceType.Magic, Duration = 2, AttackPowerModifier = -2, ApplicationChance = 20, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell BladeBarrier = new()
    {
        Name = "Blade Barrier", Description = "A wall of spinning blades that slicks the ground with oil",
        School = SpellSchool.AoE, DamageDie = DieType.D8, DamageCount = 3,
        DamageType = DamageType.Slashing, AttackBonus = 2, SpellLevel = 3, TurnMeterCost = 90,
        OnHitEffects =
        [
            new StatusEffect { Name = "Oil Slick", Type = StatusEffectType.Debuff,         ResistanceType = ResistanceType.Magic, Duration = 3, TurnMeterModifier = -4, ApplicationChance = 40, StackRule = StackRule.HighestWins },
            new StatusEffect { Name = "Thorns",    Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Magic, Duration = 2, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 30, StackRule = StackRule.HighestWins }
        ]
    };

    // ── Hero characters ──────────────────────────────────────────────────────────
    private static readonly Character Theron = new()
    {
        Name = "Theron", Level = 5, Strength = 18, Dexterity = 12, Intelligence = 10,
        Race = HumanRace,
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
        Race = OrcRace,
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
        Race = ElfRace,
        ClassId = 5, StrikeRating = 13, TurnSpeed = 8, MaxHitPoints = 30, CurrentHitPoints = 30,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Mage Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 }
        },
        MemorizedSpells = [Fireball, IceBolt, LightningStrike, BladeBarrier]
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
        DamageType = DamageType.Ice, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80,
        OnHitEffects =
        [
            new StatusEffect { Name = "Chilled",   Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Cold, Duration = 2, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 20, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell SoulDrain = new()
    {
        Name = "Soul Drain", Description = "Saps the life force of a target",
        School = SpellSchool.Other, DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Fire, AttackBonus = 1, SpellLevel = 2, TurnMeterCost = 80,
        OnHitEffects =
        [
            new StatusEffect { Name = "Burning",   Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Fire, Duration = 3, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 20, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell Root = new()
    {
        Name = "Root", Description = "Anchors the target with grasping vines",
        School = SpellSchool.CC, DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackBonus = 0, SpellLevel = 2, TurnMeterCost = 80,
        OnHitEffects =
        [
            new StatusEffect { Name = "Rooted",    Type = StatusEffectType.Root,            ResistanceType = ResistanceType.Magic, Duration = 2, ApplicationChance = 100, StackRule = StackRule.NoStack },
            new StatusEffect { Name = "Thorns",    Type = StatusEffectType.DamageOverTime,  ResistanceType = ResistanceType.Magic, Duration = 3, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 40, StackRule = StackRule.HighestWins }
        ]
    };

    // ── Enemy characters ──────────────────────────────────────────────────────────
    internal static readonly Character Krag = new()
    {
        Name = "Krag", Level = 4, Strength = 17, Dexterity = 9, Intelligence = 6,
        Race = OrcRace,
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
        Race = HumanRace,
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
        Race = DarkMageRace,
        ClassId = 5, StrikeRating = 14, TurnSpeed = 9, MaxHitPoints = 25, CurrentHitPoints = 25,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Dark Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 }
        },
        MemorizedSpells = [ShadowBolt, SoulDrain, Root]
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
