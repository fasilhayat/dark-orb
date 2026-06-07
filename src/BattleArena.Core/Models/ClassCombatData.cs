namespace BattleArena.Core.Models;

using Core.Entities.Enums;

/// <summary>
/// Static lookup for class combat data (attacks per turn, armor restrictions, etc.).
/// Used as fallback when PlayerClass is not loaded from the database.
/// Must stay in sync with PlayerClass DB seed data.
/// Class IDs: 1=Barbarian, 2=Knight, 3=Paladin, 4=Priest, 5=Mage, 6=Bard, 7=Druid, 8=Fighter, 9=Rogue, 10=Ranger
/// </summary>
public static class ClassCombatData
{
    private static readonly int[] _attacksPerTurn =
        [1,  // 0 unused
         3,  // 1 Barbarian
         2,  // 2 Knight
         2,  // 3 Paladin
         1,  // 4 Priest
         1,  // 5 Mage
         1,  // 6 Bard
         1,  // 7 Druid
         2,  // 8 Fighter
         1,  // 9 Rogue
         2,  // 10 Ranger
        ];

    private static readonly int[] _bowAttacksPerTurn =
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // 0-9
         3,  // 10 Ranger
        ];

    private static readonly string?[] _armorRestrictions =
        [null,
         "Light",   // 1 Barbarian
         null,      // 2 Knight
         null,      // 3 Paladin
         null,      // 4 Priest
         null,      // 5 Mage
         null,      // 6 Bard
         null,      // 7 Druid
         null,      // 8 Fighter
         null,      // 9 Rogue
         null,      // 10 Ranger
        ];

    private static readonly bool[] _canDualWield =
        [false,
         false,  // 1 Barbarian
         false,  // 2 Knight
         false,  // 3 Paladin
         false,  // 4 Priest
         false,  // 5 Mage
         false,  // 6 Bard
         false,  // 7 Druid
         true,   // 8 Fighter
         true,   // 9 Rogue
         true,   // 10 Ranger
        ];

    private static readonly double[] _weaponSwitchCostMultiplier =
        [1.0,
         0.0,  // 1 Barbarian
         0.5,  // 2 Knight
         0.5,  // 3 Paladin
         1.0,  // 4 Priest
         1.0,  // 5 Mage
         1.0,  // 6 Bard
         1.0,  // 7 Druid
         0.5,  // 8 Fighter
         1.0,  // 9 Rogue
         0.0,  // 10 Ranger
        ];

    private static readonly int[] _twoHandedWeaponBonus =
        [0,
         2,  // 1 Barbarian
         0,  // 2 Knight
         2,  // 3 Paladin
         0,  // 4 Priest
         0,  // 5 Mage
         0,  // 6 Bard
         0,  // 7 Druid
         0,  // 8 Fighter
         0,  // 9 Rogue
         0,  // 10 Ranger
        ];

    private static readonly int[] _shieldBonusDamage =
        [0,
         0,  // 1 Barbarian
         2,  // 2 Knight
         0,  // 3 Paladin
         0,  // 4 Priest
         0,  // 5 Mage
         0,  // 6 Bard
         0,  // 7 Druid
         0,  // 8 Fighter
         0,  // 9 Rogue
         0,  // 10 Ranger
        ];

    private static readonly int[] _rangedAttackBonus =
        [0,
         0,  // 1 Barbarian
         0,  // 2 Knight
         0,  // 3 Paladin
         0,  // 4 Priest
         0,  // 5 Mage
         0,  // 6 Bard
         0,  // 7 Druid
         0,  // 8 Fighter
         0,  // 9 Rogue
         1,  // 10 Ranger
        ];

    public static int AttacksPerTurn(int classId) =>
        classId >= 0 && classId < _attacksPerTurn.Length ? _attacksPerTurn[classId] : 1;

    public static int BowAttacksPerTurn(int classId) =>
        classId >= 0 && classId < _bowAttacksPerTurn.Length ? _bowAttacksPerTurn[classId] : 0;

    public static string? ArmorRestriction(int classId) =>
        classId >= 0 && classId < _armorRestrictions.Length ? _armorRestrictions[classId] : null;

    public static bool CanDualWield(int classId) =>
        classId >= 0 && classId < _canDualWield.Length && _canDualWield[classId];

    public static double WeaponSwitchCostMultiplier(int classId) =>
        classId >= 0 && classId < _weaponSwitchCostMultiplier.Length ? _weaponSwitchCostMultiplier[classId] : 1.0;

    public static int TwoHandedWeaponBonus(int classId) =>
        classId >= 0 && classId < _twoHandedWeaponBonus.Length ? _twoHandedWeaponBonus[classId] : 0;

    public static int ShieldBonusDamage(int classId) =>
        classId >= 0 && classId < _shieldBonusDamage.Length ? _shieldBonusDamage[classId] : 0;

    public static int RangedAttackBonus(int classId) =>
        classId >= 0 && classId < _rangedAttackBonus.Length ? _rangedAttackBonus[classId] : 0;

    public static bool IsTwoHandedArchetype(ArchetypeWeapon archetype) =>
        archetype is ArchetypeWeapon.TwoHandedSword
            or ArchetypeWeapon.TwoHandedBattleAxe
            or ArchetypeWeapon.TwoHandedWarhammer
            or ArchetypeWeapon.TwoHandedMace;

    public static bool IsBowArchetype(ArchetypeWeapon archetype) =>
        archetype is ArchetypeWeapon.Bow;
}
