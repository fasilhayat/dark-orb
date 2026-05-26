namespace BattleArena.Core.Entities;

// Represents one side in a battle — either the hero party or an enemy group.
//
// Hero party rules:
//   • Maximum 6 player characters (pets and NPCs are NOT counted here).
//   • Enemies (N) have no upper cap — encounters can scale freely.
public class Party
{
    public const int HeroPartyMaxSize = 6;

    public string Name { get; set; } = string.Empty;
    public List<PartyMember> Members { get; set; } = new();

    // True when every member is knocked out or dead.
    public bool IsDefeated => Members.All(m => !m.Character.IsAlive);

    // Characters still able to act (HP > 0).
    public IEnumerable<Character> LivingMembers =>
        Members.Select(m => m.Character).Where(c => c.IsAlive);

    // Factory — builds a single-member party (convenience for 1v1).
    public static Party Solo(Character character, IAttackSource? attackSource = null) =>
        new() { Name = character.Name, Members = { new PartyMember { Character = character, AttackSource = attackSource } } };

    // Factory — validates hero party size before creating.
    public static Party HeroParty(string name, IEnumerable<PartyMember> members)
    {
        var list = members.ToList();
        if (list.Count > HeroPartyMaxSize)
            throw new InvalidOperationException(
                $"Hero party '{name}' exceeds the maximum of {HeroPartyMaxSize} characters (got {list.Count}).");
        return new Party { Name = name, Members = list };
    }
}
