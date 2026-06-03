namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface ISpellRepository
{
    Task<List<Spell>> GetAllAsync();
    Task<List<Spell>> GetBySchoolAsync(string? school);
}
