namespace BattleArena.Core.Entities;

public class Girdle
{
    public int Id { get; set; }
    public int GearQualityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EffectType { get; set; } = "none";
    public int EffectValue { get; set; }
    public bool Cursed { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurseEffect { get; set; } = string.Empty;
}
