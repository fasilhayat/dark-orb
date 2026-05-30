namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Tracks the visual state of all combatants during playback.
/// Call <see cref="ApplyEvent"/> before passing the event to <see cref="ICombatPresenter"/>.
/// </summary>
public sealed class CombatDisplayState
{
    private readonly Dictionary<string, CharDisplayState> _chars;

    public CombatLayout Layout { get; }

    public CombatDisplayState(IEnumerable<CharDisplayState> characters, CombatLayout layout)
    {
        _chars = characters.ToDictionary(c => c.Name, c => c);
        Layout = layout;
    }

    public CharDisplayState? TryGet(string name) => _chars.GetValueOrDefault(name);
    public IReadOnlyDictionary<string, CharDisplayState> All => _chars;

    /// <summary>
    /// Ensure a summoned pet has a display entry (called when PetSummoned fires).
    /// </summary>
    public void EnsurePet(string petName, int maxHp, bool isHero)
    {
        if (!_chars.ContainsKey(petName))
        {
            _chars[petName] = new CharDisplayState
            {
                Name = petName,
                MaxHp = maxHp,
                Hp = maxHp,
                IsHero = isHero,
                MaxMana = 0,
                Mana = 0,
                Weapon = string.Empty
            };
        }
    }

    /// <summary>
    /// Apply a combat log event to update display state.
    /// MUST be called before presenting the event to the renderer.
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
                    tsSt.Weapon = e.AttackSourceName ?? tsSt.Weapon;
                break;

            case "TurnEnd":
                if (_chars.TryGetValue(e.ActorName, out var teSt))
                    teSt.Tm = e.TurnMeterAfter ?? teSt.Tm;
                break;

            case "Damage":
                if (_chars.TryGetValue(e.ActorName, out var dmgSt))
                    dmgSt.Hp = e.TargetHpAfter ?? dmgSt.Hp;
                break;

            case "DoTTick":
                if (_chars.TryGetValue(e.ActorName, out var dotSt))
                    dotSt.Hp = e.TargetHpAfter ?? Math.Max(dotSt.Hp - (e.DamageDealt ?? 0), -10);
                break;

            case "Death":
                if (_chars.TryGetValue(e.ActorName, out var dSt))
                    dSt.IsAlive = false;
                break;

            case "KnockedOut":
                if (_chars.TryGetValue(e.ActorName, out var koSt))
                    koSt.IsAlive = false;
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
        }
    }
}
