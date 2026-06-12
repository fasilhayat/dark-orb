using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BattleArena.Gui.ViewModels.WorldMap;

using System.IO;
using System.Text.Json;
using Models.WorldMap;

public class WorldMapViewModel : INotifyPropertyChanged
{
    private const string DataFile = "Data/world-map-data.json";

    public List<MapLocation> Locations { get; } = [];
    public List<RoadConnection> Roads { get; } = [];

    public MapLocation? SelectedLocation { get; set; }
    public double PartyX { get; set; } = 680;
    public double PartyY { get; set; } = 590;

    public event Action<string>? LocationEntered;
    public bool HasSelection => SelectedLocation is not null;

    public WorldMapViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, DataFile);
        if (!File.Exists(path)) return;

        try
        {
            var json = JsonDocument.Parse(File.ReadAllText(path));
            var root = json.RootElement;

            if (root.TryGetProperty("locations", out var locs))
            {
                foreach (var l in locs.EnumerateArray())
                {
                    var name = l.GetProperty("name").GetString() ?? "";
                    var typeStr = l.GetProperty("type").GetString() ?? "village";
                    var x = l.GetProperty("screenX").GetDouble();
                    var y = l.GetProperty("screenY").GetDouble();

                    if (!Enum.TryParse<LocationType>(typeStr, true, out var type))
                        type = LocationType.Village;

                    Locations.Add(new MapLocation
                    {
                        Name = name,
                        Type = type,
                        ScreenX = x,
                        ScreenY = y,
                        Description = GetDefaultDescription(type),
                        TargetMapId = GetTargetMapId(name, type),
                    });
                }
            }

            if (root.TryGetProperty("roads", out var roads))
            {
                foreach (var r in roads.EnumerateArray())
                {
                    Roads.Add(new RoadConnection
                    {
                        FromIndex = r.GetProperty("from").GetInt32(),
                        ToIndex = r.GetProperty("to").GetInt32(),
                    });
                }
            }
        }
        catch
        {
            // Fall back to empty if data file is corrupt
        }
    }

    public void SelectLocation(MapLocation loc)
    {
        if (SelectedLocation == loc) return;
        SelectedLocation = loc;
        OnPropertyChanged(nameof(SelectedLocation));
        OnPropertyChanged(nameof(HasSelection));
    }

    public void EnterLocation()
    {
        if (SelectedLocation is not { } loc) return;
        PartyX = loc.ScreenX;
        PartyY = loc.ScreenY;
        OnPropertyChanged(nameof(PartyX));
        OnPropertyChanged(nameof(PartyY));
        LocationEntered?.Invoke(loc.TargetMapId ?? "");
    }

    private static string GetDefaultDescription(LocationType type) => type switch
    {
        LocationType.Capital => "The seat of power in Aelthoria.",
        LocationType.MajorCity => "A bustling center of trade and culture.",
        LocationType.Town => "A modest settlement along the trade routes.",
        LocationType.Village => "A quiet hamlet.",
        LocationType.Fort => "A fortified outpost.",
        LocationType.Port => "A coastal harbor town.",
        LocationType.Dungeon => "A dark and dangerous place.",
        LocationType.Cave => "A dark cave nestled in the mountains.",
        LocationType.Ruins => "Ancient ruins of a forgotten civilization.",
        LocationType.Encounter => "A point of interest on the road.",
        _ => "",
    };

    private static string? GetTargetMapId(string name, LocationType type) => (name, type) switch
    {
        ("Mountain Cave", _) => "dungeon",
        _ when type == LocationType.Dungeon => "dungeon",
        _ => null,
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
