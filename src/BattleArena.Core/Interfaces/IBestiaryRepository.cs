namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IBestiaryRepository
{
    Task<List<BestiaryEntry>> GetAllAsync();
    Task<List<BestiaryEntry>> GetByCategoryAndLevelAsync(string? category, int? level);
}
