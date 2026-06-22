using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>
/// Helpers for area hazards keyed on open flames, poisons, and similar environmental dangers.
/// </summary>
public static class Hazards
{
    /// <summary>True when <paramref name="thing"/> is an active open flame (not a battery lantern).</summary>
    public static bool IsOpenFlame(Thing thing) =>
        thing.Has(Attr.Flame) && thing.Has(Attr.Lit);

    /// <summary>Open flames the player is carrying or wearing.</summary>
    public static bool HasCarriedOpenFlame(GameContext ctx) =>
        ctx.State.Inventory.Any(IsOpenFlame) || ctx.State.Worn.Any(IsOpenFlame);

    /// <summary>
    /// Open flames the player is carrying or wearing, plus any in
    /// <paramref name="room"/> (including nested open containers).
    /// </summary>
    public static IEnumerable<Thing> OpenFlames(GameContext ctx, Room? room = null)
    {
        foreach (Thing thing in ctx.State.Inventory)
            if (IsOpenFlame(thing)) yield return thing;
        foreach (Thing thing in ctx.State.Worn)
            if (IsOpenFlame(thing)) yield return thing;

        if (room is null) yield break;
        foreach (Thing thing in OpenFlamesInRoom(ctx, room))
            yield return thing;
    }

    /// <summary>True when any open flame is present on the player or in the room.</summary>
    public static bool HasOpenFlame(GameContext ctx, Room? room = null) =>
        OpenFlames(ctx, room ?? ctx.CurrentRoom).Any();

    private static IEnumerable<Thing> OpenFlamesInRoom(GameContext ctx, Room room)
    {
        foreach (Thing thing in ctx.State.ContentsOf(room))
        {
            foreach (Thing flame in OpenFlamesInThing(ctx, thing))
                yield return flame;
        }
    }

    private static IEnumerable<Thing> OpenFlamesInThing(GameContext ctx, Thing thing)
    {
        if (IsOpenFlame(thing)) yield return thing;
        if (!thing.Has(Attr.Container) || !thing.Has(Attr.Open)) yield break;
        foreach (Thing inner in ctx.State.ContentsOf(thing))
        {
            foreach (Thing flame in OpenFlamesInThing(ctx, inner))
                yield return flame;
        }
    }
}
