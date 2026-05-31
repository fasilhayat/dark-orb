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
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2,
        Archetype = ArchetypeWeapon.Sword, Hands = 1
    };
    private static readonly Weapon BattleAxe = new()
    {
        Name = "Battle Axe", DamageDie = DieType.D8, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1,
        Archetype = ArchetypeWeapon.Axe, Hands = 1
    };

    // ── Additional hero weapons ─────────────────────────────────────────────────────
    private static readonly Weapon ArcaneStaff = new()
    {
        Name = "Arcane Staff", DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 1,
        Archetype = ArchetypeWeapon.Staff, Hands = 2
    };
    private static readonly Weapon CeremonialMace = new()
    {
        Name = "Ceremonial Mace", DamageDie = DieType.D6, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 2,
        Archetype = ArchetypeWeapon.Mace, Hands = 1
    };

    // ── Hero spells ───────────────────────────────────────────────────────────────
    private static readonly Spell Fireball = new()
    {
        Name = "Fireball", Description = "A blazing orb of fire",
        School = SpellSchool.Evocation, DamageDie = DieType.D6, DamageCount = 3,
        DamageType = DamageType.Fire, AttackBonus = 2, SpellLevel = 3, TurnMeterCost = 90, ManaCost = 30,
        OnHitEffects =
        [
            new StatusEffect { Name = "Burning",   Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Fire, Duration = 3, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 30, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell IceBolt = new()
    {
        Name = "Ice Bolt", Description = "A shard of magical ice",
        School = SpellSchool.Evocation, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Ice, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 20,
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
        DamageType = DamageType.Lightning, AttackBonus = 3, SpellLevel = 4, TurnMeterCost = 100, ManaCost = 40,
        OnHitEffects =
        [
            new StatusEffect { Name = "Shocked",   Type = StatusEffectType.Debuff,         ResistanceType = ResistanceType.Magic, Duration = 2, AttackPowerModifier = -2, ApplicationChance = 20, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell BladeBarrier = new()
    {
        Name = "Blade Barrier", Description = "A wall of spinning blades that slicks the ground with oil",
        School = SpellSchool.AoE, DamageDie = DieType.D8, DamageCount = 3,
        DamageType = DamageType.Slashing, AttackBonus = 2, SpellLevel = 3, TurnMeterCost = 90, ManaCost = 25,
        OnHitEffects =
        [
            new StatusEffect { Name = "Oil Slick", Type = StatusEffectType.Debuff,         ResistanceType = ResistanceType.Magic, Duration = 3, TurnMeterModifier = -4, ApplicationChance = 40, StackRule = StackRule.HighestWins },
            new StatusEffect { Name = "Thorns",    Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Magic, Duration = 2, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 30, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell Smite = new()
    {
        Name = "Smite", Description = "A blast of holy light",
        School = SpellSchool.Evocation, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Holy, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 20,
    };
    private static readonly Spell Moonfire = new()
    {
        Name = "Moonfire", Description = "Lunar energy sears the target",
        School = SpellSchool.Evocation, DamageDie = DieType.D6, DamageCount = 2,
        DamageType = DamageType.Lightning, AttackBonus = 1, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 18,
        OnHitEffects =
        [
            new StatusEffect { Name = "Burning", Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Magic, Duration = 2, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 40, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell Entangle = new()
    {
        Name = "Entangle", Description = "Grasping vines root the target",
        School = SpellSchool.CC, DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackBonus = 0, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 15,
        OnHitEffects =
        [
            new StatusEffect { Name = "Rooted", Type = StatusEffectType.Root, ResistanceType = ResistanceType.Magic, Duration = 2, ApplicationChance = 80, StackRule = StackRule.NoStack }
        ]
    };

    // ── Summoned companions (must precede any character that references them) ────
    private static readonly Pet SpiritWolf = new()
    {
        Name = "Spirit Wolf",
        Description = "A spectral wolf bound to protect its summoner",
        MaxHitPoints = 20,
        ArmorClass = 12,
        TurnSpeed = 10,
        Strength = 14,
        StrikeRating = 14,
        AttackBonus = 2,
        DamageCount = 1,
        DamageDie = DieType.D6,
        DamageType = DamageType.Slashing,
        SummonDurationRounds = 3
    };

    private static readonly Spell SummonSpiritWolf = new()
    {
        Name = "Summon: Spirit Wolf",
        Description = "Calls a spectral wolf to defend the caster",
        School = SpellSchool.Conjuration,
        DamageDie = DieType.D4,
        DamageCount = 0,
        DamageType = DamageType.Bludgeoning,
        AttackBonus = 0,
        SpellLevel = 3,
        TurnMeterCost = 90,
        ManaCost = 35,
        SummonedPet = SpiritWolf
    };

    // ── Hero characters ──────────────────────────────────────────────────────────
    private static readonly Character Theron = new()
    {
        Name = "Theron", Level = 5, Strength = 18, Dexterity = 12, Intelligence = 10,
        Race = HumanRace,
        ClassId = 8, ClassName = "Fighter", Sex = "M", StrikeRating = 14, TurnSpeed = 10, MaxHitPoints = 50, CurrentHitPoints = 50,
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
        ClassId = 1, ClassName = "Barbarian", Sex = "M", StrikeRating = 16, TurnSpeed = 6, MaxHitPoints = 35, CurrentHitPoints = 35,
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
        ClassId = 5, ClassName = "Mage", Sex = "F", StrikeRating = 13, TurnSpeed = 8, MaxHitPoints = 30, CurrentHitPoints = 30, MaxMana = 165, CurrentMana = 165,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Mage Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 }
        },
        MemorizedSpells = [Fireball, IceBolt, LightningStrike, BladeBarrier]
    };
    private static readonly Character Sera = new()
    {
        Name = "Sera", Level = 4, Strength = 12, Dexterity = 10, Intelligence = 16,
        Race = HumanRace,
        ClassId = 4, ClassName = "Priest", Sex = "F", StrikeRating = 14, TurnSpeed = 8, MaxHitPoints = 35, CurrentHitPoints = 35, MaxMana = 130, CurrentMana = 130,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Scaled Vestments", ArmorClass = 12, Mitigation = 1, MaxDexterityBonus = 6 },
            RightHand = CeremonialMace
        },
        MemorizedSpells = [Smite]
    };
    private static readonly Character Elara = new()
    {
        Name = "Elara", Level = 4, Strength = 8, Dexterity = 14, Intelligence = 17,
        Race = ElfRace,
        ClassId = 7, ClassName = "Druid", Sex = "F", StrikeRating = 14, TurnSpeed = 9, MaxHitPoints = 28, CurrentHitPoints = 28, MaxMana = 140, CurrentMana = 140,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Druidic Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 },
            RightHand = ArcaneStaff
        },
        MemorizedSpells = [Moonfire, Entangle, SummonSpiritWolf]
    };

    // ── Enemy weapons ─────────────────────────────────────────────────────────────
    private static readonly Weapon OrcAxe = new()
    {
        Name = "Orcish Axe", DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1,
        Archetype = ArchetypeWeapon.Axe, Hands = 1
    };
    private static readonly Weapon GoblinDagger = new()
    {
        Name = "Poisoned Dagger", DamageDie = DieType.D4, DamageCount = 2,
        DamageType = DamageType.Piercing, AttackType = AttackType.Melee, AttackBonus = 3,
        Archetype = ArchetypeWeapon.Dagger, Hands = 1
    };

    // ── Enemy spells ──────────────────────────────────────────────────────────────
    private static readonly Spell ShadowBolt = new()
    {
        Name = "Shadow Bolt", Description = "A bolt of shadow energy",
        School = SpellSchool.Other, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Ice, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 20,
        OnHitEffects =
        [
            new StatusEffect { Name = "Chilled",   Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Cold, Duration = 2, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 20, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell SoulDrain = new()
    {
        Name = "Soul Drain", Description = "Saps the life force of a target",
        School = SpellSchool.Other, DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Fire, AttackBonus = 1, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 15,
        OnHitEffects =
        [
            new StatusEffect { Name = "Burning",   Type = StatusEffectType.DamageOverTime, ResistanceType = ResistanceType.Fire, Duration = 3, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 20, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell Root = new()
    {
        Name = "Root", Description = "Anchors the target with grasping vines",
        School = SpellSchool.CC, DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackBonus = 0, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 20,
        OnHitEffects =
        [
            new StatusEffect { Name = "Rooted",    Type = StatusEffectType.Root,            ResistanceType = ResistanceType.Magic, Duration = 2, ApplicationChance = 100, StackRule = StackRule.NoStack },
            new StatusEffect { Name = "Thorns",    Type = StatusEffectType.DamageOverTime,  ResistanceType = ResistanceType.Magic, Duration = 3, DoTDamageCount = 1, DoTDamageDie = DieType.D4, ApplicationChance = 40, StackRule = StackRule.HighestWins }
        ]
    };
    private static readonly Spell Curse = new()
    {
        Name = "Curse", Description = "Dark energy weakens the target",
        School = SpellSchool.CC, DamageDie = DieType.D6, DamageCount = 1,
        DamageType = DamageType.Shadow, AttackBonus = 0, SpellLevel = 2, TurnMeterCost = 70, ManaCost = 12,
        OnHitEffects =
        [
            new StatusEffect { Name = "Weakened", Type = StatusEffectType.Debuff, ResistanceType = ResistanceType.Magic, Duration = 3, AttackPowerModifier = -2, DefensePowerModifier = -2, ApplicationChance = 70, StackRule = StackRule.HighestWins }
        ]
    };

    // ── Enemy characters ──────────────────────────────────────────────────────────
    internal static readonly Character Krag = new()
    {
        Name = "Krag", Level = 4, Strength = 17, Dexterity = 9, Intelligence = 6,
        Race = OrcRace,
        ClassId = 1, ClassName = "Barbarian", Sex = "M", StrikeRating = 15, TurnSpeed = 7, MaxHitPoints = 45, CurrentHitPoints = 45,
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
        ClassId = 9, ClassName = "Rogue", Sex = "M", StrikeRating = 12, TurnSpeed = 12, MaxHitPoints = 20, CurrentHitPoints = 20,
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
        ClassId = 5, ClassName = "Mage", Sex = "M", StrikeRating = 14, TurnSpeed = 9, MaxHitPoints = 25, CurrentHitPoints = 25, MaxMana = 100, CurrentMana = 100,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Dark Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 }
        },
        MemorizedSpells = [ShadowBolt, SoulDrain, Root]
    };
    internal static readonly Character Zarath = new()
    {
        Name = "Zarath", Level = 5, Strength = 6, Dexterity = 12, Intelligence = 18,
        Race = DarkMageRace,
        ClassId = 5, ClassName = "Mage", Sex = "M", StrikeRating = 15, TurnSpeed = 8, MaxHitPoints = 28, CurrentHitPoints = 28, MaxMana = 170, CurrentMana = 170,
        Equipment = new ArmorSlots
        {
            Chest = new Armor { Name = "Shadowweave Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 }
        },
        MemorizedSpells = [ShadowBolt, SoulDrain, Curse]
    };

    // ── Lookup table initialization ──────────────────────────────────────────────

    private static void InitializeData()
    {
        AllHeroes = new()
        {
            ['T'] = Theron,
            ['G'] = Gruk,
            ['L'] = Lyra,
            ['S'] = Sera,
            ['E'] = Elara
        };
        AttackMap = new()
        {
            [Theron.Name] = Longsword,
            [Gruk.Name] = BattleAxe,
            [Lyra.Name] = null,
            [Sera.Name] = CeremonialMace,
            [Elara.Name] = null,
            [Krag.Name] = OrcAxe,
            [Skrix.Name] = GoblinDagger,
            [Mordak.Name] = null,
            [Zarath.Name] = null
        };
    }
}
