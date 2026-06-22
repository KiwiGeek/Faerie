using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>Helpers for pourable/drinkable fluids stored inside openable containers.</summary>
public static class Fluid
{
    /// <summary>True when <paramref name="fluid"/> is inside <paramref name="container"/>.</summary>
    public static bool ContainerHolds(GameState state, Thing container, Thing fluid) =>
        state.ContentsOf(container).Contains(fluid);

    /// <summary>True when the container is open and holds the fluid.</summary>
    public static bool CanPourFrom(GameState state, Thing container, Thing fluid) =>
        container.Has(Attr.Open) && ContainerHolds(state, container, fluid);

    /// <summary>Removes <paramref name="fluid"/> from play when held in <paramref name="container"/> or carried loose.</summary>
    public static bool TryConsume(GameContext ctx, Thing container, Thing fluid, Thing? offered = null)
    {
        if (offered == fluid)
        {
            ctx.Remove(fluid);
            return true;
        }

        if (offered == container && ContainerHolds(ctx.State, container, fluid))
        {
            ctx.Remove(fluid);
            return true;
        }

        return false;
    }

    /// <summary>Drops an open container in the current room after its contents are consumed.</summary>
    public static void DropOpenContainer(GameContext ctx, Thing container)
    {
        if (ctx.Carrying(container))
        {
            ctx.Remove(container);
            ctx.PlaceHere(container);
        }

        container.Set(Attr.Open, true);
    }
}
