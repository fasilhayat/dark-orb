namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Handles attack resolution, damage calculation, and combat outcomes.
/// </summary>
public class AttackResolver
{
    private readonly ICombatService _combatService;
    private readonly IDiceService _dice;
    private readonly CombatLogger _logger;
    private readonly TurnMeterProcessor _turnMeterProcessor;
    private readonly StatusEffectProcessor _statusEffectProcessor;
    private readonly SpellProcessor _spellProcessor;

    public AttackResolver(
        ICombatService combatService, 
        IDiceService dice,
        CombatLogger logger,
        TurnMeterProcessor turnMeterProcessor,
        StatusEffectProcessor statusEffectProcessor,
        SpellProcessor spellProcessor)
    {
        _combatService = combatService;
        _dice = dice;
        _logger = logger;
        _turnMeterProcessor = turnMeterProcessor;
        _statusEffectProcessor = statusEffectProcessor;
        _spellProcessor = spellProcessor;
    }

    public async Task<CombatResult?> ProcessClashAsync(
        int tick, int currentRound, CombatantState actorState, 
        IAttackSource source, Character target,
        Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        VictoryEvaluator victoryEvaluator, TerrainType terrain,
        Func<CombatLogEntry, Task> notify)
    {
        var actorName = actorState.Character.Name;
        var targetName = target.Name;
        var isSpell = source is Spell;
        var spellLevel = isSpell ? ((Spell)source).SpellLevel : (int?)null;
        var casterLevel = isSpell ? actorState.Character.Level : (int?)null;
        
        // Perform the attack
        var result = _combatService.ResolveAttack(actorState.Character, target, source, 
            actorState.EngagementRange, terrain);
        
        // Log the attack
        await notify(_logger.BuildAttackEntry(
            tick, actorName, source.Name, isSpell, targetName, result,
            source.DamageType, spellLevel, casterLevel));
        
        // Handle miss outcomes
        if (!result.IsHit && !result.IsClash)
        {
            await _turnMeterProcessor.ApplyDefenderTmBoostAsync(
                tick, actorState, target, result, stateMap, notify);
            await _statusEffectProcessor.ApplyFumblePenaltyAsync(
                tick, actorState, result, notify);
            return null;
        }
        
        // Process hit
        return await ProcessHitAsync(
            tick, currentRound, actorState, source, target, result,
            stateMap, lastAttackerOf, heroParty, enemyParty, log,
            victoryEvaluator, terrain, notify);
    }

    private async Task<CombatResult?> ProcessHitAsync(
        int tick, int currentRound, CombatantState actorState,
        IAttackSource source, Character target, AttackResult result,
        Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        VictoryEvaluator victoryEvaluator, TerrainType terrain,
        Func<CombatLogEntry, Task> notify)
    {
        lastAttackerOf[target] = actorState.Character;
        
        // Apply damage
        if (result.Damage > 0)
        {
            var hpBefore = target.CurrentHitPoints;
            target.CurrentHitPoints -= result.Damage;
            
            await notify(_logger.BuildDamageEntry(
                tick, target.Name, result.Damage,
                hpBefore, target.CurrentHitPoints));
        }
        
        // Apply status effects
        await _statusEffectProcessor.ProcessOnHitEffectsAsync(
            tick, source, target, actorState.Character.Name, notify);
        
        if (source is Spell spell && spell.ElementalType != ElementalType.None)
        {
            await _statusEffectProcessor.TryApplyElementalDoTAsync(
                tick, target, spell, notify);
        }
        
        // Check for spell disruption (melee attacks only)
        if (source.AttackType == AttackType.Melee)
        {
            await _spellProcessor.ProcessSpellDisruptionAsync(
                tick, result, target, stateMap, notify);
        }
        
        // Check concentration
        await _spellProcessor.ProcessConcentrationAsync(
            tick, target, result, stateMap, notify);
        
        // Check if target is defeated
        if (target.CurrentHitPoints <= 0)
        {
            await notify(_logger.BuildDefeatEntry(tick, target));
            var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
            var defResult = victoryEvaluator.BuildDefeatResult(
                tick, targetPartyIdx, target, heroParty, enemyParty, log);
            
            if (defResult is not null)
            {
                var deadState = stateMap.GetValueOrDefault(target);
                if (deadState?.QueuedSpell is not null) 
                    deadState.QueuedSpell = null;
                return defResult;
            }
        }
        
        return null;
    }
}