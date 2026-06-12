using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BattleArena.Gui.ViewModels.World;

using Models.World;
using Rendering;
using Rendering.Sprites;

public class WorldViewModel : INotifyPropertyChanged
{
    public MapManager MapManager { get; } = new();
    public CameraController Camera { get; } = new();
    public PlayerViewModel Player { get; } = new();
    public NpcController? NpcController { get; private set; }

    public TileMap Map => MapManager.CurrentMap;
    public string ZoneName => MapManager.CurrentZone.Name;
    public IReadOnlyList<NpcEntity> Npcs => MapManager.CurrentNpcs;

    public List<WorldObject> WorldObjects { get; } = TestMapData.CreateWorldObjects();
    public WorldObject? ActiveInteraction { get; private set; }
    public string InteractionFeedback { get; set; } = "";

    public event Action<string, string>? CombatEncounterTriggered; // heroName, enemyName

    public WorldViewModel()
    {
        var assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
        var cache = new SpriteCache(assetRoot);
        var tileset = new Tileset(cache);
        TileRenderer.CurrentTileset = tileset;
        CharacterRenderer.CurrentTileset = tileset;

        var worldMap = TestMapData.CreateDefaultMap();
        var dungeonMap = TestMapData.CreateDungeonMap();

        MapManager.RegisterMap("world", worldMap, ZoneDefinition.World, TestMapData.CreateNpcs());
        MapManager.RegisterMap("dungeon", dungeonMap, ZoneDefinition.Dungeon);

        MapManager.SwitchTo("world", new TilePosition(10, 5));
        Player.TilePosition = new TilePosition(10, 5);

        NpcController = new NpcController(worldMap, MapManager.CurrentNpcs);

        MapManager.MapChanged += spawnPos =>
        {
            Player.TilePosition = spawnPos;
            Camera.CenterOn(Map.Width * TileRenderer.TileWidth, Map.Height * TileRenderer.TileHeight, 800, 600);
            OnPropertyChanged(nameof(Map));
            OnPropertyChanged(nameof(ZoneName));
        };
    }

    public void CheckProximity()
    {
        var playerPos = Player.TilePosition;
        WorldObject? nearest = null;

        foreach (var obj in WorldObjects)
        {
            var dx = Math.Abs(obj.Position.TileX - playerPos.TileX);
            var dy = Math.Abs(obj.Position.TileY - playerPos.TileY);

            // Duel encounter triggers when stepping ON the tile
            if (obj.Type == WorldObjectType.DuelEncounter && dx == 0 && dy == 0)
            {
                CombatEncounterTriggered?.Invoke("Ser Garrick Dawnshield", "Lord Aethor Valeborn");
                return;
            }

            if (dx <= 1 && dy <= 1 && obj.IsInteractable)
            {
                nearest = obj;
                break;
            }
        }

        if (ActiveInteraction != nearest)
        {
            ActiveInteraction = nearest;
            OnPropertyChanged(nameof(ActiveInteraction));
        }
    }

    public string Interact()
    {
        if (ActiveInteraction is not { } obj) return "";

        var result = obj.Type switch
        {
            WorldObjectType.Door => ToggleDoor(obj),
            WorldObjectType.Chest => "You found some gold!",
            WorldObjectType.Sign => obj.SignText,
            _ => ""
        };

        return result;
    }

    private string ToggleDoor(WorldObject door)
    {
        door.IsOpen = !door.IsOpen;
        var tile = Map[door.Position.TileX, door.Position.TileY];
        // This only changes data model — passability needs map update
        return door.IsOpen ? "The door creaks open." : "The door swings shut.";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
