using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface IItemSetRepository
{
    Task<List<ItemSet>> GetAllSetsAsync();
    Task<List<SetBonus>> GetSetBonusesAsync(int setId);
}
