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
                    tsSt.CcStatus = null;
                    // Tick down all active effect display durations (engine already did TickAll)
                    for (var i = tsSt.ActiveEffects.Count - 1; i >= 0; i--)
                    {
                        var eff = tsSt.ActiveEffects[i];
                        if (eff.Duration > 0)
                            eff.Duration--;
                    }
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
                {
                    skSt.IsTmLocked = true;
                    skSt.CcStatus = e.CcLabel;
                }
                break;

            case "Damage":
                if (_chars.TryGetValue(e.ActorName, out var dmgSt))
                    dmgSt.Hp = e.TargetHpAfter ?? dmgSt.Hp;
                break;

            case "DoTTick":
                if (_chars.TryGetValue(e.ActorName, out var dotSt))
                {
                    dotSt.Hp = e.TargetHpAfter ?? Math.Max(dotSt.Hp - (e.DamageDealt ?? 0), -10);
                    UpdateEffectOnTick(dotSt, e);
                }
                break;

            case "HoTTick":
                if (_chars.TryGetValue(e.ActorName, out var hotSt))
                {
                    hotSt.Hp = e.TargetHpAfter ?? Math.Min(hotSt.MaxHp, hotSt.Hp + (e.DamageDealt ?? 0));
                    UpdateEffectOnTick(hotSt, e);
                }
                break;

            case "Healed":
                if (_chars.TryGetValue(e.ActorName, out var healSt))
                    healSt.Hp = e.TargetHpAfter ?? Math.Min(healSt.MaxHp, healSt.Hp + (e.DamageDealt ?? 0));
                break;

            case "ManaDeduct":
            case "ManaRegen":
                if (_chars.TryGetValue(e.ActorName, out var manaSt) && e.ManaAfter.HasValue)
                    manaSt.Mana = e.ManaAfter.Value;
                break;

            case "LeechTick":
                if (_chars.TryGetValue(e.ActorName, out var leechTargetSt))
                {
                    if (e.LeechResourceType == "HP" && e.LeechTargetAfter.HasValue)
                        leechTargetSt.Hp = e.LeechTargetAfter.Value;
                    else if (e.LeechResourceType == "Mana" && e.LeechTargetAfter.HasValue)
                        leechTargetSt.Mana = e.LeechTargetAfter.Value;
                    UpdateEffectOnTick(leechTargetSt, e);
                }
                if (e.LeechCasterName is not null && _chars.TryGetValue(e.LeechCasterName, out var leechCasterSt))
                {
                    if (e.LeechResourceType == "HP" && e.LeechCasterAfter.HasValue)
                        leechCasterSt.Hp = e.LeechCasterAfter.Value;
                    else if (e.LeechResourceType == "Mana" && e.LeechCasterAfter.HasValue)
                        leechCasterSt.Mana = e.LeechCasterAfter.Value;
                }
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
                    && _chars.TryGetValue(e.ActorName, out var applySt)
                    && EffectVisualConfig.IsDisplayed(e.StatusEffectName))
                {
                    var existing = applySt.ActiveEffects.FirstOrDefault(d => d.Name == e.StatusEffectName);
                    if (existing is not null)
                    {
                        existing.Duration = e.EffectDuration ?? existing.Duration;
                        existing.MaxDuration = Math.Max(existing.MaxDuration, e.EffectMaxDuration ?? existing.Duration);
                        existing.Stacks = e.EffectStacks ?? existing.Stacks + 1;
                    }
                    else
                    {
                        applySt.ActiveEffects.Add(new EffectDisplayData
                        {
                            Name = e.StatusEffectName,
                            Duration = e.EffectDuration ?? 3,
                            MaxDuration = e.EffectMaxDuration ?? e.EffectDuration ?? 3,
                            Stacks = e.EffectStacks ?? 1,
                            Color = EffectVisualConfig.GetColor(e.StatusEffectName),
                        });
                    }
                    if (CcVisualConfig.IsCcEffect(e.StatusEffectName))
                    {
                        applySt.IsTmLocked = true;
                        applySt.CcStatus = e.StatusEffectName.ToLowerInvariant();
                    }
                }
                break;

            case "EffectExpired":
                if (!string.IsNullOrWhiteSpace(e.StatusEffectName)
                    && _chars.TryGetValue(e.ActorName, out var expSt))
                {
                    expSt.ActiveEffects.RemoveAll(d => d.Name == e.StatusEffectName);
                    if (CcVisualConfig.IsCcEffect(e.StatusEffectName)
                        && !expSt.ActiveEffects.Any(d => CcVisualConfig.IsCcEffect(d.Name)))
                    {
                        expSt.IsTmLocked = false;
                        expSt.CcStatus = null;
                    }
                }
                break;

            case "Death":
            case "KnockedOut":
                if (_chars.TryGetValue(e.ActorName, out var deadSt))
                {
                    deadSt.IsAlive = false;
                    deadSt.CcStatus = null;
                    deadSt.ActiveEffects.Clear();
                }
                break;
        }
    }

    /// <summary>
    /// Returns the names of all active effects (used by legacy string-based display).
    /// </summary>
    public IReadOnlyList<string> GetActiveEffectNames(string characterName)
    {
        if (_chars.TryGetValue(characterName, out var st))
            return st.ActiveEffects.Select(d => d.Name).ToList();
        return Array.Empty<string>();
    }

    /// <summary>
    /// Returns true if the character has at least one active effect matching the predicate.
    /// </summary>
    public bool HasActiveEffect(string characterName, Func<string, bool> predicate)
    {
        if (_chars.TryGetValue(characterName, out var st))
            return st.ActiveEffects.Any(d => predicate(d.Name));
        return false;
    }

    /// <summary>
    /// Updates effect duration (decrements by 1 for this tick) and stacks on
    /// DoTTick / HoTTick / LeechTick events.
    /// </summary>
    private static void UpdateEffectOnTick(CharDisplayState st, CombatLogEntry e)
    {
        if (string.IsNullOrWhiteSpace(e.StatusEffectName)) return;
        var effect = st.ActiveEffects.FirstOrDefault(d => d.Name == e.StatusEffectName);
        if (effect is null) return;

        if (e.EffectDuration.HasValue)
            effect.Duration = Math.Max(0, e.EffectDuration.Value - 1);
        else
            effect.Duration = Math.Max(0, effect.Duration - 1);

        if (e.EffectStacks.HasValue)
            effect.Stacks = e.EffectStacks.Value;
    }
}
