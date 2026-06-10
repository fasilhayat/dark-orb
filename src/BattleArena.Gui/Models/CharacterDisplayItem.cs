using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using BattleArena.Core.Entities;

namespace BattleArena.Gui.Models;

public class CharacterDisplayItem : INotifyPropertyChanged
{
    public Character Character { get; }
    public Bitmap? Portrait { get; }

    public string Name => Character.Name;
    public string ClassName => Character.ClassName;
    public int Level => Character.Level;
    public int MaxHitPoints => Character.MaxHitPoints;
    public string RaceName => Character.Race?.Name ?? "";
    public int SpellCount => Character.MemorizedSpells.Count;

    private int _teamSlot;
    public int TeamSlot
    {
        get => _teamSlot;
        set
        {
            if (_teamSlot == value) return;
            _teamSlot = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BorderThickness));
            OnPropertyChanged(nameof(SelectionLabel));
            OnPropertyChanged(nameof(SelectionBadgeColor));
        }
    }

    public bool IsSelected => _teamSlot > 0;
    public string BorderBrush => IsSelected ? "#FFD700" : "#333";
    public string BorderThickness => IsSelected ? "2" : "1";
    public string SelectionLabel => IsSelected ? $"TEAM {_teamSlot}" : "";
    public string SelectionBadgeColor => _teamSlot switch
    {
        1 => "#E53935",
        2 => "#1E88E5",
        _ => ""
    };

    public CharacterDisplayItem(Character character)
    {
        Character = character;
        Portrait = PortraitResolver.GetPortrait(character.Name);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
