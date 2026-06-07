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
    private readonly SpellProcessor              _spellProcessor;
    private readonly TurnProcessor              _turnProcessor;
    private readonly AttackResolver              _attackResolver;

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
        _spellProcessor       = new SpellProcessor(combat, dice, _logger, _statusEffectProcessor);
        _heroTargetSelector  = heroTargetSelector ?? new RandomTargetSelector();
        _enemyTargetSelector = enemyTargetSelector ?? new RandomTargetSelector();
        _heroActionSource    = heroActionSource    ?? new AutoActionDecisionSource(dice);
        _enemyActionSource   = enemyActionSource   ?? new AutoActionDecisionSource(dice);
        _turnProcessor        = new TurnProcessor(dice, statusEffect, _heroActionSource, _enemyActionSource, _heroTargetSelector, _enemyTargetSelector, _spellProcessor, _turnMeterProcessor, _logger);
        _attackResolver       = new AttackResolver(combat, dice, _logger, _victoryEvaluator, _turnMeterProcessor, _statusEffectProcessor, _spellProcessor);
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

        var states         = CombatSimulatorHelpers.BuildCombatantStates(heroParty, enemyParty);
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
            _dice.CurrentActorName = null;

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

            await _turnMeterProcessor.ProcessTickMeterAndManaAsync(tick, states, Notify);
            await _turnProcessor.ProcessCrowdControlledActorsAsync(tick, states, Notify);

            foreach (var actorState in CombatSimulatorHelpers.GetActingOrder(states))
            {
                if (!actorState.Character.IsAlive) continue;
                _dice.CurrentActorName = actorState.Character.Name;
                var turnResult = await ProcessActingActorAsync(
                    tick, currentRound, actorState, states, stateMap, lastAttackerOf,
                    heroParty, enemyParty, log, Notify, name => { activeActorName = name; }, ct, terrain);
                if (turnResult is not null) return turnResult;
            }

            if (tick % RoundLength == 0)
            {
                await _statusEffectProcessor.ExpireSummonedPetsAsync(tick, currentRound, states, Notify);

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

    // ── Tick-level orchestration ────────────────────────────────────────────────

    // ── Per-actor turn orchestration ────────────────────────────────────────────

    private async Task<CombatResult?> ProcessActingActorAsync(
        int tick, int currentRound, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify, Action<string?> setActiveActor,
        CancellationToken ct, TerrainType terrain)
    {
        var setup = await _turnProcessor.SetupActorAttackAsync(tick, actorState, states, lastAttackerOf, notify, ct);
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

        if (await _spellProcessor.TryHandlePetSummonAsync(tick, actorState, setup, states, stateMap, currentRound, notify))
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
        await StatusEffectProcessor.NotifyExpiredEffectsAsync(tick, actorState.Character, expired, notify);

        // ── Healing spells take a different path ──────────────────────────
        if (setup.Source is Spell castSpell && castSpell.IsHealing)
        {
            var healResult = await _spellProcessor.ProcessHealingSpellAsync(tick, actorState, setup, castSpell, states, notify, terrain);
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

            var outcome = await _attackResolver.ResolveAttackOutcomeAsync(
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

    // ── Pet summoning ────────────────────────────────────────────────────────────

    // ── Attack outcome dispatch ──────────────────────────────────────────────────

}
