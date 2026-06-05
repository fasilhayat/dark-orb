namespace BattleArena.Core.Entities.Enums;

// AD&D 2e weapon proficiency rules expressed per archetype.
//
// Class IDs:
//   1 = Barbarian   2 = Knight   3 = Paladin   4 = Priest
//   5 = Mage        6 = Bard     7 = Druid     8 = Fighter   9 = Rogue   10 = Ranger
//
// Group shortcuts:
//   Warriors (1,2,3,8)  — unrestricted, all weapons
//   Rogues   (6,9)      — finesse/light/ranged weapons
//   Priests  (4,7)      — bludgeoning/divine only; Druids also allow edged (dagger/scimitar/spear)
//   Mages    (5)        — dagger, staff, wand only
//
// Any Weapon whose Archetype is set inherits these restrictions automatically.
public static class ArchetypeWeaponExtensions
{
    // Pre-built HashSets for O(1) lookup — allocated once at class-load time.
    private static readonly Dictionary<ArchetypeWeapon, HashSet<int>> _allowedClasses = new()
    {
        // ── Bladed / finesse ─────────────────────────────────────────────────
        // Daggers are the universal side-arm; every class can carry one,
        // including Priests who treat it as a ritual implement / ceremonial blade.
        // Rangers also carry daggers as backup weapons.
        [ArchetypeWeapon.Dagger]     = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],

        // Short swords: warriors, rogues, and rangers.
        [ArchetypeWeapon.ShortSword] = [1, 2, 3, 6, 8, 9, 10],

        // Long swords / broad swords: warriors, rogues, bard.
        // Druid may use scimitar (treated as Sword in this system).
        [ArchetypeWeapon.Sword]      = [1, 2, 3, 6, 7, 8, 9],

        // ── Two-handed swords (great swords) — warrior classes only ────────
        // Barbarians gain extra bonuses for wielding these.
        [ArchetypeWeapon.TwoHandedSword] = [1, 2, 3, 8],

        // ── Chopping ─────────────────────────────────────────────────────────
        // Axes are exclusively warrior-class weapons.
        [ArchetypeWeapon.Axe]        = [1, 2, 3, 8],

        // Two-handed battle-axes — warrior classes, Barbarian gains extra bonus.
        [ArchetypeWeapon.TwoHandedBattleAxe] = [1, 2, 3, 8],

        // ── Bludgeoning ──────────────────────────────────────────────────────
        // Maces, hammers, and morning stars are the divine casters' melee choice.
        // Warriors also wield them. Rogues and Mages do not.
        [ArchetypeWeapon.Mace]       = [1, 2, 3, 4, 7, 8],
        [ArchetypeWeapon.Hammer]     = [1, 2, 3, 4, 7, 8],

        // Two-handed warhammer — warriors + Priest; Paladin gains extra bonus.
        [ArchetypeWeapon.TwoHandedWarhammer] = [1, 2, 3, 4, 8],

        // Morning star: treated like mace but Druid traditionally skips it.
        [ArchetypeWeapon.MorningStar] = [1, 2, 3, 4, 8],

        // ── Polearms / reach ─────────────────────────────────────────────────
        // Lances: mounted warriors only.
        [ArchetypeWeapon.Lance]      = [1, 2, 3, 8],

        // Spears: broad melee weapon — warriors, bard, druid, ranger.
        [ArchetypeWeapon.Spear]      = [1, 2, 3, 6, 7, 8, 10],

        // ── Arcane / divine focus ────────────────────────────────────────────
        // Quarterstaff: the most universally allowed weapon in AD&D (every class).
        [ArchetypeWeapon.Staff]      = [1, 2, 3, 4, 5, 6, 7, 8, 9],

        // Wand: arcane focus, mage exclusive.
        [ArchetypeWeapon.Wand]       = [5],

        // ── Ranged ───────────────────────────────────────────────────────────
        // Bows: warriors and rogues. Rangers get bonuses on long-range weapons.
        [ArchetypeWeapon.Bow]        = [1, 2, 3, 6, 8, 9, 10],

        // Crossbows: same as bows (mechanical, so Druids skip).
        [ArchetypeWeapon.Crossbow]   = [1, 2, 3, 6, 8, 9, 10],

        // Slings: the divine-caster ranged option (blunt projectile); also rogues/warriors.
        // Mages are the only class that cannot use a sling.
        [ArchetypeWeapon.Sling]      = [1, 2, 3, 4, 6, 7, 8, 9],
    };

    /// <summary>
    /// Returns true if a character of the given <paramref name="classId"/> may wield any
    /// weapon whose archetype is <paramref name="archetype"/>, following AD&amp;D 2e rules.
    /// </summary>
    public static bool IsUsableByClass(this ArchetypeWeapon archetype, int classId) =>
        _allowedClasses.TryGetValue(archetype, out var allowed) && allowed.Contains(classId);
}
