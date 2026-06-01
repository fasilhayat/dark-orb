-- ============================================================
-- BattleArena - Bestiary Seed Data
-- 68 creature stat blocks across 10 categories, Levels 1-12+
-- Based on design/bestiary.md
-- ============================================================

-- ============================================================
-- 1. HUMANOIDS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Humanoid', 'Lesser Goblin', 1, 0, 1, 0, -1, -1, -1, 6, 10,
     '+0, 1D4 (rusty dagger)',
     'Pack Tactics — +1 to hit when an ally is within 5 ft. of the target.',
     'A scrawny goblin in mismatched leather scraps. It winces at every sound and its dagger is pitted with rust.'),
    ('Humanoid', 'Goblin', 2, 0, 1, 0, 0, -1, -1, 10, 11,
     '+1, 1D6 (short sword) or +1, 1D4 (short bow, ranged)',
     'Pack Tactics — +1 to hit when an ally is within 5 ft. of the target.',
     'A wiry goblin with a proper blade and a sneer of confidence. It has survived long enough to learn how to fight.'),
    ('Humanoid', 'Greater Goblin', 3, 1, 1, 1, 0, 0, -1, 18, 12,
     '+2, 1D6+1 (scimitar)',
     'Command — once per combat, grant one goblin ally an immediate extra attack. Pack Tactics — +1 to hit when an ally is within 5 ft. of the target.',
     'A scarred goblin wearing a helm too large for its head. The others flinch when it snarls orders.'),
    ('Humanoid', 'Lesser Kobold', 1, -1, 2, 0, 0, -1, -1, 5, 10,
     '+0, 1D4 (rusty spear)',
     'Magic Resistance — advantage on saves vs. magical effects. Tunnel Rat — does not provoke opportunity attacks when moving through narrow terrain.',
     'A shivering kobold clutching a sharpened stick. Its eyes dart nervously between the shadows.'),
    ('Humanoid', 'Kobold', 2, 0, 2, 0, 1, 0, -1, 9, 11,
     '+1, 1D4+1 (short spear) or +2, 1D4 (sling, ranged)',
     'Magic Resistance — advantage on saves vs. magical effects. Trap Sense — +2 to saves against traps; knows if a trapped area is nearby.',
     'A kobold wearing patched leather armour and a crude metal helm. A pouch of caltrops dangles from its belt.'),
    ('Humanoid', 'Greater Kobold', 3, 0, 2, 0, 1, 1, 0, 14, 12,
     '+1, 1D4+1 (ritual dagger)',
     'Magic Resistance — advantage on saves vs. magical effects. Cantrip — once per turn: Acid Splash (1D4 acid, 30 ft. ranged) or Daze (target loses next action on failed WIS save). Inspire Minions — allied kobolds within 20 ft. gain +1 to hit.',
     'A kobold draped in feathers and bone charms, its eyes glowing with stolen arcane fire. It chants in a tongue older than the stones it stands on.'),
    ('Humanoid', 'Lesser Orc', 3, 2, 1, 1, 0, 0, -1, 18, 12,
     '+3, 1D8+2 (battle axe)',
     'Extra Strength — +2 to melee damage rolls (included). Aggressive — may move up to its speed toward an enemy as a bonus action.',
     'A lean orc in hide armour, war paint smeared across its face. It sniffs the air and grins, sensing a fight.'),
    ('Humanoid', 'Orc', 5, 3, 0, 2, 0, 0, -1, 30, 13,
     '+5, 1D10+3 (greataxe)',
     'Extra Strength — +2 to melee damage rolls (included). Aggressive — may move up to its speed toward an enemy as a bonus action. Relentless — once per combat, when reduced to 0 HP, drop to 1 HP instead.',
     'A hulking orc in iron plate, scarred from a hundred battles. It hefts its axe and roars a challenge.'),
    ('Humanoid', 'Greater Orc', 8, 4, 1, 3, 1, 1, 0, 55, 15,
     '+7, 1D12+4 (great axe) or +6, 1D8+4 (throwing axe, 20 ft. ranged)',
     'Extra Strength — +2 to melee damage rolls (included). Aggressive — may move up to its speed toward an enemy as a bonus action. Battle Cry — once per combat, all orc allies within 30 ft. gain +1 to hit and +2 damage for 2 rounds. Relentless — once per combat, when reduced to 0 HP, drop to 1 HP instead.',
     'An orc warlord in full steel plate, its tusks sheathed in gold. It carries the scarred standard of its clan and commands absolute loyalty.'),
    ('Humanoid', 'Dark Elf Scout', 5, 0, 3, 0, 1, 1, 1, 22, 14,
     '+5, 1D6+3 (short sword) + poison, or +6, 1D6+3 (hand crossbow, ranged) + poison',
     'Magic Resistance — advantage on saves vs. magical effects. Poison — on hit, target makes a CON save or takes 1D4 poison and is Poisoned for 1 round. Darkvision — sees in magical darkness. Fade — bonus action to become Invisible for 1 round (recharges on a short rest).',
     'A lithe elf in spider-silk armour, a blade coated in venom glinting in the dim light. It moves without sound.'),
    ('Humanoid', 'Dark Elf Mage', 7, 0, 2, 0, 3, 1, 2, 28, 14,
     '+5, 1D4+1 (ritual dagger)',
     'Magic Resistance — advantage on saves vs. magical effects. Spells — Darkness (20 ft. radius, 3 rounds), Magic Missile (2D4+2 force, auto-hit), Fear (once per combat). Darkvision — sees in magical darkness.',
     'A dark elf draped in robes of deep purple and black, a spider-shaped brooch at its throat. Arcane energy crackles between its fingers.'),
    ('Humanoid', 'Greater Dark Elf', 9, 1, 2, 1, 3, 2, 3, 42, 16,
     '+7, 1D8+3 (magic rapier + poison)',
     'Magic Resistance — advantage on saves vs. magical effects. Poison — on hit, target makes a CON save or takes 2D4 poison and is Poisoned for 2 rounds. Darkvision — sees in magical darkness. Spells — Darkness, Hold Person (WIS save or Paralyzed), Cloudkill (once per combat, 20 ft. radius, 2D8 poison). Fade — bonus action to become Invisible for 2 rounds (recharges on a short rest). Lolth''s Favour — once per combat, reroll any save.',
     'A regal drow woman in a gown of woven shadow, a whip of living spider silk coiled at her hip. She does not walk — she glides, and the darkness bends to her will.'),
    ('Humanoid', 'Elven Archer', 3, 0, 2, 0, 1, 1, 1, 14, 12,
     '+4, 1D8+2 (long bow, 80 ft. ranged) or +3, 1D6+1 (short sword)',
     'Magic Resistance — advantage on saves vs. magical effects. Keen Eye — +2 to Perception; ignores half-cover bonuses.',
     'A lithe elf in studded leather, a long bow of carved yew in hand. They move through the forest without a snapped twig, and their arrows find the gaps in armour.'),
    ('Humanoid', 'Elven Mage', 6, 0, 2, 0, 3, 1, 2, 22, 13,
     '+5, 1D4 (arcane staff)',
     'Magic Resistance — advantage on saves vs. magical effects. Spells — Magic Missile (3D4+3 force, auto-hit), Sleep (WIS save or unconscious for 2 rounds), Mist Step (bonus action, teleport 20 ft., once per combat). Trance — immune to sleep effects.',
     'An elf in flowing blue robes, arcane sigils glowing along the hem. They speak a word of power and the air itself bends to their will.'),
    ('Humanoid', 'Elven Lord', 9, 2, 3, 1, 2, 2, 3, 40, 17,
     '+8, 1D10+3 (elven great sword) or +9, 1D8+3 (elven long bow, 100 ft. ranged)',
     'Magic Resistance — advantage on saves vs. magical effects. Trance — immune to sleep effects. Bladesong — once per combat, enter a defensive stance for 3 rounds: +2 AC, -2 attack. May still attack and cast spells. Fey Step — as a bonus action, teleport up to 30 ft. (recharges on short rest).',
     'A regal elf in gleaming mithril chain, a blade of pale blue flame held at rest. They have seen empires rise and fall, and your arrival is merely the latest verse in a song that has played for millennia.'),
    ('Humanoid', 'Lesser Lizard', 3, 2, 1, 1, 0, 0, -1, 16, 11,
     '+2, 1D6+2 (stone club)',
     'Poison Immunity — immune to poison damage and the poisoned condition. Hold Breath — can hold its breath for 30 minutes.',
     'A lizardfolk in primitive hides, carrying a club studded with sharp stones. Its tongue flicks out, tasting the air for prey.'),
    ('Humanoid', 'Lizard Warrior', 5, 2, 1, 2, 0, 1, 0, 28, 13,
     '+4, 1D8+2 (bone spear) or +4, 1D6+1 (javelin, 30 ft. ranged)',
     'Poison Immunity — immune to poison damage and the poisoned condition. Hold Breath — can hold its breath for 30 minutes. Natural Armour — thick scales grant base AC 7 unarmoured.',
     'A lizardfolk warrior wearing a breastplate of giant insect carapace. It moves with the fluid grace of a predator, spear held ready.'),
    ('Humanoid', 'Greater Lizard', 8, 2, 1, 2, 1, 3, 1, 40, 14,
     '+5, 1D8+2 (bone staff)',
     'Poison Immunity — immune to poison damage and the poisoned condition. Hold Breath — can hold its breath for 30 minutes. Natural Armour — thick scales grant base AC 6 unarmoured. Primal Magic — Venom Spray (once per combat, 15 ft. cone, 2D6 poison, save for half) or Regenerate (heals 2D8 HP once per combat).',
     'A lizardfolk draped in feathers and ritual scars, its eyes a reptilian gold. It chants to the old gods of scale and sun, and the swamp answers.'),
    ('Humanoid', 'Bandit', 1, 1, 1, 0, 0, 0, 0, 8, 10,
     '+1, 1D6 (short sword) or +1, 1D4 (sling, ranged)',
     'None.',
     'A rough-looking man in stained leathers. He holds a blade with the shaky confidence of someone who has only ever used it on travellers weaker than himself.'),
    ('Humanoid', 'Mercenary', 3, 1, 1, 1, 0, 0, 0, 18, 12,
     '+3, 1D8+1 (long sword) or +3, 1D6+1 (crossbow, ranged)',
     'Shield Wall — if adjacent to another Mercenary, both gain +1 AC.',
     'A scarred professional in chainmail and a battered helm. He fights for coin and has seen enough battle to know which end of the sword points toward the enemy.'),
    ('Humanoid', 'Knight', 6, 2, 1, 2, 0, 1, 1, 38, 16,
     '+5, 1D10+2 (great sword) or +4, 1D8+1 (lance, mounted)',
     'Heavy Armour — damage reduction 2 vs. non-magical weapons. Shield Bash — bonus action: 1D4+2 bludgeoning, target pushed 5 ft. on hit.',
     'A knight in gleaming full plate, a heraldic tabard over their armour. They sit a warhorse with practised ease and lower their lance.'),
    ('Humanoid', 'Captain', 9, 3, 2, 2, 2, 1, 2, 55, 17,
     '+8, 1D10+3 (bastard sword, two-handed) or +7, 1D8+1 (heavy crossbow, ranged)',
     'Heavy Armour — damage reduction 2 vs. non-magical weapons. Command — once per combat, an ally within 20 ft. may make an immediate attack out of turn. Inspiring Presence — all allies within 30 ft. gain +1 to hit and +1 to saves. Second Wind — bonus action to heal 2D10+5 HP, once per combat.',
     'A battle-hardened commander in ornate plate, a cloak of office pinned at the shoulder. Knights and soldiers snap to attention in their presence.'),
    ('Humanoid', 'Dwarf Warrior', 3, 2, 0, 2, 0, 1, 0, 22, 12,
     '+3, 1D8+2 (battle axe) or +3, 1D6+1 (throwing axe, 20 ft. ranged)',
     'Magic Resistance — advantage on saves vs. magical effects.',
     'A dwarf in chainmail, a thick beard braided with iron rings. He plants his feet and dares the enemy to move him.'),
    ('Humanoid', 'Dwarf Berserker', 5, 3, 0, 3, 0, 1, -1, 38, 13,
     '+5, 1D10+3 (greataxe)',
     'Magic Resistance — advantage on saves vs. magical effects. Berserker Rage — once per combat, enter a rage: +2 damage, damage reduction 2, but -2 AC for 3 rounds.',
     'A wild-eyed dwarf in scale mail, foam gathering at the corners of his mouth. He swings his axe in great arcs, caring nothing for his own safety.'),
    ('Humanoid', 'Dwarf Lord', 8, 3, 1, 3, 1, 2, 1, 58, 16,
     '+7, 1D10+3 (dwarven war hammer) or +6, 1D6+2 (throwing hammer, 20 ft. ranged)',
     'Magic Resistance — advantage on saves vs. magical effects. Heavy Armour — damage reduction 2 vs. non-magical weapons. Ancestral Ward — once per combat, reduce all damage taken by 5 for 2 rounds. Grudge — +1 to hit against any creature that damaged an ally in the previous round.',
     'A dwarf lord in gromril plate, a crown of mithril set upon his helm. He carries a rune-etched hammer that has served his clan for five centuries.'),
    ('Humanoid', 'Gladefolk Prowler', 2, 0, 2, 1, 0, 1, 1, 10, 11,
     '+2, 1D4+1 (short blade) or +3, 1D4+1 (sling, ranged)',
     'Taunt — bonus action: one enemy within 20 ft. makes a WIS save or has disadvantage on attacks against creatures other than the Gladefolk for 1 round. Fear Immunity — immune to fear effects.',
     'A quick-eyed Gladefolk in leather armour, a sling dangling from one hand. They whistle a cheerful tune while eyeing your vulnerable spots.'),
    ('Humanoid', 'Gladefolk Slinger', 4, 0, 3, 1, 1, 1, 1, 16, 13,
     '+4, 1D4+1 (short blade) or +5, 1D6+1 (sling, 40 ft. ranged)',
     'Taunt — bonus action: one enemy within 20 ft. makes a WIS save or has disadvantage on attacks against creatures other than the Gladefolk for 1 round. Fear Immunity — immune to fear effects. Nimble — may move through spaces occupied by larger creatures without penalty. Lucky — once per combat, reroll a missed attack.',
     'A Gladefolk in studded leather, a bandoleer of sling bullets across their chest. They move through the battlefield like a leaf on the wind, landing shots from impossible angles.'),
    ('Humanoid', 'Gladefolk Elder', 6, 0, 3, 2, 1, 2, 2, 28, 15,
     '+6, 1D6+1 (magic short sword) or +7, 1D6+2 (sling, 50 ft. ranged)',
     'Taunt — bonus action: one enemy within 30 ft. makes a WIS save or has disadvantage on attacks against creatures other than the Gladefolk for 1 round. Fear Immunity — immune to fear effects. Nimble — may move through spaces occupied by larger creatures without penalty. Lucky — once per combat, reroll a missed attack or failed save. Inspire — once per combat, all Gladefolk allies within 30 ft. gain +1 to hit and +2 damage for 2 rounds.',
     'A grey-haired Gladefolk in a travelling cloak, their face lined with a hundred smiles and a thousand battles. The younger Gladefolk watch them for the signal to strike.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 2. BEASTS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Beast', 'Giant Rat', 1, -1, 2, 0, -3, -1, -3, 5, 9,
     '+1, 1D4 (bite)',
     'Disease — on hit, target makes a CON save or contracts Filth Fever (1D4 poison damage each turn for 3 turns).',
     'A rat the size of a dog, its fur matted with filth. Its red eyes gleam with hunger as it bares yellow teeth.'),
    ('Beast', 'Wolf', 2, 1, 2, 1, -2, 1, -2, 12, 12,
     '+2, 1D6+1 (bite)',
     'Pack Tactics — +1 to hit when an ally is within 5 ft. of the target. Trip — on hit, target makes a DEX save or is knocked Prone.',
     'A grey wolf with pale eyes, padding silently through the undergrowth. It does not bark or growl — it simply appears when it is too late.'),
    ('Beast', 'Giant Spider', 3, 1, 2, 1, -2, 0, -3, 14, 12,
     '+3, 1D6+1 (bite) + poison',
     'Poison — on hit, target makes a CON save or takes 1D6 poison and is Poisoned for 2 rounds. Web — once per combat, 20 ft. ranged: target is Restrained (DEX save to escape). Climb — can move on walls and ceilings without penalty.',
     'A hairy arachnid the size of a pony, its mandibles dripping with pale venom. It descends silently from above.'),
    ('Beast', 'Bear', 4, 4, 1, 3, -2, 1, -2, 30, 12,
     '+4, 1D8+4 (claw/claw) or +4, 2D6+4 (bite)',
     'Grab — if both claws hit the same target, the bear automatically bites as a bonus action. Rage — when below 50 % HP, gains +2 damage.',
     'A massive brown bear, shoulders rippling with muscle, standing on hind legs to tower over its prey. Its roar echoes through the forest.'),
    ('Beast', 'Dire Wolf', 4, 2, 2, 2, -2, 1, -2, 22, 13,
     '+4, 1D10+2 (bite)',
     'Pack Tactics — +1 to hit when an ally is within 5 ft. of the target. Trip — on hit, target makes a DEX save or is knocked Prone. Fearsome Howl — once per combat, all enemies within 30 ft. make a WIS save or are Frightened for 1 round.',
     'The size of a small bear, this wolf has eyes that gleam with cruel intelligence. Its howl freezes the blood of even seasoned hunters.'),
    ('Beast', 'Great Stag', 6, 2, 3, 2, 2, 2, 1, 35, 14,
     '+6, 1D8+2 (antlers) or +5, 1D10+2 (hooves, charge)',
     'Charge — if the stag moves at least 20 ft. before attacking, the hooves attack deals +2D6 damage and knocks the target Prone. Fey Step — once per combat, teleport up to 30 ft. as a bonus action. Forest Walk — cannot be slowed or hindered by natural terrain.',
     'A stag with antlers of gleaming silver, its coat white as winter snow. It moves through the trees as if the forest itself parts to welcome it.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 3. MONSTROSITIES
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Monstrosity', 'Lesser Ogre', 5, 3, 0, 2, -2, -1, -1, 35, 12,
     '+4, 2D6+3 (huge club)',
     'Extra Strength — +2 to melee damage rolls (included). Massive — cannot be knocked prone by creatures smaller than Large.',
     'A young ogre, still growing into its strength. It swings a tree trunk like a bat and seems confused by anything smaller than its fist.'),
    ('Monstrosity', 'Ogre', 7, 4, 0, 3, -2, -1, -1, 50, 14,
     '+6, 2D8+4 (greatclub)',
     'Extra Strength — +2 to melee damage rolls (included). Magic Resistance — advantage on saves vs. magical effects. Massive — cannot be knocked prone by creatures smaller than Large.',
     'A full-grown ogre in patchwork plate armour stolen from fallen knights. It grins with broken teeth and cracks its knuckles.'),
    ('Monstrosity', 'Greater Ogre', 10, 5, 0, 4, -1, 0, 0, 80, 16,
     '+8, 2D10+5 (iron-bound club) or +6, 2D8+5 (boulder toss, 60 ft. ranged)',
     'Extra Strength — +2 to melee damage rolls (included). Magic Resistance — advantage on saves vs. magical effects. Massive — cannot be knocked prone by creatures smaller than Large. Earth Shake — once per combat, slam the ground: all enemies within 20 ft. make a DEX save or are knocked Prone. Enrage — when first reduced to 50 % HP, gains +2 damage for the rest of combat.',
     'A mountain of muscle and scar tissue, draped in the hides of beasts and the banners of fallen armies. It leads its tribe with an iron fist — literally.'),
    ('Monstrosity', 'Forest Troll', 7, 3, 1, 3, -1, 0, -2, 45, 14,
     '+5, 1D8+3 (claw/claw) or +5, 1D10+3 (bite)',
     'Regeneration — regains 5 HP at the start of its turn unless the damage was fire or acid. Can reattach severed limbs. Keen Scent — advantage on tracking and perception checks involving smell.',
     'A gnarled, green-skinned brute with long stringy hair and claws like scythes. It sniffs the air and lumbers forward, already healing from its last wound.'),
    ('Monstrosity', 'Cave Troll', 9, 4, 1, 4, -1, 0, -2, 65, 15,
     '+7, 2D6+4 (claw/claw) or +7, 1D12+4 (bite)',
     'Regeneration — regains 8 HP at the start of its turn unless the damage was fire or acid. Darkvision — sees in total darkness. Rending Claws — if both claw attacks hit the same target, deal an additional 2D6 slashing damage.',
     'A pale, eyeless troll that has never known sunlight. Its skin is slick with cave slime, and its claws are yellowed and long.'),
    ('Monstrosity', 'Mountain Troll', 10, 5, 0, 5, -1, 1, -2, 80, 16,
     '+8, 2D8+5 (rocky fists)',
     'Regeneration — regains 10 HP at the start of its turn unless the damage was fire or acid. Stone Hide — AC 4 unarmoured; damage reduction 2 vs. non-magical weapons. Earth Shake — once per combat, slam the ground: all enemies within 15 ft. make a DEX save or are knocked Prone. Rock Throw — +6, 2D6+5 (boulder, 50 ft. ranged).',
     'A troll that has lived so long its skin has turned to grey stone. Moss grows in the cracks of its hide. It moves with the inevitability of an avalanche.'),
    ('Monstrosity', 'Hill Giant', 10, 5, 0, 4, -2, 0, -1, 75, 16,
     '+8, 2D10+5 (giant club) or +6, 2D8+5 (boulder, 80 ft. ranged)',
     'Massive — cannot be knocked prone by creatures smaller than Huge. Crushing Blow — on a critical hit, target is knocked Prone and Stunned for 1 round.',
     'A hulking brute wrapped in furs, its head scraping the ceiling of any hall. It carries a tree trunk studded with iron spikes and laughs at the puny weapons that bounce off its hide.'),
    ('Monstrosity', 'Stone Giant', 11, 5, 1, 5, 0, 1, 0, 90, 18,
     '+9, 2D12+5 (stone greatclub) or +8, 3D8+5 (boulder, 100 ft. ranged)',
     'Massive — cannot be knocked prone by creatures smaller than Huge. Stone Skin — damage reduction 3 vs. non-magical weapons. Crushing Blow — on a critical hit, target is knocked Prone and Stunned for 1 round.',
     'A giant carved from the mountain itself, its skin a mosaic of granite and quartz. It moves deliberately, each step shaking the earth.'),
    ('Monstrosity', 'Frost Giant', 12, 6, 0, 5, 1, 1, 1, 105, 20,
     '+10, 2D12+6 (greataxe) or +8, 3D10+6 (ice boulder, 100 ft. ranged)',
     'Massive — cannot be knocked prone by creatures smaller than Huge. Icy Aura — any creature within 10 ft. at the start of its turn takes 1D6 cold damage. Crushing Blow — on a critical hit, target is knocked Prone and Stunned for 1 round. Freezing Roar — once per combat, all enemies within 30 ft. make a CON save or take 3D8 cold damage (half on save).',
     'A blue-skinned giant in plate armour of ice and iron, its beard crusted with frost. Its breath freezes the air mid-exhale.'),
    ('Monstrosity', 'Minotaur', 7, 4, 1, 3, -1, 0, -1, 52, 14,
     '+6, 2D8+4 (greataxe) or +6, 1D6+4 (gore)',
     'Charge — if the minotaur moves at least 20 ft. before attacking, the gore attack deals +2D6 damage and knocks the target Prone. Labyrinth Memory — cannot be lost or confused by maze-like terrain. Reckless — the minotaur may attack with advantage, but all attacks against it also have advantage until its next turn.',
     'A hulking brute with the head of a bull and the body of a giant. It breathes in wet snorts, its brass nose-ring clinking as it paws the ground, and lowers its horns.'),
    ('Monstrosity', 'Harpy', 5, 0, 3, 1, 0, 1, 0, 20, 13,
     '+5, 1D6+2 (talons) or +5, 1D4+2 (club)',
     'Fly — 50 ft. flying speed. Captivating Song — once per combat, all enemies within 60 ft. make a WIS save or spend their next turn moving toward the harpy (cannot attack, takes opportunity attacks as normal).',
     'A woman''s face twists in hunger above a vulture''s body. Its feathers are caked with old blood, and its song is beautiful in the same way a snake''s hiss is beautiful — because you know what comes next.'),
    ('Monstrosity', 'Basilisk', 9, 3, 1, 3, -1, 2, -2, 60, 16,
     '+7, 2D8+3 (bite)',
     'Petrifying Gaze — once per combat, all enemies within 30 ft. who meet its gaze make a CON save. On failure, they are Restrained and begin turning to stone. If already Restrained by this effect, they are fully Petrified (unconscious, cannot be revived without Stone to Flesh). Poison Bite — CON save or take 2D6 poison and be Poisoned for 3 rounds. Stony Hide — damage reduction 2 vs. non-magical weapons.',
     'A serpent the size of an oak tree, its scales the colour of weathered limestone. Eight legs scrape against the ground, and its eyes glow with a pale, hungry light — the last thing many a hero ever saw.'),
    ('Monstrosity', 'Chimera', 11, 5, 2, 4, -1, 1, 0, 85, 18,
     '+9, 1D12+5 (lion bite) or +9, 1D10+5 (goat horns) or +8, 3D8 (dragon breath, 30 ft. cone)',
     'Multiattack — the chimera attacks with its lion bite, goat horns, and dragon head in the same turn. Fire Breath — once per combat, 30 ft. cone: 3D8 fire damage (CON save for half). The dragon head may use this in place of its attack. Fly — 60 ft. flying speed.',
     'A lion''s mane, a goat''s horns, a dragon''s serpentine neck — stitched together by some primordial chaos into a single, roaring nightmare. It lands on a rooftop and all three heads sniff the air for prey.'),
    ('Monstrosity', 'Centaur Archer', 5, 2, 3, 2, 0, 1, 0, 28, 13,
     '+6, 1D8+3 (long bow, 80 ft. ranged) or +5, 1D6+2 (hooves)',
     'Skirmish — the centaur may move up to its speed and still make a ranged attack at no penalty. Charge — if the centaur moves at least 20 ft. before attacking with hooves, it deals +2D6 damage and knocks the target Prone.',
     'A creature of muscle and madness — the torso of a man, the body of a stallion. It gallops along the ridge, arrows already in flight, laughing with the wild joy of the hunt.'),
    ('Monstrosity', 'Centaur Chieftain', 8, 3, 3, 3, 1, 2, 1, 45, 15,
     '+7, 1D10+3 (great bow, 100 ft. ranged) or +7, 2D6+3 (hooves, charge)',
     'Skirmish — the centaur may move up to its speed and still make a ranged attack at no penalty. Charge — if the centaur moves at least 20 ft. before attacking with hooves, it deals +2D6 damage and knocks the target Prone. Herd Leader — once per combat, all centaur allies within 30 ft. gain +1 to hit and +1 to AC for 2 rounds. Trample — as a bonus action, the centaur may move through an enemy''s space, dealing 2D4+3 bludgeoning damage (DEX save for half).',
     'A centaur elder with scars across its flanks and a bow as tall as a man. The herd moves as one when he signals, and woe betide the formation that stands in their way.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 4. UNDEAD
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Undead', 'Skeleton', 1, 0, 1, 0, -2, -1, -2, 8, 12,
     '+1, 1D6 (rusted long sword) or +1, 1D4 (short bow, ranged)',
     'Fear Immunity — immune to fear effects. Fragile Bones — bludgeoning attacks deal +1 damage.',
     'The bones of a long-dead soldier, animated by dark will. Its empty eye sockets burn with cold blue light.'),
    ('Undead', 'Zombie', 2, 2, -1, 2, -3, -2, -2, 22, 10,
     '+2, 1D6+2 (slam)',
     'Fear Immunity — immune to fear effects. Undead Fortitude — when reduced to 0 HP, make a CON save (DC = damage taken). On success, drop to 1 HP instead. Slow — cannot take opportunity attacks.',
     'A shambling corpse with grey flesh hanging in strips. It groans and reaches with broken fingers, feeling nothing.'),
    ('Undead', 'Ghoul', 3, 1, 2, 1, -1, 0, -2, 15, 11,
     '+3, 1D6+1 (claw)',
     'Fear Immunity — immune to fear effects. Paralyzing Touch — on hit, target makes a CON save or is Paralyzed for 1 round. Carrion Hunger — the ghoul may use a bonus action to bite a Paralyzed or Prone target: 1D4+1 piercing + 1D4 necrotic.',
     'A wiry, hunched figure with sallow skin stretched tight over a skull that grins too wide. Its fingers end in yellowed claws caked with grave dirt.'),
    ('Undead', 'Draugr', 6, 3, 0, 2, 0, 0, -1, 40, 14,
     '+5, 1D10+3 (rusted great axe)',
     'Fear Immunity — immune to fear effects. Undead Fortitude — when reduced to 0 HP, make a CON save (DC = damage taken). On success, drop to 1 HP instead. Rotten Flesh — on hit, target makes a CON save or takes 1D4 necrotic and is Poisoned for 1 round. Burial Curse — while the draugr''s burial mound remains unplundered, it returns 1D4 days after being destroyed.',
     'A bloated warrior in corroded chainmail, its skin green-black and bloated. It rises from a burial mound, gripping the axe it was buried with, and fixes you with eyes that hold all the cold of the grave.'),
    ('Undead', 'Wight', 5, 2, 1, 1, 1, 1, 0, 32, 14,
     '+4, 1D8+2 (long sword, necrotic-infused)',
     'Fear Immunity — immune to fear effects. Life Drain — on hit, target loses 1 Max HP until next rest. If a creature is reduced to 0 Max HP by this, it rises as a Zombie under the wight''s control. Cause Fear — once per combat, all enemies within 15 ft. make a WIS save or are Frightened for 2 rounds.',
     'A hollow-cheeked warrior in tarnished plate armour, its eyes twin points of red hate. The air around it grows cold.'),
    ('Undead', 'Mummy', 9, 3, 0, 3, 1, 2, 1, 55, 16,
     '+7, 1D8+3 (rotting fist)',
     'Fear Immunity — immune to fear effects. Mummy Rot — on hit, target is cursed with Mummy Rot. At the start of each of its turns, it takes 1D6 necrotic and its Max HP is reduced by the same amount. The curse ends on a Remove Curse or similar magic. Fear Aura — once per combat, all enemies within 30 ft. make a WIS save or are Frightened for 3 rounds. Vulnerability to Fire — fire damage bypasses damage reduction and deals double damage. Undead Fortitude — when reduced to 0 HP, make a CON save (DC = damage taken). On success, drop to 1 HP instead.',
     'Linen wraps cover a desiccated form that once stood in the court of a pharaoh. It moves with a dry rustling sound, and the air fills with the scent of myrrh, dust, and eternity.'),
    ('Undead', 'Lich', 12, 1, 1, 2, 4, 3, 2, 85, 20,
     '+9, 2D10+3 (necrotic blast, 60 ft. ranged)',
     'Fear Immunity — immune to fear effects. Magic Resistance — advantage on saves vs. magical effects. Cause Fear — once per combat, all enemies within 30 ft. make a WIS save or are Frightened for 3 rounds. Paralyzing Touch — as an action, target makes a CON save or is Paralyzed for 1 round. Rejuvenation — unless its phylactery is destroyed, the lich reforms in 1D10 days.',
     'A crown of bone, a robe of starlit black, and eyes like twin furnaces. The lich has cheated death so long it has forgotten what life felt like.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 5. SPIRITS & GHOSTS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Spirit', 'Shade', 3, -1, 2, 0, 0, 1, -1, 10, 10,
     '+2, 1D6 (chill touch)',
     'Incorporeal — takes half damage from non-magical weapons; immune to non-magical bludgeoning/piercing/slashing. Fear Immunity — immune to fear effects. Sunlight Sensitivity — while in direct sunlight, the shade''s attacks have disadvantage.',
     'A formless silhouette against the darkness, little more than a smudge of shadow with two dim pinpricks where eyes should be. It reaches for you with a hand that passes through solid stone.'),
    ('Spirit', 'Poltergeist', 5, 0, 2, 0, 0, 1, 0, 18, 12,
     '+4, 1D8 (telekinetic blast, 30 ft. ranged)',
     'Incorporeal — takes half damage from non-magical weapons; immune to non-magical bludgeoning/piercing/slashing. Invisible — the poltergeist is naturally invisible. It can be detected by effects that reveal invisible creatures, or by observing the objects it hurls. Telekinetic Throw — as a bonus action, the poltergeist hurls a loose object (crate, chair, rock) at a target: +4, 1D6 bludgeoning.',
     'The air grows cold. A chair scrapes across the floor by itself. A vase lifts, rotates slowly, and hurls itself at your head. There is no visible attacker — only the violent evidence of a hatred that outlived its owner.'),
    ('Spirit', 'Will-o''-Wisp', 4, -2, 3, 0, 0, 1, 1, 10, 9,
     '+5, 2D4 (shock, 30 ft. ranged)',
     'Invisible in Darkness — the wisp is invisible while in dim light or darkness. Light Form — cannot be grappled, restrained, or knocked prone. Lead Astray — once per combat, one enemy within 60 ft. makes a WIS save or moves up to its speed toward the wisp (provoking opportunity attacks).',
     'A bobbing orb of pale blue light, no larger than a candle flame. It dances at the edge of vision, inviting pursuit into the marsh where the ground grows soft and the reeds hide cold water.'),
    ('Spirit', 'Banshee', 8, 0, 2, 0, 1, 2, 3, 30, 15,
     '+6, 1D10 (necrotic touch)',
     'Incorporeal — takes half damage from non-magical weapons; immune to non-magical bludgeoning/piercing/slashing. Fear Immunity — immune to fear effects. Wail — once per combat, all enemies within 40 ft. make a WIS save. On failure, they take 3D8 psychic damage and are Frightened for 3 rounds. On success, they take half damage and are not Frightened.',
     'A woman in a flowing grey shroud, her face a mask of eternal grief. She hovers above the ground, and when she opens her mouth to wail, the sound is not a scream — it is the sound of every loss you have ever suffered, given voice.'),
    ('Spirit', 'Wraith', 8, 0, 3, 0, 2, 2, 1, 45, 16,
     '+6, 1D10+2 (life siphon, touch)',
     'Fear Immunity — immune to fear effects. Incorporeal — takes half damage from non-magical weapons; immune to non-magical bludgeoning/piercing/slashing. Life Siphon — on hit, regain HP equal to half the damage dealt. Stun — as an action, target makes a CON save or is Stunned for 1 round.',
     'A shapeless shadow in the form of a hooded figure, trailing wisps of cold mist. Its touch drains the warmth from living flesh.'),
    ('Spirit', 'Ancestral Guardian', 10, 2, 1, 2, 1, 3, 2, 45, 17,
     '+8, 1D12+2 (spiritual spear) or +7, 2D6 (ancestral blast, 40 ft. ranged)',
     'Incorporeal — takes half damage from non-magical weapons; immune to non-magical bludgeoning/piercing/slashing. Fear Immunity — immune to fear effects. Guardian Ward — as a reaction when an ally within 20 ft. takes damage, the guardian may reduce that damage by 2D10+5. Ancestral Insight — the guardian may grant one ally advantage on its next attack or save once per combat.',
     'A luminous figure in ancient armour, its features obscured by a glow like moonlight on water. It speaks rarely, in a voice that sounds like many voices layered together — the accumulated wisdom of generations standing guard over the last of its bloodline.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 6. DEMONS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Demon', 'Lesser Demon', 8, 1, 3, 1, 1, 0, 0, 28, 15,
     '+6, 1D6+1 (claw) or +7, 2D4 (hellflame bolt, 40 ft. ranged)',
     'Cause Fear — once per combat, one enemy within 20 ft. makes a WIS save or is Frightened for 2 rounds. Fire Aura — any creature that starts its turn within 5 ft. takes 1D4 fire damage. Teleport — as a bonus action, teleport up to 30 ft. to a visible location.',
     'A small, winged creature of boiling flame and malice. It cackles as it hurls bolts of hellfire, dancing between dimensions.'),
    ('Demon', 'Demon', 10, 3, 2, 3, 2, 1, 1, 55, 17,
     '+8, 1D10+3 (fiery claws) or +7, 2D8 (hellflame bolt, 60 ft. ranged)',
     'Cause Fear — once per combat, all enemies within 20 ft. make a WIS save or are Frightened for 2 rounds. Fire Aura — any creature that starts its turn within 5 ft. takes 1D6 fire damage. Stun — as an action, target makes a CON save or is Stunned for 1 round. Teleport — as a bonus action, teleport up to 30 ft. to a visible location.',
     'A horned figure wreathed in black smoke and embers. Its eyes are pits of molten hate, and the ground cracks beneath its feet.'),
    ('Demon', 'Oni', 10, 5, 1, 4, 1, 1, 0, 70, 17,
     '+9, 2D10+5 (massive club, 2H) or +7, 2D8+5 (chunk of rubble, 40 ft. ranged)',
     'Cause Fear — once per combat, all enemies within 20 ft. make a WIS save or are Frightened for 2 rounds. Regeneration — regains 5 HP at the start of its turn (fire and acid suppress this for 1 round). Shapechange — as an action, the oni may appear as any Medium humanoid (no mechanical effect, but can fool observers). Darkvision — sees in magical darkness.',
     'A massive, blue-skinned giant with a single horn jutting from its forehead. It wears a tiger-skin loincloth and carries a spiked club the size of a young tree, and it grins with too many teeth.'),
    ('Demon', 'Greater Demon', 12, 4, 3, 4, 3, 2, 3, 85, 20,
     '+10, 2D10+4 (shadow claws) or +9, 3D8 (shadow bolt, 80 ft. ranged)',
     'Cause Fear — once per combat, all enemies within 30 ft. make a WIS save or are Frightened for 3 rounds. Stun — as an action, target makes a CON save or is Stunned for 2 rounds. Shadow Aura — any creature within 10 ft. at the start of its turn takes 1D8 necrotic damage. Teleport — as a bonus action, teleport up to 40 ft. to a visible location. Summon Demon — once per combat, summon a Lesser Demon that acts immediately. Shadow Form — takes half damage from non-magical weapons.',
     'A towering demon of living shadow, crowned with horns of obsidian. It speaks in whispers that echo in the skull long after the words fade.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 7. CONSTRUCTS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Construct', 'Clay Golem', 8, 4, -2, 4, -3, -2, -3, 50, 16,
     '+6, 1D10+4 (slam)',
     'Magic Immunity — immune to all spells that allow spell resistance; Disintegrate slows it for 1 round, Earthquake heals it 2D8. Berserk — when it drops below 25 HP, make a DC 10 check each turn. On failure, it attacks the nearest creature (friend or foe) for 1 round. Construct Traits — immune to fear, poison, sleep, paralysis, stun.',
     'A humanoid figure of baked clay, nine feet tall, with eyes of polished jet. It moves with the grinding sound of stone on stone.'),
    ('Construct', 'Iron Golem', 10, 5, -1, 5, -3, -2, -3, 70, 18,
     '+8, 2D8+5 (slam) or +7, 2D6+5 (poison gas breath, 15 ft. cone, once per combat)',
     'Magic Immunity — immune to all spells that allow spell resistance; fire spells heal it 1D8 per level, lightning slows it for 1 round. Construct Traits — immune to fear, poison, sleep, paralysis, stun. Poison Gas Breath — CON save or take 2D6 poison and be Poisoned for 2 rounds.',
     'A massive iron statue in the shape of an armoured knight, steam venting from the joints of its jaw. Each footfall rings like a hammer on an anvil.'),
    ('Construct', 'Adamantite Golem', 12, 6, -1, 6, -3, -2, -3, 100, 21,
     '+10, 2D10+6 (slam) or +9, 3D8 (heat beam, 60 ft. ranged, once per combat)',
     'Magic Immunity — immune to all spells that allow spell resistance. Construct Traits — immune to fear, poison, sleep, paralysis, stun. Adamantite Skin — damage reduction 5 vs. all sources. Heat Beam — target takes 3D8 fire damage; save for half. Ignites flammable materials. Crushing Blow — on a critical hit with slam, the target is knocked Prone and Stunned for 2 rounds.',
     'A golem of impossibly hard black metal, its surface etched with glowing runes of binding. It has no expression, no weakness, no mercy. It simply advances, and things die.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 8. DRAGONS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Dragon', 'Wyrmling', 10, 3, 2, 3, 2, 2, 2, 60, 18,
     '+8, 1D10+3 (claw/claw) or +8, 2D8+3 (bite)',
     'Breath Weapon — once per combat, 20 ft. cone (or 40 ft. line) of elemental damage. Targets take 4D8 (CON save for half). Frightful Presence — once per combat, all enemies within 30 ft. make a WIS save or are Frightened for 2 rounds. Fly — 60 ft. flying speed. Elemental Resistance — resistance to its own element type.',
     'A young dragon, its scales still bright and unmarred. It is curious, proud, and utterly lethal — a predator that has never known defeat.'),
    ('Dragon', 'Adult Dragon', 12, 5, 2, 5, 3, 3, 4, 100, 22,
     '+10, 2D10+5 (claw/claw) or +10, 3D10+5 (bite)',
     'Breath Weapon — once per combat, 40 ft. cone (or 60 ft. line) of elemental damage. Targets take 6D8 (CON save for half). Frightful Presence — once per combat, all enemies within 60 ft. make a WIS save or are Frightened for 3 rounds. Fly — 80 ft. flying speed. Elemental Immunity — immune to its own element type. Crush — when landing on a Huge or smaller creature, target takes 3D10 bludgeoning and is knocked Prone. Spellcasting — knows 3 spells appropriate to its element (e.g., red dragon knows Fireball, Burning Hands, Heat Metal).',
     'A dragon that has terrorised kingdoms for centuries. Its scales are scarred by the weapons of a thousand would-be heroes who now decorate its hoard.'),
    ('Dragon', 'Ancient Dragon', 12, 7, 2, 6, 5, 4, 5, 150, 24,
     '+12, 3D10+7 (claw/claw) or +12, 4D12+7 (bite)',
     'Breath Weapon — once per combat, 60 ft. cone (or 80 ft. line) of elemental devastation. Targets take 8D12 (CON save for half). Frightful Presence — every enemy within 120 ft. makes a WIS save or is Frightened for 5 rounds. Fly — 100 ft. flying speed. Elemental Immunity — immune to its own element type. Crush — when landing on a Huge or smaller creature, target takes 4D10 bludgeoning and is knocked Prone. Spellcasting — knows 6 spells appropriate to its element. Legendary Resistance — three times per combat, may choose to succeed on a failed saving throw. Tail Swipe — as a bonus action, all creatures within 15 ft. behind the dragon make a DEX save or take 3D8 bludgeoning and are knocked Prone.',
     'A living calamity. Its wings blot out the sun, its roar cracks stone, and its hoard contains the melted-down armour of every hero who thought they could kill it.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 9. CELESTIALS
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Celestial', 'Valkyrie', 11, 4, 3, 3, 2, 3, 4, 65, 19,
     '+10, 1D12+4 (spear) or +10, 2D6+3 (long sword)',
     'Fly — 60 ft. flying speed. Battle Blessing — once per combat, the valkyrie may grant one ally advantage on all attacks and saves for 1 round. Choose the Fallen — when the valkyrie reduces a creature to 0 HP, she regains 2D10 HP. Magic Resistance — advantage on saves vs. magical effects. Radiant Aura — any Undead or Demon within 10 ft. at the start of its turn takes 1D8 radiant damage.',
     'Armoured in silver and white, with wings of pale light folded behind her shoulders, the valkyrie descends like a falling star. Her spear crackles with power, and her eyes hold no mercy — only purpose.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 10. FEY
-- ============================================================

INSERT INTO arena_data.bestiary (category, name, level, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, max_hit_points, armor_class, attack_description, special_abilities, description) VALUES
    ('Fey', 'Leshy', 7, 2, 2, 2, 1, 3, 1, 35, 14,
     '+6, 1D8+2 (gnarled staff) or +7, 1D6+2 (thorn volley, 40 ft. ranged)',
     'Forest Walk — cannot be slowed or hindered by natural terrain; moves through undergrowth without penalty. Shapechange — as an action, the leshy may appear as a gnarled tree stump or a large forest animal (wolf, stag, boar). Entangle — once per combat, all enemies in a 20 ft. radius make a DEX save or are Restrained by vines and roots for 2 rounds. Fey Step — as a bonus action, teleport up to 30 ft. through living vegetation.',
     'The forest does not part for the leshy — it is the forest. Bark and moss grow across its form in place of skin, and its eyes are the colour of deep woodland pools. When it speaks, it sounds like wind through birch leaves.'),
    ('Fey', 'Kelpie', 6, 3, 2, 2, 0, 1, 2, 32, 14,
     '+6, 1D8+3 (hooves) or +6, 1D6+3 (bite)',
     'Shapechange — the kelpie appears as a magnificent horse (grey or black, with a shimmering coat and wild eyes). It may revert to its true form (a slimy, skeletal horse-creature) as a bonus action. Drown — if the kelpie hits the same target with both hooves and bite in one turn, it grapples the target and drags them underwater. The target must hold its breath or begin suffocating. Water Walk — the kelpie can move across water as though it were solid ground. Charge — if the kelpie moves at least 20 ft. before attacking with hooves, it deals +2D6 damage and knocks the target Prone.',
     'A magnificent black horse stands at the water''s edge, its coat gleaming like oil on a river. Its saddle is made of woven reeds, and its eyes — only those who look closely see the cold hunger swimming in them.')
ON CONFLICT (name) DO NOTHING;
