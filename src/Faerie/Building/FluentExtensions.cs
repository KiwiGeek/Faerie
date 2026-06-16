using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Building;

/// <summary>
/// Fluent configuration helpers for rooms. Everything is wired by reference: you pass the actual
/// <see cref="Room"/> and <see cref="Thing"/> objects, never strings.
/// </summary>
public static class RoomFluent
{
    public static Room Describe(this Room room, string description) { room.Description = description; return room; }
    public static Room Describe(this Room room, Func<GameContext, string> factory) { room.DescriptionFactory = factory; return room; }

    /// <summary>Sets the shorter description shown on re-entry to a visited room (LOOK shows the full one).</summary>
    public static Room Brief(this Room room, string description) { room.BriefDescription = description; return room; }

    /// <summary>Sets a dynamic brief description shown on re-entry.</summary>
    public static Room Brief(this Room room, Func<GameContext, string> factory) { room.BriefDescriptionFactory = factory; return room; }

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
    /// Connects this room to another in a direction. By default the reciprocal exit is created on
    /// the destination too. Returns the created <see cref="Exit"/> so doors/conditions can be added.
    /// </summary>
    public static Exit Connect(this Room room, Direction direction, Room destination, bool reciprocal = true)
    {
        Exit exit = room.SetExit(direction, destination);
        if (reciprocal && destination.ExitTo(direction.Opposite()) is null)
            destination.SetExit(direction.Opposite(), room);
        return exit;
    }

    public static Room North(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.North, dest, reciprocal); return room; }
    public static Room South(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.South, dest, reciprocal); return room; }
    public static Room East(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.East, dest, reciprocal); return room; }
    public static Room West(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.West, dest, reciprocal); return room; }
    public static Room Up(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.Up, dest, reciprocal); return room; }
    public static Room Down(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.Down, dest, reciprocal); return room; }
    public static Room In(this Room room, Room dest, bool reciprocal = true) { room.Connect(Direction.In, dest, reciprocal); return room; }

    // Note: the OnEnter / OnFirstEnter / OnTurn hooks are settable properties on Room
    // (e.g. room.OnEnter = ctx => ...). They are intentionally not offered as extension methods
    // because a method of the same name would shadow the property.

    /// <summary>Places things in this room as their starting location.</summary>
    public static Room Contains(this Room room, params Thing[] things)
    {
        foreach (Thing thing in things) thing.InitialPlacement = Placement.InRoom(room);
        return room;
    }
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

    public static Thing Switchable(this Thing thing, bool on = false)
    {
        thing.Set(Attr.Switchable);
        thing.Set(Attr.On, on);
        return thing;
    }

    public static Thing Wearable(this Thing thing) { thing.Set(Attr.Wearable); thing.Set(Attr.Takeable); return thing; }
    public static Thing Edible(this Thing thing) { thing.Set(Attr.Edible); thing.Set(Attr.Takeable); return thing; }
    public static Thing Drinkable(this Thing thing) { thing.Set(Attr.Drinkable); return thing; }

    public static Thing Readable(this Thing thing, string text)
    {
        thing.Set(Attr.Readable);
        thing.ReadableText = text;
        return thing;
    }

    public static Thing Animate(this Thing thing) { thing.Set(Attr.Animate); thing.Set(Attr.Fixed); return thing; }

    /// <summary>A custom one-line listing for when the thing sits in its original spot in a room.</summary>
    public static Thing InitialText(this Thing thing, string line) { thing.InitialDescription = line; return thing; }

    // Note: OnExamine / OnFirstExamine / OnTake are settable properties on Thing
    // (e.g. thing.OnExamine = ctx => ...), not extension methods, to avoid shadowing the property.

    // ---- initial placement ----------------------------------------------------------------

    public static Thing StartsIn(this Thing thing, Room room) { thing.InitialPlacement = Placement.InRoom(room); return thing; }
    public static Thing StartsCarried(this Thing thing) { thing.InitialPlacement = Placement.Carried; return thing; }
    public static Thing StartsWorn(this Thing thing) { thing.InitialPlacement = Placement.Worn; thing.Set(Attr.Worn); return thing; }
    public static Thing StartsInside(this Thing thing, Thing container) { thing.InitialPlacement = Placement.Inside(container); return thing; }
    public static Thing StartsOn(this Thing thing, Thing supporter) { thing.InitialPlacement = Placement.On(supporter); return thing; }
}
