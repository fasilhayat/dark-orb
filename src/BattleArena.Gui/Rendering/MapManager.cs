namespace BattleArena.Gui.Rendering;

using Models.World;

public class MapManager
{
    private readonly Dictionary<string, TileMap> _maps = new();
    private readonly Dictionary<string, ZoneDefinition> _zones = new();
    private readonly Dictionary<string, List<NpcEntity>> _npcs = new();

    public TileMap CurrentMap { get; private set; } = null!;
    public ZoneDefinition CurrentZone { get; private set; } = null!;
    public IReadOnlyList<NpcEntity> CurrentNpcs { get; private set; } = [];
    public string CurrentMapId { get; private set; } = "";

    public void RegisterMap(string mapId, TileMap map, ZoneDefinition zone, List<NpcEntity>? npcs = null)
    {
        _maps[mapId] = map;
        _zones[mapId] = zone;
        _npcs[mapId] = npcs ?? [];
    }

    public void SwitchTo(string mapId, TilePosition spawnPos)
    {
        if (!_maps.TryGetValue(mapId, out var map))
            return;

        CurrentMap = map;
        CurrentMapId = mapId;
        CurrentZone = _zones[mapId];
        CurrentNpcs = _npcs[mapId];
        MapChanged?.Invoke(spawnPos);
    }

    public event Action<TilePosition>? MapChanged;

    public MapTransition? GetTransition(TilePosition tile)
    {
        return (CurrentMapId, tile) switch
        {
            ("world", (33, 18)) => new MapTransition("dungeon", new TilePosition(7, 5)),
            ("dungeon", (7, 0)) => new MapTransition("world", new TilePosition(33, 19)),
            _ => null
        };
    }
}
