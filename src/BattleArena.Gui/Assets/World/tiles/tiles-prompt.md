Prompt for graphics AI:
Generate 9 square PNG tiles, each exactly 64×64 pixels. Each tile contains a flat-top hexagon rendered in isometric (dimetric) projection. The hex's visible footprint on screen is a diamond with these EXACT pixel coordinates within the 64×64 canvas:
Vertex 0 (top):        (32,  5)
Vertex 1 (top-right):  (60, 21)
Vertex 2 (bottom-right): (32, 37)
Vertex 3 (bottom):     (32, 37)
Vertex 4 (bottom-left):  (4, 21)
Vertex 5 (top-left):    (4, 21)
Wait — let me give you the exact math.
The engine renders flat-top hexagons with a size (radius) of 22 pixels in flat 2D space. It then applies this isometric transform:
X_iso = X_flat - Y_flat  
Y_iso = (X_flat + Y_flat) * 0.5
The result is a diamond that is approximately 56 pixels wide × 33 pixels tall, centered in a 64×64 canvas. The exact vertices of this diamond are:
- Top tip: (32, 6) — centre X, 6 from top
- Right tip: (60, 22) — 60 from left, 22 from top  
- Bottom tip: (32, 39) — centre X, 39 from top
- Left tip: (4, 22) — 4 from left, 22 from top
The diamond occupies the centre of the 64×64 square. The areas above the top tip (y: 0–6) and below the bottom tip (y: 39–63) are transparent or can contain vertical elements (tree canopies, mountain peaks, wall tops, archways). The left and right tips should be within 4 pixels of the canvas edge.
- No 3D-style base or footer — the tile is a flat 2D isometric ground plane only. Do not draw a vertical side face, extruded edge, drop shadow, or 3D block beneath the diamond. Each tile sits flush on the ground with no visible thickness or elevation at the edges.
CRITICAL: Seamless tiling
Every tile MUST tile seamlessly edge-to-edge with itself AND with Grass (the most common neighbour). This means:
1. The colour and texture at the left edge must match the right edge (for horizontal neighbours).
2. The colour and texture at the top edge must match the bottom edge (for vertical neighbours).  
3. The colour and texture at the diagonal edges must also match.
For Road tiles: the road texture at the left edge must continue onto the right edge of the neighbouring tile, so roads connect across tile boundaries. Same for Water (rivers must flow seamlessly).
Precise colour values
Tile	Hex fill (base)
Grass	#5a9e4a
Road	#9a825a
Forest	#3a7a2a
Water	#3a8fc4
Mountain	#7a8a9a
DungeonFloor	#6a6a6a
DungeonWall	#4a4a4a
Bridge	#8a6a4a
DungeonEntrance	#4a1a1a
Final checklist before delivering
1. Each PNG is exactly 64×64 pixels — no larger, no smaller, no padding.
2. The diamond footprint fits within the canvas — left tip at x≈4, right tip at x≈60, top tip at y≈6, bottom tip at y≈39.
3. All 9 tiles tile seamlessly with Grass at every edge. Test by placing them side by side in a 3×3 grid.
4. No text, no labels, no transparent gaps at tile edges.
5. Saved as individual files with the exact names: Grass.png, Road.png, Forest.png, Water.png, Mountain.png, DungeonFloor.png, DungeonWall.png, Bridge.png, DungeonEntrance.png.