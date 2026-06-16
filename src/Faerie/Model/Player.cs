namespace Faerie.Model;

/// <summary>
/// The player avatar. Its current room and inventory live in the game state; this element exists
/// so the player can be examined ("examine me") and referenced by verbs.
/// </summary>
public sealed class Player : Element
{
    public Player() : base("player", "yourself")
    {
        Article = "";
        Nouns.Clear();
        Nouns.AddRange(["me", "myself", "self", "i"]);
        Set(Attr.Animate);
        Description = "As good-looking as ever, though a little pale given the circumstances.";
    }

    public override string ToString() => "Player";
}
