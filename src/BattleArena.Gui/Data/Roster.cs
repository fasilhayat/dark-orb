namespace BattleArena.Gui.Data;

using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

internal static class Roster
{
    internal static readonly Race Human = new() { Name = "Human", BaseMovementSpeed = 30 };
    internal static readonly Race Orc = new() { Name = "Orc", BaseMovementSpeed = 30 };
    internal static readonly Race Elf = new() { Name = "Elf", BaseMovementSpeed = 35 };
    internal static readonly Race Undead = new() { Name = "Undead", BaseMovementSpeed = 30 };

    internal static readonly Weapon Longsword = new()
    {
        Name = "Longsword", DamageDie = DieType.D8, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2,
        Archetype = ArchetypeWeapon.Sword, Hands = 1
    };
    internal static readonly Weapon BattleAxe = new()
    {
        Name = "Battle Axe", DamageDie = DieType.D8, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1,
        Archetype = ArchetypeWeapon.Axe, Hands = 1
    };
    internal static readonly Weapon OrcishAxe = new()
    {
        Name = "Orcish Axe", DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1,
        Archetype = ArchetypeWeapon.Axe, Hands = 1
    };
    internal static readonly Weapon CeremonialMace = new()
    {
        Name = "Ceremonial Mace", DamageDie = DieType.D6, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 2,
        Archetype = ArchetypeWeapon.Mace, Hands = 1
    };
    internal static readonly Weapon ArcaneStaff = new()
    {
        Name = "Arcane Staff", DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee, AttackBonus = 1,
        Archetype = ArchetypeWeapon.Staff, Hands = 2
    };
    internal static readonly Weapon GoblinDagger = new()
    {
        Name = "Poisoned Dagger", DamageDie = DieType.D4, DamageCount = 2,
        DamageType = DamageType.Piercing, AttackType = AttackType.Melee, AttackBonus = 3,
        Archetype = ArchetypeWeapon.Dagger, Hands = 1
    };

    internal static readonly Spell Fireball = new()
    {
        Name = "Fireball", School = SpellSchool.Evocation, DamageDie = DieType.D6, DamageCount = 3,
        DamageType = DamageType.Fire, AttackBonus = 2, SpellLevel = 3, TurnMeterCost = 90, ManaCost = 50
    };
    internal static readonly Spell IceBolt = new()
    {
        Name = "Ice Bolt", School = SpellSchool.Evocation, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Ice, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 35
    };
    internal static readonly Spell Shock = new()
    {
        Name = "Shock", School = SpellSchool.Evocation, DamageDie = DieType.D6, DamageCount = 2,
        DamageType = DamageType.Lightning, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 75, ManaCost = 20
    };
    internal static readonly Spell Smite = new()
    {
        Name = "Smite", School = SpellSchool.Evocation, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Holy, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 35
    };
    internal static readonly Spell Heal = new()
    {
        Name = "Heal", School = SpellSchool.Healing, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Holy, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 25
    };
    internal static readonly Spell Moonfire = new()
    {
        Name = "Moonfire", School = SpellSchool.Evocation, DamageDie = DieType.D6, DamageCount = 2,
        DamageType = DamageType.Lightning, AttackBonus = 1, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 30
    };
    internal static readonly Spell Entangle = new()
    {
        Name = "Entangle", School = SpellSchool.CC, DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 25
    };
    internal static readonly Spell ShadowBolt = new()
    {
        Name = "Shadow Bolt", School = SpellSchool.Other, DamageDie = DieType.D8, DamageCount = 2,
        DamageType = DamageType.Ice, AttackBonus = 2, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 35
    };
    internal static readonly Spell SoulDrain = new()
    {
        Name = "Soul Drain", School = SpellSchool.Other, DamageDie = DieType.D10, DamageCount = 1,
        DamageType = DamageType.Fire, AttackBonus = 1, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 25
    };
    internal static readonly Spell Root = new()
    {
        Name = "Root", School = SpellSchool.CC, DamageDie = DieType.D4, DamageCount = 1,
        DamageType = DamageType.Bludgeoning, SpellLevel = 2, TurnMeterCost = 80, ManaCost = 30
    };
    internal static readonly Spell Curse = new()
    {
        Name = "Curse", School = SpellSchool.CC, DamageDie = DieType.D6, DamageCount = 1,
        DamageType = DamageType.Shadow, SpellLevel = 2, TurnMeterCost = 70, ManaCost = 20
    };

    internal static readonly Armor ChainMail = new() { Name = "Chain Mail", ArmorClass = 16, Mitigation = 2, MaxDexterityBonus = 6, MovementPenalty = 10 };
    internal static readonly Armor LeatherArmor = new() { Name = "Leather Armor", ArmorClass = 11, Mitigation = 1, MaxDexterityBonus = 6, MovementPenalty = 5 };
    internal static readonly Armor MageRobes = new() { Name = "Mage Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 };
    internal static readonly Armor ScaledVestments = new() { Name = "Scaled Vestments", ArmorClass = 12, Mitigation = 1, MaxDexterityBonus = 6, MovementPenalty = 5 };
    internal static readonly Armor DruidicRobes = new() { Name = "Druidic Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 };
    internal static readonly Armor OrcishHide = new() { Name = "Orcish Hide", ArmorClass = 12, Mitigation = 2, MaxDexterityBonus = 4, MovementPenalty = 5 };
    internal static readonly Armor WornLeather = new() { Name = "Worn Leather", ArmorClass = 11, Mitigation = 1, MaxDexterityBonus = 6, MovementPenalty = 5 };
    internal static readonly Armor DarkRobes = new() { Name = "Dark Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 };
    internal static readonly Armor ShadowweaveRobes = new() { Name = "Shadowweave Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6, TurnMeterCostReduction = 5 };

    internal static readonly Character Theron = new()
    {
        Name = "Theron", Level = 5, Strength = 18, Dexterity = 12, Intelligence = 10,
        Race = Human, ClassId = 8, ClassName = "Fighter", Sex = "M",
        StrikeRating = 14, TurnSpeed = 10, MaxHitPoints = 50, CurrentHitPoints = 50,
        Equipment = new ArmorSlots { Chest = ChainMail, RightHand = Longsword }
    };

    internal static readonly Character Gruk = new()
    {
        Name = "Gruk", Level = 3, Strength = 16, Dexterity = 8, Intelligence = 8,
        Race = Orc, ClassId = 1, ClassName = "Barbarian", Sex = "M",
        StrikeRating = 16, TurnSpeed = 6, MaxHitPoints = 35, CurrentHitPoints = 35,
        Equipment = new ArmorSlots { Chest = LeatherArmor, RightHand = BattleAxe }
    };

    internal static readonly Character Lyra = new()
    {
        Name = "Lyra", Level = 5, Strength = 8, Dexterity = 14, Intelligence = 18,
        Race = Elf, ClassId = 5, ClassName = "Mage", Sex = "F",
        StrikeRating = 13, TurnSpeed = 8, MaxHitPoints = 30, CurrentHitPoints = 30,
        MaxMana = 155, CurrentMana = 155,
        Equipment = new ArmorSlots { Chest = MageRobes },
        MemorizedSpells = [Fireball, IceBolt, Shock]
    };

    internal static readonly Character Sera = new()
    {
        Name = "Sera", Level = 4, Strength = 12, Dexterity = 10, Intelligence = 16,
        Race = Human, ClassId = 4, ClassName = "Priest", Sex = "F",
        StrikeRating = 14, TurnSpeed = 8, MaxHitPoints = 35, CurrentHitPoints = 35,
        MaxMana = 100, CurrentMana = 100,
        Equipment = new ArmorSlots { Chest = ScaledVestments, RightHand = CeremonialMace },
        MemorizedSpells = [Smite, Heal]
    };

    internal static readonly Character Elara = new()
    {
        Name = "Elara", Level = 4, Strength = 8, Dexterity = 14, Intelligence = 17,
        Race = Elf, ClassId = 7, ClassName = "Druid", Sex = "F",
        StrikeRating = 14, TurnSpeed = 9, MaxHitPoints = 28, CurrentHitPoints = 28,
        MaxMana = 110, CurrentMana = 110,
        Equipment = new ArmorSlots { Chest = DruidicRobes, RightHand = ArcaneStaff },
        MemorizedSpells = [Moonfire, Entangle]
    };

    internal static readonly List<Character> AllHeroes = [Theron, Gruk, Lyra, Sera, Elara];

    internal static readonly Character Krag = new()
    {
        Name = "Krag", Level = 4, Strength = 17, Dexterity = 9, Intelligence = 6,
        Race = Orc, ClassId = 1, ClassName = "Barbarian", Sex = "M",
        StrikeRating = 15, TurnSpeed = 7, MaxHitPoints = 45, CurrentHitPoints = 45,
        Equipment = new ArmorSlots { Chest = OrcishHide, RightHand = OrcishAxe }
    };

    internal static readonly Character Skrix = new()
    {
        Name = "Skrix", Level = 2, Strength = 9, Dexterity = 16, Intelligence = 10,
        Race = Human, ClassId = 9, ClassName = "Rogue", Sex = "M",
        StrikeRating = 12, TurnSpeed = 12, MaxHitPoints = 20, CurrentHitPoints = 20,
        Equipment = new ArmorSlots { Chest = WornLeather, RightHand = GoblinDagger }
    };

    internal static readonly Character Mordak = new()
    {
        Name = "Mordak", Level = 3, Strength = 7, Dexterity = 12, Intelligence = 16,
        Race = Undead, ClassId = 5, ClassName = "Mage", Sex = "M",
        StrikeRating = 14, TurnSpeed = 9, MaxHitPoints = 25, CurrentHitPoints = 25,
        MaxMana = 60, CurrentMana = 60,
        Equipment = new ArmorSlots { Chest = DarkRobes },
        MemorizedSpells = [ShadowBolt, SoulDrain, Root]
    };

    internal static readonly Character Zarath = new()
    {
        Name = "Zarath", Level = 5, Strength = 6, Dexterity = 12, Intelligence = 18,
        Race = Undead, ClassId = 5, ClassName = "Mage", Sex = "M",
        StrikeRating = 15, TurnSpeed = 8, MaxHitPoints = 28, CurrentHitPoints = 28,
        MaxMana = 85, CurrentMana = 85,
        Equipment = new ArmorSlots { Chest = ShadowweaveRobes },
        MemorizedSpells = [ShadowBolt, SoulDrain, Curse]
    };

    internal static readonly List<Character> AllEnemies = [Krag, Skrix, Mordak, Zarath];

    internal static IAttackSource? GetAttackSource(Character c)
    {
        if (c.MemorizedSpells.Count > 0)
            return null;
        return (IAttackSource?)c.Equipment.RightHand ?? UnarmedStrike.Default;
    }
}
