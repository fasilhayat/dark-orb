namespace BattleArena.Gui.ViewModels.World;

using System.ComponentModel;
using System.Runtime.CompilerServices;

using Models.World;

public class PlayerViewModel : INotifyPropertyChanged
{
    private TilePosition _tilePosition;
    private FacingDirection _facing;
    private bool _isMoving;

    public TilePosition TilePosition
    {
        get => _tilePosition;
        set
        {
            if (_tilePosition == value) return;
            _tilePosition = value;
            OnPropertyChanged();
        }
    }

    public FacingDirection Facing
    {
        get => _facing;
        set
        {
            if (_facing == value) return;
            _facing = value;
            OnPropertyChanged();
        }
    }

    public bool IsMoving
    {
        get => _isMoving;
        set
        {
            if (_isMoving == value) return;
            _isMoving = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}