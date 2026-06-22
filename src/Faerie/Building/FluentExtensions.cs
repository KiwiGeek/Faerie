using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Building;

/// <summary>Fluent helpers for <see cref="Exit"/> conditions and room connections.</summary>
public static class ExitFluent
{
    /// <summary>Sets a gate returning null to pass or a message to block.</summary>
    public static Exit When(this Exit exit, Func<GameContext, string?> gate)
    {
        exit.Gate = ctx => gate(ctx);
        return exit;
    }

    /// <summary>Sets <see cref="Exit.Condition"/> and optional <see cref="Exit.BlockedMessage"/>.</summary>
    public static Exit When(this Exit exit, Func<GameContext, bool> condition, string? blocked = null)
    {
        exit.Condition = condition;
        exit.BlockedMessage = blocked;
        return exit;
    }

    /// <summary>Opens the exit when <paramref name="key"/> is true.</summary>
    public static Exit When(this Exit exit, StateKey<bool> key, string? blocked = null) =>
        exit.When(ctx => ctx.Get(key), blocked);

    /// <summary>Blocks passage while the player is holding anything (worn items are allowed).</summary>
    public static Exit RequiresEmptyHands(this Exit exit, string? blocked = null) =>
        exit.When(ctx => Encumbrance.HandsEmpty(ctx), blocked ?? Encumbrance.DefaultEmptyHandsMessage);

    /// <summary>Blocks passage while <see cref="GameState.TotalLoad"/> is greater than zero.</summary>
    public static Exit RequiresNoLoad(this Exit exit, string? blocked = null) =>
        exit.When(ctx => Encumbrance.NoLoad(ctx), blocked ?? Encumbrance.DefaultNoLoadMessage);

    public static Exit OnPass(this Exit exit, Action<GameContext> onPass)
    {
        exit.OnPass = ctx => { onPass(ctx); return true; };
        return exit;
    }

    public static Exit OnPass(this Exit exit, Func<GameContext, bool> onPass)
    {
        exit.OnPass = onPass;
        return exit;
    }

    internal static void ApplyGate(
        Exit exit,
        Func<GameContext, bool>? when,
        string? blocked,
        Func<GameContext, string?>? gate = null,
        Func<GameContext, bool>? onPass = null)
    {
        if (gate is not null)
            exit.Gate = ctx => gate(ctx);
        else if (when is not null)
        {
            exit.Condition = when;
            exit.BlockedMessage = blocked;
        }

        if (onPass is not null)
            exit.OnPass = onPass;
    }

    internal static void ApplyReciprocalExit(
        Room from,
        Direction direction,
        Room to,
        bool reciprocal,
        Func<GameContext, bool>? when,
        string? blocked,
        Func<GameContext, string?>? gate,
        Func<GameContext, bool>? onPass)
    {
        if (!reciprocal) return;

        Direction opposite = direction.Opposite();
        if (to.ExitTo(opposite) is not null) return;

        Exit reciprocalExit = to.SetExit(opposite, from);
        ApplyGate(reciprocalExit, when, blocked, gate, onPass);
    }
}

/// <summary>
/// Fluent configuration helpers for rooms. Everything is wired by reference: you pass the actual
/// <see cref="Room"/> and <see cref="Thing"/> objects, never strings — except <see cref="RoomRef"/>
/// for forward-declared destinations.
/// </summary>
public static class RoomFluent
{
    public static Room Describe(this Room room, string description) { room.Description = description; return room; }
    public static Room Describe(this Room room, Func<GameContext, string> factory) { room.DescriptionFactory = factory; return room; }

    /// <summary>Sets the shorter description shown on re-entry to a visited room (LOOK shows the full one).</summary>
    public static Room Brief(this Room room, string description) { room.BriefDescription = description; return room; }

    /// <summary>Sets a dynamic brief description shown on re-entry.</summary>
    public static Room Brief(this Room room, Func<GameContext, string> factory) { room.BriefDescriptionFactory = factory; return room; }

    /// <summary>Sets the one-line title shown in Sierra-style room banners.</summary>
    public static Room ShortTitle(this Room room, string title) { room.ShortTitle = title; return room; }

    /// <summary>Sets a dynamic one-line title for Sierra-style room banners.</summary>
    public static Room ShortTitle(this Room room, Func<GameContext, string> factory) { room.ShortTitleFactory = factory; return room; }

    /// <summary>Marks the room as dark (needs a light source or <see cref="LitWhen"/> to see).</summary>
    public static Room Dark(this Room room, bool dark = true) { room.IsDark = dark; return room; }

    /// <summary>
    /// Ambient light from game state (daylight, open grating, etc.). Composes with <see cref="Dark"/>
    /// and carried or in-room light sources.
    /// </summary>
    public static Room LitWhen(this Room room, Func<GameContext, bool> factory)
    {
        room.IsLitFactory = factory;
        return room;
    }

    /// <summary>
    /// Runs <paramref name="action"/> when the player enters while <paramref name="when"/> is true.
    /// Chains with any existing <see cref="Room.OnEnter"/> handler.
    /// </summary>
    public static Room HazardOnEnter(this Room room, Func<GameContext, bool> when, Action<GameContext> action)
    {
        Action<GameContext>? prior = room.OnEnter;
        room.OnEnter = ctx =>
        {
            if (when(ctx))
            {
                action(ctx);
                if (ctx.State.IsOver) return;
            }
            prior?.Invoke(ctx);
        };
        return room;
    }

    /// <summary>Runs <paramref name="action"/> on every entry to this room.</summary>
    public static Room HazardOnEnter(this Room room, Action<GameContext> action) =>
        room.HazardOnEnter(_ => true, action);

    /// <summary>
    /// Connects this room to another in a direction. By default the reciprocal exit is created on
    /// the destination too. Returns the created <see cref="Exit"/> so doors/conditions can be added.
    /// </summary>
    public static Exit Connect(
        this Room room,
        Direction direction,
        Room destination,
        bool reciprocal = true,
        Func<GameContext, bool>? when = null,
        string? blocked = null,
        Func<GameContext, string?>? gate = null,
        Func<GameContext, bool>? onPass = null)
    {
        Exit exit = room.SetExit(direction, destination);
        ApplyGate(exit, when, blocked, gate, onPass);
        ExitFluent.ApplyReciprocalExit(room, direction, destination, reciprocal, when, blocked, gate, onPass);
        return exit;
    }

    /// <summary>
    /// Connects to a <see cref="RoomRef"/> destination that may not exist yet. The link is created
    /// when the destination room is registered with a matching <see cref="Element.Id"/>.
    /// </summary>
    public static Room Connect(
        this Room room,
        Direction direction,
        RoomRef destination,
        bool reciprocal = true,
        Func<GameContext, bool>? when = null,
        string? blocked = null,
        Func<GameContext, string?>? gate = null,
        Func<GameContext, bool>? onPass = null)
    {
        destination.QueueFrom(room, direction, reciprocal, when, blocked, gate, onPass);
        return room;
    }

    public static Room North(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.North, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room South(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.South, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room East(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.East, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room West(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.West, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room Up(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.Up, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room Down(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.Down, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room In(this Room room, Room dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.In, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room North(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.North, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room South(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.South, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room East(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.East, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room West(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.West, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room Up(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.Up, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room Down(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.Down, dest, reciprocal, when, blocked, gate, onPass); return room; }

    public static Room In(this Room room, RoomRef dest, bool reciprocal = true, Func<GameContext, bool>? when = null, string? blocked = null, Func<GameContext, string?>? gate = null, Func<GameContext, bool>? onPass = null)
    { room.Connect(Direction.In, dest, reciprocal, when, blocked, gate, onPass); return room; }

    /// <summary>Places things in this room as their starting location.</summary>
    public static Room Contains(this Room room, params Thing[] things)
    {
        foreach (Thing thing in things) thing.InitialPlacement = Placement.InRoom(room);
        return room;
    }

    private static void ApplyGate(Exit exit, Func<GameContext, bool>? when, string? blocked, Func<GameContext, string?>? gate, Func<GameContext, bool>? onPass) =>
        ExitFluent.ApplyGate(exit, when, blocked, gate, onPass);
}

/// <summary>Fluent configuration helpers for things (items, scenery, doors, creatures).</summary>
public static class ThingFluent
{
    public static Thing Describe(this Thing thing, string description) { thing.Description = description; return thing; }
    public static Thing Describe(this Thing thing, Func<GameContext, string> factory) { thing.DescriptionFactory = factory; return thing; }

    /// <summary>Adds words the player can use to refer to this thing.</summary>
    public static Thing Called(this Thing thing, params string[] nouns)
    {
        foreach (string n in nouns)
            if (!thing.Nouns.Contains(n.ToLowerInvariant())) thing.Nouns.Add(n.ToLowerInvariant());
        return thing;
    }

    /// <summary>Adds disambiguating adjectives ("brass", "rusty").</summary>
    public static Thing Adjectives(this Thing thing, params string[] adjectives)
    {
        foreach (string a in adjectives)
            if (!thing.Adjectives.Contains(a.ToLowerInvariant())) thing.Adjectives.Add(a.ToLowerInvariant());
        return thing;
    }

    public static Thing Article(this Thing thing, string article) { thing.Article = article; return thing; }
    public static Thing Proper(this Thing thing) { thing.Article = ""; return thing; }

    public static Thing Takeable(this Thing thing, bool value = true) { thing.Set(Attr.Takeable, value); if (value) thing.Set(Attr.Fixed, false); return thing; }

    /// <summary>Sets encumbrance weight for carry-limit checks.</summary>
    public static Thing Size(this Thing thing, int size)
    {
        thing.Size = size;
        return thing;
    }

    /// <summary>
    /// Allows PUT (and similar) to pass carried objects through this opening into
    /// <paramref name="destination"/>.
    /// </summary>
    public static Thing PassObjectsTo(
        this Thing thing,
        Room destination,
        int? maxSize = null,
        string? tooLargeMessage = null,
        string? successMessage = null)
    {
        thing.PassageDestination = destination;
        thing.PassageMaxSize = maxSize;
        thing.PassageTooLargeMessage = tooLargeMessage;
        thing.PassageSuccessMessage = successMessage;
        return thing;
    }

    public static Thing Fixed(this Thing thing) { thing.Set(Attr.Fixed); thing.Set(Attr.Takeable, false); return thing; }
    public static Thing Plural(this Thing thing) { thing.Set(Attr.Plural); return thing; }
    public static Thing Concealed(this Thing thing, bool value = true) { thing.Set(Attr.Concealed, value); return thing; }

    public static Thing Container(this Thing thing, bool open = false)
    {
        thing.Set(Attr.Container);
        thing.Set(Attr.Openable);
        thing.Set(Attr.Open, open);
        return thing;
    }

    public static Thing Supporter(this Thing thing) { thing.Set(Attr.Supporter); return thing; }

    public static Thing Openable(this Thing thing, bool open = false)
    {
        thing.Set(Attr.Openable);
        thing.Set(Attr.Open, open);
        return thing;
    }

    /// <summary>Makes the thing lockable, locked by default, opened by <paramref name="key"/>.</summary>
    public static Thing LockedWith(this Thing thing, Thing key)
    {
        thing.Set(Attr.Openable);
        thing.Set(Attr.Lockable);
        thing.Set(Attr.Locked);
        thing.Set(Attr.Open, false);
        thing.Key = key;
        return thing;
    }

    public static Thing LightSource(this Thing thing, bool lit = false)
    {
        thing.Set(Attr.LightSource);
        thing.Set(Attr.Lit, lit);
        thing.Set(Attr.Switchable);
        thing.Set(Attr.On, lit);
        return thing;
    }

    /// <summary>
    /// Marks an open flame (candles, torch, match). Use <see cref="LightSource"/> for battery lanterns.
    /// </summary>
    public static Thing OpenFlame(this Thing thing, bool lit = false)
    {
        thing.Set(Attr.Flame);
        thing.LightSource(lit);
        return thing;
    }

    /// <summary>Can be broken with the standard <c>break</c> verb.</summary>
    public static Thing Breakable(this Thing thing, string? alreadyBrokenMessage = null, string? successMessage = null)
    {
        thing.Set(Attr.Breakable);
        thing.BreakAlreadyMessage = alreadyBrokenMessage;
        thing.BreakSuccessMessage = successMessage;
        return thing;
    }

    /// <summary>Registers <paramref name="mirror"/> as the mirror terminal in <paramref name="room"/>.</summary>
    public static Thing MirrorIn(this Thing mirror, MirrorPair pair, Room room)
    {
        pair.RegisterMirror(mirror, room);
        mirror.MirrorLink = pair;
        mirror.Fixed().Breakable();
        return mirror;
    }

    public static Thing Switchable(this Thing thing, bool on = false)
    {
        thing.Set(Attr.Switchable);
        thing.Set(Attr.On, on);
        return thing;
    }

    public static Thing Wearable(this Thing thing) { thing.Set(Attr.Wearable); thing.Set(Attr.Takeable); return thing; }
    public static Thing Edible(this Thing thing) { thing.Set(Attr.Edible); thing.Set(Attr.Takeable); return thing; }
    public static Thing Drinkable(this Thing thing) { thing.Set(Attr.Drinkable); return thing; }

    /// <summary>
    /// Names this stock item for purchase when <paramref name="vendor"/> is in the current room,
    /// even before the item has been spawned into the world.
    /// </summary>
    public static Thing OrderableFrom(this Thing thing, Thing vendor)
    {
        thing.Set(Attr.Orderable);
        thing.Vendor = vendor;
        return thing;
    }

    public static Thing Readable(this Thing thing, string text)
    {
        thing.Set(Attr.Readable);
        thing.ReadableText = text;
        return thing;
    }

    public static Thing Animate(this Thing thing) { thing.Set(Attr.Animate); thing.Set(Attr.Fixed); return thing; }

    /// <summary>A custom one-line listing for when the thing sits in its original spot in a room.</summary>
    public static Thing InitialText(this Thing thing, string line) { thing.InitialDescription = line; return thing; }

    public static Thing StartsIn(this Thing thing, Room room) { thing.InitialPlacement = Placement.InRoom(room); return thing; }
    public static Thing StartsCarried(this Thing thing) { thing.InitialPlacement = Placement.Carried; return thing; }
    public static Thing StartsWorn(this Thing thing) { thing.InitialPlacement = Placement.Worn; thing.Set(Attr.Worn); return thing; }
    public static Thing StartsInside(this Thing thing, Thing container) { thing.InitialPlacement = Placement.Inside(container); return thing; }
    public static Thing StartsOn(this Thing thing, Thing supporter) { thing.InitialPlacement = Placement.On(supporter); return thing; }
}
