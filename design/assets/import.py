import pandas as pd, os, math
from pathlib import Path
out = Path('output')
out.mkdir(exist_ok=True)
rows = []
items = [
('Race','Human'),('Race','Elf'),('Race','Dwarf'),('Race','Lizard'),('Race','Kobold'),('Race','Demon'),('Race','Orc'),('Race','Ogre'),('Race','Gladefolk'),('Race','Half-Elf'),
('Sub-race','High Elf'),('Sub-race','Dark Elf'),('Sub-race','Forest Elf'),('Sub-race','Mountain Dwarf'),('Sub-race','Hill Dwarf'),('Sub-race','Swamp Lizard'),('Sub-race','Desert Lizard'),('Sub-race','Forest Lizard'),('Sub-race','Fire Demon'),('Sub-race','Shadow Demon'),('Sub-race','Green Orc'),('Sub-race','Blue Orc'),('Sub-race','Red Orc'),('Sub-race','Mountain Ogre'),('Sub-race','Hill Ogre'),('Sub-race','Desert Ogre'),('Sub-race','Forest Ogre'),('Sub-race','Forest Gladefolk'),('Sub-race','Hill Gladefolk'),('Sub-race','Half-High-Elf'),('Sub-race','Half-Wood-Elf'),
('Class','Barbarian'),('Class','Knight'),('Class','Paladin'),('Class','Priest'),('Class','Mage'),('Class','Bard'),('Class','Fighter'),('Class','Rogue'),('Class','Druid'),('Class','Ranger'),
('Deity','Heaven'),('Deity','Star'),('Deity','Constellations'),('Deity','Moon'),('Deity','Fire'),('Deity','Darkness'),('Deity','Smoke'),('Deity','Shadow')
]
cols = 6
size = 1024
cell = size // cols
for i,(cat,name) in enumerate(items):
    r = i // cols
    c = i % cols
    rows.append({
        'index': i+1,
        'category': cat,
        'name': name,
        'row': r+1,
        'col': c+1,
        'x0': c*cell,
        'y0': r*cell,
        'x1': (c+1)*cell,
        'y1': (r+1)*cell,
        'center_x': c*cell + cell/2,
        'center_y': r*cell + cell/2,
    })
df = pd.DataFrame(rows)
csv_path = out/'icon_sheet_coordinates.csv'
df.to_csv(csv_path, index=False)
# also write a compact txt guide
with open(out/'icon_sheet_instructions.txt','w') as f:
    f.write('Use the coordinates as pixel bounds on the sheet image. Each icon occupies one grid cell. Crop with x0,y0,x1,y1.\n')
    f.write('If your image editor uses center + size, use center_x, center_y with width=cell and height=cell.\n')
    f.write('Grid: 6 columns, 9 rows, 1024x1024 sheet, each cell 170x170 except the last partial row space.\n')
    f.write('Order: Races, Sub-races, Classes, Deities.\n')
    for _,r in df.iterrows():
        f.write(f"{int(r['index']):02d} | {r['category']}: {r['name']} | row {int(r['row'])}, col {int(r['col'])} | x0={int(r['x0'])}, y0={int(r['y0'])}, x1={int(r['x1'])}, y1={int(r['y1'])}\n")
# create a simple reference sheet preview for download only as csv/txt artifacts
print(df.head(3).to_string(index=False))
print(csv_path)