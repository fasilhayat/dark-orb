namespace BattleArena.Demo;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

internal class ConsoleActionDecisionSource : IActionDecisionSource
{
    private readonly Action<int, string?> _redrawScreen;

    public ConsoleActionDecisionSource(Action<int, string?> redrawScreen)
    {
        _redrawScreen = redrawScreen;
    }

    public Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct,
        EngagementRange engagementRange = EngagementRange.Melee)
    {
        _redrawScreen(currentTick, actor.Name);

        var options = BuildActionOptions(actor);

        while (true)
        {
            Console.WriteLine();
            ColoredLine($"  {actor.Name}'s turn — Choose action:", ConsoleColor.Yellow);
            Console.WriteLine();

            for (var i = 0; i < options.Count; i++)
            {
                var (key, label, _) = options[i];
                Colored($"    [{key}]  ", ConsoleColor.Cyan);
                ColoredLine(label, ConsoleColor.White);
            }

            Console.WriteLine();
            Colored("  > ", ConsoleColor.Cyan);
            var pick = char.ToUpperInvariant(Console.ReadKey(true).KeyChar);

            var match = options.FirstOrDefault(o => o.key == pick);
            if (match.source is not null || match.key == options[^1].key)
            {
                ColoredLine($"  -> {match.label}", ConsoleColor.Green);
                return Task.FromResult(match.source);
            }

            ColoredLine("  Invalid choice — try again.", ConsoleColor.Yellow);
        }
    }

    private static void Colored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static void ColoredLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static List<(char key, string label, IAttackSource? source)> BuildActionOptions(Character actor)
    {
        var options = new List<(char, string, IAttackSource?)>();
        var nextKey = '1';

        var weapon = actor.Equipment.RightHand;
        if (weapon is not null)
        {
            var rangedTag = weapon.AttackType == AttackType.Ranged ? " (ranged)" : "";
            options.Add((nextKey++, $"Attack: {weapon.Name}{rangedTag}", weapon));
        }

        foreach (var spell in actor.MemorizedSpells.Where(s => actor.CanCast(s)))
        {
            if (spell.ManaCost <= 0 || actor.CurrentMana >= spell.ManaCost)
            {
                var manaInfo = spell.ManaCost > 0 ? $"  ({spell.ManaCost} MP)" : "";
                options.Add((nextKey++, $"{spell.Name}{manaInfo}", spell));
            }
        }

        if (options.Count == 0 || actor.Equipment.RightHand is null)
        {
            options.Add((nextKey++, "Unarmed Strike", UnarmedStrike.Default));
        }

        var speed = actor.EffectiveMovementSpeed;
        options.Add((nextKey++, $"Move  — reposition ({speed} ft)", new MoveIntent()));
        options.Add((nextKey, "Skip  — do nothing", null));

        return options;
    }
}
