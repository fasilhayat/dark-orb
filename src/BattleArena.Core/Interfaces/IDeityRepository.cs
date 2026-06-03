namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IDeityRepository
{
    Task<List<Deity>> GetAllAsync();
    Task<List<Deity>> GetByAlignmentAsync(string? alignment);
}
