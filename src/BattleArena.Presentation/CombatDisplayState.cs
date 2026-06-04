namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Tracks the visual state of all combatants during playback.
/// Call <see cref="ApplyEvent"/> before passing the event to <see cref="ICombatPresenter"/>.
/// </summary>
public sealed class CombatDisplayState
{
    private readonly Dictionary<string, CharDisplayState> _chars;
    private readonly HashSet<string> _heroSideNames;

    public CombatLayout Layout { get; }

    public bool IsApiMode { get; }

    public CombatDisplayState(IEnumerable<CharDisplayState> characters, CombatLayout layout, bool isApiMode = false)
    {
        _chars = characters.ToDictionary(c => c.Name, c => c);
        Layout = layout;
        _heroSideNames = Layout.HeroNames.ToHashSet();
        IsApiMode = isApiMode;
    }

    public CharDisplayState? TryGet(string name) => _chars.GetValueOrDefault(name);
    public IReadOnlyDictionary<string, CharDisplayState> All => _chars;

    /// <summary>Returns true when <paramref name="name"/> is on the hero side (including summoned pets).</summary>
    public bool IsHeroSide(string? name) => name is not null && _heroSideNames.Contains(name);

    /// <summary>
    /// Ensure a summoned pet has a display entry (called when PetSummoned fires).
    /// The pet inherits its side from the summoner.
    /// </summary>
    public void EnsurePet(string petName, int maxHp, string summonerName)
    {
        if (_chars.ContainsKey(petName)) return;

        if (_heroSideNames.Contains(summonerName))
            _heroSideNames.Add(petName);

        _chars[petName] = new CharDisplayState
        {
            Name   = petName,
            MaxHp  = maxHp,
            Hp     = maxHp,
            MaxMana = 0,
            Mana   = 0,
            Weapon = string.Empty,
            Race   = "Pet"
        };
    }

    /// <summary>
    /// Apply a combat log event to update display state.
    /// In <see cref="CombatPlaybackEngine.PlayTurnBased"/> this is called lazily
    /// (inside FlushTurn) so each event is applied immediately before it is rendered.
    /// </summary>
    public void ApplyEvent(CombatLogEntry e)
    {
        switch (e.EventType)
        {
            case "TurnMeterGain":
                if (_chars.TryGetValue(e.ActorName, out var tmSt))
                    tmSt.Tm = e.TurnMeterAfter ?? tmSt.Tm;
                break;

            case "TurnStart":
                if (_chars.TryGetValue(e.ActorName, out var tsSt))
                {
                    tsSt.Weapon = e.AttackSourceName ?? tsSt.Weapon;
                    tsSt.IsTmLocked = false;
                }
                if (e.TurnMeterSnapshot is not null)
                {
                    foreach (var (name, tm) in e.TurnMeterSnapshot)
                        if (_chars.TryGetValue(name, out var snapSt))
                            snapSt.Tm = tm;
                }
                break;

            case "TurnEnd":
                if (_chars.TryGetValue(e.ActorName, out var teSt))
                    teSt.Tm = e.TurnMeterAfter ?? teSt.Tm;
                break;

            case "SkippedTurn":
                if (_chars.TryGetValue(e.ActorName, out var skSt))
                    skSt.IsTmLocked = true;
                break;

            case "Damage":
                if (_chars.TryGetValue(e.ActorName, out var dmgSt))
                    dmgSt.Hp = e.TargetHpAfter ?? dmgSt.Hp;
                break;

            case "DoTTick":
                if (_chars.TryGetValue(e.ActorName, out var dotSt))
                    dotSt.Hp = e.TargetHpAfter ?? Math.Max(dotSt.Hp - (e.DamageDealt ?? 0), -10);
                break;

            case "ManaDeduct":
            case "ManaRegen":
                if (_chars.TryGetValue(e.ActorName, out var manaSt) && e.ManaAfter.HasValue)
                    manaSt.Mana = e.ManaAfter.Value;
                break;

            case "PetSummoned":
                if (!string.IsNullOrWhiteSpace(e.SummonedPetName)
                    && _chars.TryGetValue(e.SummonedPetName, out var summonedPet))
                {
                    summonedPet.Hp = summonedPet.MaxHp;
                    summonedPet.IsAlive = true;
                }
                break;

            case "PetExpired":
                if (!string.IsNullOrWhiteSpace(e.SummonedPetName)
                    && _chars.TryGetValue(e.SummonedPetName, out var petSt))
                {
                    petSt.Hp = 0;
                    petSt.IsAlive = false;
                }
                break;

            case "EffectApplied":
                if (!string.IsNullOrWhiteSpace(e.StatusEffectName)
                    && _chars.TryGetValue(e.ActorName, out var applySt))
                {
                    if (!applySt.ActiveEffects.Contains(e.StatusEffectName))
                        applySt.ActiveEffects.Add(e.StatusEffectName);
                }
                break;

            case "EffectExpired":
                if (!string.IsNullOrWhiteSpace(e.StatusEffectName)
                    && _chars.TryGetValue(e.ActorName, out var expSt))
                {
                    expSt.ActiveEffects.Remove(e.StatusEffectName);
                }
                break;

            case "Death":
            case "KnockedOut":
                if (_chars.TryGetValue(e.ActorName, out var deadSt))
                {
                    deadSt.IsAlive = false;
                    deadSt.ActiveEffects.Clear();
                }
                break;
        }
    }
}
