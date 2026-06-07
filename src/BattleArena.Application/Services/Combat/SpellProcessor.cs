namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Handles all spell-related combat mechanics including casting, mana, and concentration.
/// </summary>
public class SpellProcessor
{
    private readonly ICombatService _combatService;
    private readonly IDiceService _dice;
    private readonly CombatLogger _logger;
    private readonly StatusEffectProcessor _statusEffectProcessor;

    public SpellProcessor(ICombatService combatService, IDiceService dice, CombatLogger logger, StatusEffectProcessor statusEffectProcessor)
    {
        _combatService = combatService;
        _dice = dice;
        _logger = logger;
        _statusEffectProcessor = statusEffectProcessor;
    }

    public async Task<CombatResult?> ProcessHealingSpellAsync(
        int tick, CombatantState actorState, Spell spell, Character target,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        VictoryEvaluator victoryEvaluator, TerrainType terrain,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.IsGroupHeal)
        {
            var allies = actorState.PartyIndex == 0 
                ? heroParty.Members.Select(m => m.Character).Where(c => c.IsAlive)
                : enemyParty.Members.Select(m => m.Character).Where(c => c.IsAlive);
                
            foreach (var ally in allies)
            {
                var hpBefore = ally.CurrentHitPoints;
                var healAmount = _combatService.ResolveHealing(actorState.Character, ally, spell, terrain);
                ally.CurrentHitPoints = Math.Min(ally.CurrentHitPoints + healAmount, ally.MaxHitPoints);
                
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = ally.Name,
                    EventType        = "Heal",
                    TargetHpBefore   = hpBefore,
                    TargetHpAfter    = ally.CurrentHitPoints,
                    AttackSourceName = spell.Name,
                    IsSpell          = true,
                    Message          = $"{ally.Name} is healed for {healAmount} by {spell.Name}. HP: {hpBefore} -> {ally.CurrentHitPoints}"
                });
            }
        }
        else
        {
            var hpB = target.CurrentHitPoints;
            var heal = _combatService.ResolveHealing(actorState.Character, target, spell, terrain);
            target.CurrentHitPoints = Math.Min(target.CurrentHitPoints + heal, target.MaxHitPoints);
            
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "Heal",
                TargetHpBefore   = hpB,
                TargetHpAfter    = target.CurrentHitPoints,
                AttackSourceName = spell.Name,
                IsSpell          = true,
                Message          = $"{target.Name} is healed for {heal} by {spell.Name}. HP: {hpB} -> {target.CurrentHitPoints}"
            });
        }
        
        await _statusEffectProcessor.ProcessSelfBuffsAsync(tick, actorState.Character, spell, notify);
        return null;
    }

    public async Task QueueSpellAsync(
        int tick, CombatantState actorState, Spell spell, Character target,
        Func<CombatLogEntry, Task> notify)
    {
        actorState.QueuedSpell = new QueuedSpellInfo(spell, target, spell.TurnMeterCost);
        
        await notify(new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorState.Character.Name,
            TargetName       = target.Name,
            EventType        = "SpellQueued",
            AttackSourceName = spell.Name,
            Message          = $"{actorState.Character.Name} begins casting {spell.Name} at {target.Name}..."
        });
    }

    public async Task DeductManaCostAsync(
        int tick, CombatantState actorState, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.ManaCost <= 0) return;
        
        var manaBefore = actorState.Character.CurrentMana;
        actorState.Character.CurrentMana -= spell.ManaCost;
        
        await notify(_logger.BuildManaCostEntry(
            tick, actorState.Character.Name, spell.Name, 
            manaBefore, actorState.Character.CurrentMana, spell.ManaCost));
    }

    public async Task<bool> TryHandlePetSummonAsync(
        int tick, CombatantState actorState, Spell spell, int currentRound,
        List<CombatantState> states, Party heroParty, Party enemyParty,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.SummonedPet == null) 
            return false;
        
        var summonedPet = spell.SummonedPet;
        
        // Check if pet already exists
        if (states.Any(s => s.Character.Name == summonedPet.Name && s.Character.IsAlive))
        {
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "SummonFailed",
                Message   = $"{actorState.Character.Name} cannot summon {summonedPet.Name} - already present!"
            });
            return true; // Spell was handled but failed
        }
        
        // Create summoned pet character
        var pet = new Character
        {
            Name = summonedPet.Name,
            Level = 1,
            CurrentHitPoints = summonedPet.MaxHitPoints,
            MaxHitPoints = summonedPet.MaxHitPoints,
            TurnSpeed = summonedPet.TurnSpeed,
            StrikeRating = summonedPet.StrikeRating,
            Strength = summonedPet.Strength,
            Dexterity = 10,
            Stamina = 10,
            Intelligence = 10,
            Wisdom = 10,
            ClassId = actorState.Character.ClassId
        };
        
        // Create pet state
        var petState = new CombatantState(pet, null, actorState.PartyIndex)
        {
            SummonedBy = actorState.Character,
            SummonExpiryRound = currentRound + summonedPet.SummonDurationRounds
        };
        
        states.Add(petState);
        
        // Add to party
        var party = actorState.PartyIndex == 0 ? heroParty : enemyParty;
        party.Members.Add(new PartyMember { Character = pet, AttackSource = null });
        
        await notify(new CombatLogEntry
        {
            Tick      = tick,
            ActorName = actorState.Character.Name,
            EventType = "SummonPet",
            Message   = $"{actorState.Character.Name} summons {pet.Name}! (expires round {petState.SummonExpiryRound})"
        });
        
        return true; // Spell was handled
    }

    public async Task ProcessSpellDisruptionAsync(
        int tick, AttackResult result, Character target,
        Dictionary<Character, CombatantState> stateMap,
        Func<CombatLogEntry, Task> notify)
    {
        if (result.Damage <= 0 || target.MemorizedSpells.Count == 0) return;
        
        var targetState = stateMap[target];
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

    public async Task ProcessConcentrationAsync(
        int tick, Character target, AttackResult result,
        Dictionary<Character, CombatantState> stateMap,
        Func<CombatLogEntry, Task> notify)
    {
        if (result.Damage <= 0) return;
        
        var concState = stateMap.GetValueOrDefault(target);
        if (concState?.QueuedSpell is null) return;
        
        var dc = Math.Max(10, result.Damage / 2);
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

    public bool ShouldReflectSpell(Character target)
    {
        var reflectChance = 0;
        if (target.ActiveStatusEffects.Any(e => e.Name == "Spell Reflection"))
            reflectChance += 30;
        if (target.Equipment.Neck?.Name == "Amulet of Reflection")
            reflectChance += 20;
            
        return reflectChance > 0 && _dice.Roll(DieType.D100) <= reflectChance;
    }
}