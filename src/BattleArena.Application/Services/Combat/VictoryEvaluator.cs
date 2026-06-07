namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Evaluates victory conditions and builds combat results.
/// </summary>
public class VictoryEvaluator
{
    private readonly IDiceService _dice;

    public VictoryEvaluator(IDiceService dice)
    {
        _dice = dice;
    }

    public CombatResult? BuildDefeatResult(
        int tick, int defeatedPartyIndex, Character defeatedChar, 
        Party heroParty, Party enemyParty, List<CombatLogEntry> log)
    {
        // Determine if this defeat ends combat
        var defeatedParty = defeatedPartyIndex == 0 ? heroParty : enemyParty;
        var defeatedSurvivors = defeatedParty.Members.Count(m => m.Character.IsAlive);
        
        if (defeatedSurvivors > 0)
            return null; // Combat continues
        
        // Combat ends - determine winner
        var winnerParty = defeatedPartyIndex == 0 ? enemyParty : heroParty;
        var loserParty = defeatedParty;
        
        return new CombatResult
        {
            WinningParty  = winnerParty,
            LosingParty   = loserParty,
            TotalTicks    = tick,
            LoserStatus   = defeatedChar.CurrentHitPoints <= -10 ? CharacterVitalStatus.Dead : CharacterVitalStatus.KnockedOut,
            Log           = log,
            Seed          = _dice.Seed,
            Party1        = heroParty,
            Party2        = enemyParty
        };
    }

    public bool IsPartyDefeated(Party party)
    {
        return party.Members.All(m => !m.Character.IsAlive);
    }

    public CombatResult BuildMaxTicksResult(int maxTicks, List<CombatLogEntry> log, Party heroParty, Party enemyParty)
    {
        return new CombatResult 
        { 
            MaxTicksReached = true, 
            TotalTicks = maxTicks, 
            Log = log, 
            Seed = _dice.Seed, 
            Party1 = heroParty, 
            Party2 = enemyParty 
        };
    }
}