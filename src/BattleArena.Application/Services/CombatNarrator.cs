namespace BattleArena.Application.Services;

using Application.Models;
using Core.Entities.Enums;

// Picks a flavour phrase for each combat event based on how the roll went.
// Phrases use {attacker}, {target} and {element} as placeholders, replaced at call time.
// Weapon and spell attacks use separate phrase banks — element-specific for spells.
// All phrase banks are hardcoded — no database or config needed.
public static class CombatNarrator
{
    // ── Weapon phrase banks ─────────────────────────────────────────────────────

    private static readonly string[] CriticalPhrases =
    [
        "{attacker} strikes with blinding precision — a devastating CRITICAL HIT!",
        "Steel sings as {attacker} finds the perfect gap in {target}'s defense!",
        "With terrifying accuracy {attacker} drives the blade home — CRITICAL BLOW!",
        "Time slows as {attacker}'s weapon arcs perfectly through {target}'s guard!",
        "{target} staggers — {attacker} has landed a flawless, bone-crushing critical strike!",
        "A perfect opening, exploited ruthlessly: CRITICAL HIT from {attacker}!",
        "{attacker} channels every ounce of fury into one catastrophic critical strike!",
        "The crowd gasps — {attacker} lands a once-in-a-combat perfect critical blow!"
    ];

    private static readonly string[] FumblePhrases =
    [
        "{attacker} stumbles and nearly drops their weapon in a catastrophic fumble!",
        "Disaster! {attacker}'s attack collapses into chaos — a mortifying fumble!",
        "{attacker}'s footing betrays them completely, the swing going nowhere near {target}!",
        "An embarrassing fumble! {attacker} overreaches and almost falls over their own feet!",
        "{attacker} loses their grip at the worst possible moment — a shameful fumble!",
        "The attack disintegrates as {attacker} fumbles in a moment of pure incompetence!",
        "{attacker} trips on thin air, weapon flailing wildly past a bemused {target}!",
        "Even {target} winces in secondhand embarrassment as {attacker} fumbles spectacularly!"
    ];

    private static readonly string[] CrushingPhrases =
    [
        "{attacker} smashes through {target}'s guard with bone-shattering force!",
        "A thunderous blow! {attacker} hammers {target} with devastating power!",
        "{target}'s defenses crumble under {attacker}'s relentless, crushing assault!",
        "The impact echoes as {attacker} drives {target} back with a tremendous strike!",
        "{attacker} tears through {target}'s defense, landing a punishing, crushing blow!",
        "There is no subtlety here — {attacker} simply overwhelms {target} with raw power!",
        "{attacker} bulldozes straight through {target}'s guard — a shattering hit!"
    ];

    private static readonly string[] SolidPhrases =
    [
        "{attacker} connects cleanly — the weapon biting deep into {target}.",
        "A well-placed strike! {attacker} finds the mark on {target}.",
        "The attack lands true as {attacker} cuts through {target}'s guard.",
        "{attacker} drives the strike home with confident, practised precision.",
        "Clean and effective — {attacker}'s blow lands exactly where intended.",
        "{attacker} reads {target}'s defence perfectly and steps through it.",
        "A textbook strike: {attacker} lands a solid, damaging blow on {target}."
    ];

    private static readonly string[] GlancingPhrases =
    [
        "{attacker}'s weapon grazes {target} — barely squeezing past the defence.",
        "A glancing blow! {attacker} clips {target} with the edge of the weapon.",
        "The attack deflects off {target}'s armour but still draws blood.",
        "{attacker} finds a narrow gap and scrapes through with a partial hit.",
        "Not clean, but enough — {attacker}'s weapon skims {target} for light damage.",
        "{target} almost turns it aside, but {attacker}'s blow nicks through."
    ];

    private static readonly string[] NearMissPhrases =
    [
        "{attacker}'s weapon whistles past {target}'s ear by a hair's breadth!",
        "So close! {attacker}'s strike glances off {target}'s armour harmlessly.",
        "{target} twists aside just in time, leaving {attacker}'s blow to cut only air!",
        "{attacker}'s attack skims the surface of {target}'s shield without connecting.",
        "A near miss — {attacker}'s weapon was a heartbeat away from finding its mark.",
        "{target} sucks in their gut and {attacker}'s blade passes by without drawing blood."
    ];

    private static readonly string[] WideMissPhrases =
    [
        "{attacker}'s wild swing misses {target} by a wide margin.",
        "The attack swings wide — {attacker} completely misjudges the distance.",
        "{target} sidesteps with ease as {attacker}'s clumsy blow finds nothing but air.",
        "{attacker} overcommits and misses entirely, leaving an opening of their own.",
        "A poor attempt — {attacker}'s strike goes wide and {target} doesn't even flinch.",
        "{target} barely has to move as {attacker}'s attack sails harmlessly past.",
        "{attacker} swings with confidence and connects with absolutely nothing."
    ];

    // ── Spell phrase banks (element-aware) ──────────────────────────────────────

    private static readonly string[] SpellCriticalPhrases =
    [
        "A searing eruption of {element} energy engulfs {target} — {attacker} lands a CRITICAL spell!",
        "{attacker}'s {element} magic detonates with catastrophic force on {target}!",
        "Pure {element} energy erupts around {target} — {attacker}'s critical blast burns through!",
        "The {element} surges through {target}'s defences — a perfect critical strike by {attacker}!"
    ];

    private static readonly string[] SpellFumblePhrases =
    [
        "{attacker}'s {element} magic backfires spectacularly!",
        "{attacker} loses control of the {element} energy, the spell sputtering into failure!",
        "Arcane feedback jolts {attacker} as their {element} spell collapses mid-cast!"
    ];

    private static readonly string[] SpellCrushingPhrases =
    [
        "A torrent of {element} energy slams into {target} with overwhelming power!",
        "{attacker} unleashes a devastating {element} blast that consumes {target}!",
        "{target} is engulfed in {element} fury conjured by {attacker}!",
        "The {element} magic tears through {target}'s resistance like paper!"
    ];

    private static readonly string[] SpellSolidPhrases =
    [
        "{attacker}'s {element} spell strikes {target} squarely.",
        "A well-aimed bolt of {element} from {attacker} finds its mark on {target}.",
        "{target} is caught in the full force of {attacker}'s {element} magic.",
        "{attacker}'s {element} energy lances into {target} with precise accuracy."
    ];

    private static readonly string[] SpellGlancingPhrases =
    [
        "The edge of {attacker}'s {element} blast grazes {target}.",
        "{target} partially deflects the {element} magic, taking only a glancing hit.",
        "Sparks of {element} energy catch {target} as the spell barely connects.",
        "{attacker}'s {element} spell clips {target} — not a clean hit, but enough."
    ];

    private static readonly string[] SpellMissPhrases =
    [
        "{target} dodges as {attacker}'s {element} bolt sizzles past harmlessly!",
        "{attacker}'s {element} spell flies wide, leaving {target} untouched.",
        "The {element} energy dissipates harmlessly as {target} sidesteps the spell!",
        "{target} weaves aside as {attacker}'s {element} magic fizzles into nothing."
    ];

    // ── Public API ────────────────────────────────────────────────────────────

    public static NarrativeContext GetContext(
        int hitRoll, int total, int defensePower, int? defenseRoll,
        bool isHit, bool isCritical, bool isFumble)
    {
        if (isFumble  || hitRoll == 1)  return NarrativeContext.Fumble;
        if (isCritical || hitRoll == 20) return NarrativeContext.CriticalHit;

        var defenseTotal = defensePower + (defenseRoll ?? 0);
        var margin = total - defenseTotal;

        if (isHit)
        {
            if (margin >= 8) return NarrativeContext.CrushingHit;
            if (margin >= 4) return NarrativeContext.SolidHit;
            return NarrativeContext.GlancingHit;
        }
        else
        {
            if (margin >= -5) return NarrativeContext.NearMiss;
            return NarrativeContext.WideMiss;
        }
    }

    public static string GetPhrase(string attacker, string target, NarrativeContext context,
        bool isSpell = false, DamageType damageType = DamageType.Slashing,
        Func<int, int>? rollIndex = null)
    {
        if (isSpell)
            return GetSpellPhrase(attacker, target, context, damageType, rollIndex);

        var bank = SelectWeaponPhraseBank(context);
        return bank[(rollIndex ?? Random.Shared.Next)(bank.Length)]
            .Replace("{attacker}", attacker)
            .Replace("{target}",   target);
    }

    private static string GetSpellPhrase(
        string attacker, string target, NarrativeContext context,
        DamageType damageType, Func<int, int>? rollIndex)
    {
        var element   = SelectSpellElement(damageType);
        var spellBank = SelectSpellPhraseBank(context);
        return spellBank[(rollIndex ?? Random.Shared.Next)(spellBank.Length)]
            .Replace("{attacker}", attacker)
            .Replace("{target}",   target)
            .Replace("{element}",  element);
    }

    private static string SelectSpellElement(DamageType damageType) => damageType switch
    {
        DamageType.Fire      => "fire",
        DamageType.Ice       => "ice",
        DamageType.Lightning => "lightning",
        _                    => "arcane"
    };

    private static string[] SelectSpellPhraseBank(NarrativeContext context) => context switch
    {
        NarrativeContext.CriticalHit => SpellCriticalPhrases,
        NarrativeContext.Fumble      => SpellFumblePhrases,
        NarrativeContext.CrushingHit => SpellCrushingPhrases,
        NarrativeContext.SolidHit    => SpellSolidPhrases,
        NarrativeContext.GlancingHit => SpellGlancingPhrases,
        NarrativeContext.NearMiss    => SpellMissPhrases,
        NarrativeContext.WideMiss    => SpellMissPhrases,
        _                            => SpellSolidPhrases
    };

    private static string[] SelectWeaponPhraseBank(NarrativeContext context) => context switch
    {
        NarrativeContext.CriticalHit => CriticalPhrases,
        NarrativeContext.Fumble      => FumblePhrases,
        NarrativeContext.CrushingHit => CrushingPhrases,
        NarrativeContext.SolidHit    => SolidPhrases,
        NarrativeContext.GlancingHit => GlancingPhrases,
        NarrativeContext.NearMiss    => NearMissPhrases,
        NarrativeContext.WideMiss    => WideMissPhrases,
        _                            => SolidPhrases
    };
}
