namespace BattleArena.Gui.Rendering;

using Models.World;

public static class Pathfinder
{
    private static readonly (int Dx, int Dy, int Cost)[] Neighbors =
    [
        (0, -1, 10),  // N
        (1, -1, 14),  // NE
        (1, 0, 10),   // E
        (1, 1, 14),   // SE
        (0, 1, 10),   // S
        (-1, 1, 14),  // SW
        (-1, 0, 10),  // W
        (-1, -1, 14), // NW
    ];

    public static PathResult FindPath(TileMap map, TilePosition start, TilePosition end)
    {
        if (!map[end.TileX, end.TileY].IsPassable)
            return new PathResult([], false, 0);

        if (start == end)
            return new PathResult([start], true, 0);

        var closed = new HashSet<TilePosition>();
        var open = new SortedSet<(int F, int G, int X, int Y)>();
        var gScores = new Dictionary<TilePosition, int> { [start] = 0 };
        var cameFrom = new Dictionary<TilePosition, TilePosition>();

        var h = Heuristic(start, end);
        open.Add((h, 0, start.TileX, start.TileY));

        while (open.Count > 0)
        {
            var (_, g, cx, cy) = open.Min;
            open.Remove(open.Min);
            var current = new TilePosition(cx, cy);

            if (current == end)
                return BuildPath(cameFrom, start, end, g);

            if (!closed.Add(current))
                continue;

            foreach (var (dx, dy, stepCost) in Neighbors)
            {
                var nx = current.TileX + dx;
                var ny = current.TileY + dy;

                if (nx < 0 || nx >= map.Width || ny < 0 || ny >= map.Height)
                    continue;

                if (!map[nx, ny].IsPassable)
                    continue;

                var neighbor = new TilePosition(nx, ny);
                if (closed.Contains(neighbor))
                    continue;

                var tentativeG = g + stepCost;

                if (gScores.TryGetValue(neighbor, out var existingG) && tentativeG >= existingG)
                    continue;

                gScores[neighbor] = tentativeG;
                cameFrom[neighbor] = current;
                var f = tentativeG + Heuristic(neighbor, end);
                open.Add((f, tentativeG, nx, ny));
            }
        }

        return new PathResult([], false, 0);
    }

    private static int Heuristic(TilePosition a, TilePosition b)
    {
        var dx = Math.Abs(a.TileX - b.TileX);
        var dy = Math.Abs(a.TileY - b.TileY);
        return Math.Max(dx, dy) * 10;
    }

    private static PathResult BuildPath(
        Dictionary<TilePosition, TilePosition> cameFrom,
        TilePosition start, TilePosition end, int totalCost)
    {
        var path = new List<TilePosition>();
        var current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return new PathResult(path.AsReadOnly(), true, totalCost);
    }
}
