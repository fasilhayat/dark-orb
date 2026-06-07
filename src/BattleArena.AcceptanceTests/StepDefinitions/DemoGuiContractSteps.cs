namespace BattleArena.ReqnrollTests.StepDefinitions;

using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

// Step definitions for DemoGuiContract.feature.
// Validates that the combat simulation's data model satisfies every field
// required by the GUI display contract (gui-display-contract.json).
// All step patterns are unique to this class — no conflicts with other binding classes.
[Binding]
public class DemoGuiContractSteps
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private GuiDisplayContract _contract = null!;
    private Character _fighter1 = null!;
    private Character _fighter2 = null!;
    private CombatResult _combatResult = null!;
    private int _hitLabelInputDamage;
    private int _hitLabelInputMaxHp;

    // ── Contract loading ───────────────────────────────────────────────────────

    [Given(@"the GUI display contract is loaded from ""([^""]*)""")]
    public void GivenContractIsLoaded(string filename)
    {
        var path = Path.Combine(AppContext.BaseDirectory, filename);

        // When running via 'dotnet test BattleArena.sln' the file isn't copied here;
        // walk up from the output dir to find the Demo project source folder.
        if (!File.Exists(path))
        {
            var dir = AppContext.BaseDirectory;
            while (dir is not null)
            {
                var candidate = Path.Combine(dir, "BattleArena.Demo", filename);
                if (File.Exists(candidate)) { path = candidate; break; }
                dir = Path.GetDirectoryName(dir);
            }
        }

        Assert.True(File.Exists(path), $"GUI display contract not found at: {path}");
        _contract = JsonSerializer.Deserialize<GuiDisplayContract>(File.ReadAllText(path), JsonOptions)!;
        Assert.NotNull(_contract);
        Assert.NotNull(_contract.Screens);
    }

    // ── Fighter setup ──────────────────────────────────────────────────────────

    [Given(@"two standard fighters are set up for GUI contract testing")]
    public void GivenTwoStandardFighters()
    {
        _fighter1 = new Character
        {
            Name             = "Aldric",
            Level            = 5,
            Strength         = 16,
            Dexterity        = 12,
            StrikeRating     = 4,
            TurnSpeed        = 10,
            MaxHitPoints     = 50,
            CurrentHitPoints = 50
        };
        _fighter1.Equipment.RightHand = new Weapon
        {
            Name         = "Broadsword",
            DamageDie    = DieType.D8,
            DamageCount  = 1,
            DamageType   = DamageType.Slashing,
            AttackType   = AttackType.Melee,
            AttackBonus  = 2
        };

        _fighter2 = new Character
        {
            Name             = "Gorak",
            Level            = 4,
            Strength         = 14,
            Dexterity        = 10,
            StrikeRating     = 5,
            TurnSpeed        = 9,
            MaxHitPoints     = 45,
            CurrentHitPoints = 45
        };
        _fighter2.Equipment.RightHand = new Weapon
        {
            Name        = "Battleaxe",
            DamageDie   = DieType.D10,
            DamageCount = 1,
            DamageType  = DamageType.Slashing,
            AttackType  = AttackType.Melee,
            AttackBonus = 1
        };
    }

    // ── Combat simulation ──────────────────────────────────────────────────────

    [When(@"a GUI contract combat is simulated with (\d+) ticks")]
    public void WhenGuiContractCombatIsSimulated(int maxTicks)
    {
        // Seeded dice → deterministic outcome so these scenarios never flake.
        var dice       = new DiceService(seed: 42);
        var stats      = new CombatStatsService();
        var combat     = new CombatService(dice, stats);
        var turnmeter  = new TurnmeterService();
        var effect     = new StatusEffectService();
        var simulator  = new CombatSimulator(combat, turnmeter, effect, dice);

        _combatResult = simulator.Simulate(
            _fighter1, _fighter1.Equipment.RightHand,
            _fighter2, _fighter2.Equipment.RightHand,
            maxTicks);

        Assert.False(_combatResult.MaxTicksReached,
            $"GUI contract combat timed out — increase maxTicks or adjust fighter stats. Log:\n{_combatResult.FormatLog()}");
    }

    // ── Character card assertions ──────────────────────────────────────────────

    [Then(@"the character card contract fields are satisfied for each combatant")]
    public void ThenCharacterCardFieldsAreSatisfied()
    {
        if (_contract.Screens.CharacterCard.Enabled == false)
            return;

        var required = RequiredFields(_contract.Screens.CharacterCard);

        foreach (var character in new[] { _fighter1, _fighter2 })
        {
            if (required.Contains("Name"))
                Assert.False(string.IsNullOrWhiteSpace(character.Name),
                    $"characterCard.Name must not be empty for {character.Name}");

            if (required.Contains("MaxHp"))
                Assert.True(character.MaxHitPoints > 0,
                    $"characterCard.MaxHp must be > 0 for {character.Name}");

            if (required.Contains("CurrentHp"))
                _ = character.CurrentHitPoints; // always present; value may be negative after KO

            if (required.Contains("IsAlive"))
                _ = character.CurrentHitPoints > 0; // derivable — no assertion on value

            if (required.Contains("TurnMeter"))
            {
                var hasTm = _combatResult.Log.Any(e =>
                    e.ActorName == character.Name &&
                    e.EventType is "TurnMeterGain" or "TurnEnd" &&
                    e.TurnMeterAfter.HasValue);
                Assert.True(hasTm,
                    $"characterCard.TurnMeter: no TurnMeterGain/TurnEnd event with TurnMeterAfter found for {character.Name}");
            }

            if (required.Contains("CurrentWeapon"))
            {
                var hasWeapon = _combatResult.Log.Any(e =>
                    e.ActorName == character.Name &&
                    e.EventType == "TurnStart" &&
                    !string.IsNullOrEmpty(e.AttackSourceName));
                Assert.True(hasWeapon,
                    $"characterCard.CurrentWeapon: no TurnStart with AttackSourceName found for {character.Name}");
            }
        }
    }

    // ── Attack event assertions ────────────────────────────────────────────────

    [Then(@"all attack event contract fields are populated in the combat log")]
    public void ThenAttackEventFieldsArePopulated()
    {
        if (_contract.Screens.AttackEvent.Enabled == false)
            return;

        var required     = RequiredFields(_contract.Screens.AttackEvent);
        var attackEvents = _combatResult.Log.Where(e => e.EventType == "Attack").ToList();
        Assert.NotEmpty(attackEvents);

        foreach (var e in attackEvents)
        {
            if (required.Contains("ActorName"))
                Assert.False(string.IsNullOrEmpty(e.ActorName), "Attack.ActorName must not be empty");
            if (required.Contains("TargetName"))
                Assert.False(string.IsNullOrEmpty(e.TargetName), "Attack.TargetName must not be empty");
            if (required.Contains("AttackSourceName"))
                Assert.False(string.IsNullOrEmpty(e.AttackSourceName), "Attack.AttackSourceName must not be empty");
            if (required.Contains("DieRoll"))
                Assert.True(e.DieRoll.HasValue, "Attack.DieRoll must be set");
            if (required.Contains("AttackPower"))
                Assert.True(e.AttackPower.HasValue, "Attack.AttackPower must be set");
            if (required.Contains("DefensePower"))
                Assert.True(e.DefensePower.HasValue, "Attack.DefensePower must be set");
            if (required.Contains("IsHit"))
                Assert.True(e.IsHit.HasValue, "Attack.IsHit must be set");
            if (required.Contains("IsCritical"))
                Assert.True(e.IsCritical.HasValue, "Attack.IsCritical must be set");
            if (required.Contains("IsFumble"))
                Assert.True(e.IsFumble.HasValue, "Attack.IsFumble must be set");
            if (required.Contains("DamageDealt") && e.IsHit == true)
                Assert.True(e.DamageDealt is > 0,
                    $"Attack.DamageDealt must be > 0 on a hit (tick {e.Tick}, {e.ActorName} → {e.TargetName})");
        }
    }

    // ── Damage event assertions ────────────────────────────────────────────────

    [Then(@"all damage event contract fields are populated in the combat log")]
    public void ThenDamageEventFieldsArePopulated()
    {
        if (_contract.Screens.DamageEvent.Enabled == false)
            return;

        var required      = RequiredFields(_contract.Screens.DamageEvent);
        var damageEvents  = _combatResult.Log.Where(e => e.EventType == "Damage").ToList();
        Assert.NotEmpty(damageEvents);

        foreach (var e in damageEvents)
        {
            if (required.Contains("ActorName"))
                Assert.False(string.IsNullOrEmpty(e.ActorName), "Damage.ActorName must not be empty");
            if (required.Contains("DamageDealt"))
                Assert.True(e.DamageDealt is > 0, "Damage.DamageDealt must be > 0");
            if (required.Contains("TargetHpBefore"))
                Assert.True(e.TargetHpBefore.HasValue, "Damage.TargetHpBefore must be set");
            if (required.Contains("TargetHpAfter"))
                Assert.True(e.TargetHpAfter.HasValue, "Damage.TargetHpAfter must be set");
        }
    }

    // ── Combat summary assertions ──────────────────────────────────────────────

    [Then(@"the combat summary contract fields are all populated")]
    public void ThenCombatSummaryFieldsArePopulated()
    {
        if (_contract.Screens.CombatSummary.Enabled == false)
            return;

        var required = RequiredFields(_contract.Screens.CombatSummary);

        if (required.Contains("CombatId"))
            Assert.NotEqual(Guid.Empty, _combatResult.CombatId);
        if (required.Contains("TotalTicks"))
            Assert.True(_combatResult.TotalTicks > 0, "CombatSummary.TotalTicks must be > 0");
        if (required.Contains("WinnerName"))
            Assert.False(string.IsNullOrEmpty(_combatResult.WinningParty?.Name),
                "CombatSummary.WinnerName (WinningParty.Name) must not be empty");
        if (required.Contains("LoserName"))
            Assert.False(string.IsNullOrEmpty(_combatResult.LosingParty?.Name),
                "CombatSummary.LoserName (LosingParty.Name) must not be empty");
        if (required.Contains("LoserStatus"))
            Assert.True(
                _combatResult.LoserStatus is CharacterVitalStatus.Dead or CharacterVitalStatus.KnockedOut,
                $"CombatSummary.LoserStatus should be Dead or KnockedOut, got {_combatResult.LoserStatus}");
    }

    // ── Hit severity label assertions ──────────────────────────────────────────

    [When(@"the hit severity label is computed for (\d+) damage against a target with (\d+) max HP")]
    public void WhenHitLabelIsComputed(int damage, int maxHp)
    {
        _hitLabelInputDamage = damage;
        _hitLabelInputMaxHp  = maxHp;
    }

    [Then(@"the hit label should be ""([^""]*)""")]
    public void ThenHitLabelShouldBe(string expectedLabel)
    {
        var actual = CombatHitLabelService.GetLabel(_hitLabelInputDamage, _hitLabelInputMaxHp);
        Assert.Equal(expectedLabel, actual);
    }

    // ── Spellcaster setup ──────────────────────────────────────────────────────

    [Given(@"a spellcaster and a fighter are set up for GUI contract testing")]
    public void GivenSpellcasterAndFighter()
    {
        var fireball = new Spell
        {
            Name          = "Fireball",
            School        = SpellSchool.Stormcraft,
            DamageDie     = DieType.D6,
            DamageCount   = 3,
            DamageType    = DamageType.Fire,
            AttackBonus   = 2,
            SpellLevel    = 3,
            TurnMeterCost = 90,
            ManaCost      = 30
        };
        _fighter1 = new Character
        {
            Name             = "Lyra",
            Level            = 5,
            Strength         = 8,
            Dexterity        = 14,
            Intelligence     = 18,
            StrikeRating     = 13,
            TurnSpeed        = 10,
            MaxHitPoints     = 30,
            CurrentHitPoints = 30,
            MaxMana          = 165,
            CurrentMana      = 165,
            MemorizedSpells  = [fireball]
        };

        _fighter2 = new Character
        {
            Name             = "Gorak",
            Level            = 4,
            Strength         = 14,
            Dexterity        = 10,
            StrikeRating     = 5,
            TurnSpeed        = 9,
            MaxHitPoints     = 45,
            CurrentHitPoints = 45
        };
        _fighter2.Equipment.RightHand = new Weapon
        {
            Name        = "Battleaxe",
            DamageDie   = DieType.D10,
            DamageCount = 1,
            DamageType  = DamageType.Slashing,
            AttackType  = AttackType.Melee,
            AttackBonus = 1
        };
    }

    // ── Spellcaster combat simulation ──────────────────────────────────────────

    [When(@"a spellcaster GUI contract combat is simulated with (\d+) ticks")]
    public void WhenSpellcasterGuiContractCombatIsSimulated(int maxTicks)
    {
        var dice      = new DiceService(seed: 77);
        var stats     = new CombatStatsService();
        var combat    = new CombatService(dice, stats);
        var turnmeter = new TurnmeterService();
        var effect    = new StatusEffectService();
        var simulator = new CombatSimulator(combat, turnmeter, effect, dice);

        // AttackSource = null for caster → simulator picks spells via ResolveAttackSource
        var heroParty = new Party
        {
            Name    = "Heroes",
            Members = [new PartyMember { Character = _fighter1, AttackSource = null }]
        };
        var enemyParty = new Party
        {
            Name    = "Enemies",
            Members = [new PartyMember { Character = _fighter2, AttackSource = _fighter2.Equipment.RightHand }]
        };

        _combatResult = simulator.Simulate(heroParty, enemyParty, maxTicks);
    }

    // ── Round bar assertions ───────────────────────────────────────────────────

    [Then(@"the round bar contract fields are populated in the combat log")]
    public void ThenRoundBarFieldsArePopulated()
    {
        if (_contract.Screens.RoundBar?.Enabled == false) return;

        var roundStarts = _combatResult.Log.Where(e => e.EventType == "RoundStart").ToList();
        Assert.NotEmpty(roundStarts);

        foreach (var e in roundStarts)
        {
            Assert.True(e.RoundNumber is > 0,
                $"roundBar.RoundNumber must be > 0 on RoundStart (tick {e.Tick})");
            Assert.True(e.Tick > 0,
                $"roundBar.CurrentTick must be > 0 on RoundStart (tick {e.Tick})");

            // tick-in-round formula: ((Tick-1) % 10) + 1 must be in range 1–10
            var tickInRound = (e.Tick - 1) % 10 + 1;
            Assert.InRange(tickInRound, 1, 10);
        }

        // RoundEnd events must also carry RoundNumber
        var roundEnds = _combatResult.Log.Where(e => e.EventType == "RoundEnd").ToList();
        Assert.NotEmpty(roundEnds);
        Assert.All(roundEnds, e => Assert.True(e.RoundNumber is > 0,
            $"roundBar.RoundNumber must be > 0 on RoundEnd (tick {e.Tick})"));
    }

    // ── Mana event assertions ──────────────────────────────────────────────────

    [Then(@"the mana event contract fields are populated in the combat log")]
    public void ThenManaEventFieldsArePopulated()
    {
        if (_contract.Screens.ManaEvent?.Enabled == false) return;

        var deducts = _combatResult.Log.Where(e => e.EventType == "ManaDeduct").ToList();
        Assert.NotEmpty(deducts);

        foreach (var e in deducts)
        {
            Assert.False(string.IsNullOrEmpty(e.ActorName), "manaEvent.ActorName must not be empty on ManaDeduct");
            Assert.True(e.ManaCost is > 0,    $"manaEvent.ManaCost must be > 0 on ManaDeduct (tick {e.Tick})");
            Assert.True(e.ManaAfter.HasValue,  $"manaEvent.ManaAfter must be set on ManaDeduct (tick {e.Tick})");
        }

        var regens = _combatResult.Log.Where(e => e.EventType == "ManaRegen").ToList();
        Assert.NotEmpty(regens);

        foreach (var e in regens)
        {
            Assert.False(string.IsNullOrEmpty(e.ActorName), "manaEvent.ActorName must not be empty on ManaRegen");
            Assert.True(e.ManaRegen.HasValue,  $"manaEvent.ManaRegen must be set on ManaRegen events (tick {e.Tick})");
            Assert.True(e.ManaAfter.HasValue,  $"manaEvent.ManaAfter must be set on ManaRegen events (tick {e.Tick})");
        }
    }

    // ── Spellcaster card assertions ────────────────────────────────────────────

    [Then(@"the spellcaster card spell list fields are satisfied")]
    public void ThenSpellcasterCardSpellListFieldsSatisfied()
    {
        // Verify the contract field SpellListRow is satisfiable: the character exposes MemorizedSpells
        Assert.NotEmpty(_fighter1.MemorizedSpells);
        Assert.All(_fighter1.MemorizedSpells, spell =>
            Assert.False(string.IsNullOrWhiteSpace(spell.Name),
                "characterCard.SpellListRow: every memorized spell must have a Name"));

        // The fighter has no spells — card row must be blank (empty list, not null)
        Assert.NotNull(_fighter2.MemorizedSpells);
        Assert.Empty(_fighter2.MemorizedSpells);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static HashSet<string> RequiredFields(GuiDisplayScreen screen) =>
        screen.RequiredFields
            .Where(f => f.Enabled != false)
            .Select(f => f.Field)
            .ToHashSet();
}

// ── Contract deserialization types ─────────────────────────────────────────────

internal sealed record GuiDisplayContract(
    [property: JsonPropertyName("version")]     string Version,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("screens")]     GuiDisplayScreens Screens);

internal sealed record GuiDisplayScreens(
    [property: JsonPropertyName("characterCard")] GuiDisplayScreen  CharacterCard,
    [property: JsonPropertyName("roundBar")]      GuiDisplayScreen? RoundBar,
    [property: JsonPropertyName("attackEvent")]   GuiDisplayScreen  AttackEvent,
    [property: JsonPropertyName("manaEvent")]     GuiDisplayScreen? ManaEvent,
    [property: JsonPropertyName("damageEvent")]   GuiDisplayScreen  DamageEvent,
    [property: JsonPropertyName("combatSummary")] GuiDisplayScreen  CombatSummary,
    [property: JsonPropertyName("hitLabels")]     GuiHitLabels      HitLabels);

internal sealed record GuiDisplayScreen(
    [property: JsonPropertyName("enabled")]        bool?                 Enabled,
    [property: JsonPropertyName("description")]    string                Description,
    [property: JsonPropertyName("requiredFields")] List<GuiDisplayField> RequiredFields);

internal sealed record GuiDisplayField(
    [property: JsonPropertyName("enabled")]     bool?  Enabled,
    [property: JsonPropertyName("field")]       string Field,
    [property: JsonPropertyName("source")]      string Source,
    [property: JsonPropertyName("description")] string Description);

internal sealed record GuiHitLabels(
    [property: JsonPropertyName("description")]    string            Description,
    [property: JsonPropertyName("requiredLabels")] List<GuiHitLabel> RequiredLabels);

internal sealed record GuiHitLabel(
    [property: JsonPropertyName("label")]     string Label,
    [property: JsonPropertyName("condition")] string Condition);
