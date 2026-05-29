namespace BattleArena.Application.Interfaces;

using Core.Entities.Enums;

public interface IDiceService
{
    // The random seed used by this instance. Capture it to enable deterministic replay.
    int Seed { get; }

    // Set by CombatSimulator each tick so dice-log implementations can stamp
    // their entries with the tick they were rolled on.
    int CurrentTick { get; set; }

    int Roll(DieType dieType);
    int Roll(int count, int sides);
    int RollWithAdvantage(DieType dieType);
    int RollWithDisadvantage(DieType dieType);

    // Returns a value in [0, maxExclusive). Use instead of Random.Shared for determinism.
    int RollIndex(int maxExclusive);
}
