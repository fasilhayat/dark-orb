namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IItemSetRepository
{
    Task<List<ItemSet>> GetAllSetsAsync();
    Task<List<SetBonus>> GetSetBonusesAsync(int setId);
}
