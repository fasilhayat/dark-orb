namespace BattleArena.Gui.Rendering.Sprites;

using System.Collections.Concurrent;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

public class SpriteCache
{
    private readonly string _assetRoot;
    private readonly ConcurrentDictionary<string, Bitmap?> _cache = new();
    private bool _preloaded;

    public SpriteCache(string assetRoot)
    {
        _assetRoot = assetRoot;
    }

    public Bitmap? GetSprite(string relativePath)
    {
        return _cache.GetOrAdd(relativePath, path =>
        {
            var fullPath = Path.Combine(_assetRoot, "World", path);
            if (File.Exists(fullPath))
            {
                try { return new Bitmap(fullPath); }
                catch { return null; }
            }
            return null;
        });
    }

    public void PreloadAll()
    {
        if (_preloaded) return;
        _preloaded = true;

        if (!Directory.Exists(_assetRoot)) return;
        var worldDir = Path.Combine(_assetRoot, "World");
        if (!Directory.Exists(worldDir)) return;

        foreach (var file in Directory.EnumerateFiles(worldDir, "*.png", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(worldDir, file);
            GetSprite(relative);
        }
    }
}
