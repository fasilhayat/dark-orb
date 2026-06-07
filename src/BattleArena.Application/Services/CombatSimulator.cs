namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Application.Services.Combat;
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
    public const int DefaultMaxTicks = 1000;

    private readonly ICombatService       _combat;
    private readonly ITurnmeterService    _turnmeter;
    private readonly IStatusEffectService _statusEffect;
    private readonly IDiceService         _dice;
    private readonly ITargetSelector      _heroTargetSelector;
    private readonly ITargetSelector      _enemyTargetSelector;
    private readonly IActionDecisionSource      _heroActionSource;
    private readonly IActionDecisionSource      _enemyActionSource;
    private readonly CombatLogger              _logger;
    private readonly VictoryEvaluator           _victoryEvaluator;
    private readonly StatusEffectProcessor      _statusEffectProcessor;
    private readonly TurnMeterProcessor          _turnMeterProcessor;

    public CombatSimulator(
        ICombatService combat,
        ITurnmeterService turnmeter,
        IStatusEffectService statusEffect,
        IDiceService dice,
        ITargetSelector? heroTargetSelector  = null,
        ITargetSelector? enemyTargetSelector = null,
        IActionDecisionSource? heroActionSource  = null,
        IActionDecisionSource? enemyActionSource = null)
    {
        _combat              = combat;
        _turnmeter           = turnmeter;
        _statusEffect        = statusEffect;
        _dice                = dice;
        _logger              = new CombatLogger();
        _victoryEvaluator    = new VictoryEvaluator(dice);
        _statusEffectProcessor = new StatusEffectProcessor(statusEffect, dice, _logger);
        _turnMeterProcessor   = new TurnMeterProcessor(turnmeter, _logger);
        _heroTargetSelector  = heroTargetSelector ?? new RandomTargetSelector();
        _enemyTargetSelector = enemyTargetSelector ?? new RandomTargetSelector();
        _heroActionSource    = heroActionSource    ?? new AutoActionDecisionSource(dice);
        _enemyActionSource   = enemyActionSource   ?? new AutoActionDecisionSource(dice);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    // Party-vs-party async entry point — supports 1v1, 1vN, NvN (hero side max 6).
    // Observer receives every event in real time (use for GUI animation).
    // CancellationToken allows forfeiting or time-out from the caller.
    public async Task<CombatResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = DefaultMaxTicks,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains)
    {
        const int RoundLength = 10;

        var states         = BuildCombatantStates(heroParty, enemyParty);
        foreach (var s in states)
        {
            var gain = _turnmeter.ComputeGainPerTick(s.Character);
            s.Meter.CurrentValue = Math.Min(gain * RoundLength, TurnmeterState.TurnThreshold);
        }
        var stateMap       = states.ToDictionary(s => s.Character);
        var currentRound   = 0;
        var lastAttackerOf = new Dictionary<Character, Character>();
        string? activeActorName = null;
        var log            = new List<CombatLogEntry>();

        // Log + notify the observer for every event in one call.
        // Automatically stamps the currently-acting character so consumers
        // never need to track it themselves.
        async Task Notify(CombatLogEntry entry)
        {
            entry.ActiveActorName = activeActorName;
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

            await ProcessTickMeterAndManaAsync(tick, states, Notify);
            await ProcessCrowdControlledActorsAsync(tick, states, Notify);

            foreach (var actorState in GetActingOrder(states))
            {
                if (!actorState.Character.IsAlive) continue;
                var turnResult = await ProcessActingActorAsync(
                    tick, currentRound, actorState, states, stateMap, lastAttackerOf,
                    heroParty, enemyParty, log, Notify, name => { activeActorName = name; }, ct, terrain);
                if (turnResult is not null) return turnResult;
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
        int maxTicks = DefaultMaxTicks,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(
            Party.Solo(fighter,  fighterAttack),
            Party.Solo(opponent, opponentAttack),
            maxTicks, observer, ct, terrain);

    // Sync wrappers — safe for console/test contexts (no sync context).
    // Do not call from a UI thread.
    public CombatResult Simulate(Party heroParty, Party enemyParty, int maxTicks = DefaultMaxTicks,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(heroParty, enemyParty, maxTicks, terrain: terrain).GetAwaiter().GetResult();

    public CombatResult Simulate(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = DefaultMaxTicks,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(fighter, fighterAttack, opponent, opponentAttack, maxTicks, terrain: terrain)
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

    private CombatLogEntry BuildAfterTurnEntry(CombatantState state, int tick, int tmCost = TurnmeterState.TurnThreshold)
    {
        var before = state.Meter.CurrentValue;
        state.Meter = _turnmeter.AfterTurn(state.Meter, tmCost);
        return _logger.BuildAfterTurnEntry(tick, state.Character.Name, before, state.Meter.CurrentValue, tmCost);
    }

    private CombatLogEntry BuildDefeatEntry(int tick, Character target) =>
        _logger.BuildDefeatEntry(tick, target);

    private CombatLogEntry BuildAttackEntry(
        int tick, string actorName, string attackSourceName, bool isSpell,
        string targetName, AttackResult result, DamageType damageType = DamageType.Slashing,
        int? spellLevel = null, int? casterLevel = null) =>
        _logger.BuildAttackEntry(tick, actorName, attackSourceName, isSpell, targetName, result,
            damageType, spellLevel, casterLevel, _dice.RollIndex);

    private CombatLogEntry BuildDamageEntry(
        int tick, string targetName, int damage, int hpBefore, int hpAfter) =>
        _logger.BuildDamageEntry(tick, targetName, damage, hpBefore, hpAfter);

    // ── Healing helpers ────────────────────────────────────────────────────────

    private async Task<CombatResult?> ProcessHealingSpellAsync(
        int tick, CombatantState actorState, ActorSetup setup, Spell spell,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify, TerrainType terrain)
    {
        if (spell.IsGroupHeal)
        {
            var allies = states
                .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive && s.Character.CurrentHitPoints < s.Character.MaxHitPoints)
                .ToList();

            foreach (var ally in allies)
            {
                var healAmount = _combat.ResolveHealing(actorState.Character, ally.Character, spell, terrain);
                var hpBefore = ally.Character.CurrentHitPoints;
                ally.Character.CurrentHitPoints = Math.Min(ally.Character.MaxHitPoints, hpBefore + healAmount);
                await notify(new CombatLogEntry
                {
                    Tick            = tick,
                    ActorName       = ally.Character.Name,
                    TargetName      = ally.Character.Name,
                    EventType       = "Healed",
                    DamageDealt     = healAmount,
                    TargetHpBefore  = hpBefore,
                    TargetHpAfter   = ally.Character.CurrentHitPoints,
                    AttackSourceName = spell.Name,
                    IsSpell         = true,
                    Message         = $"{ally.Character.Name} is healed for {healAmount} by {spell.Name}.  HP: {hpBefore} -> {ally.Character.CurrentHitPoints}"
                });
            }
            return null;
        }

        // Single-target heal: pick the ally with the lowest HP.
        var target = states
            .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive && s.Character.CurrentHitPoints < s.Character.MaxHitPoints)
            .MinBy(s => s.Character.CurrentHitPoints);

        if (target is null) return null;

        var heal = _combat.ResolveHealing(actorState.Character, target.Character, spell, terrain);
        var hpB = target.Character.CurrentHitPoints;
        target.Character.CurrentHitPoints = Math.Min(target.Character.MaxHitPoints, hpB + heal);
        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = target.Character.Name,
            TargetName      = target.Character.Name,
            EventType       = "Healed",
            DamageDealt     = heal,
            TargetHpBefore  = hpB,
            TargetHpAfter   = target.Character.CurrentHitPoints,
            AttackSourceName = spell.Name,
            IsSpell         = true,
            Message         = $"{target.Character.Name} is healed for {heal} by {spell.Name}.  HP: {hpB} -> {target.Character.CurrentHitPoints}"
        });
        return null;
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
        foreach (var s in states)
        {
            if (!s.IsSummoned || !s.Character.IsAlive || s.SummonExpiryRound <= 0 || s.SummonExpiryRound > currentRound)
                continue;
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

    // ── Tick-level orchestration ────────────────────────────────────────────────

    private List<CombatantState> GetActingOrder(List<CombatantState> states) =>
        states
            .Where(s => s.Character.IsAlive && s.Meter.IsReady && s.Character.TryGetCrowdControlLabel() is null)
            .OrderByDescending(s => s.Meter.CurrentValue)
            .ToList();

    private async Task ProcessTickMeterAndManaAsync(
        int tick, List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states)
        {
            if (!s.Character.IsAlive) continue;

            s.SnapshotMeter();
            s.Meter = _turnmeter.Tick(s.Character, s.Meter);
            await notify(_logger.BuildTurnMeterGainEntry(tick, s.Character.Name, s.PrevMeter, s.Meter.CurrentValue, s.Meter.IsReady, s.Meter.IsActive));

            if (s.Character.MaxMana > 0 && s.Character.CurrentMana < s.Character.EffectiveMaxMana)
            {
                var regen = s.Character.ManaRegenPerTick;

                // Check for mana leech — redirect regen to caster instead
                var leech = s.Character.ActiveStatusEffects
                    .FirstOrDefault(e => e.Type == StatusEffectType.Leech
                        && e.LeechPerTurn > 0
                        && e.LeechResourceType == "Mana"
                        && !string.IsNullOrEmpty(e.CasterName));

                if (leech is not null)
                {
                    // Redirect regen to leech caster (capped by LeechPerTurn)
                    var redirectAmount = Math.Min(regen, leech.LeechPerTurn);
                    var leechCaster = states.FirstOrDefault(cs => cs.Character.Name == leech.CasterName);
                    if (leechCaster?.Character.IsAlive == true)
                    {
                        var casterManaBefore = leechCaster.Character.CurrentMana;
                        leechCaster.Character.CurrentMana = Math.Min(
                            leechCaster.Character.EffectiveMaxMana,
                            casterManaBefore + redirectAmount);

                        await notify(new CombatLogEntry
                        {
                            Tick               = tick,
                            ActorName          = s.Character.Name,
                            EventType          = "LeechTick",
                            LeechAmount        = redirectAmount,
                            LeechCasterName    = leech.CasterName,
                            LeechResourceType  = "Mana",
                            LeechTargetAfter   = s.Character.CurrentMana,
                            LeechCasterAfter   = leechCaster.Character.CurrentMana,
                            StatusEffectName   = leech.Name,
                            Message            = $"{s.Character.Name}'s mana regen ({regen}) redirected to {leech.CasterName} by {leech.Name}.  {leech.CasterName} gains {redirectAmount} mana."
                        });
                        continue; // Skip normal regen notification
                    }
                }

                // Normal regen (no leech or caster dead)
                var manaBefore = s.Character.CurrentMana;
                s.Character.CurrentMana = Math.Min(s.Character.EffectiveMaxMana, manaBefore + regen);
                await notify(new CombatLogEntry
                {
                    Tick      = tick,
                    ActorName = s.Character.Name,
                    EventType = "ManaRegen",
                    ManaRegen = regen,
                    ManaAfter = s.Character.CurrentMana,
                    Message   = $"{s.Character.Name} regens {regen} mana  ({manaBefore} -> {s.Character.CurrentMana})"
                });
            }

            if (s.QueuedSpell is not null)
                s.QueuedSpell.RemainingCost -= _turnmeter.ComputeGainPerTick(s.Character);
        }
    }

    private async Task ProcessCrowdControlledActorsAsync(
        int tick, List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states)
        {
            if (!s.Character.IsAlive || !s.Meter.IsReady) continue;
            var ccLabel = s.Character.TryGetCrowdControlLabel();
            if (ccLabel is null) continue;

            var expired = _statusEffect.TickAll(s.Character);
            await NotifyExpiredEffectsAsync(tick, s.Character, expired, notify);

            if (s.QueuedSpell is not null)
            {
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = s.Character.Name,
                    EventType        = "SpellLost",
                    AttackSourceName = s.QueuedSpell.Spell.Name,
                    Message          = $"{s.Character.Name} loses concentration on {s.QueuedSpell.Spell.Name} — crowd controlled!"
                });
                s.QueuedSpell = null;
            }

            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = s.Character.Name,
                EventType = "SkippedTurn",
                CcLabel   = ccLabel,
                Message   = $"{s.Character.Name} is {ccLabel} and cannot act!"
            });
        }
    }

    // ── Per-actor turn orchestration ────────────────────────────────────────────

    private async Task<CombatResult?> ProcessActingActorAsync(
        int tick, int currentRound, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify, Action<string?> setActiveActor,
        CancellationToken ct, TerrainType terrain)
    {
        var setup = await SetupActorAttackAsync(tick, actorState, states, lastAttackerOf, notify, ct);
        if (setup is null) return null;

        actorState.Meter.IsActive = true;
        setActiveActor(actorState.Character.Name);

        // Set remaining attacks for multi-attack support
        actorState.AttacksRemaining = setup.IsSpell ? 1 : actorState.Character.AttacksPerTurn;

        await notify(new CombatLogEntry
        {
            Tick               = tick,
            ActorName          = actorState.Character.Name,
            EventType          = "TurnStart",
            TurnMeterBefore    = actorState.Meter.CurrentValue,
            IsReady            = true,
            IsActive           = true,
            AttackSourceName   = setup.Source.Name,
            IsSpell            = setup.IsSpell,
            TargetName         = setup.Target.Name,
            TurnMeterSnapshot  = states
                .Where(s => s.Character.IsAlive)
                .ToDictionary(s => s.Character.Name, s => s.Meter.CurrentValue),
            SpellLevel         = setup.IsSpell && setup.Source is Spell ss ? ss.SpellLevel : null,
            CasterLevel        = actorState.Character.Level,
            Message            = $"{actorState.Character.Name} takes their turn  (TM: {actorState.Meter.CurrentValue})"
        });

        if (await TryHandlePetSummonAsync(tick, actorState, setup, states, stateMap, currentRound, notify))
        {
            setActiveActor(null);
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return null;
        }

        await _statusEffectProcessor.ProcessActorHoTAsync(tick, actorState, notify);

        await _statusEffectProcessor.ProcessActorLeechAsync(tick, actorState, states, notify);

        var dotDefeated = await _statusEffectProcessor.ProcessActorDoTAsync(tick, actorState, notify);
        if (dotDefeated)
        {
            var dotResult = _victoryEvaluator.BuildDefeatResult(
                tick, actorState.PartyIndex, actorState.Character,
                heroParty, enemyParty, log);
            setActiveActor(null);
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            if (dotResult is not null) return dotResult;
        }

        var expired = _statusEffect.TickAll(actorState.Character);
        await NotifyExpiredEffectsAsync(tick, actorState.Character, expired, notify);

        // ── Healing spells take a different path ──────────────────────────
        if (setup.Source is Spell castSpell && castSpell.IsHealing)
        {
            var healResult = await ProcessHealingSpellAsync(tick, actorState, setup, castSpell, states, notify, terrain);
            setActiveActor(null);
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return healResult;
        }

        // ── Multi-attack loop ──────────────────────────────────────────────
        CombatResult? multiAttackOutcome = null;
        while (actorState.AttacksRemaining > 0)
        {
            actorState.AttacksRemaining--;

            var attackNum = actorState.Character.AttacksPerTurn - actorState.AttacksRemaining;
            if (attackNum > 1)
            {
                await notify(new CombatLogEntry
                {
                    Tick      = tick,
                    ActorName = actorState.Character.Name,
                    EventType = "ExtraAttack",
                    Message   = $"{actorState.Character.Name} strikes again! (attack {attackNum} of {actorState.Character.AttacksPerTurn})"
                });
            }

            var result = _combat.ResolveAttack(actorState.Character, setup.Target, setup.Source, actorState.EngagementRange, terrain);

            var spellLevel = setup.IsSpell && setup.Source is Spell attackSpell ? attackSpell.SpellLevel : (int?)null;
            await notify(BuildAttackEntry(tick, actorState.Character.Name, setup.Source.Name, setup.IsSpell, setup.Target.Name, result, setup.Source.DamageType, spellLevel, actorState.Character.Level));

            var outcome = await ResolveAttackOutcomeAsync(
                tick, actorState, setup, result, states, stateMap, lastAttackerOf,
                heroParty, enemyParty, log, notify);
            if (outcome is not null)
            {
                multiAttackOutcome = outcome;
                break;
            }

            await _turnMeterProcessor.ApplyDefenderTmBoostAsync(tick, actorState, setup.Target, result, stateMap, notify);
            await _statusEffectProcessor.ApplyFumblePenaltyAsync(tick, actorState, result, notify);

            // ── Self-buffs from protective spells ──────────────────────────
            if (setup.Source is Spell spellWithBuffs && spellWithBuffs.OnHitEffects.Count > 0)
                await _statusEffectProcessor.ProcessSelfBuffsAsync(tick, actorState.Character, spellWithBuffs, notify);

            // If target died, re-select for remaining attacks
            if (!setup.Target.IsAlive && actorState.AttacksRemaining > 0)
            {
                var liveEnemies = states
                    .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                    .ToList();
                if (liveEnemies.Count == 0)
                {
                    multiAttackOutcome = null;
                    break;
                }
                var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                var newTarget = await selector.SelectTargetAsync(
                    actorState.Character, liveEnemies.Select(s => s.Character), ct);
                setup = new ActorSetup(setup.Source, newTarget, setup.TmCost, setup.IsSpell);
            }
        }

        if (multiAttackOutcome is not null)
        {
            setActiveActor(null);
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return multiAttackOutcome;
        }

        setActiveActor(null);
        actorState.Meter.IsActive = false;
        await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
        return null;
    }

    // ── Attack setup (queued-spell path vs new-attack path) ─────────────────────

    private async Task<ActorSetup?> SetupActorAttackAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        if (actorState.QueuedSpell is not null)
            return await HandleQueuedSpellAsync(tick, actorState, states, notify, ct);
        return await HandleNewAttackSetupAsync(tick, actorState, states, lastAttackerOf, notify, ct);
    }

    private async Task<ActorSetup?> HandleQueuedSpellAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var qs = actorState.QueuedSpell!;

        if (qs.RemainingCost > 0)
        {
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "SpellCharging",
                Message   = $"{actorState.Character.Name} is charging {qs.Spell.Name}  (need {qs.RemainingCost} more TM)"
            });
            return null;
        }

        actorState.QueuedSpell = null;
        var target = qs.Target;

        if (!target.IsAlive)
        {
            if (qs.Spell.IsHealing)
            {
                var liveAllies = states
                    .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive)
                    .Select(s => s.Character)
                    .ToList();
                var healTarget = liveAllies
                    .Where(a => a.CurrentHitPoints < a.MaxHitPoints)
                    .MinBy(a => a.CurrentHitPoints);
                target = healTarget ?? actorState.Character;
            }
            else
            {
                var liveEnemies = states
                    .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                    .ToList();
                if (liveEnemies.Count == 0) return null;
                var reSelector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                target = await reSelector.SelectTargetAsync(
                    actorState.Character, liveEnemies.Select(s => s.Character), ct);
            }
        }

        var actualTmCost = actorState.Character.ComputeSpellTurnMeterCost(qs.Spell);
        await DeductManaCostAsync(tick, actorState, qs.Spell, notify);
        return new ActorSetup(qs.Spell, target, actualTmCost, IsSpell: true);
    }

    private async Task<ActorSetup?> HandleNewAttackSetupAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var enemies = states
            .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
            .ToList();
        if (enemies.Count == 0) return null;

        var allies = states
            .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive)
            .Select(s => s.Character)
            .ToList();

        var decisionSource = actorState.PartyIndex == 0 ? _heroActionSource : _enemyActionSource;
        var attackSource = await decisionSource.ChooseAttackAsync(
            actorState.Character,
            actorState.AttackSource,
            enemies.Select(s => s.Character).ToList(),
            allies,
            tick,
            ct);

        if (attackSource is null)
        {
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "SkippedTurn",
                Message   = $"{actorState.Character.Name} skips their turn."
            });
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, TurnmeterState.TurnThreshold));
            return null;
        }

        if (attackSource is MoveIntent)
        {
            var speed = actorState.Character.EffectiveMovementSpeed;
            var from = actorState.EngagementRange;
            actorState.EngagementRange = from switch
            {
                EngagementRange.Melee => EngagementRange.Short,
                EngagementRange.Long => EngagementRange.Short,
                EngagementRange.Short => EngagementRange.Melee,
                _ => EngagementRange.Melee
            };
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "Move",
                Message   = $"{actorState.Character.Name} moves 15 ft ({from} → {actorState.EngagementRange}). Speed: {speed} ft"
            });
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, TurnmeterState.TurnThreshold));
            return null;
        }

        var isSpell = attackSource is Spell;
        var meterNow = actorState.Meter.CurrentValue;
        var tmCost = isSpell ? actorState.Character.ComputeSpellTurnMeterCost((Spell)attackSource) : 100;

        if (attackSource is UnarmedStrike && actorState.Character.MemorizedSpells.Count > 0)
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "InsufficientMana",
                Message   = $"{actorState.Character.Name} lacks mana for spells — resorting to unarmed strike!"
            });

        if (isSpell && meterNow < tmCost)
        {
            await QueueSpellAsync(tick, actorState, (Spell)attackSource, enemies, allies, tmCost, meterNow, notify, ct);
            return null;
        }

        Character target;
        if (attackSource is Spell castSpell && castSpell.IsHealing)
        {
            // For healing spells, pick the most injured ally as the logged target
            var healTarget = allies
                .Where(a => a.CurrentHitPoints < a.MaxHitPoints)
                .MinBy(a => a.CurrentHitPoints);
            target = healTarget ?? actorState.Character;
        }
        else
        {
            target = await SelectActorTargetAsync(actorState, enemies, lastAttackerOf, ct);
        }
        await DeductManaCostAsync(tick, actorState, isSpell ? (Spell)attackSource : null, notify);
        return new ActorSetup(attackSource, target, tmCost, isSpell);
    }

    private async Task<Character> SelectActorTargetAsync(
        CombatantState actorState, List<CombatantState> enemies,
        Dictionary<Character, Character> lastAttackerOf, CancellationToken ct)
    {
        if (actorState.IsSummoned && actorState.SummonedBy is { } master)
        {
            var last = lastAttackerOf.GetValueOrDefault(master);
            if (last?.IsAlive == true && enemies.Any(e => e.Character == last))
                return last;
            return enemies.OrderBy(e => e.Character.CurrentHitPoints).First().Character;
        }
        var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
        return await selector.SelectTargetAsync(actorState.Character, enemies.Select(s => s.Character), ct);
    }

    private async Task QueueSpellAsync(
        int tick, CombatantState actorState, Spell spell,
        List<CombatantState> enemies, List<Character> allies, int tmCost, int meterNow,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        Character target;
        if (spell.IsHealing)
        {
            var healTarget = allies
                .Where(a => a.CurrentHitPoints < a.MaxHitPoints)
                .MinBy(a => a.CurrentHitPoints);
            target = healTarget ?? actorState.Character;
        }
        else
        {
            var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
            target = await selector.SelectTargetAsync(
                actorState.Character, enemies.Select(s => s.Character), ct);
        }
        actorState.QueuedSpell = new QueuedSpellInfo(spell, target, tmCost - meterNow);
        await notify(new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorState.Character.Name,
            EventType        = "SpellQueued",
            AttackSourceName = spell.Name,
            TargetName       = target.Name,
            TurnMeterBefore  = meterNow,
            IsSpell          = true,
            Message          = $"{actorState.Character.Name} begins charging {spell.Name} on {target.Name}  (need {tmCost - meterNow} more TM)"
        });
    }

    private async Task DeductManaCostAsync(
        int tick, CombatantState actorState, Spell? spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell is null || spell.ManaCost <= 0) return;
        var before = actorState.Character.CurrentMana;
        actorState.Character.CurrentMana = Math.Max(0, before - spell.ManaCost);
        await notify(new CombatLogEntry
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

    // ── Pet summoning ────────────────────────────────────────────────────────────

    private async Task<bool> TryHandlePetSummonAsync(
        int tick, CombatantState actorState, ActorSetup setup,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap, int currentRound,
        Func<CombatLogEntry, Task> notify)
    {
        if (!setup.IsSpell || setup.Source is not Spell castSpell || castSpell.SummonedPet is null)
            return false;

        var pet         = castSpell.SummonedPet;
        var expiryRound = pet.SummonDurationRounds > 0 ? currentRound + pet.SummonDurationRounds : 0;

        var petChar = new Character
        {
            Name             = pet.Name,
            MaxHitPoints     = pet.MaxHitPoints,
            CurrentHitPoints = pet.MaxHitPoints,
            StrikeRating     = pet.StrikeRating,
            TurnSpeed        = pet.TurnSpeed,
            Strength         = pet.Strength,
            Level            = 1,
            ClassId          = 8,
            Equipment        = new ArmorSlots
            {
                Chest = new Armor
                {
                    Name              = $"{pet.Name} Hide",
                    ArmorClass        = pet.ArmorClass,
                    MaxDexterityBonus = 6
                }
            }
        };
        var petWeapon = new Weapon
        {
            Name        = $"{pet.Name}'s Attack",
            DamageDie   = pet.DamageDie,
            DamageCount = pet.DamageCount,
            AttackBonus = pet.AttackBonus,
            DamageType  = pet.DamageType,
            AttackType  = AttackType.Melee,
        };
        var newState = new CombatantState(petChar, petWeapon, actorState.PartyIndex)
        {
            SummonedBy        = actorState.Character,
            SummonExpiryRound = expiryRound,
        };
        states.Add(newState);
        stateMap[petChar] = newState;

        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = actorState.Character.Name,
            EventType       = "PetSummoned",
            SummonedPetName = pet.Name,
            RoundNumber     = currentRound,
            Message         = $"{actorState.Character.Name} summons {pet.Name}!" +
                              (expiryRound > 0 ? $"  (lasts until end of round {expiryRound})" : "  (until slain)")
        });
        return true;
    }

    // ── Attack outcome dispatch ──────────────────────────────────────────────────

    private async Task<CombatResult?> ResolveAttackOutcomeAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        if (result.IsClash)
            return await ProcessClashAsync(
                tick, actorState, setup, result, states, stateMap, lastAttackerOf,
                heroParty, enemyParty, log, notify);
        if (result.IsHit)
            return await ProcessHitAsync(
                tick, actorState, setup, result, states, stateMap, lastAttackerOf,
                heroParty, enemyParty, log, notify);
        return null;
    }

    private async Task<CombatResult?> ProcessClashAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var target        = setup.Target;
        var defenderState = stateMap[target];
        var counterSource = defenderState.AttackSource ?? (IAttackSource)UnarmedStrike.Default;
        var counterDc     = _combat.ResolveDamage(target, actorState.Character, counterSource);
        var counterDamage = Math.Max(0, counterDc.FinalDamage / 2);

        if (result.Damage > 0)
        {
            var defHpBefore = target.CurrentHitPoints;
            target.CurrentHitPoints -= result.Damage;
            lastAttackerOf[target] = actorState.Character;
            await notify(BuildDamageEntry(tick, target.Name, result.Damage, defHpBefore, target.CurrentHitPoints));
        }
        if (counterDamage > 0)
        {
            var atkHpBefore = actorState.Character.CurrentHitPoints;
            actorState.Character.CurrentHitPoints -= counterDamage;
            await notify(BuildDamageEntry(tick, actorState.Character.Name, counterDamage, atkHpBefore, actorState.Character.CurrentHitPoints));
        }

        await notify(new CombatLogEntry
        {
            Tick       = tick,
            ActorName  = actorState.Character.Name,
            EventType  = "Clash",
            TargetName = target.Name,
            Message    = $"[CLASH] Both weapons collide! {actorState.Character.Name} and {target.Name} exchange glancing blows."
        });

        if (actorState.Character.CurrentHitPoints <= 0)
        {
            await notify(BuildDefeatEntry(tick, actorState.Character));
            var r = _victoryEvaluator.BuildDefeatResult(tick, actorState.PartyIndex, actorState.Character, heroParty, enemyParty, log);
            if (r is not null) return r;
        }
        if (target.CurrentHitPoints <= 0)
        {
            await notify(BuildDefeatEntry(tick, target));
            var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
            var r = _victoryEvaluator.BuildDefeatResult(tick, targetPartyIdx, target, heroParty, enemyParty, log);
            if (r is not null)
            {
                var deadState = stateMap.GetValueOrDefault(target);
                if (deadState?.QueuedSpell is not null) deadState.QueuedSpell = null;
                return r;
            }
        }
        return null;
    }

    private async Task<CombatResult?> ProcessHitAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var target   = setup.Target;
        var hpBefore = target.CurrentHitPoints;
        target.CurrentHitPoints -= result.Damage;
        lastAttackerOf[target] = actorState.Character;

        if (result.Damage > 0)
            await notify(BuildDamageEntry(tick, target.Name, result.Damage, hpBefore, target.CurrentHitPoints));

        if (result.IsDevastatingStrike)
            await notify(new CombatLogEntry
            {
                Tick        = tick,
                ActorName   = actorState.Character.Name,
                EventType   = "DevastatingStrike",
                TargetName  = target.Name,
                DamageDealt = result.Damage,
                Message     = $"[DEVASTATING STRIKE] {actorState.Character.Name} shatters {target.Name}'s guard! x3 damage!"
            });

        if (setup.Source is Spell hitSpell)
            await _statusEffectProcessor.ProcessOnHitEffectsAsync(tick, actorState.Character, target, hitSpell, notify);

        await ProcessSpellDisruptionAsync(tick, setup, result, stateMap, notify);
        await ProcessConcentrationAsync(tick, target, result, stateMap, notify);

        if (target.CurrentHitPoints > 0) return null;

        await notify(BuildDefeatEntry(tick, target));
        var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
        var defResult = _victoryEvaluator.BuildDefeatResult(tick, targetPartyIdx, target, heroParty, enemyParty, log);
        if (defResult is not null)
        {
            var deadState = stateMap.GetValueOrDefault(target);
            if (deadState?.QueuedSpell is not null) deadState.QueuedSpell = null;
            return defResult;
        }
        return null;
    }

    private async Task ProcessSpellDisruptionAsync(
        int tick, ActorSetup setup, AttackResult result,
        Dictionary<Character, CombatantState> stateMap, Func<CombatLogEntry, Task> notify)
    {
        if (setup.Source.AttackType != AttackType.Melee) return;
        if (result.Damage <= 0 || setup.Target.MemorizedSpells.Count == 0) return;
        var targetState = stateMap[setup.Target];
        await TryApplySpellDisruptionAsync(tick, targetState, notify);
    }

    private async Task TryApplySpellDisruptionAsync(
        int tick, CombatantState targetState, Func<CombatLogEntry, Task> notify)
    {
        if (targetState.Meter.CurrentValue <= 0) return;
        if (_dice.Roll(DieType.D100) > 20) return;
        var tmLoss = Math.Min(25, targetState.Meter.CurrentValue);
        var before = targetState.Meter.CurrentValue;
        targetState.Meter.CurrentValue -= tmLoss;
        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = targetState.Character.Name,
            EventType       = "SpellDisrupted",
            TurnMeterBefore = before,
            TurnMeterAfter  = targetState.Meter.CurrentValue,
            Message         = $"{targetState.Character.Name}'s spellcasting is disrupted! TM reduced by {tmLoss}."
        });
    }

    private async Task ProcessConcentrationAsync(
        int tick, Character target, AttackResult result,
        Dictionary<Character, CombatantState> stateMap, Func<CombatLogEntry, Task> notify)
    {
        if (result.Damage <= 0) return;
        var concState = stateMap.GetValueOrDefault(target);
        if (concState?.QueuedSpell is null) return;
        var dc   = Math.Max(10, result.Damage / 2);
        var roll = _dice.Roll(DieType.D20) + concState.Character.Level;
        if (roll < dc)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "SpellLost",
                AttackSourceName = concState.QueuedSpell.Spell.Name,
                Message          = $"{target.Name} loses concentration on {concState.QueuedSpell.Spell.Name}! (rolled {roll} vs DC {dc})"
            });
            concState.QueuedSpell = null;
        }
        else
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "ConcentrationPass",
                AttackSourceName = concState.QueuedSpell.Spell.Name,
                Message          = $"{target.Name} maintains concentration on {concState.QueuedSpell.Spell.Name}. (rolled {roll} vs DC {dc})"
            });
        }
    }

}
