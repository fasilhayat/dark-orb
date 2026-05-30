namespace BattleArena.Presentation;

/// <summary>
/// Prunes combat log files from a directory, keeping only the most recent pairs.
/// Each combat run produces a .json + .txt pair with the same base name.
/// </summary>
public static class CombatLogPruner
{
    /// <summary>
    /// Deletes the oldest .json files (and their .txt counterparts) from
    /// <paramref name="directory"/>, keeping the <paramref name="keep"/> most recent.
    /// </summary>
    public static void Prune(DirectoryInfo directory, int keep = 10)
    {
        var jsonFiles = directory.GetFiles("*.json")
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        foreach (var surplus in jsonFiles.Skip(keep))
        {
            surplus.Delete();
            var matchingTxt = Path.ChangeExtension(surplus.FullName, ".txt");
            if (File.Exists(matchingTxt))
                File.Delete(matchingTxt);
        }
    }
}
