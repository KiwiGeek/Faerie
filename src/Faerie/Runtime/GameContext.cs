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
    public void OverwriteLine(string markup) => Out.OverwriteLine(markup);
    public void Blank() => Out.Blank();

    /// <summary>
    /// Pauses briefly while allowing the host to repaint (interactive terminals). Headless hosts
    /// and tests fall back to <see cref="Thread.Sleep(int)"/>. A delay of 0 pumps one frame when a
    /// presentation host is wired; otherwise it is a no-op.
    /// </summary>
    public void Delay(int milliseconds)
    {
        if (Engine.QuitRequested) return;
        if (Engine.PresentationDelay is { } delay)
            delay(milliseconds);
        else if (milliseconds > 0)
            Thread.Sleep(milliseconds);
    }

    /// <summary>True when the host repaints mid-turn (e.g. Avalonia); false in headless/script runs.</summary>
    public bool LivePresentation => Engine.PresentationDelay is not null;

    /// <summary>
    /// When set during a handler (e.g. <see cref="Room.OnEnter"/>), remaining commands on the
    /// current <see cref="GameEngine.SubmitLine"/> input line are not executed.
    /// </summary>
    public bool StopCommandChain { get; set; }

    /// <summary>Prints a prompt and blocks until the player enters a line (mid-turn).</summary>
    public string PromptLine(string prompt)
    {
        if (Engine.QuitRequested) return "";
        Out.Print(prompt);
        return Engine.RequirePlayerInput().ReadLine();
    }

    /// <summary>Prints a prompt and blocks until the player presses an accepted key (mid-turn).</summary>
    public char PromptKey(string prompt, ReadOnlySpan<char> validKeys)
    {
        if (Engine.QuitRequested) return validKeys.Length > 0 ? validKeys[0] : '\0';
        Out.Print(prompt);
        char key = Engine.RequirePlayerInput().ReadKey(validKeys);
        Out.NewLine();
        foreach (char valid in validKeys)
        {
            if (char.ToLowerInvariant(valid) == char.ToLowerInvariant(key))
                return valid;
        }

        return key;
    }

    // ---- state helpers --------------------------------------------------------------------

    public bool Carrying(Thing thing) => State.IsCarried(thing);
    public bool Wearing(Thing thing) => State.IsWorn(thing);
    public bool Here(Thing thing) => State.IsPresent(thing);
    public bool InRoom(Room room) => State.CurrentRoom == room;

    /// <summary>
    /// True when the thing is physically in <paramref name="room"/>, not in the player's inventory.
    /// See <see cref="GameState.IsLocatedIn"/>.
    /// </summary>
    public bool LocatedIn(Thing thing, Room room) => State.IsLocatedIn(thing, room);

    /// <summary>Carry limit for this game, or null if encumbrance is disabled.</summary>
    public int? CarryLimit => Engine.Game.CarryLimit;

    /// <summary>Current total carry load. See <see cref="GameState.TotalLoad"/>.</summary>
    public int TotalLoad => State.TotalLoad;

    /// <summary>True when <see cref="Encumbrance.CanTake"/> allows picking up <paramref name="thing"/>.</summary>
    public bool CanCarry(Thing thing) => Encumbrance.CanTake(this, thing);

    /// <summary>True when the player is not holding anything in inventory.</summary>
    public bool HandsEmpty => Encumbrance.HandsEmpty(this);

    /// <summary>The room a thing currently sits in, or null if it is carried, worn, or offstage.</summary>
    public Room? RoomOf(Thing thing) => State.RoomOf(thing);

    /// <summary>
    /// Things in <paramref name="room"/>. By default only loose floor items are returned.
    /// When <paramref name="includePresent"/> is true, also includes things inside in-room
    /// containers and things carried or worn by the player while in that room (including items
    /// nested inside carried containers). For room contents excluding inventory, use
    /// <see cref="ThingsLocatedIn"/>.
    /// </summary>
    public IEnumerable<Thing> ThingsIn(Room room, bool includePresent = false) =>
        includePresent
            ? World.Things.Where(t => RoomOf(t) == room)
            : State.ContentsOf(room);

    /// <summary>
    /// Everything physically in the room (floor, containers, creatures), excluding the player's
    /// inventory and worn items.
    /// </summary>
    public IEnumerable<Thing> ThingsLocatedIn(Room room) =>
        World.Things.Where(t => State.IsLocatedIn(t, room));

    /// <summary>Things physically in the player's current room. See <see cref="ThingsLocatedIn"/>.</summary>
    public IEnumerable<Thing> ThingsLocatedHere() => ThingsLocatedIn(CurrentRoom);

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

    /// <summary>Runs the per-turn room refresh hook when the game defines one.</summary>
    public void RefreshRoomDisplay() => Engine.RefreshRoomDisplay();

    /// <summary>Legacy alias for <see cref="RefreshRoomDisplay"/>.</summary>
    public void PrintRoomBanner() => RefreshRoomDisplay();

    /// <summary>
    /// Recomputes title/status bars mid-turn (e.g. after spending money during slots). Interactive
    /// hosts pump one frame so the bar paints before the verb continues.
    /// </summary>
    public void RefreshStatusBars()
    {
        Engine.RefreshStatusBars();
        if (Engine.PresentationDelay is { } delay)
            delay(0);
    }

    /// <summary>Ends the game. <paramref name="won"/> selects the win or lose framing.</summary>
    public void EndGame(bool won, string? message = null)
    {
        if (message is not null) Say(message);
        State.EndGame(won);
    }

    public void Win(string message) => EndGame(true, message);
    public void Lose(string message) => EndGame(false, message);

    /// <summary>
    /// Player death. Runs <see cref="Game.OnDeath"/> handlers; ends the game unless one sets
    /// <see cref="DeathContext.Revived"/>.
    /// </summary>
    public void Die(string message)
    {
        if (State.IsOver) return;
        Say(message);

        DeathContext death = new(this);
        foreach (Action<DeathContext> handler in Engine.Game.OnDeath)
        {
            handler(death);
            if (death.Revived) return;
        }

        State.EndGame(false);
    }

    /// <summary>Schedules a one-shot action to run after <paramref name="turns"/> player turns.</summary>
    public void ScheduleIn(int turns, Action<GameContext> action, Func<GameContext, bool>? when = null) =>
        Engine.ScheduleIn(null, turns, action, when);

    /// <summary>Schedules a named one-shot action; an existing timer with the same name is cancelled.</summary>
    public void ScheduleIn(string name, int turns, Action<GameContext> action, Func<GameContext, bool>? when = null) =>
        Engine.ScheduleIn(name, turns, action, when);

    /// <summary>Cancels a named scheduled timer before it fires.</summary>
    public void CancelSchedule(string name) => Engine.CancelSchedule(name);
}
