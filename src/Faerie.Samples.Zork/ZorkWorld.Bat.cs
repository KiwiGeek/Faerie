using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Samples.Zork;

/// <summary>Bat Room — Infocom bat steal/repel (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private Room ShaftRoom => R(ZorkIds.ShaftRoom);

    private void DefineBatAndGarlic()
    {
        BatRoom.OnEnter = BatRoomOnEnter;
    }

    private void BatRoomOnEnter(GameContext ctx)
    {
        if (HasGarlic(ctx, Garlic))
        {
            ctx.Say("In the corner of the room, a small chimney leads upward. " +
                    "The smell of garlic drives the bat away.");
            return;
        }

        TryBatSteal(ctx);
    }

    private void TryBatSteal(GameContext ctx)
    {
        Thing? stolen = ctx.State.Inventory.FirstOrDefault(IsTreasure);
        if (stolen is null)
        {
            ctx.Die("A bat swoops down and carries you off into the darkness.");
            return;
        }

        ctx.Remove(stolen);
        ctx.State.Move(stolen, Placement.InRoom(ShaftRoom));
        ctx.Say("The bat picks you up and flits away through a crack in the ceiling. " +
                "You are dropped into a small room. The bat leaves, carrying something shiny with it.");
        ctx.MovePlayerTo(ShaftRoom);
    }

    private static bool HasGarlic(GameContext ctx, Thing garlic) =>
        ctx.Carrying(garlic) || ctx.Wearing(garlic);
}
