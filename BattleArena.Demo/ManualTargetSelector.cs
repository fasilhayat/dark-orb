namespace BattleArena.Demo;

using Application.Interfaces;
using Core.Entities;

class ManualConsoleTargetSelector : ITargetSelector
{
    public Task<Character> SelectTargetAsync(
        Character actor,
        IEnumerable<Character> livingEnemies,
        CancellationToken ct = default)
    {
        var targets = livingEnemies.ToList();

        if (targets.Count == 1) return Task.FromResult(targets[0]);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  MANUAL TARGET  --  {actor.Name} is ready to act!");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  " + new string('-', 50));
        Console.ResetColor();
        Console.WriteLine();

        for (var i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            var pct = (double)Math.Max(0, t.CurrentHitPoints) / Math.Max(1, t.MaxHitPoints);
            var (filled, empty) = BuildHpBar(t.CurrentHitPoints, t.MaxHitPoints, 16);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  [{i + 1}]  ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{t.Name,-12}");
            Console.ResetColor();
            Console.Write("  HP [");
            Console.ForegroundColor = pct > 0.5 ? ConsoleColor.Green : pct > 0.25 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.Write(filled);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(empty);
            Console.ResetColor();
            Console.WriteLine($"]  {Math.Max(0, t.CurrentHitPoints),3} / {t.MaxHitPoints,3}");
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  > ");
        Console.ResetColor();

        while (true)
        {
            if (ct.IsCancellationRequested) return Task.FromResult(targets[0]);

            var k = Console.ReadKey(true).KeyChar;
            if (int.TryParse(k.ToString(), out var idx) && idx >= 1 && idx <= targets.Count)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(targets[idx - 1].Name);
                Console.ResetColor();
                return Task.FromResult(targets[idx - 1]);
            }
        }
    }

    private static (string filled, string empty) BuildHpBar(int current, int max, int w)
    {
        var pct = (double)Math.Max(0, current) / Math.Max(1, max);
        var filled = current > 0 ? Math.Max(1, (int)(pct * w)) : 0;
        return (new string('\u2588', filled), new string('\u2591', w - filled));
    }
}
