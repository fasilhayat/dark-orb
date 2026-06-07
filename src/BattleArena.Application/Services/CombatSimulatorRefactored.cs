namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Application.Services.Combat;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Orchestrates turn-based combat simulation by coordinating specialized processors.
/// Refactored from the original monolithic CombatSimulator (1578 lines) into focused components.
/// </summary>
public class CombatSimulatorRefactored : ICombatSimulator
{
    public const int DefaultMaxTicks = 1000;
    private const int RoundLength = 10;

    // Core services
    private readonly ICombatService _combat;
    private readonly ITurnmeterService _turnmeter;
    private readonly IStatusEffectService _statusEffect;
    private readonly IDiceService _dice;

    // Specialized processors
    private readonly CombatLogger _logger;
    private readonly TurnMeterProcessor _turnMeterProcessor;
    private readonly VictoryEvaluator _victoryEvaluator;
    private readonly StatusEffectProcessor _statusEffectProcessor;
    private readonly SpellProcessor _spellProcessor;
    private readonly AttackResolver _attackResolver;
    private readonly TurnProcessor _turnProcessor;

    public CombatSimulatorRefactored(
        ICombatService combat,
        ITurnmeterService turnmeter,
        IStatusEffectService statusEffect,
        IDiceService dice,
        ITargetSelector? heroTargetSelector = null,
        ITargetSelector? enemyTargetSelector = null,
        IActionDecisionSource? heroActionSource = null,
        IActionDecisionSource? enemyActionSource = null)
    {
        _combat = combat;
        _turnmeter = turnmeter;
        _statusEffect = statusEffect;
        _dice = dice;

        // Initialize processors
        _logger = new CombatLogger();
        _turnMeterProcessor = new TurnMeterProcessor(turnmeter, _logger);
        _victoryEvaluator = new VictoryEvaluator(dice);
        _statusEffectProcessor = new StatusEffectProcessor(statusEffect, dice, _logger);
        _spellProcessor = new SpellProcessor(combat, dice, _logger, _statusEffectProcessor);
        _attackResolver = new AttackResolver(combat, dice, _logger, _turnMeterProcessor, _statusEffectProcessor, _spellProcessor);
        
        // Use provided or default selectors/sources
        var heroTarget = heroTargetSelector ?? new RandomTargetSelector();
        var enemyTarget = enemyTargetSelector ?? new RandomTargetSelector();
        var heroAction = heroActionSource ?? new AutoActionDecisionSource(dice);
        var enemyAction = enemyActionSource ?? new AutoActionDecisionSource(dice);
        
        _turnProcessor = new TurnProcessor(dice, heroAction, enemyAction, heroTarget, enemyTarget, _spellProcessor, _logger);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public async Task<CombatResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = DefaultMaxTicks,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains)
    {
        // Initialize combat state
        var states = BuildCombatantStates(heroParty, enemyParty);
        InitializeTurnMeters(states);
        
        var stateMap = states.ToDictionary(s => s.Character);
        var currentRound = 0;
        var lastAttackerOf = new Dictionary<Character, Character>();
        string? activeActorName = null;
        var log = new List<CombatLogEntry>();

        // Notification helper
        async Task Notify(CombatLogEntry entry)
        {
            entry.ActiveActorName = activeActorName;
            log.Add(entry);
            if (observer != null)
                await observer.OnEventAsync(entry, ct);
        }

        // Main combat loop
        for (var tick = 1; tick <= maxTicks; tick++)
        {
            ct.ThrowIfCancellationRequested();
            _dice.CurrentTick = tick;

            // Round management
            if ((tick - 1) % RoundLength == 0)
            {
                currentRound++;
                await Notify(_logger.BuildRoundStartEntry(tick, currentRound));
            }

            // Process turn meter and mana
            await _turnMeterProcessor.ProcessTickMeterAndManaAsync(tick, states, Notify);

            // Process crowd-controlled actors
            await _turnProcessor.ProcessCrowdControlledActorsAsync(tick, states, Notify);

            // Process acting actors
            foreach (var actorState in GetActingOrder(states))
            {
                if (!actorState.Character.IsAlive) continue;
                
                activeActorName = actorState.Character.Name;
                var turnResult = await ProcessActingActorAsync(
                    tick, currentRound, actorState, states, stateMap, lastAttackerOf,
                    heroParty, enemyParty, log, Notify, ct, terrain);
                    
                if (turnResult is not null) 
                    return turnResult;
            }

            // End of round processing
            if (tick % RoundLength == 0)
            {
                await _statusEffectProcessor.ExpireSummonedPetsAsync(tick, currentRound, states, Notify);
                await Notify(_logger.BuildRoundEndEntry(tick, currentRound));
            }
        }

        return _victoryEvaluator.BuildMaxTicksResult(maxTicks, log, heroParty, enemyParty);
    }

    // 1v1 convenience wrapper
    public Task<CombatResult> SimulateAsync(
        Character fighter, IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = DefaultMaxTicks,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(
            Party.Solo(fighter, fighterAttack),
            Party.Solo(opponent, opponentAttack),
            maxTicks, observer, ct, terrain);

    // Sync wrappers
    public CombatResult Simulate(Party heroParty, Party enemyParty, int maxTicks = DefaultMaxTicks,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(heroParty, enemyParty, maxTicks, terrain: terrain).GetAwaiter().GetResult();

    public CombatResult Simulate(
        Character fighter, IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = DefaultMaxTicks,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(fighter, fighterAttack, opponent, opponentAttack, maxTicks, terrain: terrain)
            .GetAwaiter().GetResult();

    // ── Private orchestration methods ──────────────────────────────────────────

    private async Task<CombatResult?> ProcessActingActorAsync(
        int tick, int currentRound, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify, CancellationToken ct, TerrainType terrain)
    {
        // Process status effects first
        var defeatedByEffects = await ProcessActorStatusEffectsAsync(
            tick, actorState, states, stateMap, heroParty, enemyParty, log, notify);
        if (defeatedByEffects != null) return defeatedByEffects;

        // Check if still alive and can act
        if (!actorState.Character.IsAlive || TryGetCrowdControlLabel(actorState.Character) != null)
            return null;

        // Log turn start
        await notify(new CombatLogEntry
        {
            Tick      = tick,
            ActorName = actorState.Character.Name,
            EventType = "TurnStart",
            IsActive  = true,
            Message   = $"══ {actorState.Character.Name}'s turn ══"
        });

        // Setup attack
        var setup = await _turnProcessor.SetupActorAttackAsync(
            tick, actorState, heroParty, enemyParty, notify);

        if (setup == null)
        {
            // No action taken (charging spell or no valid targets)
            await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick));
            return null;
        }

        // Handle special spell cases
        if (setup.IsSpell && setup.Source is Spell spell)
        {
            // Deduct mana if not already deducted
            if (actorState.QueuedSpell == null)
                await _spellProcessor.DeductManaCostAsync(tick, actorState, spell, notify);

            // Handle summon pets
            if (await _spellProcessor.TryHandlePetSummonAsync(
                tick, actorState, spell, currentRound, states, heroParty, enemyParty, notify))
            {
                await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, setup.TmCost));
                return null;
            }

            // Handle healing spells
            if (spell.IsHealing)
            {
                var result = await _spellProcessor.ProcessHealingSpellAsync(
                    tick, actorState, spell, setup.Target, heroParty, enemyParty, log,
                    _victoryEvaluator, terrain, notify);
                await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, setup.TmCost));
                return result;
            }

            // Check spell reflection
            if (_spellProcessor.ShouldReflectSpell(setup.Target))
            {
                setup = new TurnProcessor.ActorSetup(setup.Source, actorState.Character, setup.TmCost, true);
                await notify(new CombatLogEntry
                {
                    Tick      = tick,
                    ActorName = actorState.Character.Name,
                    EventType = "SpellReflected",
                    Message   = $"{setup.Target.Name}'s spell reflection bounces {spell.Name} back!"
                });
            }
        }

        // Process combat clash
        var clashResult = await _attackResolver.ProcessClashAsync(
            tick, currentRound, actorState, setup.Source, setup.Target,
            stateMap, lastAttackerOf, heroParty, enemyParty, log,
            _victoryEvaluator, terrain, notify);

        // End turn
        await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, setup.TmCost));

        return clashResult;
    }

    private async Task<CombatResult?> ProcessActorStatusEffectsAsync(
        int tick, CombatantState actorState, List<CombatantState> states,
        Dictionary<Character, CombatantState> stateMap,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var character = actorState.Character;

        // Process each status effect
        foreach (var effect in character.ActiveStatusEffects.ToList())
        {
            // Leech effects
            if (effect.Type == StatusEffectType.Leech && effect.CasterName != null && !string.IsNullOrEmpty(effect.CasterName))
            {
                var casterState = states.FirstOrDefault(s => s.Character.Name == effect.CasterName);
                if (casterState?.Character.IsAlive == true)
                {
                    await _statusEffectProcessor.ProcessActorLeechAsync(
                        tick, actorState, casterState, effect, notify);
                }
            }

            // Damage over time
            if (effect.Type == StatusEffectType.DamageOverTime)
            {
                var defeated = await _statusEffectProcessor.ProcessActorDoTAsync(
                    tick, actorState, effect, notify);
                if (defeated)
                {
                    var partyIdx = actorState.PartyIndex;
                    return _victoryEvaluator.BuildDefeatResult(
                        tick, partyIdx, character, heroParty, enemyParty, log);
                }
            }

            // Healing over time
            if (effect.Type == StatusEffectType.HealOverTime)
            {
                await _statusEffectProcessor.ProcessActorHoTAsync(
                    tick, actorState, effect, notify);
            }
        }

        // Tick and expire effects
        _statusEffect.TickAll(character);
        await NotifyExpiredEffectsAsync(tick, character, notify);

        return null;
    }

    private static async Task NotifyExpiredEffectsAsync(
        int tick, Character character, Func<CombatLogEntry, Task> notify)
    {
        var expired = character.ActiveStatusEffects.Where(e => e.Duration <= 0).ToList();
        foreach (var effect in expired)
        {
            character.ActiveStatusEffects.Remove(effect);
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = character.Name,
                EventType        = "EffectExpired",
                StatusEffectName = effect.Name,
                Message          = $"{effect.Name} on {character.Name} has expired."
            });
        }
    }

    private static List<CombatantState> BuildCombatantStates(Party heroParty, Party enemyParty)
    {
        var states = new List<CombatantState>();
        foreach (var m in heroParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 0));
        foreach (var m in enemyParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 1));
        return states;
    }

    private void InitializeTurnMeters(List<CombatantState> states)
    {
        foreach (var s in states)
        {
            var gain = _turnmeter.ComputeGainPerTick(s.Character);
            s.Meter.CurrentValue = Math.Min(gain * RoundLength, TurnmeterState.TurnThreshold);
        }
    }

    private List<CombatantState> GetActingOrder(List<CombatantState> states) =>
        states.Where(s => s.Character.IsAlive && s.Meter.IsReady)
              .OrderByDescending(s => s.Meter.CurrentValue)
              .ThenBy(s => s.Character.Id)
              .ToList();

    private static string? TryGetCrowdControlLabel(Character character)
    {
        if (character.ActiveStatusEffects.Any(e => e.Type == StatusEffectType.Stun))
            return "stunned";
        if (character.ActiveStatusEffects.Any(e => e.Type == StatusEffectType.Root))
            return "rooted";
        if (character.ActiveStatusEffects.Any(e => e.Type == StatusEffectType.Fear))
            return "feared";
        return null;
    }
}