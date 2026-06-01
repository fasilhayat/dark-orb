namespace BattleArena.Application.Modifiers;

using Core.Entities.Enums;
using Core.Interfaces;
using Core.Models;

/// <summary>
/// Adjusts attack and defense power based on the combat terrain and each
/// combatant's racial affinity. Each race thrives in some environments and
/// struggles in others. Humans are adaptable and receive no modifiers.
///
/// Priority band 20 — environmental.
/// </summary>
public sealed class TerrainModifier : ICombatModifier
{
    public string      Name     => "Terrain";
    public int         Priority => 20;
    public CombatPhase Phase    => CombatPhase.AttackRoll;

    private static readonly Dictionary<string, Dictionary<TerrainType, (int Atk, int Def)>> RaceTerrainMap = new()
    {
        ["Elf"] = new()
        {
            { TerrainType.Forest,  (2, 0) },
            { TerrainType.Desert,  (-1, 0) },
            { TerrainType.Swamp,   (-1, 0) }
        },
        ["Dwarf"] = new()
        {
            { TerrainType.Mountain, (0, 2) },
            { TerrainType.Rocky,    (0, 1) },
            { TerrainType.Swamp,    (0, -1) }
        },
        ["Lizard"] = new()
        {
            { TerrainType.Desert, (1, 1) },
            { TerrainType.Swamp,  (1, 1) },
            { TerrainType.Icy,    (-1, -1) }
        },
        ["Orc"] = new()
        {
            { TerrainType.Desert,  (1, 0) },
            { TerrainType.Mountain, (1, 0) },
            { TerrainType.Forest,  (-1, 0) },
            { TerrainType.Swamp,   (-1, 0) }
        },
        ["Ogre"] = new()
        {
            { TerrainType.Mountain, (2, 0) },
            { TerrainType.Rocky,    (0, 1) },
            { TerrainType.Forest,   (-1, 0) }
        },
        ["Kobold"] = new()
        {
            { TerrainType.Desert, (1, 0) },
            { TerrainType.Rocky,  (1, 0) },
            { TerrainType.Forest, (-1, 0) }
        },
        ["Gladefolk"] = new()
        {
            { TerrainType.Forest, (1, 1) },
            { TerrainType.Jungle, (1, 0) },
            { TerrainType.Desert, (-1, 0) }
        },
        ["Undead"] = new()
        {
            { TerrainType.Icy, (1, 0) }
        },
        ["Demon"] = new()
        {
            { TerrainType.Desert, (1, 0) }
        }
    };

    public void Apply(CombatModifierContext ctx)
    {
        if (ctx.Terrain == TerrainType.Plains)
            return;

        var attackerRace = ctx.Attacker.Race?.Name ?? "";
        var defenderRace = ctx.Defender.Race?.Name ?? "";

        if (RaceTerrainMap.TryGetValue(attackerRace, out var atkMap) &&
            atkMap.TryGetValue(ctx.Terrain, out var atkMod))
        {
            ctx.AttackPowerDelta += atkMod.Atk;
        }

        if (RaceTerrainMap.TryGetValue(defenderRace, out var defMap) &&
            defMap.TryGetValue(ctx.Terrain, out var defMod))
        {
            ctx.DefensePowerDelta += defMod.Def;
        }
    }
}
