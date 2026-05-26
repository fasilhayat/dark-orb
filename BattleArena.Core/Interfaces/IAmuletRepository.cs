namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IAmuletRepository
{
    Task<List<Amulet>> GetAllAsync();
}
