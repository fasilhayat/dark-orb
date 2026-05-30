namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

// Drives the turn-based combat loop for any NvN configuration (1v1, 1vN, up to 6vN).
//
//   1. Every tick all living combatants gain turnmeter (TurnSpeed + DEX mod - armor penalty).
//   2. All combatants whose meter reaches 100 act that tick, highest meter first.
//   3. Each actor picks a random living enemy via ITargetSelector and resolves an attack.
//   4. HP can go negative: 0 to -9 = knocked out, -10 or lower = dead.
//   5. A fumble applies -2 AttackPower for the fumbler's next turn.
//   6. Combat ends when one party has no living members, or maxTicks is exhausted.
//   7. Every event is recorded in a CombatLogEntry with full detail.
public class CombatSimulator : ICombatSimulator
{
    private readonly ICombatService       _combat;
    private readonly ITurnmeterService    _turnmeter;
    private readonly IStatusEffectService _statusEffect;
    private readonly IDiceService         _dice;
    // Separate selectors so hero and enemy parties can use different strategies.
    private readonly ITargetSelector      _heroTargetSelector;
    private readonly ITargetSelector      _enemyTargetSelector;

    public CombatSimulator(
        ICombatService combat,
        ITurnmeterService turnmeter,
        IStatusEffectService statusEffect,
        IDiceService dice,
        ITargetSelector? heroTargetSelector  = null,
        ITargetSelector? enemyTargetSelector = null)
    {
        _combat              = combat;
        _turnmeter           = turnmeter;
        _statusEffect        = statusEffect;
        _dice                = dice;
        _heroTargetSelector  = heroTargetSelector  ?? new RandomTargetSelector();
        _enemyTargetSelector = enemyTargetSelector ?? new RandomTargetSelector();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    // Party-vs-party async entry point — supports 1v1, 1vN, NvN (hero side max 6).
    // Observer receives every event in real time (use for GUI animation).
    // CancellationToken allows forfeiting or time-out from the caller.
    public async Task<CombatResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default)
    {
        const int RoundLength = 10;

        var log            = new List<CombatLogEntry>();
        var states         = BuildCombatantStates(heroParty, enemyParty);
        var currentRound   = 0;
        var lastAttackerOf = new Dictionary<Character, Character>();

        // Log + notify the observer for every event in one call.
        // Automatically stamps the currently-acting character so consumers
        // never need to track it themselves.
        async Task Notify(CombatLogEntry entry)
        {
            entry.ActiveActorName = states.FirstOrDefault(s => s.Meter.IsActive)?.Character.Name;
            log.Add(entry);
            if (observer != null)
                await observer.OnEventAsync(entry, ct);
        }

        for (var tick = 1; tick <= maxTicks; tick++)
        {
            ct.ThrowIfCancellationRequested();
            _dice.CurrentTick = tick;

            if ((tick - 1) % RoundLength == 0)
            {
                currentRound++;
                await Notify(new CombatLogEntry
                {
                    Tick = tick,
                    EventType = "RoundStart",
                    RoundNumber = currentRound,
                    Message = $"══ Round {currentRound} begins ══"
                });
            }

            // ── TICK: advance meters for every living combatant ────────────────
            foreach (var s in states.Where(s => s.Character.IsAlive))
            {
                s.SnapshotMeter();
                s.Meter = _turnmeter.Tick(s.Character, s.Meter);
                await Notify(BuildTurnMeterGainEntry(tick, s));
            }

            // ── MANA REGEN (only characters with MaxMana > 0) ──────────────────
            foreach (var s in states.Where(s => s.Character.IsAlive && s.Character.MaxMana > 0))
            {
                var regen     = s.Character.ManaRegenPerTick;
                var manaBefore = s.Character.CurrentMana;
                s.Character.CurrentMana = Math.Min(s.Character.EffectiveMaxMana, manaBefore + regen);
                await Notify(new CombatLogEntry
                {
                    Tick      = tick,
                    ActorName = s.Character.Name,
                    EventType = "ManaRegen",
                    ManaRegen = regen,
                    ManaAfter = s.Character.CurrentMana,
                    Message   = $"{s.Character.Name} regens {regen} mana  ({manaBefore} -> {s.Character.CurrentMana})"
                });
            }

            // ── ADVANCE QUEUED SPELLS (reduce remaining by this tick's gain) ──
            foreach (var s in states.Where(s => s.QueuedSpell is not null && s.Character.IsAlive))
            {
                var gain = _turnmeter.ComputeGainPerTick(s.Character);
                s.QueuedSpell!.RemainingCost -= gain;
            }

            // ── ACTING ORDER: all ready combatants not CC'd, highest meter first ──
            var acting = states
                .Where(s => s.Character.IsAlive && s.Meter.IsReady && !IsCrowdControlled(s.Character))
                .OrderByDescending(s => s.Meter.CurrentValue)
                .ToList();

            // Log skipped turns for CC'd characters who are ready but cannot act
            foreach (var s in states.Where(s =>
                s.Character.IsAlive && s.Meter.IsReady && IsCrowdControlled(s.Character)))
            {
                var expired = _statusEffect.TickAll(s.Character);
                await NotifyExpiredEffectsAsync(tick, s.Character, expired, Notify);

                if (s.QueuedSpell is not null)
                {
                    await Notify(new CombatLogEntry
                    {
                        Tick      = tick,
                        ActorName = s.Character.Name,
                        EventType = "SpellLost",
                        AttackSourceName = s.QueuedSpell.Spell.Name,
                        Message   = $"{s.Character.Name} loses concentration on {s.QueuedSpell.Spell.Name} — crowd controlled!"
                    });
                    s.QueuedSpell = null;
                }

                await Notify(new CombatLogEntry
                {
                    Tick      = tick,
                    ActorName = s.Character.Name,
                    EventType = "SkippedTurn",
                    Message   = $"{s.Character.Name} is {GetCrowdControlLabel(s.Character)} and cannot act!"
                });
            }

            if (acting.Count > 0)
            {
                foreach (var actorState in acting)
            {
                if (!actorState.Character.IsAlive) continue; // killed earlier this tick

                // ── QUEUED SPELL HANDLING ──────────────────────────────────────
                IAttackSource attackSource;
                Character target;
                bool isSpell;
                int tmCost;

                if (actorState.QueuedSpell is not null)
                {
                    if (actorState.QueuedSpell.RemainingCost > 0)
                    {
                        // Still charging — skip the turn
                        await Notify(new CombatLogEntry
                        {
                            Tick      = tick,
                            ActorName = actorState.Character.Name,
                            EventType = "SpellCharging",
                            Message   = $"{actorState.Character.Name} is charging {actorState.QueuedSpell.Spell.Name}  (need {actorState.QueuedSpell.RemainingCost} more TM)"
                        });
                        continue;
                    }

                    // Queued spell is ready — fire it
                    var qs = actorState.QueuedSpell;
                    actorState.QueuedSpell = null;
                    attackSource  = qs.Spell;
                    isSpell       = true;
                    tmCost        = 100;

                    // Retarget if original target is no longer alive
                    if (!qs.Target.IsAlive)
                    {
                        var liveEnemies = states
                            .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                            .ToList();
                        if (liveEnemies.Count == 0) break;
                        var reSelector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                        target = await reSelector.SelectTargetAsync(actorState.Character, liveEnemies.Select(s => s.Character), ct);
                    }
                    else
                    {
                        target = qs.Target;
                    }

                    // ── MANA DEDUCTION (queued cast) ──────────────────────────
                    if (qs.Spell.ManaCost > 0)
                    {
                        var before = actorState.Character.CurrentMana;
                        actorState.Character.CurrentMana = Math.Max(0, before - qs.Spell.ManaCost);
                        await Notify(new CombatLogEntry
                        {
                            Tick             = tick,
                            ActorName        = actorState.Character.Name,
                            EventType        = "ManaDeduct",
                            ManaCost         = qs.Spell.ManaCost,
                            ManaAfter        = actorState.Character.CurrentMana,
                            AttackSourceName = qs.Spell.Name,
                            Message          = $"{actorState.Character.Name} spends {qs.Spell.ManaCost} mana to cast {qs.Spell.Name}. ({before} -> {actorState.Character.CurrentMana})"
                        });
                    }
                }
                else
                {
                    var enemies = states
                        .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                        .ToList();

                    if (enemies.Count == 0) break;

                    // ── RESOLVE ATTACK SOURCE ──────────────────────────────────
                    attackSource = ResolveAttackSource(actorState);

                    var meterNow = actorState.Meter.CurrentValue;
                    isSpell  = attackSource is Spell;
                    tmCost   = isSpell ? actorState.Character.ComputeSpellTurnMeterCost((Spell)attackSource) : 100;

                    if (!isSpell && actorState.Character.MemorizedSpells.Count > 0)
                    {
                        await Notify(new CombatLogEntry
                        {
                            Tick      = tick,
                            ActorName = actorState.Character.Name,
                            EventType = "InsufficientMana",
                            Message   = $"{actorState.Character.Name} lacks mana for spells — resorting to unarmed strike!"
                        });
                    }

                    // ── QUEUE CHECK — ready to cast but TM < cost → queue ──────
                    if (isSpell && meterNow < tmCost)
                    {
                        // Select target before queuing
                        var qEnemies = states
                            .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                            .ToList();
                        var qSelector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                        var qTarget   = await qSelector.SelectTargetAsync(
                            actorState.Character,
                            qEnemies.Select(s => s.Character),
                            ct);

                        var spell = (Spell)attackSource;
                        actorState.QueuedSpell = new QueuedSpellInfo(spell, qTarget, tmCost - meterNow);

                        await Notify(new CombatLogEntry
                        {
                            Tick             = tick,
                            ActorName        = actorState.Character.Name,
                            EventType        = "SpellQueued",
                            AttackSourceName = spell.Name,
                            TargetName       = qTarget.Name,
                            TurnMeterBefore  = meterNow,
                            IsSpell          = true,
                            Message          = $"{actorState.Character.Name} begins charging {spell.Name} on {qTarget.Name}  (need {tmCost - meterNow} more TM)"
                        });
                        continue;
                    }

                    // ── TARGET SELECTION ───────────────────────────────────────
                    if (actorState.IsSummoned && actorState.SummonedBy is { } master)
                    {
                        if (enemies.Count == 0) break;

                        if (lastAttackerOf.TryGetValue(master, out var lastAttacker)
                            && lastAttacker.IsAlive
                            && enemies.Any(e => e.Character == lastAttacker))
                        {
                            target = lastAttacker;
                        }
                        else
                        {
                            // last attacker dead or unknown — pick the most wounded enemy
                            target = enemies
                                .OrderBy(e => e.Character.CurrentHitPoints)
                                .First().Character;
                        }
                    }
                    else
                    {
                        var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                        target = await selector.SelectTargetAsync(
                            actorState.Character,
                            enemies.Select(s => s.Character),
                            ct);
                    }

                    // ── MANA DEDUCTION (direct cast) ─────────────────────────
                    if (isSpell)
                    {
                        var spell = (Spell)attackSource;
                        if (spell.ManaCost > 0)
                        {
                            var before = actorState.Character.CurrentMana;
                            actorState.Character.CurrentMana = Math.Max(0, before - spell.ManaCost);
                            await Notify(new CombatLogEntry
                            {
                                Tick             = tick,
                                ActorName        = actorState.Character.Name,
                                EventType        = "ManaDeduct",
                                ManaCost         = spell.ManaCost,
                                ManaAfter        = actorState.Character.CurrentMana,
                                AttackSourceName = spell.Name,
                                Message          = $"{actorState.Character.Name} spends {spell.ManaCost} mana to cast {spell.Name}. ({before} -> {actorState.Character.CurrentMana})"
                            });
                        }
                    }
                }

                actorState.Meter.IsActive = true;
                var turnMeterNow = actorState.Meter.CurrentValue;

                await Notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = actorState.Character.Name,
                    EventType        = "TurnStart",
                    TurnMeterBefore  = turnMeterNow,
                    IsReady          = true,
                    IsActive         = true,
                    AttackSourceName = attackSource.Name,
                    IsSpell          = isSpell,
                    TargetName       = target.Name,
                    Message          = $"{actorState.Character.Name} takes their turn  (TM: {turnMeterNow})"
                });

                if (isSpell && attackSource is Spell castSpell && castSpell.SummonedPet is { } petTemplate)
                {
                    var petChar = new Character
                    {
                        Name = petTemplate.Name,
                        MaxHitPoints = petTemplate.MaxHitPoints,
                        CurrentHitPoints = petTemplate.MaxHitPoints,
                        StrikeRating = petTemplate.StrikeRating,
                        TurnSpeed = petTemplate.TurnSpeed,
                        Strength = petTemplate.Strength,
                        Level = 1,
                        ClassId = 8,
                        Equipment = new ArmorSlots
                        {
                            Chest = new Armor
                            {
                                Name = $"{petTemplate.Name} Hide",
                                ArmorClass = petTemplate.ArmorClass,
                                MaxDexterityBonus = 6
                            }
                        }
                    };
                    var petWeapon = new Weapon
                    {
                        Name = $"{petTemplate.Name}'s Attack",
                        DamageDie = petTemplate.DamageDie,
                        DamageCount = petTemplate.DamageCount,
                        AttackBonus = petTemplate.AttackBonus,
                        DamageType = petTemplate.DamageType,
                        AttackType = AttackType.Melee,
                    };
                    var expiryRound = petTemplate.SummonDurationRounds > 0
                        ? currentRound + petTemplate.SummonDurationRounds
                        : 0;
                    var petState = new CombatantState(petChar, petWeapon, actorState.PartyIndex)
                    {
                        SummonedBy = actorState.Character,
                        SummonExpiryRound = expiryRound,
                    };
                    states.Add(petState);

                    await Notify(new CombatLogEntry
                    {
                        Tick = tick,
                        ActorName = actorState.Character.Name,
                        EventType = "PetSummoned",
                        SummonedPetName = petTemplate.Name,
                        RoundNumber = currentRound,
                        Message = $"{actorState.Character.Name} summons {petTemplate.Name}!" +
                                  (expiryRound > 0 ? $"  (lasts until end of round {expiryRound})" : "  (until slain)")
                    });

                    actorState.Meter.IsActive = false;
                    await Notify(BuildAfterTurnEntry(actorState, tick, tmCost));
                    continue;
                }

                // ── DOT TICK: DamageOverTime at start of actor's turn ────────────
                var dotResult = await ProcessActorDoTAsync(tick, actorState, heroParty, enemyParty, log, Notify);
                if (dotResult is not null)
                {
                    actorState.Meter.IsActive = false;
                    await Notify(BuildAfterTurnEntry(actorState, tick, tmCost));
                    return dotResult;
                }

                // ── TICK ALL EFFECTS (decrement durations) ────────────────────
                var expired = _statusEffect.TickAll(actorState.Character);
                await NotifyExpiredEffectsAsync(tick, actorState.Character, expired, Notify);

                // ── ATTACK RESOLUTION ──────────────────────────────────────────
                var result = _combat.ResolveAttack(actorState.Character, target, attackSource);
                await Notify(BuildAttackEntry(tick, actorState.Character.Name, attackSource.Name, isSpell, target.Name, result, attackSource.DamageType));

                // ── APPLY DAMAGE ───────────────────────────────────────────────
                if (result.IsHit)
                {
                    var hpBefore = target.CurrentHitPoints;
                    target.CurrentHitPoints -= result.Damage;
                    lastAttackerOf[target] = actorState.Character;

                    // Only emit a Damage event when damage actually got through.
                    // A 0-damage hit (fully absorbed by armor mitigation) is already
                    // communicated by the Attack event's IsHit=true — no separate entry.
                    if (result.Damage > 0)
                        await Notify(BuildDamageEntry(tick, target.Name, result.Damage, hpBefore, target.CurrentHitPoints));

                    // ── ON-HIT EFFECTS (spell after-effects) ──────────────────
                    if (attackSource is Spell spell)
                        await ProcessOnHitEffectsAsync(tick, target, spell, Notify);

                    // ── SPELL DISRUPTION ────────────────────────────────────
                    // Melee hits on a spellcaster have a chance to disrupt their
                    // spellcasting, reducing turnmeter progress.
                    if (attackSource.AttackType == AttackType.Melee
                        && result.IsHit && result.Damage > 0
                        && target.MemorizedSpells.Count > 0)
                    {
                        var targetState = states.First(s => s.Character == target);
                        if (targetState.Meter.CurrentValue > 0 && _dice.Roll(DieType.D100) <= 20)
                        {
                            var tmLoss = Math.Min(25, targetState.Meter.CurrentValue);
                            var before = targetState.Meter.CurrentValue;
                            targetState.Meter.CurrentValue -= tmLoss;
                            await Notify(new CombatLogEntry
                            {
                                Tick = tick,
                                ActorName = target.Name,
                                EventType = "SpellDisrupted",
                                TurnMeterBefore = before,
                                TurnMeterAfter = targetState.Meter.CurrentValue,
                                Message = $"{target.Name}'s spellcasting is disrupted! TM reduced by {tmLoss}."
                            });
                        }
                    }

                    // ── CONCENTRATION CHECK (target has queued spell) ─────────
                    if (result.IsHit && result.Damage > 0)
                    {
                        var concState = states.FirstOrDefault(s => s.Character == target);
                        if (concState?.QueuedSpell is not null)
                        {
                            var dc = Math.Max(10, result.Damage / 2);
                            var roll = _dice.Roll(DieType.D20) + concState.Character.Level;
                            if (roll < dc)
                            {
                                await Notify(new CombatLogEntry
                                {
                                    Tick = tick,
                                    ActorName = target.Name,
                                    EventType = "SpellLost",
                                    AttackSourceName = concState.QueuedSpell.Spell.Name,
                                    Message = $"{target.Name} loses concentration on {concState.QueuedSpell.Spell.Name}! (rolled {roll} vs DC {dc})"
                                });
                                concState.QueuedSpell = null;
                            }
                            else
                            {
                                await Notify(new CombatLogEntry
                                {
                                    Tick = tick,
                                    ActorName = target.Name,
                                    EventType = "ConcentrationPass",
                                    AttackSourceName = concState.QueuedSpell.Spell.Name,
                                    Message = $"{target.Name} maintains concentration on {concState.QueuedSpell.Spell.Name}. (rolled {roll} vs DC {dc})"
                                });
                            }
                        }
                    }

                    if (target.CurrentHitPoints <= 0)
                    {
                        await Notify(BuildDefeatEntry(tick, target));
                        var targetPartyIndex = actorState.PartyIndex == 0 ? 1 : 0;
                        var defResult = BuildDefeatResult(tick, targetPartyIndex, target, heroParty, enemyParty, log);
                        if (defResult is not null)
                        {
                            actorState.Meter.IsActive = false;
                            var deadState = states.FirstOrDefault(s => s.Character == target);
                            if (deadState?.QueuedSpell is not null)
                                deadState.QueuedSpell = null;
                            await Notify(BuildAfterTurnEntry(actorState, tick, tmCost));
                            return defResult;
                        }
                    }
                }

                // ── FUMBLE PENALTY ─────────────────────────────────────────────
                if (result.IsFumble)
                {
                    _statusEffect.Apply(actorState.Character, new StatusEffect
                    {
                        Name                = "Fumble Penalty",
                        Type                = StatusEffectType.Debuff,
                        AttackPowerModifier = -2,
                        Duration            = 1,
                        StackRule           = StackRule.NoStack,
                        Source              = "Fumble"
                    });
                    await Notify(new CombatLogEntry
                    {
                        Tick      = tick,
                        ActorName = actorState.Character.Name,
                        EventType = "FumblePenalty",
                        Message   = $"[FUMBLE] {actorState.Character.Name} fumbles! -2 AttackPower applied for next turn."
                    });
                }

                // ── END TURN ───────────────────────────────────────────────────
                actorState.Meter.IsActive = false;
                await Notify(BuildAfterTurnEntry(actorState, tick, tmCost));
                }
            }

            if (tick % RoundLength == 0)
            {
                await ExpireSummonedPetsAsync(tick, currentRound, states, Notify);

                await Notify(new CombatLogEntry
                {
                    Tick = tick,
                    EventType = "RoundEnd",
                    RoundNumber = currentRound,
                    Message = $"── Round {currentRound} ends ──"
                });
            }
        }

        return new CombatResult { MaxTicksReached = true, TotalTicks = maxTicks, Log = log, Seed = _dice.Seed, Party1 = heroParty, Party2 = enemyParty };
    }

    // 1v1 async convenience wrapper.
    public Task<CombatResult> SimulateAsync(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default) =>
        SimulateAsync(
            Party.Solo(fighter,  fighterAttack),
            Party.Solo(opponent, opponentAttack),
            maxTicks, observer, ct);

    // Sync wrappers — safe for console/test contexts (no sync context).
    // Do not call from a UI thread.
    public CombatResult Simulate(Party heroParty, Party enemyParty, int maxTicks = 1000) =>
        SimulateAsync(heroParty, enemyParty, maxTicks).GetAwaiter().GetResult();

    public CombatResult Simulate(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000) =>
        SimulateAsync(fighter, fighterAttack, opponent, opponentAttack, maxTicks)
            .GetAwaiter().GetResult();

    // ── Private helpers ────────────────────────────────────────────────────────

    private static List<CombatantState> BuildCombatantStates(Party heroParty, Party enemyParty)
    {
        var states = new List<CombatantState>();
        foreach (var m in heroParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 0));
        foreach (var m in enemyParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 1));
        return states;
    }

    private IAttackSource ResolveAttackSource(CombatantState state)
    {
        if (state.AttackSource is not null) return state.AttackSource;

        var spells = state.Character.MemorizedSpells;
        if (spells.Count > 0)
        {
            var spell = spells[_dice.RollIndex(spells.Count)];
            if (spell.ManaCost > 0 && state.Character.CurrentMana < spell.ManaCost)
                return UnarmedStrike.Default;
            return spell;
        }

        return UnarmedStrike.Default;
    }

    private CombatLogEntry BuildAfterTurnEntry(CombatantState state, int tick, int tmCost = 100)
    {
        var before = state.Meter.CurrentValue;
        state.Meter = _turnmeter.AfterTurn(state.Meter, tmCost);
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = state.Character.Name,
            EventType       = "TurnEnd",
            TurnMeterBefore = before,
            TurnMeterAfter  = state.Meter.CurrentValue,
            IsReady         = state.Meter.IsReady,
            IsActive        = false,
            Message         = $"{state.Character.Name} ends turn.  TM: {before} -> {state.Meter.CurrentValue} (cost: {tmCost})"
        };
    }

    private static CombatLogEntry BuildTurnMeterGainEntry(int tick, CombatantState s)
    {
        var before = s.PrevMeter;
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = s.Character.Name,
            EventType       = "TurnMeterGain",
            TurnMeterBefore = before,
            TurnMeterAfter  = s.Meter.CurrentValue,
            IsReady         = s.Meter.IsReady,
            IsActive        = s.Meter.IsActive,
            Message         = $"{s.Character.Name}  TM: {before} -> {s.Meter.CurrentValue}  (+{s.Meter.CurrentValue - before})"
        };
    }

    private static CombatLogEntry BuildDefeatEntry(int tick, Character target) => new()
    {
        Tick      = tick,
        ActorName = target.Name,
        EventType = target.IsDead ? "Death" : "KnockedOut",
        Message   = target.IsDead
            ? $"[DEAD] {target.Name} has been slain! (HP: {target.CurrentHitPoints})"
            : $"{target.Name} is unconscious! (HP: {target.CurrentHitPoints})"
    };

    private static CombatLogEntry BuildAttackEntry(
        int tick, string actorName, string attackSourceName, bool isSpell,
        string targetName, AttackResult result, DamageType damageType = DamageType.Slashing)
    {
        var outcome = result.IsCriticalHit ? "CRITICAL HIT!" :
                      result.IsFumble      ? "FUMBLE!"       :
                      result.IsHit         ? "HIT"           : "MISS";

        var msg = $"{actorName} [{attackSourceName}] -> {targetName}: " +
                  $"d20={result.HitRoll} + AP={result.AttackPower} = {result.HitRoll + result.AttackPower} " +
                  $"vs DP={result.DefensePower} -> {outcome}";

        if (result.IsHit && result.DamageContext is { } dc)
        {
            var critTag = result.IsCriticalHit ? " [x2 CRIT]" : "";
            msg += $" | Dmg: roll({dc.WeaponDiceRoll}) + attr({dc.AttributeModifier}) + flat({dc.FlatBonuses})" +
                   $" = {dc.BaseDamage}{critTag} x{dc.TypeMultiplier:0.0} - mit({dc.ArmorMitigation}) + elem({dc.ElementalModifiers}) = {dc.FinalDamage}";
        }

        var ctx    = CombatNarrator.GetContext(
            result.HitRoll, result.HitRoll + result.AttackPower, result.DefensePower,
            result.IsHit || result.IsCriticalHit, result.IsCriticalHit, result.IsFumble);
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx, isSpell, damageType);

        return new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorName,
            EventType        = "Attack",
            DieRoll          = result.HitRoll,
            AttackPower      = result.AttackPower,
            DefensePower     = result.DefensePower,
            IsHit            = result.IsHit,
            IsCritical       = result.IsCriticalHit,
            IsFumble         = result.IsFumble,
            DamageDealt      = result.Damage,
            AttackSourceName = attackSourceName,
            IsSpell          = isSpell,
            TargetName       = targetName,
            Phrase           = phrase,
            Message          = msg
        };
    }

    private static CombatLogEntry BuildDamageEntry(
        int tick, string targetName, int damage, int hpBefore, int hpAfter) => new()
    {
        Tick           = tick,
        ActorName      = targetName,
        EventType      = "Damage",
        DamageDealt    = damage,
        TargetHpBefore = hpBefore,
        TargetHpAfter  = hpAfter,
        Message        = $"{targetName} takes {damage} damage.  HP: {hpBefore} -> {hpAfter}"
    };

    // ── Status effect helpers ─────────────────────────────────────────────────

    private bool IsCrowdControlled(Character character) =>
        _statusEffect.HasEffectType(character, StatusEffectType.Stun) ||
        _statusEffect.HasEffectType(character, StatusEffectType.Root);

    private static string GetCrowdControlLabel(Character character)
    {
        var effect = character.ActiveStatusEffects.FirstOrDefault(
            e => e.Type is StatusEffectType.Stun or StatusEffectType.Root);
        return effect?.Type switch
        {
            StatusEffectType.Stun => "stunned",
            StatusEffectType.Root => "rooted",
            _                     => "crowd-controlled"
        };
    }

    private int RollDie(DieType die) => _dice.Roll(die);

    // ── Extracted acting-loop helpers ───────────────────────────────────────

    private async Task<CombatResult?> ProcessActorDoTAsync(
        int tick, CombatantState actorState,
        Party heroParty, Party enemyParty,
        List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var dotEffect in actorState.Character.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.DamageOverTime && e.DamagePerTurn > 0)
            .ToList())
        {
            var dotName = dotEffect.Name;
            var dotDmg  = dotEffect.DamagePerTurn;
            actorState.Character.CurrentHitPoints -= dotDmg;

            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = actorState.Character.Name,
                EventType        = "DoTTick",
                DamageDealt      = dotDmg,
                TargetHpAfter    = actorState.Character.CurrentHitPoints,
                StatusEffectName = dotName,
                Message          = $"{actorState.Character.Name} suffers {dotDmg} {dotName} damage."
            });

            if (actorState.Character.CurrentHitPoints <= 0)
            {
                await notify(BuildDefeatEntry(tick, actorState.Character));
                var defResult = BuildDefeatResult(
                    tick, actorState.PartyIndex, actorState.Character,
                    heroParty, enemyParty, log);
                if (defResult is not null) return defResult;
            }
        }
        return null;
    }

    private async Task ProcessOnHitEffectsAsync(
        int tick, Character target, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.OnHitEffects.Count == 0) return;

        foreach (var template in spell.OnHitEffects)
        {
            var dmgPerTurn = template.DamagePerTurn;
            if (dmgPerTurn <= 0 && template.DoTDamageCount > 0)
                for (var i = 0; i < template.DoTDamageCount; i++)
                    dmgPerTurn += RollDie(template.DoTDamageDie);

            var effect = new StatusEffect
            {
                Name                 = template.Name,
                Type                 = template.Type,
                ResistanceType       = template.ResistanceType,
                ResistanceBonuses    = template.ResistanceBonuses,
                Duration             = template.Duration,
                DamagePerTurn        = dmgPerTurn,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                TurnMeterModifier    = template.TurnMeterModifier,
                StackRule            = template.StackRule,
                ApplicationChance    = template.ApplicationChance,
                Source               = spell.Name
            };

            var resistance = target.ComputeResistance(effect.ResistanceType);
            var appResult = _statusEffect.TryApply(target, effect, resistance, _dice);

            if (appResult.Applied)
            {
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = target.Name,
                    EventType        = "EffectApplied",
                    StatusEffectName = effect.Name,
                    Message          = $"{target.Name} is afflicted with {effect.Name}!"
                });
            }
            else if (appResult.WasResisted)
            {
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = target.Name,
                    EventType        = "EffectResisted",
                    StatusEffectName = effect.Name,
                    ResistRoll       = appResult.Roll,
                    ResistThreshold  = appResult.TotalResistance,
                    Message          = $"{target.Name} resists {effect.Name}! (rolled {appResult.Roll} vs {appResult.TotalResistance} resistance)"
                });
            }
        }
    }

    private static async Task NotifyExpiredEffectsAsync(
        int tick, Character character,
        IReadOnlyList<string> expired,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var name in expired)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = character.Name,
                EventType        = "EffectExpired",
                StatusEffectName = name,
                Message          = $"{name} has worn off {character.Name}."
            });
        }
    }

    private static async Task ExpireSummonedPetsAsync(
        int tick,
        int currentRound,
        List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states.Where(s =>
            s.IsSummoned &&
            s.Character.IsAlive &&
            s.SummonExpiryRound > 0 &&
            s.SummonExpiryRound <= currentRound).ToList())
        {
            s.Character.CurrentHitPoints = -999;
            await notify(new CombatLogEntry
            {
                Tick = tick,
                EventType = "PetExpired",
                ActorName = s.Character.Name,
                SummonedPetName = s.Character.Name,
                RoundNumber = currentRound,
                Message = $"{s.Character.Name} fades away as the summoning ends."
            });
        }
    }

    private CombatResult? BuildDefeatResult(
        int tick, int defeatedPartyIndex,
        Character defeatedCharacter,
        Party heroParty, Party enemyParty,
        List<CombatLogEntry> log)
    {
        var losingParty = defeatedPartyIndex == 0 ? heroParty : enemyParty;
        if (!losingParty.IsDefeated) return null;

        return new CombatResult
        {
            WinningParty = defeatedPartyIndex == 0 ? enemyParty : heroParty,
            LosingParty  = losingParty,
            LoserStatus  = defeatedCharacter.VitalStatus,
            TotalTicks   = tick,
            Log          = log,
            Seed         = _dice.Seed,
            Party1       = heroParty,
            Party2       = enemyParty
        };
    }

    // Tracks a spell being charged over multiple ticks.
    private class QueuedSpellInfo
    {
        public Spell     Spell         { get; }
        public Character Target        { get; set; }
        public int       RemainingCost { get; set; }

        public QueuedSpellInfo(Spell spell, Character target, int remainingCost)
        {
            Spell         = spell;
            Target        = target;
            RemainingCost = remainingCost;
        }
    }

    // Tracks per-combatant state during a simulation run.
    private class CombatantState
    {
        public Character         Character         { get; }
        public IAttackSource?    AttackSource      { get; }
        public int               PartyIndex        { get; }   // 0 = hero party, 1 = enemy party
        public TurnmeterState    Meter             { get; set; }
        public int               PrevMeter         { get; set; }  // value before this tick's gain
        public QueuedSpellInfo?  QueuedSpell       { get; set; }
        public Character?        SummonedBy        { get; set; }
        public int               SummonExpiryRound { get; set; }
        public bool              IsSummoned        => SummonedBy is not null;

        public CombatantState(Character character, IAttackSource? attackSource, int partyIndex)
        {
            Character    = character;
            AttackSource = attackSource;
            PartyIndex   = partyIndex;
            Meter        = new TurnmeterState { CharacterId = character.Id, CharacterName = character.Name };
        }

        // Called at the start of each tick before Tick() is applied.
        public void SnapshotMeter() => PrevMeter = Meter.CurrentValue;
    }
}
