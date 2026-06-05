namespace BattleArena.Gui.Data;

using BattleArena.Application.Services;
using BattleArena.Core.Entities;

internal static class Roster
{
    private static RosterData? _data;
    private static readonly object _lock = new();

    internal static List<Character> AllHeroes  => EnsureLoaded().Heroes;
    internal static List<Character> AllEnemies => EnsureLoaded().Enemies;
    internal static List<Character> AllDummies => EnsureLoaded().Dummies;

    private static RosterData EnsureLoaded()
    {
        if (_data is not null)
            return _data;

        lock (_lock)
        {
            if (_data is not null)
                return _data;

            var jsonPath = Path.Combine(AppContext.BaseDirectory, "roster.json");
            _data = RosterLoader.Load(jsonPath);
            return _data;
        }
    }

    internal static IAttackSource? GetAttackSource(Character c)
    {
        if (c.MemorizedSpells.Count > 0)
            return null;
        return (IAttackSource?)c.Equipment.RightHand ?? UnarmedStrike.Default;
    }
}
