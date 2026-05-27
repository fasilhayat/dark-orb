namespace BattleArena.Application.Models;

// ─────────────────────────────────────────────────────────────────────────────
// Self-contained snapshot of everything needed to replay a combat identically.
//
// Replay recipe:
//   1. Read the .json file → deserialise to CombatSnapshot
//   2. Call CombatSnapshot.ToParties() to reconstruct Party objects
//   3. new DiceService(snapshot.Seed) to get the identical RNG sequence
//   4. Run CombatSimulator.Simulate(party1, party2) → identical CombatResult
//
// All enum values are stored as strings so the file is human-readable
// and survives refactoring of numeric enum ordinals.
// ─────────────────────────────────────────────────────────────────────────────

using Core.Entities;
using Core.Entities.Enums;

public class CombatSnapshot
{
    public int    Seed      { get; set; }
    public string Label     { get; set; } = "";
    public string Timestamp { get; set; } = "";

    public SnapshotParty Party1 { get; set; } = new();
    public SnapshotParty Party2 { get; set; } = new();

    // ── Reconstruct domain objects ─────────────────────────────────────────────

    public (Party p1, Party p2) ToParties()
    {
        return (Party1.ToParty(), Party2.ToParty());
    }

    // ── Build from a live CombatResult ────────────────────────────────────────

    public static CombatSnapshot From(CombatResult result, string label)
    {
        var p1 = result.Party1 ?? result.WinningParty ?? throw new InvalidOperationException("No Party1 on result");
        var p2 = result.Party2 ?? result.LosingParty  ?? throw new InvalidOperationException("No Party2 on result");

        return new CombatSnapshot
        {
            Seed      = result.Seed,
            Label     = label,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Party1    = SnapshotParty.From(p1),
            Party2    = SnapshotParty.From(p2),
        };
    }
}

// ── Party ─────────────────────────────────────────────────────────────────────

public class SnapshotParty
{
    public string Name { get; set; } = "";
    public List<SnapshotMember> Members { get; set; } = [];

    public Party ToParty()
    {
        var party = new Party { Name = Name };
        foreach (var m in Members)
            party.Members.Add(m.ToMember());
        return party;
    }

    public static SnapshotParty From(Party p) => new()
    {
        Name    = p.Name,
        Members = p.Members.Select(SnapshotMember.From).ToList()
    };
}

// ── PartyMember ───────────────────────────────────────────────────────────────

public class SnapshotMember
{
    public SnapshotCharacter Character    { get; set; } = new();
    public SnapshotWeapon?   AttackSource { get; set; }  // null = pure spellcaster

    public PartyMember ToMember() => new()
    {
        Character    = Character.ToCharacter(),
        AttackSource = AttackSource?.ToWeapon()
    };

    public static SnapshotMember From(PartyMember m) => new()
    {
        Character    = SnapshotCharacter.From(m.Character),
        AttackSource = m.AttackSource is Weapon w ? SnapshotWeapon.From(w) : null
    };
}

// ── Character ─────────────────────────────────────────────────────────────────

public class SnapshotCharacter
{
    public string Name         { get; set; } = "";
    public int    Level        { get; set; }
    public int    Strength     { get; set; }
    public int    Dexterity    { get; set; }
    public int    Intelligence { get; set; }
    public int    StrikeRating { get; set; }
    public int    TurnSpeed    { get; set; }
    public int    MaxHitPoints { get; set; }
    public int    ClassId      { get; set; }

    public SnapshotArmor?        Armor  { get; set; }
    public SnapshotWeapon?       Weapon { get; set; }
    public List<SnapshotSpell>   Spells { get; set; } = [];
    public SnapshotRace?         Race   { get; set; }

    public Character ToCharacter()
    {
        var ch = new Character
        {
            Name             = Name,
            Level            = Level,
            Strength         = Strength,
            Dexterity        = Dexterity,
            Intelligence     = Intelligence,
            StrikeRating     = StrikeRating,
            TurnSpeed        = TurnSpeed,
            MaxHitPoints     = MaxHitPoints,
            CurrentHitPoints = MaxHitPoints,
            ClassId          = ClassId,
            Equipment        = new ArmorSlots
            {
                Chest     = Armor?.ToArmor(),
                RightHand = Weapon?.ToWeapon()
            },
            MemorizedSpells = Spells.Select(s => s.ToSpell()).ToList(),
            Race            = Race?.ToRace()
        };
        return ch;
    }

    public static SnapshotCharacter From(Character ch) => new()
    {
        Name         = ch.Name,
        Level        = ch.Level,
        Strength     = ch.Strength,
        Dexterity    = ch.Dexterity,
        Intelligence = ch.Intelligence,
        StrikeRating = ch.StrikeRating,
        TurnSpeed    = ch.TurnSpeed,
        MaxHitPoints = ch.MaxHitPoints,
        ClassId      = ch.ClassId,
        Armor        = ch.Equipment.Chest   is { } a ? SnapshotArmor.From(a) : null,
        Weapon       = ch.Equipment.RightHand is Weapon w ? SnapshotWeapon.From(w) : null,
        Spells       = ch.MemorizedSpells.Select(SnapshotSpell.From).ToList(),
        Race         = ch.Race is { } r ? SnapshotRace.From(r) : null
    };
}

// ── Armor ─────────────────────────────────────────────────────────────────────

public class SnapshotArmor
{
    public string            Name               { get; set; } = "";
    public int               ArmorClass         { get; set; }
    public int               Mitigation         { get; set; }
    public int               MaxDexterityBonus  { get; set; }
    public int               TurnMeterPenalty   { get; set; }
    public List<SnapshotRes> Resistances        { get; set; } = [];

    public Armor ToArmor() => new()
    {
        Name              = Name,
        ArmorClass        = ArmorClass,
        Mitigation        = Mitigation,
        MaxDexterityBonus = MaxDexterityBonus,
        TurnMeterPenalty  = TurnMeterPenalty,
        Resistances       = Resistances.Select(r => new ResistanceBonus(
            Enum.Parse<ResistanceType>(r.Type), r.Value)).ToList()
    };

    public static SnapshotArmor From(Armor a) => new()
    {
        Name              = a.Name,
        ArmorClass        = a.ArmorClass,
        Mitigation        = a.Mitigation,
        MaxDexterityBonus = a.MaxDexterityBonus,
        TurnMeterPenalty  = a.TurnMeterPenalty,
        Resistances       = a.Resistances.Select(SnapshotRes.From).ToList()
    };
}

// ── Weapon ────────────────────────────────────────────────────────────────────

public class SnapshotWeapon
{
    public string Name        { get; set; } = "";
    public string DamageDie   { get; set; } = "D8";
    public int    DamageCount { get; set; } = 1;
    public string DamageType  { get; set; } = "Slashing";
    public string AttackType  { get; set; } = "Melee";
    public int    AttackBonus { get; set; }

    public Weapon ToWeapon() => new()
    {
        Name        = Name,
        DamageDie   = Enum.Parse<DieType>(DamageDie),
        DamageCount = DamageCount,
        DamageType  = Enum.Parse<DamageType>(DamageType),
        AttackType  = Enum.Parse<AttackType>(AttackType),
        AttackBonus = AttackBonus
    };

    public static SnapshotWeapon From(Weapon w) => new()
    {
        Name        = w.Name,
        DamageDie   = w.DamageDie.ToString(),
        DamageCount = w.DamageCount,
        DamageType  = w.DamageType.ToString(),
        AttackType  = w.AttackType.ToString(),
        AttackBonus = w.AttackBonus
    };
}

// ── Spell ─────────────────────────────────────────────────────────────────────

public class SnapshotSpell
{
    public string                    Name          { get; set; } = "";
    public string                    Description   { get; set; } = "";
    public string                    DamageDie     { get; set; } = "D6";
    public int                       DamageCount   { get; set; } = 1;
    public string                    DamageType    { get; set; } = "Fire";
    public int                       AttackBonus   { get; set; }
    public int                       SpellLevel    { get; set; }
    public string                    School        { get; set; } = "Evocation";
    public List<SnapshotStatusEffect> OnHitEffects { get; set; } = [];

    public Spell ToSpell() => new()
    {
        Name          = Name,
        Description   = Description,
        DamageDie     = Enum.Parse<DieType>(DamageDie),
        DamageCount   = DamageCount,
        DamageType    = Enum.Parse<DamageType>(DamageType),
        AttackBonus   = AttackBonus,
        SpellLevel    = SpellLevel,
        School        = Enum.Parse<SpellSchool>(School),
        OnHitEffects  = OnHitEffects.Select(e => e.ToStatusEffect()).ToList()
    };

    public static SnapshotSpell From(Spell s) => new()
    {
        Name         = s.Name,
        Description  = s.Description ?? "",
        DamageDie    = s.DamageDie.ToString(),
        DamageCount  = s.DamageCount,
        DamageType   = s.DamageType.ToString(),
        AttackBonus  = s.AttackBonus,
        SpellLevel   = s.SpellLevel,
        School       = s.School.ToString(),
        OnHitEffects = s.OnHitEffects.Select(SnapshotStatusEffect.From).ToList()
    };
}

// ── StatusEffect ──────────────────────────────────────────────────────────────

public class SnapshotStatusEffect
{
    public string Name                { get; set; } = "";
    public string Type                { get; set; } = "Debuff";
    public string ResistanceType      { get; set; } = "Magic";
    public int    Duration            { get; set; }
    public int    ApplicationChance   { get; set; }
    public int    DoTDamageCount      { get; set; }
    public string DoTDamageDie        { get; set; } = "D4";
    public int    TurnMeterModifier   { get; set; }
    public int    AttackPowerModifier { get; set; }
    public string StackRule           { get; set; } = "HighestWins";

    public StatusEffect ToStatusEffect() => new()
    {
        Name                = Name,
        Type                = Enum.Parse<StatusEffectType>(Type),
        ResistanceType      = Enum.Parse<ResistanceType>(ResistanceType),
        Duration            = Duration,
        ApplicationChance   = ApplicationChance,
        DoTDamageCount      = DoTDamageCount,
        DoTDamageDie        = Enum.Parse<DieType>(DoTDamageDie),
        TurnMeterModifier   = TurnMeterModifier,
        AttackPowerModifier = AttackPowerModifier,
        StackRule           = Enum.Parse<StackRule>(StackRule)
    };

    public static SnapshotStatusEffect From(StatusEffect e) => new()
    {
        Name                = e.Name,
        Type                = e.Type.ToString(),
        ResistanceType      = e.ResistanceType.ToString(),
        Duration            = e.Duration,
        ApplicationChance   = e.ApplicationChance,
        DoTDamageCount      = e.DoTDamageCount,
        DoTDamageDie        = e.DoTDamageDie.ToString(),
        TurnMeterModifier   = e.TurnMeterModifier,
        AttackPowerModifier = e.AttackPowerModifier,
        StackRule           = e.StackRule.ToString()
    };
}

// ── Race / Feats / Resistance ─────────────────────────────────────────────────

public class SnapshotRace
{
    public string            Name  { get; set; } = "";
    public List<SnapshotFeat> Feats { get; set; } = [];

    public Race ToRace() => new()
    {
        Name  = Name,
        Feats = Feats.Select(f => f.ToFeat()).ToList()
    };

    public static SnapshotRace From(Race r) => new()
    {
        Name  = r.Name,
        Feats = r.Feats.Select(SnapshotFeat.From).ToList()
    };
}

public class SnapshotFeat
{
    public string            Name        { get; set; } = "";
    public string            Description { get; set; } = "";
    public List<SnapshotRes> Resistances { get; set; } = [];

    public Feat ToFeat() => new()
    {
        Name        = Name,
        Description = Description,
        Resistances = Resistances.Select(r =>
            new ResistanceBonus(Enum.Parse<ResistanceType>(r.Type), r.Value)).ToList()
    };

    public static SnapshotFeat From(Feat f) => new()
    {
        Name        = f.Name,
        Description = f.Description ?? "",
        Resistances = f.Resistances.Select(SnapshotRes.From).ToList()
    };
}

public class SnapshotRes
{
    public string Type  { get; set; } = "";
    public int    Value { get; set; }

    public static SnapshotRes From(ResistanceBonus r) => new() { Type = r.Type.ToString(), Value = r.Value };
}
