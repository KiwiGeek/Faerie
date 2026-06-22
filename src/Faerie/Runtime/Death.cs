using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>Helpers for death and revival sequences.</summary>
public static class Death
{
    /// <summary>
    /// Moves carried and worn items to random rooms. Treasures use
    /// <paramref name="treasureRooms"/>; everything else uses <paramref name="junkRooms"/>.
    /// </summary>
    public static void ScatterCarried(
        GameContext ctx,
        Func<Thing, bool> isTreasure,
        IReadOnlyList<Room> treasureRooms,
        IReadOnlyList<Room> junkRooms,
        Action<GameContext, Thing>? relocate = null)
    {
        if (treasureRooms.Count == 0 && junkRooms.Count == 0) return;

        List<Thing> carried = ctx.State.Inventory.Concat(ctx.State.Worn).ToList();
        foreach (Thing thing in carried)
        {
            relocate?.Invoke(ctx, thing);
            if (!ctx.State.IsCarried(thing)) continue;

            IReadOnlyList<Room> pool = isTreasure(thing) ? treasureRooms : junkRooms;
            if (pool.Count == 0) continue;

            Room dest = pool[ctx.Random.Next(pool.Count)];
            thing.Set(Attr.Concealed, false);
            ctx.Move(thing, Placement.InRoom(dest));
        }
    }
}
