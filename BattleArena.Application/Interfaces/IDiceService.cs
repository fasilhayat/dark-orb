namespace BattleArena.Application.Interfaces;

using Core.Entities.Enums;

public interface IDiceService
{
    // The random seed used by this instance. Capture it to enable deterministic replay.
    int Seed { get; }

    int Roll(DieType dieType);
    int Roll(int count, int sides);
    int RollWithAdvantage(DieType dieType);
    int RollWithDisadvantage(DieType dieType);

    // Returns a value in [0, maxExclusive). Use instead of Random.Shared for determinism.
    int RollIndex(int maxExclusive);
}
