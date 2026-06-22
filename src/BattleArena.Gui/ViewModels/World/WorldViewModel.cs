namespace BattleArena.Gui.ViewModels.World;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Data;
using Models.World;
using Rendering;

public class WorldViewModel : INotifyPropertyChanged
{
    public CameraController Camera { get; } = new();

    private TileMap _map = TestMapData.CreateArenaMap();
    public TileMap Map
    {
        get => _map;
        set
        {
            if (_map == value) return;
            _map = value;
            OnPropertyChanged();
        }
    }

    private string _zoneName = "Tactical Arena";
    public string ZoneName
    {
        get => _zoneName;
        set => SetField(ref _zoneName, value);
    }

    private string _terrainName = "Plains";
    public string TerrainName
    {
        get => _terrainName;
        set => SetField(ref _terrainName, value);
    }

    public IReadOnlyList<MapData> AvailableMaps => MapLoader.ListMaps();

    private MapData? _currentMapData;
    public MapData? CurrentMapData
    {
        get => _currentMapData;
        set
        {
            if (_currentMapData?.Id == value?.Id) return;
            _currentMapData = value;
            if (value is not null)
            {
                Map = MapLoader.LoadMap(value);
                ZoneName = value.Name;
            }
        }
    }

    public List<CombatantTile> Combatants { get; } =
    [
        new() { Name = "Ser Garrick Dawnshield", ClassName = "Paladin", Race = "Human", Position = new TilePosition(1, 3), IsHero = true, MaxHp = 96, CurrentHp = 96 },
        new() { Name = "Elira Vane",            ClassName = "Priest",  Race = "Human",  Position = new TilePosition(1, 4), IsHero = true, MaxHp = 56, CurrentHp = 56 },
        new() { Name = "Vaelith Moonveil",      ClassName = "Mage",    Race = "High Elf", Position = new TilePosition(2, 2), IsHero = true, MaxHp = 68, CurrentHp = 68 },
        new() { Name = "Finnick Bramblefoot",   ClassName = "Rogue",   Race = "Human",  Position = new TilePosition(2, 5), IsHero = true, MaxHp = 44, CurrentHp = 44 },
        new() { Name = "Lord Aethor Valeborn",  ClassName = "Fighter", Race = "Human",  Position = new TilePosition(10, 3), IsHero = false, MaxHp = 88, CurrentHp = 88 },
        new() { Name = "Korg Stonefist",         ClassName = "Barbarian", Race = "Dwarf", Position = new TilePosition(10, 4), IsHero = false, MaxHp = 72, CurrentHp = 72 },
        new() { Name = "Graveworm",             ClassName = "Monster", Race = "Undead", Position = new TilePosition(9, 2), IsHero = false, MaxHp = 60, CurrentHp = 60 },
        new() { Name = "Shadowmere",            ClassName = "Monster", Race = "Undead", Position = new TilePosition(9, 5), IsHero = false, MaxHp = 48, CurrentHp = 48 },
    ];

    private Bitmap? _backgroundImage;
    public Bitmap? BackgroundImage
    {
        get => _backgroundImage;
        set => SetField(ref _backgroundImage, value);
    }

    public bool HasBackgroundImage => _backgroundImage is not null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadMapById(string id)
    {
        var match = AvailableMaps.FirstOrDefault(m => m.Id == id);
        if (match is not null)
        {
            TerrainName = match.Terrain;
            CurrentMapData = match;
        }
    }

    public void LoadFirstMap()
    {
        var maps = AvailableMaps;
        if (maps.Count > 0)
        {
            TerrainName = maps[0].Terrain;
            CurrentMapData = maps[0];
        }
        else
        {
            Map = TestMapData.CreateArenaMap();
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}