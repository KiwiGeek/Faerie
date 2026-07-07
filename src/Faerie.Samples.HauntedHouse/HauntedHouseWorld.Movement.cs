using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using static Faerie.Verbs.StandardVerbIds;

namespace Faerie.Samples.HauntedHouse;

internal sealed partial class HauntedHouseWorld
{
    private Room? _roomBeforeMove;

    private void DefineMovementHooks()
    {
        FrontLobby.OnEnter = LobbyEnter;
        CobwebbedRoom.OnTurn = CobwebbedRoomTurn;

        _house.Reactions.BeforeAny(MovementBefore);
        _house.Reactions.AfterAny(MovementAfter);
    }

    private void LobbyEnter(GameContext ctx)
    {
        if (!ctx.Get(LobbyDoorOpen)) return;
        ctx.Set(LobbyDoorOpen, false);
        ctx.Say("The door slams shut behind you!");
    }

    private void CobwebbedRoomTurn(GameContext ctx)
    {
        if (ctx.Get(VacuumOn) || ctx.Random.Next(2) != 0) return;
        ctx.Set(GhostsBlocking, true);
    }

    private VerbResult MovementBefore(VerbContext ctx)
    {
        if (ctx.Verb.Id != Go) return VerbResult.Pass;

        if (ctx.Direction is not null)
            _roomBeforeMove = ctx.CurrentRoom;

        return CheckMovementBlock(ctx);
    }

    private void MovementAfter(VerbContext ctx)
    {
        if (ctx.Verb.Id != Go || ctx.Direction is null) return;

        if (_roomBeforeMove is not null && ctx.CurrentRoom != _roomBeforeMove)
            ctx.Say("Ready");

        _roomBeforeMove = null;
    }

    private VerbResult CheckMovementBlock(VerbContext ctx)
    {
        if (ctx.Direction is null) return VerbResult.Pass;

        if (ctx.Get(TreeClimbing) && ctx.CurrentRoom == ThickForest)
        {
            ctx.Say("crash! You fell out of the tree!");
            ctx.Set(TreeClimbing, false);
            return VerbResult.Done;
        }

        if (ctx.Get(GhostsBlocking) && ctx.CurrentRoom == UpperGallery)
        {
            ctx.Say("Ghosts will not let you move!");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == IceColdChamber && ctx.Carrying(Painting) && !ctx.Get(SpellsBarrierDown))
        {
            ctx.Say("A magical barrier has appeared");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == DarkAlcove && !HasLight(ctx) &&
            (ctx.Direction == Direction.North || ctx.Direction == Direction.West))
        {
            ctx.Say("It is too dark to move and you need a light. Have you found the glove and candle?");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == Marsh && !ctx.Carrying(SmallBoat))
        {
            ctx.Die("You are stuck! For you the game is over. Better luck next time.");
            return VerbResult.Done;
        }

        if (ctx.Carrying(SmallBoat) && !IsMarshArea(ctx.CurrentRoom))
        {
            ctx.Say("You cannot carry a boat!");
            return VerbResult.Done;
        }

        if (IsDarkHall(ctx.CurrentRoom) && !HasLight(ctx))
        {
            ctx.Say("It is too dark to move");
            return VerbResult.Done;
        }

        if (ctx.Get(VampiresPresent) && ctx.CurrentRoom == RearTurret && ctx.Random.Next(3) != 2)
        {
            ctx.Say("{fg:red}{bold}VAMPIRES ATTACKING!{/}{/}");
            return VerbResult.Done;
        }

        return VerbResult.Pass;
    }

    private bool IsMarshArea(Room room) =>
        room == MarshByWall || room == Marsh || room == SoggyPath || room == CliffPathMarsh;

    private bool IsDarkHall(Room room) =>
        room == ImpressiveHallway || room == LockedDoorHall || room == TrophyRoom;
}
