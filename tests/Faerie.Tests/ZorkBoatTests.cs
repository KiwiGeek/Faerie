using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkBoatTests
{
    private static Room RiverWithLandingTo(Room beach, Direction fromBeach)
    {
        Direction riverDir = fromBeach switch
        {
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(fromBeach))
        };
        return beach.Exits[fromBeach].Destination;
    }

    private static (GameEngine engine, InMemoryTerminal term, Room damBase, Room river4, Room midRiver, Room beach1, Thing boat, Thing pump)
        BuildAtDam()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room damBase = game.World.Rooms.First(r => r.Name == "Dam Base");
        Room river4 = game.World.Rooms.First(r =>
            r.Exits.TryGetValue(Direction.West, out Exit? west) && west.Destination == damBase);
        Room beach1 = game.World.Rooms.First(r =>
            r.Name == "White Cliffs Beach" &&
            r.Exits.TryGetValue(Direction.East, out Exit? east) &&
            east.Destination.Exits.TryGetValue(Direction.West, out Exit? back) &&
            back.Destination == r);
        Room midRiver = RiverWithLandingTo(beach1, Direction.East);
        Thing boat = game.World.Things.First(t => t.Nouns.Contains("boat"));
        Thing plastic = game.World.Things.First(t => t.Nouns.Contains("plastic"));
        Thing pump = game.World.Things.First(t => t.Nouns.Contains("pump"));
        Thing lantern = game.World.Things.First(t => t.Nouns.Contains("lantern"));

        engine.MovePlayerTo(damBase);
        engine.State.TakeIntoInventory(pump);
        engine.State.TakeIntoInventory(plastic);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);

        return (engine, term, damBase, river4, midRiver, beach1, boat, pump);
    }

    private static void InflateBoat(GameEngine engine, Thing plastic, Thing boat)
    {
        engine.SubmitLine("inflate plastic");
        Assert.True(engine.State.IsCarried(boat));
    }

    [Fact]
    public void Zork_Boat_BoardLaunchAndLand_ReachesBeach()
    {
        (GameEngine engine, InMemoryTerminal term, Room damBase, Room river4, Room midRiver, Room beach1, Thing boat, Thing plastic) = BuildAtDam();

        InflateBoat(engine, plastic, boat);
        engine.SubmitLine("board boat");
        engine.SubmitLine("launch");
        Assert.Equal(river4, engine.State.CurrentRoom);

        engine.SubmitLine("down");
        engine.SubmitLine("down");
        engine.SubmitLine("down");
        Assert.Equal(midRiver, engine.State.CurrentRoom);

        engine.SubmitLine("west");
        Assert.Equal(beach1, engine.State.CurrentRoom);

        engine.SubmitLine("disembark");
        Assert.Equal(beach1, engine.State.RoomOf(boat));

        term.Reset();
        engine.SubmitLine("north");
        Assert.NotEqual(beach1, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_Boat_DisembarkOnRiver_IsBlocked()
    {
        (GameEngine engine, InMemoryTerminal term, _, Room river4, _, _, Thing boat, Thing plastic) = BuildAtDam();

        InflateBoat(engine, plastic, boat);
        engine.SubmitLine("board boat");
        engine.SubmitLine("launch");
        term.Reset();
        engine.SubmitLine("disembark");

        Assert.Equal(river4, engine.State.CurrentRoom);
        Assert.Contains("fatal", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Boat_BoardWithSword_IsBlocked()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, _, _, Thing boat, Thing plastic) = BuildAtDam();
        Thing sword = engine.Game.World.Things.First(t => t.Nouns.Contains("sword"));

        InflateBoat(engine, plastic, boat);
        engine.State.TakeIntoInventory(sword);
        term.Reset();
        engine.SubmitLine("board boat");

        Assert.Contains("sharp", term.Output, StringComparison.OrdinalIgnoreCase);
        engine.SubmitLine("drop sword");
        term.Reset();
        engine.SubmitLine("board boat");
        Assert.Contains("now in", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Boat_TakeOnBeach_IsBlocked()
    {
        (GameEngine engine, InMemoryTerminal term, _, Room river4, Room midRiver, Room beach1, Thing boat, Thing plastic) = BuildAtDam();

        InflateBoat(engine, plastic, boat);
        engine.SubmitLine("board boat");
        engine.SubmitLine("launch");
        engine.SubmitLine("down");
        engine.SubmitLine("down");
        engine.SubmitLine("down");
        engine.SubmitLine("west");
        engine.SubmitLine("disembark");

        term.Reset();
        engine.SubmitLine("take boat");

        Assert.Contains("awkward", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beach1, engine.State.RoomOf(boat));
    }
}
