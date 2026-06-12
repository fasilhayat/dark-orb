namespace BattleArena.Gui.Models.World;

public static class TestMapData
{
    public static TileMap CreateDefaultMap()
    {
        const int width = 40;
        const int height = 30;
        var tiles = new Tile[width * height];

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            tiles[y * width + x] = CreateTile(x, y, width, height);

        return new TileMap(width, height, tiles);
    }

    public static List<WorldObject> CreateWorldObjects()
    {
        return
        [
            new WorldObject
            {
                Position = new TilePosition(9, 11),
                Type = WorldObjectType.Door,
                Label = "Wooden Gate",
                IsOpen = false,
            },
            new WorldObject
            {
                Position = new TilePosition(7, 10),
                Type = WorldObjectType.Chest,
                Label = "Chest",
            },
            new WorldObject
            {
                Position = new TilePosition(14, 10),
                Type = WorldObjectType.Sign,
                Label = "Sign",
                SignText = "Road to Mountains →",
            },
            new WorldObject
            {
                Position = new TilePosition(20, 10),
                Type = WorldObjectType.DuelEncounter,
                Label = "???" ,
            },
        ];
    }

    public static List<NpcEntity> CreateNpcs()
    {
        var guardRoute = new List<TilePosition>
        {
            new(7, 10), new(8, 9), new(9, 10), new(8, 11)
        };

        return
        [
            new NpcEntity
            {
                Name = "Merchant",
                Position = new TilePosition(8, 10),
                Behavior = NpcBehavior.Stationary,
                Facing = FacingDirection.South,
            },
            new NpcEntity
            {
                Name = "Guard",
                Position = new TilePosition(8, 9),
                Behavior = NpcBehavior.Patrolling,
                PatrolRoute = guardRoute,
                Facing = FacingDirection.East,
            },
            new NpcEntity
            {
                Name = "Villager",
                Position = new TilePosition(9, 11),
                Behavior = NpcBehavior.Wandering,
                Facing = FacingDirection.South,
            },
        ];
    }

    private static Tile CreateTile(int x, int y, int mapW, int mapH)
    {
        var cx = mapW / 2;
        var cy = mapH / 2;

        // ── River (diagonal from NW to SE) ──
        var riverDist = Math.Abs((x - 5) - (y - 5));
        if (riverDist <= 1 && y > 3 && y < mapH - 3)
            return new Tile(TileType.Water, int.MaxValue, false);

        // ── Lakes ──
        var lakeDx = x - 30;
        var lakeDy = y - 5;
        var lakeDist = Math.Sqrt(lakeDx * lakeDx + lakeDy * lakeDy);
        if (lakeDist < 3)
            return new Tile(TileType.Water, int.MaxValue, false);

        // ── Mountain range (east side) ──
        var mtnDx = x - 32;
        var mtnDy = y - 18;
        var mtnDist = Math.Sqrt(mtnDx * mtnDx + mtnDy * mtnDy);

        // Dungeon entrance inside the mountain
        if (x == 33 && y == 18)
            return new Tile(TileType.DungeonEntrance, 1, true);

        if (mtnDist < 5)
            return new Tile(TileType.Mountain, 3, true);
        if (mtnDist < 7)
            return new Tile(TileType.Forest, 2, true);

        // ── Forest (south-east) ──
        var forestDx = x - 35;
        var forestDy = y - 26;
        if (Math.Abs(forestDx) < 4 && Math.Abs(forestDy) < 3)
            return new Tile(TileType.Forest, 2, true);

        // ── Village (center-left, around bridge) ──
        var villageCenterX = 8;
        var villageCenterY = 10;
        var vDist = Math.Sqrt(Math.Pow(x - villageCenterX, 2) + Math.Pow(y - villageCenterY, 2));
        if (vDist < 3)
            return new Tile(TileType.Road, 1, true);

        // ── Roads ──
        // Main road: horizontal at y=10, vertical at x=8
        if (y == 10 || x == 8)
            return new Tile(TileType.Road, 1, true);

        // Path to village
        if (x == 8 && y > 3 && y < 10)
            return new Tile(TileType.Road, 1, true);

        // Path from village to mountain
        if (y == 10 && x >= 12 && x <= 28)
            return new Tile(TileType.Road, 1, true);

        // Bridge over river
        if (x == 8 && y >= 4 && y <= 8 && riverDist <= 1)
            return new Tile(TileType.Bridge, 1, true);

        // ── Center pond ──
        var pondDx = x - cx;
        var pondDy = y - cy;
        var pondDist = Math.Sqrt(pondDx * pondDx + pondDy * pondDy);
        if (pondDist < 3)
            return new Tile(TileType.Water, int.MaxValue, false);
        if (pondDist < 4)
            return new Tile(TileType.Forest, 2, true);

        // ── Grass (default) ──
        return new Tile(TileType.Grass, 1, true);
    }

    public static TileMap CreateDungeonMap()
    {
        const int width = 15;
        const int height = 10;
        var tiles = new Tile[width * height];

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            tiles[y * width + x] = CreateDungeonTile(x, y, width, height);

        return new TileMap(width, height, tiles);
    }

    private static Tile CreateDungeonTile(int x, int y, int w, int h)
    {
        // Walls around the border
        if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
            return new Tile(TileType.DungeonWall, int.MaxValue, false);

        // Exit at top center
        if (x == 7 && y == 0)
            return new Tile(TileType.DungeonFloor, 1, true);

        // Pillars
        if ((x == 3 || x == 11) && (y == 3 || y == 7))
            return new Tile(TileType.DungeonWall, int.MaxValue, false);

        return new Tile(TileType.DungeonFloor, 1, true);
    }
}
