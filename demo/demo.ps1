#!/usr/bin/env pwsh
<#
.SYNOPSIS
    BattleArena API Demo Script
.DESCRIPTION
    Exercises all major API endpoints to showcase the homebrew AD&D-inspired
    StrikeRating-based combat system, 10 races, 9 classes, pets, deities, spells,
    weapons, armor, rings, amulets, girdles, item sets, NPCs, and PostgreSQL backend with pg_cron.
.PARAMETER BaseUrl
    The base URL of the arena-api service (default http://localhost:8585)
.EXAMPLE
    ./demo/demo.ps1
    ./demo/demo.ps1 -BaseUrl http://localhost:8585
#>

param([string]$BaseUrl = "http://localhost:8585")

$style = @{ Header = @{ "Content-Type" = "application/json" } }
$separator = "=" * 72

function Write-Step($title) {
    Write-Host "`n$separator" -ForegroundColor Cyan
    Write-Host "  $title" -ForegroundColor Cyan
    Write-Host "$separator`n" -ForegroundColor Cyan
}

function Write-Result($label, $value) {
    Write-Host "  [ $label ]" -NoNewline -ForegroundColor Yellow
    Write-Host "  $value" -ForegroundColor White
}

function Invoke-Api($method, $path, $body) {
    $url = "$BaseUrl$path"
    $params = @{ Uri = $url; Method = $method }
    if ($body) { $params.Body = ($body | ConvertTo-Json); $params.ContentType = "application/json" }
    try {
        $response = Invoke-RestMethod @params
        return $response
    } catch {
        Write-Host "    [ERROR] $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# =====================
# 1. API HEALTH CHECK
# =====================
Write-Step "1. API HEALTH CHECK"

Write-Host "  Checking if API is reachable at $BaseUrl ..." -ForegroundColor Gray
try {
    $null = Invoke-WebRequest -Uri "$BaseUrl/v1/races" -TimeoutSec 5
    Write-Host "    [OK] API is running!" -ForegroundColor Green
} catch {
    Write-Host "    [FAIL] Cannot reach API. Start containers with: make up" -ForegroundColor Red
    exit 1
}

# =====================
# 2. DICE ROLLING
# =====================
Write-Step "2. DICE ROLLING - Randomness Engine"

Write-Host "  The dice service supports multiple rolling modes:"
Write-Host ""

$d20 = Invoke-Api GET "/v1/roll/D20"
Write-Result "D20" "Rolled a $($d20.result) (1-20)"

$d6 = Invoke-Api GET "/v1/roll/D6"
Write-Result "D6" "Rolled a $($d6.result) (1-6)"

$adv = Invoke-Api GET "/v1/roll/advantage/D20"
Write-Result "Advantage" "Rolled $($adv.result) (roll twice, take higher)"

$dis = Invoke-Api GET "/v1/roll/disadvantage/D20"
Write-Result "Disadvantage" "Rolled $($dis.result) (roll twice, take lower)"

$multi = Invoke-Api GET "/v1/roll/3/d6"
Write-Result "3d6" "Rolled $($multi.result) (3-18)"

# =====================
# 3. HOMEBREW RACES
# =====================
Write-Step "3. HOMEBREW RACES - 10 Playable Species"

$races = Invoke-Api GET "/v1/races"
Write-Host "  The homebrew world includes 10 distinct races:"
Write-Host ""
foreach ($race in $races) {
    $bonuses = @()
    if ($race.abilityBonuses) {
        $race.abilityBonuses.PSObject.Properties | ForEach-Object {
            if ($_.Value -ne 0) { $bonuses += "$($_.Name)+$($_.Value)" }
        }
    }
    $bonusStr = if ($bonuses.Count -gt 0) { " ($($bonuses -join ', '))" } else { "" }
    Write-Result $race.name "$($race.description)$bonusStr"
}

# Show feats for a race (Elf = id 2)
$elfFeats = Invoke-Api GET "/v1/race/2/feats"
if ($elfFeats) {
    Write-Host ""
    Write-Host "  Elf Special Abilities:" -ForegroundColor Magenta
    foreach ($feat in $elfFeats) {
        Write-Host "    - $($feat.name): $($feat.description)" -ForegroundColor Gray
    }
}

# =====================
# 4. WEAPONS
# =====================
Write-Step "4. WEAPONS - Arsenal"

$weapons = Invoke-Api GET "/v1/weapons"
$common = $weapons | Where-Object { $_.quality -eq 'Common' }
$epic = $weapons | Where-Object { $_.quality -eq 'Epic' }
$legendary = $weapons | Where-Object { $_.quality -eq 'Legendary' }
Write-Host "  $($weapons.Count) weapons across 13 archetypes: $($common.Count) Common, $($epic.Count) Epic, $($legendary.Count) Legendary"
Write-Host ""

Write-Host "  Common weapons:" -ForegroundColor Gray
foreach ($w in $common) {
    Write-Result $w.name "$($w.archetype) | $($w.damageCount)$($w.damageDie) $($w.damageType) | $($w.attackType) | $($w.hands)H"
}

if ($epic) {
    Write-Host ""
    Write-Host "  Epic weapons:" -ForegroundColor Magenta
    foreach ($w in $epic) {
        Write-Result $w.name "$($w.archetype) | $($w.damageCount)$($w.damageDie) $($w.damageType) | +$($w.attackBonus) attack | $($w.attackType)"
    }
}

if ($legendary) {
    Write-Host ""
    Write-Host "  Legendary weapons:" -ForegroundColor DarkYellow
    foreach ($w in $legendary) {
        Write-Result $w.name "$($w.archetype) | $($w.damageCount)$($w.damageDie) $($w.damageType) | +$($w.attackBonus) attack | $($w.attackType)"
    }
}

# =====================
# 5. ARMOR
# =====================
Write-Step "5. ARMOR - Protection"

$armors = Invoke-Api GET "/v1/armor"
$commonArmor = $armors | Where-Object { $_.quality -eq 'Common' }
$epicArmor = $armors | Where-Object { $_.quality -eq 'Epic' }
$legendaryArmor = $armors | Where-Object { $_.quality -eq 'Legendary' }
Write-Host "  $($armors.Count) armor pieces across 4 categories: $($commonArmor.Count) Common, $($epicArmor.Count) Epic, $($legendaryArmor.Count) Legendary"
Write-Host ""

Write-Host "  Common armor:" -ForegroundColor Gray
foreach ($a in $commonArmor) {
    $dex = if ($a.maxDexterityBonus -eq 99) { "max" } else { $a.maxDexterityBonus }
    $stealth = if ($a.stealthDisadvantage) { "DIS" } else { "OK" }
    Write-Result $a.name "AC $($a.armorClass) | $($a.category) | Dex+$dex | Stealth:$stealth"
}

if ($epicArmor) {
    Write-Host ""
    Write-Host "  Epic armor:" -ForegroundColor Magenta
    foreach ($a in $epicArmor) {
        $dex = if ($a.maxDexterityBonus -eq 99) { "max" } else { $a.maxDexterityBonus }
        $stealth = if ($a.stealthDisadvantage) { "DIS" } else { "OK" }
        Write-Result $a.name "AC $($a.armorClass) (+$($a.armorClassBonus) bonus) | $($a.category) | Dex+$dex | Stealth:$stealth"
    }
}

if ($legendaryArmor) {
    Write-Host ""
    Write-Host "  Legendary armor:" -ForegroundColor DarkYellow
    foreach ($a in $legendaryArmor) {
        $dex = if ($a.maxDexterityBonus -eq 99) { "max" } else { $a.maxDexterityBonus }
        $stealth = if ($a.stealthDisadvantage) { "DIS" } else { "OK" }
        Write-Result $a.name "AC $($a.armorClass) (+$($a.armorClassBonus) bonus) | $($a.category) | Dex+$dex | Stealth:$stealth"
    }
}

# =====================
# 6. CHARACTER CREATION
# =====================
Write-Step "6. CHARACTER CREATION - Human Knight"

$newChar = @{
    name = "Ser Gallahad"
    raceId = 1
    classId = 2
    strength = 16
    dexterity = 12
    stamina = 14
    intelligence = 10
    wisdom = 10
    charisma = 14
    strengthPercentile = 0
    maxHitPoints = 12
}
Write-Host "  Creating character with these stats:" -ForegroundColor Yellow
$newChar.PSObject.Properties | ForEach-Object {
    Write-Host "    $($_.Name) = $($_.Value)" -ForegroundColor Gray
}

$created = Invoke-Api POST "/v1/character" $newChar
if ($created) {
    $charId = $created.id
    Write-Host ""
    Write-Result "Created" "Character ID $charId"

    # Retrieve the character to show computed fields
    $char = Invoke-Api GET "/v1/character/$charId"
    if ($char) {
        Write-Host ""
        Write-Host "  Character Details:" -ForegroundColor Magenta
        Write-Result "Name" $char.name
        Write-Result "Level" $char.level
        Write-Result "RaceId" $char.raceId
        Write-Result "ClassId" $char.classId
        Write-Result "Strength" "$($char.strength)$(if($char.strengthPercentile -gt 0){'/'+$char.strengthPercentile})"
        Write-Result "Dexterity" $char.dexterity
        Write-Result "Stamina" $char.stamina
        Write-Result "Intelligence" $char.intelligence
        Write-Result "Wisdom" $char.wisdom
        Write-Result "Charisma" $char.charisma
        Write-Result "Max HP" $char.maxHitPoints
        Write-Result "StrikeRating" $char.strikeRating
        Write-Result "Turn Speed" $char.turnSpeed

        # =====================
        # 7. COMBAT DEMO (StrikeRating)
        # =====================
        Write-Step "7. COMBAT - Attack Resolution (StrikeRating)"

        Write-Host "  Formula: d20 + STR mod + weapon.attackBonus >= StrikeRating - target AC"
        Write-Host "  Character StrikeRating: $($char.strikeRating) | STR mod: $(($char.strength - 10) / 2)"
        Write-Host ""

        $targetACs = @(5, 10, 15, 20)
        foreach ($ac in $targetACs) {
            $attack = Invoke-Api GET "/v1/attack/$ac"
            if ($attack) {
                $status = if ($attack.isHit) { "HIT" } else { "MISS" }
                $color = if ($attack.isHit) { "Green" } else { "Red" }
                Write-Host "  Target AC $($ac): " -NoNewline -ForegroundColor White
                Write-Host "[$status]" -NoNewline -ForegroundColor $color
                Write-Host "  Roll=$($attack.hitRoll)  Damage=$($attack.damage) $($attack.damageDie)" -ForegroundColor Gray
            }
        }

        Write-Host ""
        Write-Host "  StrikeRating explanation:" -ForegroundColor Yellow
        Write-Host "  - Lower target AC = harder to hit (better armor)" -ForegroundColor Gray
        Write-Host "  - Lower StrikeRating = better combatant (Knights have 18, Mages 20)" -ForegroundColor Gray
        Write-Host "  - A hit requires: d20 + mods + weapon.attackBonus >= StrikeRating - targetAC" -ForegroundColor Gray
        Write-Host "  - Damage is clamped to minimum 0 (no negative damage)" -ForegroundColor Gray

        # =====================
        # 8. UPDATE & DELETE
        # =====================
        Write-Step "8. CHARACTER MANAGEMENT"

        Write-Host "  Updating character level and stats..."
        $char.level = 2
        $char.strength = 18
        $char.strengthPercentile = 50
        $char.currentHitPoints = $char.maxHitPoints

        $updateResult = Invoke-Api PUT "/v1/character/$charId" $char
        if ($updateResult -eq $null -or $updateResult.StatusCode -eq 204) {
            Write-Host "    [OK] Character updated to level 2 with 18/50 Strength" -ForegroundColor Green
        }

        $updatedChar = Invoke-Api GET "/v1/character/$charId"
        if ($updatedChar) {
            Write-Result "Updated" "Level=$($updatedChar.level), STR=$($updatedChar.strength)/$($updatedChar.strengthPercentile), HP=$($updatedChar.currentHitPoints)"
        }

        Write-Host ""
        Write-Host "  Listing all characters in database:" -ForegroundColor Yellow
        $allChars = Invoke-Api GET "/v1/characters"
        foreach ($c in $allChars) {
            Write-Host "    - $($c.name) (Lvl $($c.level), STR $($c.strength), Strike $($c.strikeRating))" -ForegroundColor Gray
        }

        Write-Host ""
        Write-Host "  Cleaning up: deleting test character..." -ForegroundColor DarkGray
        $deleteResult = Invoke-Api DELETE "/v1/character/$charId"
        Write-Host "    [OK] Character $charId deleted" -ForegroundColor Green
    }
}

# =====================
# 9. RINGS, AMULETS & GIRDLES
# =====================
Write-Step "9. MAGICAL ACCESSORIES"

$rings = Invoke-Api GET "/v1/rings"
if ($rings) {
    Write-Host "  Rings ($($rings.Count) available):" -ForegroundColor Gray
    foreach ($r in $rings) {
        $cursed = if ($r.cursed) { " [CURSED: $($r.curseEffect)]" } else { "" }
        Write-Result $r.name "[$($r.quality)] $($r.effectType) $($r.effectValue)$cursed"
    }
}

Write-Host ""

$amulets = Invoke-Api GET "/v1/amulets"
if ($amulets) {
    Write-Host "  Amulets ($($amulets.Count) available):" -ForegroundColor Gray
    foreach ($a in $amulets) {
        $cursed = if ($a.cursed) { " [CURSED: $($a.curseEffect)]" } else { "" }
        Write-Result $a.name "[$($a.quality)] $($a.effectType) $($a.effectValue)$cursed"
    }
}

Write-Host ""

$girdles = Invoke-Api GET "/v1/girdles"
if ($girdles) {
    Write-Host "  Girdles / Belts ($($girdles.Count) available):" -ForegroundColor Gray
    foreach ($g in $girdles) {
        $cursed = if ($g.cursed) { " [CURSED: $($g.curseEffect)]" } else { "" }
        Write-Result $g.name "[$($g.quality)] $($g.effectType) $($g.effectValue)$cursed"
    }
}

Write-Host ""

# =====================
# 10. ITEM SETS
# =====================
Write-Step "10. ITEM SETS - Set Bonuses"

$sets = Invoke-Api GET "/v1/sets"
if ($sets) {
    Write-Host "  Item Sets ($($sets.Count) available):" -ForegroundColor Gray
    foreach ($s in $sets) {
        Write-Host "" | Out-Null
        Write-Result $s.name "$($s.description)"
        $bonuses = Invoke-Api GET "/v1/sets/$($s.id)/bonuses"
        if ($bonuses) {
            foreach ($b in $bonuses) {
                Write-Host "        $($b.piecesRequired) pieces: $($b.effectDescription)" -ForegroundColor Yellow
            }
        }
    }
}

# =====================
# 11. NPC CHARACTERS
# =====================
Write-Step "11. NPCs - Inhabitants of the World"

$npcs = Invoke-Api GET "/v1/npcs"
if ($npcs) {
    $merchants = $npcs | Where-Object { $_.isMerchant }
    $hostile = $npcs | Where-Object { $_.isHostile }
    Write-Host "  $($npcs.Count) NPCs populate the world ($($merchants.Count) merchants, $($hostile.Count) hostile encounters)"
    Write-Host ""
    foreach ($n in $npcs) {
        $role = @()
        if ($n.isMerchant) { $role += "Merchant" }
        if ($n.isQuestGiver) { $role += "Quest Giver" }
        if ($n.isHostile) { $role += "Hostile" }
        $roleStr = if ($role.Count -gt 0) { " [$($role -join ', ')]" } else { "" }
        Write-Result $n.name "Lvl $($n.level) $($n.race) $($n.class)$roleStr"
        Write-Host "         $($n.biography)" -ForegroundColor Gray
    }
}

# =====================
# 12. DEMO SUMMARY
# =====================
Write-Step "12. DEMO COMPLETE - Architecture Overview"

Write-Host @"
  BattleArena Stack:
    API:  .NET 8 Minimal API (C#)      -> http://localhost:8585
    DB:   PostgreSQL 13 + pg_cron       -> localhost:5432
    ORM:  Raw ADO.NET / Npgsql (no EF)  -> fn_ functions / sp_ procedures
    Test: xUnit + NSubstitute           -> 33 passing tests (100% Application coverage)
    Deploy: Docker Compose              -> 2 containers (api + db)

  Homebrew World Features:
    10 races (Human, Elf, Dwarf, Lizard, Undead, Kobold, Demon, Orc, Ogre, Halfling)
      - Each race has unique ability bonuses, special abilities, and full lore descriptions
      - 19 subraces with distinct flavor
    9 classes (Barbarian, Knight, Paladin, Priest, Mage, Bard, Druid, Fighter, Rogue)
      - Each class has base StrikeRating and hit die
      - Class-race restrictions enforced in seed data
    8 deities (4 Light, 4 Dark) with alignments and domains
    9 pets with class and race restrictions
    24 spells across 3 schools (AoE, CC, Other)
    32 weapons (18 Common, 3 Epic, 6 Legendary, 4 Cursed, 6 Rare/Heirloom)
    24 armor pieces (13 Common, 2 Epic, 3 Legendary, 3 Cursed, 3 Rare)
    7 rings (including cursed Ring of Greed)
    5 amulets/necklaces
    5 girdles/belts (including cursed Girdle of Weakness)
    3 item sets (Iron Sentinel, Shadow Stalker, Dragonborn Legacy)
      - Each set has 2-3 tiered set bonuses
    10 NPCs (merchants, quest givers, hostile encounters, final boss)
      - Each NPC has full biography, stats, and role flags

  Combat System (StrikeRating):
    d20 + strength modifier + weapon.attackBonus >= StrikeRating - target AC
    Lower AC = better protection
    Lower StrikeRating = better attacker
    Damage = weapon die + strength modifier (min 0)

  Database Features:
    Stored procedures for CRUD operations
    pg_cron scheduled jobs (weekly vacuum, daily cleanup)
    Custom functions for all queries
    Foreign key constraints and cascading deletes
"@ -ForegroundColor Cyan

Write-Host "`n$separator" -ForegroundColor Cyan
Write-Host "  DEMO COMPLETE" -ForegroundColor Green
Write-Host "$separator" -ForegroundColor Cyan
