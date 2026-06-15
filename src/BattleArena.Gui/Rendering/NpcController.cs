namespace BattleArena.Gui.Rendering;

using System.Linq;
using Avalonia.Threading;
using Models.World;

public class NpcController
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(2.5);
    private static readonly TimeSpan MoveDuration = TimeSpan.FromMilliseconds(400);

    private readonly TileMap _map;
    private readonly IReadOnlyList<NpcEntity> _npcs;
    private readonly DispatcherTimer _timer;
    private int _patrolIndex;

    private static readonly (int Dx, int Dy)[] Directions =
    [
        (0, -1), (1, -1), (1, 0), (1, 1),
        (0, 1), (-1, 1), (-1, 0), (-1, -1)
    ];

    public NpcController(TileMap map, IReadOnlyList<NpcEntity> npcs)
    {
        _map = map;
        _npcs = npcs;
        _timer = new DispatcherTimer { Interval = TickInterval };
        _timer.Tick += OnTick;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void OnTick(object? sender, EventArgs e)
    {
        foreach (var npc in _npcs)
        {
            if (npc.IsMoving) continue;

            var target = npc.Behavior switch
            {
                NpcBehavior.Wandering => PickRandomAdjacent(npc),
                NpcBehavior.Patrolling => PickNextPatrol(npc),
                _ => (TilePosition?)null
            };

            if (target is null) continue;

            npc.MoveFrom = npc.Position;
            npc.MoveTo = target.Value;
            npc.MoveStartTime = DateTime.UtcNow;
            npc.IsMoving = true;
        }
    }

    public static void UpdateNpcAnimation(NpcEntity npc)
    {
        if (!npc.IsMoving) return;

        var elapsed = DateTime.UtcNow - npc.MoveStartTime;
        var t = Math.Clamp(elapsed.TotalMilliseconds / MoveDuration.TotalMilliseconds, 0.0, 1.0);

        if (t >= 1.0)
        {
            npc.Position = npc.MoveTo;
            npc.Facing = GetFacing(npc.MoveFrom, npc.MoveTo);
            npc.IsMoving = false;
        }
    }

    private TilePosition? PickRandomAdjacent(NpcEntity npc)
    {
        var shuffled = Directions.OrderBy(_ => Random.Shared.Next()).ToList();
        foreach (var (dx, dy) in shuffled)
        {
            var nx = npc.Position.TileX + dx;
            var ny = npc.Position.TileY + dy;
            if (nx >= 0 && nx < _map.Width && ny >= 0 && ny < _map.Height && _map[nx, ny].IsPassable)
                return new TilePosition(nx, ny);
        }
        return null;
    }

    private TilePosition? PickNextPatrol(NpcEntity npc)
    {
        if (npc.PatrolRoute is null || npc.PatrolRoute.Count == 0)
            return null;

        _patrolIndex = (_patrolIndex + 1) % npc.PatrolRoute.Count;
        var target = npc.PatrolRoute[_patrolIndex];

        // Only move if the patrol point is adjacent
        var dx = Math.Abs(target.TileX - npc.Position.TileX);
        var dy = Math.Abs(target.TileY - npc.Position.TileY);
        if (dx <= 1 && dy <= 1 && _map[target.TileX, target.TileY].IsPassable)
            return target;

        return PickRandomAdjacent(npc);
    }

    private static FacingDirection GetFacing(TilePosition from, TilePosition to)
    {
        var dx = to.TileX - from.TileX;
        var dy = to.TileY - from.TileY;
        return (dx, dy) switch
        {
            (0, -1) => FacingDirection.North,
            (1, -1) => FacingDirection.NorthEast,
            (1, 0) => FacingDirection.East,
            (1, 1) => FacingDirection.SouthEast,
            (0, 1) => FacingDirection.South,
            (-1, 1) => FacingDirection.SouthWest,
            (-1, 0) => FacingDirection.West,
            (-1, -1) => FacingDirection.NorthWest,
            _ => FacingDirection.South
        };
    }
}
