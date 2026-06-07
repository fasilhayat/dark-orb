namespace BattleArena.Application.Services.Combat;

using Application.Models.Combat;
using Core.Entities;

internal static class CombatSimulatorHelpers
{
    public static List<CombatantState> BuildCombatantStates(Party heroParty, Party enemyParty)
    {
        var states = new List<CombatantState>();
        foreach (var m in heroParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 0));
        foreach (var m in enemyParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 1));
        return states;
    }

    public static List<CombatantState> GetActingOrder(List<CombatantState> states) =>
        states
            .Where(s => s.Character.IsAlive && s.Meter.IsReady && s.Character.TryGetCrowdControlLabel() is null)
            .OrderByDescending(s => s.Meter.CurrentValue)
            .ToList();
}
