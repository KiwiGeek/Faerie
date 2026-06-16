using Faerie.Model;
using Faerie.Presentation;

namespace Faerie.Runtime;

/// <summary>
/// The ambient context handed to every verb handler and event hook. It bundles the world, the
/// mutable state, the output writer and convenience helpers so game code reads naturally:
/// <c>ctx.Say("...")</c>, <c>ctx.MovePlayerTo(kitchen)</c>, <c>ctx.Carrying(lantern)</c>.
/// </summary>
public class GameContext
{
    public GameContext(GameEngine engine, GameState state, OutputWriter output)
    {
        Engine = engine;
        State = state;
        Out = output;
    }

    public GameEngine Engine { get; }
    public GameState State { get; }
    public OutputWriter Out { get; }

    public World World => State.World;
    public Player Player => World.Player;
    public Room CurrentRoom => State.CurrentRoom;
    public Random Random => Engine.Random;

    // ---- output helpers -------------------------------------------------------------------

    public void Say(string markup) => Out.PrintLine(markup);
    public void SayInline(string markup) => Out.Print(markup);
    public void Blank() => Out.Blank();

    // ---- state helpers --------------------------------------------------------------------

    public bool Carrying(Thing thing) => State.IsCarried(thing);
    public bool Wearing(Thing thing) => State.IsWorn(thing);
    public bool Here(Thing thing) => State.IsPresent(thing);
    public bool InRoom(Room room) => State.CurrentRoom == room;

    /// <summary>The room a thing currently sits in, or null if it is carried, worn, or offstage.</summary>
    public Room? RoomOf(Thing thing) => State.RoomOf(thing);

    /// <summary>
    /// Things in <paramref name="room"/>. By default only loose floor items are returned.
    /// When <paramref name="includePresent"/> is true, also includes things inside in-room
    /// containers and things carried or worn by the player while in that room (including items
    /// nested inside carried containers).
    /// </summary>
    public IEnumerable<Thing> ThingsIn(Room room, bool includePresent = false) =>
        includePresent
            ? World.Things.Where(t => RoomOf(t) == room)
            : State.ContentsOf(room);

    /// <summary>Things in the player's current room. See <see cref="ThingsIn(Room, bool)"/>.</summary>
    public IEnumerable<Thing> ThingsHere(bool includePresent = false) => ThingsIn(CurrentRoom, includePresent);

    /// <summary>True if <paramref name="room"/> is one exit away from the current room.</summary>
    public bool IsAdjacent(Room room) =>
        CurrentRoom.Exits.Values.Any(e => e.Destination == room);

    /// <summary>
    /// True if <paramref name="thing"/> is in the current room or in a room one exit away.
    /// Carried and worn things count as being in the current room.
    /// </summary>
    public bool Nearby(Thing thing)
    {
        Room? room = RoomOf(thing);
        return room is not null && (room == CurrentRoom || IsAdjacent(room));
    }

    public void Move(Thing thing, Placement placement) => State.Move(thing, placement);
    public void Take(Thing thing) => State.TakeIntoInventory(thing);
    public void PlaceHere(Thing thing) => State.MoveTo(thing, State.CurrentRoom);
    public void Remove(Thing thing) => State.Move(thing, Placement.Offstage);

    public T Get<T>(StateKey<T> key) => State.Get(key);
    public void Set<T>(StateKey<T> key, T value) => State.Set(key, value);

    /// <summary>Moves the player to a room, firing the room's enter hooks and describing it.</summary>
    public void MovePlayerTo(Room room) => Engine.MovePlayerTo(room);

    /// <summary>Ends the game. <paramref name="won"/> selects the win or lose framing.</summary>
    public void EndGame(bool won, string? message = null)
    {
        if (message is not null) Say(message);
        State.EndGame(won);
    }

    public void Win(string message) => EndGame(true, message);
    public void Lose(string message) => EndGame(false, message);
}
