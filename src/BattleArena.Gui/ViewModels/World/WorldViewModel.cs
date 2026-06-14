using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BattleArena.Gui.ViewModels.World;

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

    public string ZoneName => "Tactical Arena";

    public List<CombatantTile> Combatants { get; } =
    [
        new() { Name = "Ser Garrick Dawnshield", Position = new TilePosition(1, 3), IsHero = true, MaxHp = 96, CurrentHp = 96 },
        new() { Name = "Sister Elira Vane",     Position = new TilePosition(1, 4), IsHero = true, MaxHp = 52, CurrentHp = 52 },
        new() { Name = "Vaelith Moonveil",      Position = new TilePosition(2, 2), IsHero = true, MaxHp = 68, CurrentHp = 68 },
        new() { Name = "Finnick Bramblefoot",   Position = new TilePosition(2, 5), IsHero = true, MaxHp = 44, CurrentHp = 44 },
        new() { Name = "Lord Aethor Valeborn",  Position = new TilePosition(10, 3), IsHero = false, MaxHp = 88, CurrentHp = 88 },
        new() { Name = "Korg Stonefist",         Position = new TilePosition(10, 4), IsHero = false, MaxHp = 72, CurrentHp = 72 },
        new() { Name = "Graveworm",             Position = new TilePosition(9, 2), IsHero = false, MaxHp = 60, CurrentHp = 60 },
        new() { Name = "Shadowmere",            Position = new TilePosition(9, 5), IsHero = false, MaxHp = 48, CurrentHp = 48 },
    ];

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
