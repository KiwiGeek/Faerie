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

    /// <summary>NPC or shopkeeper who sells this when it is <see cref="Attr.Orderable"/>.</summary>
    public Thing? Vendor { get; set; }

    /// <summary>Where this thing begins the game. Set by the fluent placement helpers; resolved at build.</summary>
    internal Placement InitialPlacement { get; set; } = Placement.Offstage;

    /// <summary>
    /// Encumbrance weight for carry-limit checks. Zero (the default) does not add to
    /// <see cref="GameState.TotalLoad"/>.
    /// </summary>
    public int Size { get; set; }

    /// <summary>Optional override when a take is blocked by carry limit.</summary>
    public string? OnTakeBlockedMessage { get; set; }

    /// <summary>When set, the player can PUT a carried item through this opening into the room.</summary>
    public Room? PassageDestination { get; set; }

    /// <summary>When set, this thing is a mirror terminal for the pair.</summary>
    public MirrorPair? MirrorLink { get; set; }

    /// <summary>Message when a breakable thing is already broken.</summary>
    public string? BreakAlreadyMessage { get; set; }

    /// <summary>Message after a successful break.</summary>
    public string? BreakSuccessMessage { get; set; }

    /// <summary>Optional hook after <see cref="Attr.Broken"/> is set.</summary>
    public Action<GameContext>? OnBreak { get; set; }

    /// <summary>Maximum <see cref="Size"/> allowed through this passage; null means no limit.</summary>
    public int? PassageMaxSize { get; set; }

    /// <summary>When true (default), an <see cref="Attr.Openable"/> passage must be open to pass objects.</summary>
    public bool PassageRequiresOpen { get; set; } = true;

    /// <summary>Message when passage is closed. Defaults to <see cref="Passage.DefaultClosedMessage"/>.</summary>
    public string? PassageClosedMessage { get; set; }

    /// <summary>Message when an item is too large. Defaults to <see cref="Passage.DefaultTooLargeMessage"/>.</summary>
    public string? PassageTooLargeMessage { get; set; }

    /// <summary>Message after a successful pass. Defaults to a generic line in <see cref="Passage.TryPass"/>.</summary>
    public string? PassageSuccessMessage { get; set; }

    /// <summary>Optional hook invoked the first time the player examines this thing.</summary>
    public Action<GameContext>? OnFirstExamine { get; set; }

    /// <summary>Optional hook invoked whenever the player examines this thing.</summary>
    public Action<GameContext>? OnExamine { get; set; }

    /// <summary>
    /// Optional hook invoked when the player tries to take this thing. Return true if the take
    /// was fully handled here (the default take behaviour is then skipped).
    /// </summary>
    public Func<GameContext, bool>? OnTake { get; set; }

    /// <summary>
    /// Optional hook invoked when the player drops this thing. Return true if the drop was fully
    /// handled here (the default drop behaviour is then skipped).
    /// </summary>
    public Func<GameContext, bool>? OnDrop { get; set; }

    public override string ToString() => $"Thing({Id})";
}
