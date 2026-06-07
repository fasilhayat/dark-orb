namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;
using System.Linq;

/// <summary>
/// Handles turn execution, action decisions, and target selection.
/// </summary>
internal class TurnProcessor
{
    private readonly IDiceService _dice;
    private readonly IActionDecisionSource _heroActionSource;
    private readonly IActionDecisionSource _enemyActionSource;
    private readonly ITargetSelector _heroTargetSelector;
    private readonly ITargetSelector _enemyTargetSelector;
    private readonly SpellProcessor _spellProcessor;
    private readonly CombatLogger _logger;

    public TurnProcessor(
        IDiceService dice,
        IActionDecisionSource heroActionSource,
        IActionDecisionSource enemyActionSource,
        ITargetSelector heroTargetSelector,
        ITargetSelector enemyTargetSelector,
        SpellProcessor spellProcessor,
        CombatLogger logger)
    {
        _dice = dice;
        _heroActionSource = heroActionSource;
        _enemyActionSource = enemyActionSource;
        _heroTargetSelector = heroTargetSelector;
        _enemyTargetSelector = enemyTargetSelector;
        _spellProcessor = spellProcessor;
        _logger = logger;
    }

    public async Task<CombatResult?> ProcessCrowdControlledActorsAsync(
        int tick, List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        var ccActors = states.Where(s => 
            s.Character.IsAlive && 
            s.Meter.IsReady &&
            s.Character.GetCrowdControlType() is not null).ToList();
            
        foreach (var actor in ccActors)
        {
            var ccType = actor.Character.GetCrowdControlType();
            actor.Meter.CurrentValue = Math.Max(0, actor.Meter.CurrentValue - TurnmeterState.TurnThreshold);
            
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actor.Character.Name,
                EventType = "SkippedTurn",
                Message   = $"{actor.Character.Name} is {ccType} and cannot act!"
            });
        }
        
        return null;
    }

    public async Task<ActorSetup?> SetupActorAttackAsync(
        int tick, CombatantState actorState, 
        Party heroParty, Party enemyParty,
        Func<CombatLogEntry, Task> notify)
    {
        // Check for queued spell
        if (actorState.QueuedSpell is not null)
        {
            return await HandleQueuedSpellAsync(tick, actorState, notify);
        }
        
        // Setup new attack
        return await HandleNewAttackSetupAsync(
            tick, actorState, heroParty, enemyParty, notify);
    }

    private async Task<ActorSetup?> HandleQueuedSpellAsync(
        int tick, CombatantState actorState,
        Func<CombatLogEntry, Task> notify)
    {
        var queued = actorState.QueuedSpell!;
        queued.RemainingCost = Math.Max(0, queued.RemainingCost - TurnmeterState.TurnThreshold);
        
        if (queued.RemainingCost > 0)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = actorState.Character.Name,
                EventType        = "SpellCharging",
                AttackSourceName = queued.Spell.Name,
                Message          = $"{actorState.Character.Name} continues casting {queued.Spell.Name}... ({queued.RemainingCost} TM remaining)"
            });
            return null; // Continue charging
        }
        
        // Spell is ready
        var target = queued.Target;
        actorState.QueuedSpell = null;
        
        await notify(new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorState.Character.Name,
            EventType        = "SpellReady",
            AttackSourceName = queued.Spell.Name,
            Message          = $"{actorState.Character.Name} unleashes {queued.Spell.Name}!"
        });
        
        return new ActorSetup(queued.Spell, target, 0, true);
    }

    private async Task<ActorSetup?> HandleNewAttackSetupAsync(
        int tick, CombatantState actorState,
        Party heroParty, Party enemyParty,
        Func<CombatLogEntry, Task> notify)
    {
        var enemies = GetEnemies(actorState, heroParty, enemyParty);
        if (!enemies.Any()) return null;
        var allies = GetAllies(actorState, heroParty, enemyParty);
        var alliesReadOnly = allies.AsReadOnly();
        var enemiesReadOnly = enemies.AsReadOnly();
        
        // Get action decision
        var actionSource = actorState.PartyIndex == 0 ? _heroActionSource : _enemyActionSource;
        var chosenAttack = await actionSource.ChooseAttackAsync(
            actorState.Character, actorState.AttackSource, enemiesReadOnly, alliesReadOnly, tick, CancellationToken.None);
        
        // Select target
        var targetSelector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
        var target = await SelectActorTargetAsync(
            tick, actorState.Character, enemies, targetSelector, notify);
        
        if (target == null) return null;
        
        // Handle spell queueing if needed
        if (chosenAttack is Spell spell && spell.TurnMeterCost > TurnmeterState.TurnThreshold)
        {
            await _spellProcessor.QueueSpellAsync(tick, actorState, spell, target, notify);
            await _spellProcessor.DeductManaCostAsync(tick, actorState, spell, notify);
            return null; // Spell queued, no immediate action
        }
        
        return new ActorSetup(
            chosenAttack ?? actorState.AttackSource ?? UnarmedStrike.Default,
            target,
            chosenAttack is Spell s ? s.TurnMeterCost : TurnmeterState.TurnThreshold,
            chosenAttack is Spell);
    }

    private async Task<Character?> SelectActorTargetAsync(
        int tick, Character actor, List<Character> enemies,
        ITargetSelector selector, Func<CombatLogEntry, Task> notify)
    {
        var selected = await selector.SelectTargetAsync(actor, enemies);
        return selected;
    }

    private static List<Character> GetEnemies(
        CombatantState actorState, Party heroParty, Party enemyParty)
    {
        var enemyParty2 = actorState.PartyIndex == 0 ? enemyParty : heroParty;
        return enemyParty2.Members
            .Select(m => m.Character)
            .Where(c => c.IsAlive)
            .ToList();
    }

    private static List<Character> GetAllies(
        CombatantState actorState, Party heroParty, Party enemyParty)
    {
        var allyParty = actorState.PartyIndex == 0 ? heroParty : enemyParty;
        return allyParty.Members
            .Select(m => m.Character)
            .Where(c => c.IsAlive)
            .ToList();
    }
}