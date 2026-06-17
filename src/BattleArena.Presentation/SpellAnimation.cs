namespace BattleArena.Presentation;

/// <summary>
/// Visual animation applied to character card borders, HP/TM/Mana bar borders,
/// and spell overlay text during and after a spell event.
/// </summary>
public enum SpellAnimation
{
    /// <summary>No animation — passive/utility effects.</summary>
    None,

    /// <summary>
    /// Brief single-shot border colour that resets after ~400 ms.
    /// Used for: attack hits, lightning discharge, command magic.
    /// </summary>
    Flash,

    /// <summary>
    /// Rapid 300 ms bright/dark alternation on the card border.
    /// Used for: Burning, Bleeding, Poisoned, Acid, active DoT effects.
    /// </summary>
    Flicker,

    /// <summary>
    /// Slow 800 ms on/off toggle on the card border.
    /// Used for: hard CC — Stun, Freeze, Petrify, Sleep, Fear.
    /// </summary>
    Blink,

    /// <summary>
    /// Smooth 1 500 ms breathing in-and-out glow.
    /// Used for: Aegis wards, Verdancy buffs, Deity protection auras.
    /// </summary>
    Pulse,

    /// <summary>
    /// Rapid 150 ms prismatic shimmer, cycling through brightness levels.
    /// Used for: Mirage/illusion spells — Invisibility, Mirror Image, Blink.
    /// </summary>
    Shimmer,

    /// <summary>
    /// 600 ms directional drain glow on source and destination cards.
    /// Used for: Umbramancy leech — Vampiric Touch, Mind Siphon, Leech.
    /// </summary>
    Drain,

    /// <summary>
    /// Opacity-sweep glow across the HP bar segment that was restored.
    /// Used for: Deity healing spells — Cure Wounds, Heal, Mass Heal.
    /// </summary>
    HealGlow,
}

public static class SpellAnimationExtensions
{
    /// <summary>Timer interval in milliseconds for repeating animations.</summary>
    public static int IntervalMs(this SpellAnimation animation) => animation switch
    {
        SpellAnimation.Shimmer  => 150,
        SpellAnimation.Flicker  => 300,
        SpellAnimation.Flash    => 400,
        SpellAnimation.Drain    => 600,
        SpellAnimation.Blink    => 800,
        SpellAnimation.Pulse    => 1500,
        SpellAnimation.HealGlow => 1200,
        _                       => 0,
    };
}
