using Faerie.Runtime;

namespace Faerie.Model;

/// <summary>
/// A tangible object in the world: items, scenery, doors, containers, and (when flagged
/// <see cref="Attr.Animate"/>) creatures and NPCs. Things are wired to one another and to
/// rooms by reference through the fluent builder.
/// </summary>
public class Thing : Element
{
    public Thing(string id, string name) : base(id, name) { }

    /// <summary>Text shown when the thing is read (requires <see cref="Attr.Readable"/>).</summary>
    public string? ReadableText { get; set; }

    /// <summary>
    /// A custom one-line listing used when this thing is sitting in a room in its original
    /// position (e.g. "A rusty lantern hangs from a hook."). When null a default listing is used.
    /// </summary>
    public string? InitialDescription { get; set; }

    /// <summary>The thing that unlocks this one when it is <see cref="Attr.Lockable"/>.</summary>
    public Thing? Key { get; set; }

    /// <summary>Where this thing begins the game. Set by the fluent placement helpers; resolved at build.</summary>
    internal Placement InitialPlacement { get; set; } = Placement.Offstage;

    /// <summary>Optional hook invoked the first time the player examines this thing.</summary>
    public Action<GameContext>? OnFirstExamine { get; set; }

    /// <summary>Optional hook invoked whenever the player examines this thing.</summary>
    public Action<GameContext>? OnExamine { get; set; }

    /// <summary>
    /// Optional hook invoked when the player tries to take this thing. Return true if the take
    /// was fully handled here (the default take behaviour is then skipped).
    /// </summary>
    public Func<GameContext, bool>? OnTake { get; set; }

    public override string ToString() => $"Thing({Id})";
}
