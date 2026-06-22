using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>
/// All mutable runtime state for a game in progress: where every thing currently is, the player's
/// location, global flags and counters, the turn count, and whether the game has ended. This is
/// the object that gets serialized when the player saves.
/// </summary>
public sealed class GameState
{
    private readonly Dictionary<Thing, Placement> _locations = [];
    private readonly Dictionary<object, object?> _globals = [];

    public GameState(World world)
    {
        World = world;
        foreach ((Thing thing, Placement placement) in world.InitialPlacements)
            _locations[thing] = placement;

        // Things never explicitly placed begin offstage.
        foreach (Thing thing in world.Things)
            _locations.TryAdd(thing, Placement.Offstage);

        CurrentRoom = world.StartRoom
            ?? throw new InvalidOperationException("The world has no start room. Call StartIn(room) when building the game.");
    }

    public World World { get; }

    /// <summary>The room the player is currently in.</summary>
    public Room CurrentRoom { get; set; }

    /// <summary>Total number of turns the player has taken.</summary>
    public int TurnCount { get; set; }

    /// <summary>The player's current score. Surfaced in the default title bar.</summary>
    public int Score { get; set; }

    /// <summary>
    /// Adds <paramref name="delta"/> to <see cref="Score"/>, clamping to <c>[0, maxScore]</c> when
    /// <paramref name="maxScore"/> is positive.
    /// </summary>
    public int AdjustScore(int delta, int maxScore = 0) => Scoring.AdjustScore(this, delta, maxScore);

    /// <summary>True once the game has ended (win or lose).</summary>
    public bool IsOver { get; private set; }

    /// <summary>True if the game ended in victory.</summary>
    public bool PlayerWon { get; private set; }

    /// <summary>The last thing referred to, used to resolve the pronoun "it".</summary>
    public Thing? LastReferencedThing { get; set; }

    public void EndGame(bool won)
    {
        IsOver = true;
        PlayerWon = won;
    }

    internal void RestoreOutcome(bool over, bool won)
    {
        IsOver = over;
        PlayerWon = won;
    }

    // ---- global typed state ---------------------------------------------------------------

    private readonly Dictionary<string, object> _globalKeysByName = [];

    public T Get<T>(StateKey<T> key)
    {
        _globalKeysByName.TryAdd(key.Name, key);
        return _globals.TryGetValue(key, out object? v) && v is T t ? t : key.Default;
    }

    public void Set<T>(StateKey<T> key, T value)
    {
        _globalKeysByName.TryAdd(key.Name, key);
        _globals[key] = value;
    }

    internal IReadOnlyDictionary<object, object?> RawGlobals => _globals;

    /// <summary>Global keys seen so far, by name (used for save/restore).</summary>
    internal IReadOnlyDictionary<string, object> GlobalKeysByName => _globalKeysByName;

    /// <summary>Restores a global value from a save file, matching the live <see cref="StateKey{T}"/> by name.</summary>
    internal void RestoreGlobal(string name, object value)
    {
        if (_globalKeysByName.TryGetValue(name, out object? key))
            _globals[key] = value;
    }

    // ---- placement queries ----------------------------------------------------------------

    public Placement PlacementOf(Thing thing) =>
        _locations.TryGetValue(thing, out Placement p) ? p : Placement.Offstage;

    /// <summary>Moves a thing to a new placement.</summary>
    public void Move(Thing thing, Placement placement) => _locations[thing] = placement;

    public void MoveTo(Thing thing, Room room) => Move(thing, Placement.InRoom(room));
    public void TakeIntoInventory(Thing thing) => Move(thing, Placement.Carried);

    /// <summary>Things loose in the given room.</summary>
    public IEnumerable<Thing> ContentsOf(Room room) =>
        _locations.Where(kv => kv.Value is { Anchor: Anchor.Room } p && p.Room == room).Select(kv => kv.Key);

    /// <summary>Things inside (or, when <paramref name="onTop"/> is true, on top of) a container.</summary>
    public IEnumerable<Thing> ContentsOf(Thing container, bool onTop = false) =>
        _locations.Where(kv =>
            kv.Value.Container == container &&
            kv.Value.Anchor == (onTop ? Anchor.On : Anchor.Inside)).Select(kv => kv.Key);

    /// <summary>Everything the player is carrying (not counting worn items).</summary>
    public IEnumerable<Thing> Inventory =>
        _locations.Where(kv => kv.Value.Anchor == Anchor.Carried).Select(kv => kv.Key);

    /// <summary>Everything the player is wearing.</summary>
    public IEnumerable<Thing> Worn =>
        _locations.Where(kv => kv.Value.Anchor == Anchor.Worn).Select(kv => kv.Key);

    public bool IsCarried(Thing thing) =>
        PlacementOf(thing).Anchor is Anchor.Carried or Anchor.Worn;

    public bool IsWorn(Thing thing) => PlacementOf(thing).Anchor == Anchor.Worn;

    /// <summary>
    /// Sum of <see cref="Thing.Size"/> for everything in the player's inventory tree (carried,
    /// worn, and nested contents). Zero-sized things do not contribute.
    /// </summary>
    public int TotalLoad => CarriedTree().Sum(t => t.Size);

    /// <summary>
    /// Total size of <paramref name="thing"/> and everything inside or on it. Used when picking
    /// something up from the world.
    /// </summary>
    public int LoadOf(Thing thing) => ThingAndDescendants(thing).Sum(t => t.Size);

    private IEnumerable<Thing> CarriedTree()
    {
        foreach (Thing root in Inventory.Concat(Worn))
        {
            foreach (Thing t in ThingAndDescendants(root))
                yield return t;
        }
    }

    private IEnumerable<Thing> ThingAndDescendants(Thing thing)
    {
        yield return thing;
        foreach (Thing inside in ContentsOf(thing))
            foreach (Thing nested in ThingAndDescendants(inside))
                yield return nested;
        foreach (Thing onTop in ContentsOf(thing, onTop: true))
            foreach (Thing nested in ThingAndDescendants(onTop))
                yield return nested;
    }

    /// <summary>Walks up the containment chain to find the room a thing is ultimately in (if any).</summary>
    public Room? RoomOf(Thing thing)
    {
        Placement p = PlacementOf(thing);
        return p.Anchor switch
        {
            Anchor.Room => p.Room,
            Anchor.Carried or Anchor.Worn => CurrentRoom,
            Anchor.Inside or Anchor.On => p.Container is null ? null : RoomOf(p.Container),
            _ => null
        };
    }

    /// <summary>True if the thing is in the player's current room (directly or via a container).</summary>
    public bool IsPresent(Thing thing) => RoomOf(thing) == CurrentRoom || IsCarried(thing);

    /// <summary>
    /// True when the thing is physically in <paramref name="room"/> (including inside in-room
    /// containers). Carried and worn items return false — use <see cref="IsPresent"/> or
    /// <see cref="RoomOf"/> when inventory should count as being in the current room.
    /// </summary>
    public bool IsLocatedIn(Thing thing, Room room) =>
        !IsCarried(thing) && RoomOf(thing) == room;

    internal IReadOnlyDictionary<Thing, Placement> RawLocations => _locations;
    internal void RestoreLocation(Thing thing, Placement placement) => _locations[thing] = placement;
}
