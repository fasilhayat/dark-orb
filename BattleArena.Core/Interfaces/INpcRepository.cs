using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface INpcRepository
{
    Task<List<Npc>> GetAllAsync(bool? merchant = null, bool? hostile = null);
}
