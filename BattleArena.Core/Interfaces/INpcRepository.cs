namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface INpcRepository
{
    Task<List<Npc>> GetAllAsync(bool? merchant = null, bool? hostile = null);
}
