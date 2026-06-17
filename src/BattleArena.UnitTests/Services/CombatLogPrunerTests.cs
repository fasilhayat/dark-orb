namespace BattleArena.UnitTests.Services;

using BattleArena.Presentation;

public class CombatLogPrunerTests : IDisposable
{
    private readonly string _scratchRoot = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..",
        "BattleArena.UnitTests",
        "TestResults",
        "CombatLogPrunerTests",
        Guid.NewGuid().ToString("N")));

    public CombatLogPrunerTests() => Directory.CreateDirectory(_scratchRoot);

    public void Dispose()
    {
        if (!Directory.Exists(_scratchRoot)) return;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                Directory.Delete(_scratchRoot, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(50);
            }
        }
    }

    private void CreateFilePair(string baseName, DateTime writeTime)
    {
        var json = Path.Combine(_scratchRoot, $"{baseName}.json");
        var txt = Path.Combine(_scratchRoot, $"{baseName}.txt");
        File.WriteAllText(json, "{}");
        File.WriteAllText(txt, "log");
        File.SetLastWriteTime(json, writeTime);
        File.SetLastWriteTime(txt, writeTime);
    }

    [Fact]
    public void Prune_FewerThanKeepFiles_NothingDeleted()
    {
        CreateFilePair("run_001", DateTime.Now.AddMinutes(-5));
        CreateFilePair("run_002", DateTime.Now.AddMinutes(-3));

        CombatLogPruner.Prune(new DirectoryInfo(_scratchRoot), keep: 10);

        Assert.Equal(2, Directory.GetFiles(_scratchRoot, "*.json").Length);
        Assert.Equal(2, Directory.GetFiles(_scratchRoot, "*.txt").Length);
    }

    [Fact]
    public void Prune_MoreThanKeepFiles_OldestDeleted()
    {
        var now = DateTime.Now;
        for (int i = 1; i <= 12; i++)
            CreateFilePair($"run_{i:D3}", now.AddMinutes(-i));

        CombatLogPruner.Prune(new DirectoryInfo(_scratchRoot), keep: 10);

        Assert.Equal(10, Directory.GetFiles(_scratchRoot, "*.json").Length);
        Assert.Equal(10, Directory.GetFiles(_scratchRoot, "*.txt").Length);
    }

    [Fact]
    public void Prune_OldestPairDeleted_NewestKept()
    {
        var now = DateTime.Now;
        CreateFilePair("newest", now);
        CreateFilePair("oldest", now.AddHours(-1));

        CombatLogPruner.Prune(new DirectoryInfo(_scratchRoot), keep: 1);

        Assert.True(File.Exists(Path.Combine(_scratchRoot, "newest.json")));
        Assert.False(File.Exists(Path.Combine(_scratchRoot, "oldest.json")));
        Assert.False(File.Exists(Path.Combine(_scratchRoot, "oldest.txt")));
    }

    [Fact]
    public void Prune_JsonWithoutTxt_DeletesJsonOnly()
    {
        var now = DateTime.Now;
        CreateFilePair("recent", now);
        var orphan = Path.Combine(_scratchRoot, "orphan.json");
        File.WriteAllText(orphan, "{}");
        File.SetLastWriteTime(orphan, now.AddHours(-2));

        CombatLogPruner.Prune(new DirectoryInfo(_scratchRoot), keep: 1);

        Assert.True(File.Exists(Path.Combine(_scratchRoot, "recent.json")));
        Assert.False(File.Exists(orphan));
    }

    [Fact]
    public void Prune_EmptyDirectory_NoException()
    {
        CombatLogPruner.Prune(new DirectoryInfo(_scratchRoot), keep: 10);
    }
}
