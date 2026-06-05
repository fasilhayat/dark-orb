using System.Text.Json;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Services;

public sealed class RosterData
{
    public List<Character> Heroes { get; init; } = [];
    public List<Character> Enemies { get; init; } = [];
}

public static class RosterLoader
{
    private static RosterData? _cached;
    private static readonly object _lock = new();

    public static RosterData Load(string jsonPath)
    {
        if (_cached is not null)
            return _cached;

        lock (_lock)
        {
            if (_cached is not null)
                return _cached;
            return ForceLoad(jsonPath);
        }
    }

    /// <summary>Bypasses cache — used by tests and multi-consumer scenarios.</summary>
    public static RosterData ForceLoad(string jsonPath)
    {
        var text = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dto = JsonSerializer.Deserialize<RosterFileDto>(text, options)
            ?? throw new InvalidOperationException("Failed to deserialize roster.json");

        var races   = BuildRaces(dto.Races);
        var weapons = BuildWeapons(dto.Weapons);
        var spells  = BuildSpells(dto.Spells);
        var armors  = BuildArmors(dto.Armors);

        var heroes  = BuildCharacters(dto.Heroes, races, weapons, spells, armors);
        var enemies = BuildCharacters(dto.Enemies, races, weapons, spells, armors);

        _cached = new RosterData { Heroes = heroes, Enemies = enemies };
        return _cached;
    }

    private static Dictionary<string, Race> BuildRaces(IReadOnlyList<RaceDto> dtos)
    {
        var result = new Dictionary<string, Race>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in dtos)
            result[d.Name] = new Race { Name = d.Name, BaseMovementSpeed = d.BaseMovementSpeed };
        return result;
    }

    private static Dictionary<string, Weapon> BuildWeapons(IReadOnlyList<WeaponDto> dtos)
    {
        var result = new Dictionary<string, Weapon>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in dtos)
        {
            result[d.Name] = new Weapon
            {
                Name        = d.Name,
                DamageDie   = ParseEnum<DieType>(d.DamageDie),
                DamageCount = d.DamageCount,
                DamageType  = ParseEnum<DamageType>(d.DamageType),
                AttackType  = ParseEnum<AttackType>(d.AttackType),
                AttackBonus = d.AttackBonus,
                Archetype   = ParseEnum<ArchetypeWeapon>(d.Archetype),
                Hands       = d.Hands
            };
        }
        return result;
    }

    private static Dictionary<string, Spell> BuildSpells(IReadOnlyList<SpellDto> dtos)
    {
        var result = new Dictionary<string, Spell>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in dtos)
        {
            var damageType = ParseEnum<DamageType>(d.DamageType);
            var spell = new Spell
            {
                Name          = d.Name,
                School        = ParseEnum<SpellSchool>(d.School),
                DamageDie     = ParseEnum<DieType>(d.DamageDie),
                DamageCount   = d.DamageCount,
                DamageType    = damageType,
                ElementalType = InferElementalType(damageType),
                AttackBonus   = d.AttackBonus,
                SpellLevel    = d.SpellLevel,
                TurnMeterCost = d.TurnMeterCost,
                ManaCost      = d.ManaCost
            };
            if (d.OnHitEffects is { Count: > 0 })
            {
                foreach (var e in d.OnHitEffects)
                {
                    spell.OnHitEffects.Add(new StatusEffect
                    {
                        Name              = e.Name,
                        Type              = ParseEnum<StatusEffectType>(e.Type),
                        Target            = ParseEnum<EffectTarget>(e.Target),
                        ResistanceType    = ParseEnum<ResistanceType>(e.ResistanceType),
                        Duration          = e.Duration,
                        ApplicationChance = e.ApplicationChance,
                        ReflectChance     = e.ReflectChance
                    });
                }
            }
            result[d.Name] = spell;
        }
        return result;
    }

    private static Dictionary<string, Armor> BuildArmors(IReadOnlyList<ArmorDto> dtos)
    {
        var result = new Dictionary<string, Armor>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in dtos)
        {
            result[d.Name] = new Armor
            {
                Name                  = d.Name,
                ArmorClass            = d.ArmorClass,
                Mitigation            = d.Mitigation,
                MaxDexterityBonus     = d.MaxDexterityBonus,
                MovementPenalty       = d.MovementPenalty,
                TurnMeterCostReduction = d.TurnMeterCostReduction
            };
        }
        return result;
    }

    private static List<Character> BuildCharacters(
        IReadOnlyList<CharacterDto> dtos,
        Dictionary<string, Race>   races,
        Dictionary<string, Weapon> weapons,
        Dictionary<string, Spell>  spells,
        Dictionary<string, Armor>  armors)
    {
        var result = new List<Character>(dtos.Count);
        foreach (var d in dtos)
        {
            var ch = new Character
            {
                Name             = d.Name,
                Level            = d.Level,
                Strength         = d.Strength,
                Dexterity        = d.Dexterity,
                Stamina          = d.Stamina,
                Intelligence     = d.Intelligence,
                Wisdom           = d.Wisdom,
                Charisma         = d.Charisma,
                Race             = races.GetValueOrDefault(d.Race),
                ClassId          = d.ClassId,
                ClassName        = d.ClassName,
                Sex              = d.Sex,
                StrikeRating     = d.StrikeRating,
                TurnSpeed        = d.TurnSpeed,
                MaxHitPoints     = d.MaxHitPoints,
                CurrentHitPoints = d.MaxHitPoints,
                MaxMana          = d.MaxMana,
                CurrentMana      = d.MaxMana
            };

            var slots = new ArmorSlots();
            foreach (var (slotKey, itemName) in d.Equipment ?? new Dictionary<string, string>())
            {
                var normalized = slotKey.ToLowerInvariant();
                if (normalized is "chest" or "head" or "hands" or "waist" or "boots" or "neck" or "back")
                {
                    if (armors.TryGetValue(itemName, out var armor))
                        SetArmorSlot(slots, normalized, armor);
                }
                else if (normalized is "righthand" or "lefthand")
                {
                    if (weapons.TryGetValue(itemName, out var weapon))
                        SetWeaponSlot(slots, normalized, weapon);
                }
            }

            if (slots.Chest is not null || slots.RightHand is not null)
                ch.Equipment = slots;

            if (d.MemorizedSpells is { Count: > 0 })
            {
                foreach (var spellName in d.MemorizedSpells)
                    if (spells.TryGetValue(spellName, out var spell))
                        ch.MemorizedSpells.Add(spell);
            }

            result.Add(ch);
        }
        return result;
    }

    private static void SetArmorSlot(ArmorSlots slots, string normalized, Armor armor)
    {
        switch (normalized)
        {
            case "head":  slots.Head  = armor; break;
            case "chest": slots.Chest = armor; break;
            case "hands": slots.Hands = armor; break;
            case "waist": slots.Waist = armor; break;
            case "boots": slots.Boots = armor; break;
            case "neck":  slots.Neck  = armor; break;
            case "back":  slots.Back  = armor; break;
        }
    }

    private static void SetWeaponSlot(ArmorSlots slots, string normalized, Weapon weapon)
    {
        switch (normalized)
        {
            case "righthand": slots.RightHand = weapon; break;
            case "lefthand":  slots.LeftHand  = weapon; break;
        }
    }

    private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) return default;
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : default;
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    private sealed class RosterFileDto
    {
        public List<RaceDto>      Races   { get; init; } = [];
        public List<WeaponDto>    Weapons { get; init; } = [];
        public List<SpellDto>     Spells  { get; init; } = [];
        public List<ArmorDto>     Armors  { get; init; } = [];
        public List<CharacterDto> Heroes  { get; init; } = [];
        public List<CharacterDto> Enemies { get; init; } = [];
    }

    private sealed class RaceDto
    {
        public string Name              { get; init; } = "";
        public int    BaseMovementSpeed { get; init; } = 30;
    }

    private static ElementalType InferElementalType(DamageType damageType) => damageType switch
    {
        DamageType.Fire      => ElementalType.Fire,
        DamageType.Ice       => ElementalType.Ice,
        DamageType.Lightning => ElementalType.Lightning,
        DamageType.Poison    => ElementalType.Poison,
        DamageType.Holy      => ElementalType.Holy,
        DamageType.Shadow    => ElementalType.Shadow,
        DamageType.Acid      => ElementalType.Acid,
        _ => ElementalType.None
    };

    private sealed class WeaponDto
    {
        public string Name        { get; init; } = "";
        public string DamageDie   { get; init; } = "D4";
        public int    DamageCount { get; init; } = 1;
        public string DamageType  { get; init; } = "Bludgeoning";
        public string AttackType  { get; init; } = "Melee";
        public int    AttackBonus { get; init; }
        public string Archetype   { get; init; } = "Dagger";
        public int    Hands       { get; init; } = 1;
    }

    private sealed class SpellDto
    {
        public string Name          { get; init; } = "";
        public string School        { get; init; } = "Other";
        public string DamageDie     { get; init; } = "D4";
        public int    DamageCount   { get; init; } = 1;
        public string DamageType    { get; init; } = "Bludgeoning";
        public int    AttackBonus   { get; init; }
        public int    SpellLevel    { get; init; }
        public int    TurnMeterCost { get; init; } = 100;
        public int    ManaCost      { get; init; }
        public List<StatusEffectDto>? OnHitEffects { get; init; }
    }

    private sealed class StatusEffectDto
    {
        public string Name              { get; init; } = "";
        public string Type              { get; init; } = "";
        public string Target            { get; init; } = "Target";
        public string ResistanceType    { get; init; } = "Magic";
        public int    Duration          { get; init; } = 1;
        public int    ApplicationChance { get; init; } = 100;
        public int    ReflectChance     { get; init; }
    }

    private sealed class ArmorDto
    {
        public string Name                  { get; init; } = "";
        public int    ArmorClass            { get; init; }
        public int    Mitigation            { get; init; }
        public int    MaxDexterityBonus     { get; init; }
        public int    MovementPenalty       { get; init; }
        public int    TurnMeterCostReduction { get; init; }
    }

    private sealed class CharacterDto
    {
        public string                      Name           { get; init; } = "";
        public int                         Level          { get; init; } = 1;
        public int                         Strength       { get; init; } = 10;
        public int                         Dexterity      { get; init; } = 10;
        public int                         Stamina        { get; init; } = 10;
        public int                         Intelligence   { get; init; } = 10;
        public int                         Wisdom         { get; init; } = 10;
        public int                         Charisma       { get; init; } = 10;
        public string                      Race           { get; init; } = "";
        public int                         ClassId        { get; init; }
        public string                      ClassName      { get; init; } = "";
        public string                      Sex            { get; init; } = "Unknown";
        public int                         StrikeRating   { get; init; }
        public int                         TurnSpeed      { get; init; }
        public int                         MaxHitPoints   { get; init; }
        public int                         MaxMana        { get; init; }
        public Dictionary<string, string>? Equipment      { get; init; }
        public List<string>?               MemorizedSpells { get; init; }
    }
}
