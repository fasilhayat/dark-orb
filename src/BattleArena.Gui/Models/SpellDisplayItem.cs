namespace BattleArena.Gui.Models;

using Core.Entities;
using Core.Entities.Enums;
using BattleArena.Presentation;

public sealed class SpellDisplayItem
{
    private static readonly string[] DieLabels = ["", "", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9", "d10", "d11", "d12", "d13", "d14", "d15", "d16", "d17", "d18", "d19", "d20"];

    public Spell Spell { get; }
    public string Name => Spell.Name;
    public string Color => EffectVisualConfig.GetElementColor(Spell.ElementalType);
    public string? AfterburnEffectName => EffectVisualConfig.GetElementDoTName(Spell.ElementalType);
    public string AfterburnColor => AfterburnEffectName is { } name ? EffectVisualConfig.GetColor(name) : "#ffffff";
    public string DamageInfo
    {
        get
        {
            var sides = (int)Spell.DamageDie;
            var dieLabel = sides < DieLabels.Length ? DieLabels[sides] : $"d{sides}";
            var dmg = Spell.IsHealing ? "Healing" : Spell.DamageType.ToString();
            return Spell.DamageCount > 0
                ? $"{Spell.DamageCount}{dieLabel} {dmg}"
                : dmg;
        }
    }
    public string ManaInfo => $"Mana: {Spell.ManaCost}";
    public string FullDescription => Spell.Description;

    public SpellDisplayItem(Spell spell)
    {
        Spell = spell;
    }
}
