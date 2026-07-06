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
        // Bats attack on any action taken in the rear turret except spraying them (1/3 chance
        // of being left alone each turn), matching the original's flag/verb check exactly.
        if (ctx.Get(BatsPresent) && ctx.CurrentRoom == RearTurret &&
            ctx.Verb.Id != "spray" && ctx.Random.Next(3) != 2)
        {
            ctx.Say("{fg:red}{bold}BATS ATTACKING!{/}{/}");
            return VerbResult.Done;
        }

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

        if (ctx.CurrentRoom == PoolOfLight && !HasLight(ctx) &&
            (ctx.Direction == Direction.North || ctx.Direction == Direction.East))
        {
            ctx.Say("It is too dark to move and you need a light. Have you found the candlestick and candle?");
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

        return VerbResult.Pass;
    }

    private bool IsMarshArea(Room room) =>
        room == MarshByWall || room == Marsh || room == SoggyPath || room == CliffPathMarsh;

    private bool IsDarkHall(Room room) =>
        room == ImpressiveHallway || room == LockedDoorHall || room == TrophyRoom;
}
