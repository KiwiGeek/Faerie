using Faerie.Runtime;

namespace Faerie.Model;

/// <summary>
/// The static definition of a game world: every room, thing and creature, their initial
/// placement, and the room the player starts in. Mutable runtime state lives in
/// <see cref="GameState"/>; the world itself is the unchanging blueprint.
/// </summary>
public sealed class World
{
    private readonly List<Room> _rooms = [];
    private readonly List<Thing> _things = [];
    private readonly Dictionary<Thing, Placement> _initialPlacements = [];

    public Player Player { get; } = new();

    public IReadOnlyList<Room> Rooms => _rooms;
    public IReadOnlyList<Thing> Things => _things;
    public IReadOnlyDictionary<Thing, Placement> InitialPlacements => _initialPlacements;

    /// <summary>The room the player begins in. Must be set before the game can start.</summary>
    public Room? StartRoom { get; set; }

    internal void Register(Room room) => _rooms.Add(room);
    internal void Register(Thing thing) => _things.Add(thing);

    /// <summary>Records where a thing begins the game.</summary>
    internal void PlaceInitially(Thing thing, Placement placement) => _initialPlacements[thing] = placement;

    /// <summary>Finds a room by id (used by save/load); returns null if not found.</summary>
    public Room? FindRoom(string id) => _rooms.FirstOrDefault(r => r.Id == id);

    /// <summary>Finds a thing by id (used by save/load); returns null if not found.</summary>
    public Thing? FindThing(string id) => _things.FirstOrDefault(t => t.Id == id);

    /// <summary>Paired mirror rooms registered at build time.</summary>
    public IReadOnlyList<MirrorPair> MirrorPairs { get; internal set; } = [];
}
