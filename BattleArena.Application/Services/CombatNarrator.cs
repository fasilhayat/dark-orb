namespace BattleArena.Application.Services;

using Application.Models;

// Picks a flavour phrase for each combat event based on how the roll went.
// Phrases use {attacker} and {target} as placeholders, replaced at call time.
// All phrase banks are hardcoded — no database or config needed.
public static class CombatNarrator
{
    // ── Phrase banks ──────────────────────────────────────────────────────────

    private static readonly string[] CriticalPhrases =
    [
        "{attacker} strikes with blinding precision — a devastating CRITICAL HIT!",
        "Steel sings as {attacker} finds the perfect gap in {target}'s defense!",
        "With terrifying accuracy {attacker} drives the blade home — CRITICAL BLOW!",
        "Time slows as {attacker}'s weapon arcs perfectly through {target}'s guard!",
        "{target} staggers — {attacker} has landed a flawless, bone-crushing critical strike!",
        "A perfect opening, exploited ruthlessly: CRITICAL HIT from {attacker}!",
        "{attacker} channels every ounce of fury into one catastrophic critical strike!",
        "The crowd gasps — {attacker} lands a once-in-a-battle perfect critical blow!"
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

    // ── Public API ────────────────────────────────────────────────────────────

    public static NarrativeContext GetContext(
        int hitRoll, int total, int defensePower,
        bool isHit, bool isCritical, bool isFumble)
    {
        if (isFumble  || hitRoll == 1)  return NarrativeContext.Fumble;
        if (isCritical || hitRoll == 20) return NarrativeContext.CriticalHit;

        var margin = total - defensePower;

        if (isHit)
        {
            if (margin >= 8) return NarrativeContext.CrushingHit;
            if (margin >= 4) return NarrativeContext.SolidHit;
            return NarrativeContext.GlancingHit;
        }
        else
        {
            if (margin >= -3) return NarrativeContext.NearMiss;
            return NarrativeContext.WideMiss;
        }
    }

    public static string GetPhrase(string attacker, string target, NarrativeContext context)
    {
        var bank = context switch
        {
            NarrativeContext.CriticalHit  => CriticalPhrases,
            NarrativeContext.Fumble       => FumblePhrases,
            NarrativeContext.CrushingHit  => CrushingPhrases,
            NarrativeContext.SolidHit     => SolidPhrases,
            NarrativeContext.GlancingHit  => GlancingPhrases,
            NarrativeContext.NearMiss     => NearMissPhrases,
            NarrativeContext.WideMiss     => WideMissPhrases,
            _                             => SolidPhrases
        };

        return bank[Random.Shared.Next(bank.Length)]
            .Replace("{attacker}", attacker)
            .Replace("{target}",   target);
    }
}
