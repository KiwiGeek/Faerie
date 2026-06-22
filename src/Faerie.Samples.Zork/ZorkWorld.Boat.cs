using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Frigid River navigation — board, launch, land, disembark (Infocom Zork I).</summary>
internal sealed partial class ZorkWorld
{
    private Verb _board = null!;
    private Verb _disembark = null!;
    private Verb _launch = null!;

    private Room DamBase => R(ZorkIds.DamBase);
    private Room River1 => R(ZorkIds.River1);
    private Room River2 => R(ZorkIds.River2);
    private Room River3 => R(ZorkIds.River3);
    private Room River4 => R(ZorkIds.River4);
    private Room Beach1 => R(ZorkIds.Beach1);
    private Room Beach2 => R(ZorkIds.Beach2);
    private Room SandyBeach => R(ZorkIds.SandyBeach);
    private Room Shore => R(ZorkIds.Shore);

    private void DefineBoatRiver()
    {
        _board = _b.DefineVerb("board", ["board", "get in"],
            VerbForms.Transitive | VerbForms.Ditransitive, BoardHandler);
        _disembark = _b.DefineVerb("disembark", ["disembark", "get out", "stand", "land"],
            VerbForms.Transitive | VerbForms.Intransitive, DisembarkHandler);
        _launch = _b.DefineVerb("launch", ["launch"], VerbForms.Intransitive, LaunchHandler);

        _b.On(Boat).Before(_board, BoardBoatReaction);
        _b.On(Boat).Before(_b.Verbs.Take!, BoatTakeBlocked);
        _b.On(Boat).Before(_deflate, ctx =>
        {
            if (!ctx.Get(_inBoat)) return VerbResult.Pass;
            ctx.Say("You can't deflate the boat while you're in it.");
            return VerbResult.Done;
        });

        WireBoatMovementSync();
        WireBoatRoomEnter();
    }

    private void ConfigureBoatExits()
    {
        BlockFootEntryToRiver();
        GateRiverMovementUnlessAboard();
        GateRiverLandings();
        GateBeachMovementWhileAboard();
    }

    private void BlockFootEntryToRiver()
    {
        string swim = "The river is very treacherous. Perhaps you can cross it downstream.";
        GateExit(ZorkIds.DamBase, Direction.East, swim);
        GateExit(ZorkIds.Beach1, Direction.East, swim);
        GateExit(ZorkIds.Beach2, Direction.East, swim);
        GateExit(ZorkIds.SandyBeach, Direction.West, swim);
        GateExit(ZorkIds.Shore, Direction.West, swim);
    }

    private void GateRiverMovementUnlessAboard()
    {
        string swept = "You'd be swept away by the current.";
        foreach (Room river in new[] { River1, FrigidRiver, River2, River3, River4 })
        {
            foreach (Exit exit in river.Exits.Values)
            {
                if (exit.Destination == river) continue;
                exit.Gate = CombineGate(exit.Gate, ctx =>
                    ctx.Get(_inBoat) ? ExitGate.Open : ExitGate.Block(swept));
            }
        }
    }

    private void GateRiverLandings()
    {
        string needBoat = "You'd be swept away by the current.";
        GateExit(ZorkIds.FrigidRiver, Direction.West, needBoat, requireBoat: true);
        GateExit(ZorkIds.FrigidRiver, Direction.East, needBoat, requireBoat: true);
        GateExit(ZorkIds.River2, Direction.West, needBoat, requireBoat: true);
        GateExit(ZorkIds.River4, Direction.West, needBoat, requireBoat: true);
        GateExit(ZorkIds.River1, Direction.East, needBoat, requireBoat: true);
    }

    private void GateBeachMovementWhileAboard()
    {
        string disembarkFirst = "You'll have to disembark first.";
        foreach (Room beach in new[] { Beach1, Beach2, SandyBeach })
        {
            foreach (Exit exit in beach.Exits.Values)
            {
                if (exit.Destination == beach) continue;
                exit.Gate = CombineGate(exit.Gate, ctx =>
                    ctx.Get(_inBoat) ? ExitGate.Block(disembarkFirst) : ExitGate.Open);
            }
        }

        foreach ((Direction dir, Exit exit) in DamBase.Exits)
        {
            if (exit.Destination == DamBase) continue;
            if (dir is Direction.Up or Direction.North)
            {
                exit.Gate = CombineGate(exit.Gate, ctx =>
                    ctx.Get(_inBoat) ? ExitGate.Block("You can't go there in a magic boat.") : ExitGate.Open);
            }
            else
            {
                exit.Gate = CombineGate(exit.Gate, ctx =>
                    ctx.Get(_inBoat) ? ExitGate.Block(disembarkFirst) : ExitGate.Open);
            }
        }
    }

    private static Func<GameContext, ExitGate> CombineGate(
        Func<GameContext, ExitGate>? existing,
        Func<GameContext, ExitGate> added) =>
        ctx =>
        {
            if (existing is not null)
            {
                ExitGate prior = existing(ctx);
                if (!prior.CanPass) return prior;
            }

            return added(ctx);
        };

    private void GateExit(string from, Direction dir, string blocked, bool requireBoat = false)
    {
        if (R(from).ExitTo(dir) is not { } exit) return;
        exit.Gate = CombineGate(exit.Gate, ctx =>
        {
            if (requireBoat)
                return ctx.Get(_inBoat) ? ExitGate.Open : ExitGate.Block(blocked);
            return ExitGate.Block(blocked);
        });
    }

    private void WireBoatMovementSync()
    {
        _b.EveryTurn(ctx =>
        {
            if (!ctx.Get(_inBoat)) return;
            if (ctx.RoomOf(Boat) != ctx.CurrentRoom)
                ctx.State.Move(Boat, Placement.InRoom(ctx.CurrentRoom));
        }, when: ctx => ctx.Get(_inBoat));
    }

    private void WireBoatRoomEnter()
    {
        void OnEnter(GameContext ctx)
        {
            if (!ctx.Get(_inBoat)) return;
            SyncBoatToRoom(ctx);
            ctx.Say("You are in the magic boat.");
        }

        foreach (Room room in new Room[] { River1, FrigidRiver, River2, River3, River4, Beach1, Beach2, SandyBeach, DamBase, Shore })
            room.OnEnter = ChainOnEnter(room.OnEnter, OnEnter);
    }

    private static Action<GameContext>? ChainOnEnter(Action<GameContext>? existing, Action<GameContext> added) =>
        existing is null ? added : ctx => { existing(ctx); added(ctx); };

    private VerbResult BoardHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not null && ctx.DirectObject != Boat)
        {
            ctx.Say("You can only board the magic boat.");
            return VerbResult.Done;
        }

        return BoardBoatReaction(ctx);
    }

    private VerbResult BoardBoatReaction(VerbContext ctx)
    {
        if (ctx.Get(_inBoat))
        {
            ctx.Say("You're already in the boat.");
            return VerbResult.Done;
        }

        if (!ctx.Get(_boatInflated))
        {
            ctx.Say("The boat is deflated.");
            return VerbResult.Done;
        }

        if (!BoatAccessible(ctx))
        {
            ctx.Say("I don't see any boat here.");
            return VerbResult.Done;
        }

        if (CarryingPointyObject(ctx))
        {
            ctx.Say("The boat is fragile. You can't board while carrying something sharp.");
            return VerbResult.Done;
        }

        EnterBoat(ctx);
        ctx.Say("You are now in the magic boat.");
        return VerbResult.Done;
    }

    private VerbResult DisembarkHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not null && ctx.DirectObject != Boat)
        {
            ctx.Say("You can only disembark from the magic boat.");
            return VerbResult.Done;
        }

        if (!ctx.Get(_inBoat))
        {
            ctx.Say("You're not in the boat.");
            return VerbResult.Done;
        }

        if (IsRiverRoom(ctx.CurrentRoom))
        {
            ctx.Say("You realize, just in time, that disembarking here would probably be fatal.");
            return VerbResult.Done;
        }

        LeaveBoat(ctx);
        ctx.Say("You are on your own feet again.");
        return VerbResult.Done;
    }

    private VerbResult LaunchHandler(VerbContext ctx)
    {
        if (!ctx.Get(_inBoat))
        {
            ctx.Say("You have to be in the boat to launch it.");
            return VerbResult.Done;
        }

        Room? river = LaunchTarget(ctx.CurrentRoom);
        if (river is null)
        {
            ctx.Say("You can't launch here.");
            return VerbResult.Done;
        }

        ctx.MovePlayerTo(river);
        SyncBoatToRoom(ctx);
        return VerbResult.Done;
    }

    private VerbResult BoatTakeBlocked(VerbContext ctx)
    {
        if (ctx.Get(_inBoat))
        {
            ctx.Say("You're inside it.");
            return VerbResult.Done;
        }

        if (ctx.Get(_boatInflated) && IsBeachRoom(ctx.CurrentRoom))
        {
            ctx.Say("The inflated boat is too awkward to carry on the beach.");
            return VerbResult.Done;
        }

        return VerbResult.Pass;
    }

    private void EnterBoat(GameContext ctx)
    {
        if (ctx.Carrying(Boat))
            ctx.Remove(Boat);
        else if (ctx.LocatedIn(Boat, ctx.CurrentRoom))
            ctx.State.Move(Boat, Placement.InRoom(ctx.CurrentRoom));

        ctx.Set(_inBoat, true);
        Boat.Set(Attr.Concealed, true);
        SyncBoatToRoom(ctx);
    }

    private void LeaveBoat(GameContext ctx)
    {
        ctx.Set(_inBoat, false);
        Boat.Set(Attr.Concealed, false);
        SyncBoatToRoom(ctx);
    }

    private void SyncBoatToRoom(GameContext ctx) =>
        ctx.State.Move(Boat, Placement.InRoom(ctx.CurrentRoom));

    private bool BoatAccessible(GameContext ctx) =>
        ctx.Carrying(Boat) || ctx.LocatedIn(Boat, ctx.CurrentRoom);

    private bool IsRiverRoom(Room room) =>
        room == River1 || room == FrigidRiver || room == River2 || room == River3 || room == River4;

    private bool IsBeachRoom(Room room) =>
        room == Beach1 || room == Beach2 || room == SandyBeach;

    private Room? LaunchTarget(Room from)
    {
        if (from == DamBase) return River4;
        if (from == Beach1 || from == SandyBeach) return FrigidRiver;
        if (from == Beach2) return River2;
        if (from == Shore) return River1;
        return null;
    }

    private bool CarryingPointyObject(GameContext ctx) =>
        ctx.State.Inventory.Any(IsPointy);

    private bool IsPointy(Thing thing) =>
        thing == Sword || thing == NastyKnife || thing == RustyKnife || thing == Sceptre || thing == Shovel;

    private bool BoatPresentForFlood(GameContext ctx) =>
        ctx.Get(_inBoat) || ctx.Carrying(Boat) || ctx.LocatedIn(Boat, ctx.CurrentRoom);
}
