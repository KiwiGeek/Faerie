using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>
/// Links two rooms whose loose contents swap when the player rubs a registered mirror.
/// </summary>
public sealed class MirrorPair(Room roomA, Room roomB)
{
    public Room RoomA { get; } = roomA;
    public Room RoomB { get; } = roomB;

    /// <summary>When set, both mirrors share this broken flag.</summary>
    public StateKey<bool>? BrokenKey { get; init; }

    public Thing? MirrorA { get; private set; }
    public Thing? MirrorB { get; private set; }

    public bool IsBroken(GameContext ctx) => BrokenKey is not null && ctx.Get(BrokenKey);

    public Room Partner(Room room) =>
        room == RoomA ? RoomB : room == RoomB ? RoomA
        : throw new ArgumentException($"Room {room.Id} is not part of this mirror pair.");

    public void RegisterMirror(Thing mirror, Room room)
    {
        if (room == RoomA) MirrorA = mirror;
        else if (room == RoomB) MirrorB = mirror;
        else throw new ArgumentException($"Room {room.Id} is not part of this mirror pair.");
    }

    /// <summary>Things loose in <paramref name="room"/> that participate in a mirror swap.</summary>
    public IEnumerable<Thing> SwappableContents(GameState state, Room room)
    {
        foreach (Thing thing in state.ContentsOf(room))
        {
            if (thing == MirrorA || thing == MirrorB) continue;
            if (thing.Has(Attr.Fixed) || thing.Has(Attr.Scenery) || thing.Has(Attr.Animate)) continue;
            yield return thing;
        }
    }

    /// <summary>Exchanges portable contents between the two rooms.</summary>
    public void SwapContents(GameContext ctx)
    {
        Room here = ctx.CurrentRoom;
        Room there = Partner(here);
        List<Thing> fromHere = SwappableContents(ctx.State, here).ToList();
        List<Thing> fromThere = SwappableContents(ctx.State, there).ToList();
        foreach (Thing thing in fromHere)
            ctx.Move(thing, Placement.InRoom(there));
        foreach (Thing thing in fromThere)
            ctx.Move(thing, Placement.InRoom(here));
    }
}
