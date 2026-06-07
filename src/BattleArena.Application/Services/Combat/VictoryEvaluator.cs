namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Core.Entities;

public class VictoryEvaluator
{
    private readonly IDiceService _dice;

    public VictoryEvaluator(IDiceService dice)
    {
        _dice = dice;
    }

    public CombatResult? BuildDefeatResult(
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
