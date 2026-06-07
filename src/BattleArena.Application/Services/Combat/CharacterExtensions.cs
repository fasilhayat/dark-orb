namespace BattleArena.Application.Services.Combat;

using Core.Entities;
using Core.Entities.Enums;
using System.Linq;

/// <summary>
/// Extension methods for Character to simplify combat operations.
/// </summary>
public static class CharacterExtensions
{
    public static bool HasStatusEffect(this Character character, StatusEffectType type)
    {
        return character.ActiveStatusEffects.Any(e => e.Type == type);
    }
    
    public static bool IsStunned(this Character character)
    {
        return character.HasStatusEffect(StatusEffectType.Stun);
    }
    
    public static bool IsRooted(this Character character)
    {
        return character.HasStatusEffect(StatusEffectType.Root);
    }
    
    public static bool IsFeared(this Character character)
    {
        return character.HasStatusEffect(StatusEffectType.Fear);
    }
    
    public static bool IsCrowdControlled(this Character character)
    {
        return character.IsStunned() || character.IsRooted() || character.IsFeared();
    }
    
    public static string? GetCrowdControlType(this Character character)
    {
        if (character.IsStunned()) return "stunned";
        if (character.IsRooted()) return "rooted";
        if (character.IsFeared()) return "feared";
        return null;
    }
    
    public static void TickStatusEffects(this Character character)
    {
        foreach (var effect in character.ActiveStatusEffects)
        {
            effect.Duration--;
        }
        character.ActiveStatusEffects.RemoveAll(e => e.Duration <= 0);
    }
}