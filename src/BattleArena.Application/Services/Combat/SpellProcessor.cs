namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

internal class SpellProcessor
{
    private readonly ICombatService _combat;
    private readonly IDiceService _dice;
    private readonly CombatLogger _logger;
    private readonly StatusEffectProcessor _statusEffectProcessor;

    public SpellProcessor(ICombatService combat, IDiceService dice, CombatLogger logger, StatusEffectProcessor statusEffectProcessor)
    {
        _combat = combat;
        _dice = dice;
        _logger = logger;
        _statusEffectProcessor = statusEffectProcessor;
    }

    public async Task<CombatResult?> ProcessHealingSpellAsync(
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

    public async Task QueueSpellAsync(
        int tick, CombatantState actorState, Spell spell,
        List<CombatantState> enemies, List<Character> allies, int tmCost, int meterNow,
        Func<CombatLogEntry, Task> notify, CancellationToken ct,
        ITargetSelector? heroTargetSelector = null, ITargetSelector? enemyTargetSelector = null)
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
            var selector = actorState.PartyIndex == 0 ? heroTargetSelector : enemyTargetSelector;
            target = await selector!.SelectTargetAsync(
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

    public async Task DeductManaCostAsync(
        int tick, CombatantState actorState, Spell? spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell is null || spell.ManaCost <= 0) return;
        if (actorState.Character.RemainingCasts > 0)
            actorState.Character.RemainingCasts--;
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

    public async Task<bool> TryHandlePetSummonAsync(
        int tick, CombatantState actorState, ActorSetup setup,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap, int currentRound,
        Func<CombatLogEntry, Task> notify)
    {
        if (!setup.IsSpell || setup.Source is not Spell castSpell || castSpell.SummonedPet is null)
            return false;

        var pet = castSpell.SummonedPet;
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

    public async Task ProcessSpellDisruptionAsync(
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

    // Per-effect overloads for refactored callers
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
                var healAmount = _combat.ResolveHealing(actorState.Character, ally, spell, terrain);
                ally.CurrentHitPoints = Math.Min(ally.CurrentHitPoints + healAmount, ally.MaxHitPoints);
                await notify(new CombatLogEntry
                {
                    Tick = tick, ActorName = ally.Name, EventType = "Healed",
                    DamageDealt = healAmount, TargetHpBefore = hpBefore,
                    TargetHpAfter = ally.CurrentHitPoints, AttackSourceName = spell.Name,
                    IsSpell = true,
                    Message = $"{ally.Name} is healed for {healAmount} by {spell.Name}. HP: {hpBefore} -> {ally.CurrentHitPoints}"
                });
            }
        }
        else
        {
            var hpB = target.CurrentHitPoints;
            var heal = _combat.ResolveHealing(actorState.Character, target, spell, terrain);
            target.CurrentHitPoints = Math.Min(target.CurrentHitPoints + heal, target.MaxHitPoints);
            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = target.Name, EventType = "Healed",
                DamageDealt = heal, TargetHpBefore = hpB,
                TargetHpAfter = target.CurrentHitPoints, AttackSourceName = spell.Name,
                IsSpell = true,
                Message = $"{target.Name} is healed for {heal} by {spell.Name}. HP: {hpB} -> {target.CurrentHitPoints}"
            });
        }
        await _statusEffectProcessor.ProcessSelfBuffsAsync(tick, actorState.Character, spell, notify);
        var party = actorState.PartyIndex == 0 ? heroParty : enemyParty;
        var partyMembers = party.Members.Select(m => m.Character).ToList();
        await _statusEffectProcessor.ProcessPartyBuffsAsync(tick, actorState.Character, spell, partyMembers, notify);
        return null;
    }

    public async Task QueueSpellAsync(
        int tick, CombatantState actorState, Spell spell, Character target,
        Func<CombatLogEntry, Task> notify)
    {
        actorState.QueuedSpell = new QueuedSpellInfo(spell, target, spell.TurnMeterCost);
        await notify(new CombatLogEntry
        {
            Tick = tick, ActorName = actorState.Character.Name,
            TargetName = target.Name, EventType = "SpellQueued",
            AttackSourceName = spell.Name,
            Message = $"{actorState.Character.Name} begins casting {spell.Name} at {target.Name}..."
        });
    }

    public async Task<bool> TryHandlePetSummonAsync(
        int tick, CombatantState actorState, Spell spell, int currentRound,
        List<CombatantState> states, Party heroParty, Party enemyParty,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.SummonedPet == null) return false;
        var summonedPet = spell.SummonedPet;
        if (states.Any(s => s.Character.Name == summonedPet.Name && s.Character.IsAlive))
        {
            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = actorState.Character.Name,
                EventType = "SummonFailed",
                Message = $"{actorState.Character.Name} cannot summon {summonedPet.Name} - already present!"
            });
            return true;
        }
        var pet = new Character
        {
            Name = summonedPet.Name, Level = 1,
            CurrentHitPoints = summonedPet.MaxHitPoints, MaxHitPoints = summonedPet.MaxHitPoints,
            TurnSpeed = summonedPet.TurnSpeed, StrikeRating = summonedPet.StrikeRating,
            Strength = summonedPet.Strength, Dexterity = 10, Stamina = 10,
            Intelligence = 10, Wisdom = 10, ClassId = actorState.Character.ClassId
        };
        var petState = new CombatantState(pet, null, actorState.PartyIndex)
        {
            SummonedBy = actorState.Character,
            SummonExpiryRound = currentRound + summonedPet.SummonDurationRounds
        };
        states.Add(petState);
        var party = actorState.PartyIndex == 0 ? heroParty : enemyParty;
        party.Members.Add(new PartyMember { Character = pet, AttackSource = null });
        await notify(new CombatLogEntry
        {
            Tick = tick, ActorName = actorState.Character.Name,
            EventType = "SummonPet",
            Message = $"{actorState.Character.Name} summons {pet.Name}! (expires round {petState.SummonExpiryRound})"
        });
        return true;
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
            Tick = tick, ActorName = targetState.Character.Name,
            EventType = "SpellDisrupted",
            TurnMeterBefore = before, TurnMeterAfter = targetState.Meter.CurrentValue,
            Message = $"{targetState.Character.Name}'s spellcasting is disrupted! TM reduced by {tmLoss}."
        });
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

    public async Task ProcessConcentrationAsync(
        int tick, Character target, AttackResult result,
        Dictionary<Character, CombatantState> stateMap, Func<CombatLogEntry, Task> notify)
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
}
