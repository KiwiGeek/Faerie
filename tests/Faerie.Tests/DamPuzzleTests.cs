using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class DamPuzzleTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room maintenance, Room dam,
        Room reservoirSouth, Room reservoir, Thing wrench, Thing lantern)
        BuildAtDam()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room maintenance = game.World.Rooms.First(r => r.Name == "Maintenance Room");
        Room dam = game.World.Rooms.First(r => r.Name == "Dam");
        Room reservoirSouth = game.World.Rooms.First(r => r.Name == "Reservoir South");
        Room reservoir = game.World.Rooms.First(r => r.Name == "Reservoir");
        Thing wrench = game.World.Things.First(t => t.Name == "wrench");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        engine.State.TakeIntoInventory(wrench);
        engine.State.CurrentRoom = maintenance;

        return (engine, term, maintenance, dam, reservoirSouth, reservoir, wrench, lantern);
    }

    [Fact]
    public void Zork_Dam_BoltRequiresYellowButton()
    {
        (GameEngine engine, InMemoryTerminal term, _, Room dam, _, _, _, _) = BuildAtDam();

        engine.State.CurrentRoom = dam;
        term.Reset();
        engine.Submit("turn bolt");

        Assert.Contains("won't turn", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Dam_YellowButtonThenBolt_OpensGatesAndDrainsReservoir()
    {
        (GameEngine engine, InMemoryTerminal term, Room maintenance, Room dam,
            Room reservoirSouth, Room reservoir, _, _) = BuildAtDam();

        term.Reset();
        engine.Submit("push yellow button");
        Assert.Contains("Click.", term.Output);

        engine.State.CurrentRoom = dam;
        term.Reset();
        engine.Submit("turn bolt");
        Assert.Contains("sluice gates open", term.Output, StringComparison.OrdinalIgnoreCase);

        for (int i = 0; i < 8; i++)
            engine.Submit("wait");

        engine.State.CurrentRoom = reservoirSouth;
        term.Reset();
        engine.Submit("north");
        Assert.Equal(reservoir, engine.State.CurrentRoom);
        Assert.DoesNotContain("drown", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Dam_ReservoirBlockedBeforeDrain()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, Room reservoirSouth, Room reservoir, _, _) = BuildAtDam();

        engine.State.CurrentRoom = reservoirSouth;
        term.Reset();
        engine.Submit("north");

        Assert.NotEqual(reservoir, engine.State.CurrentRoom);
        Assert.Contains("drown", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Dam_BlueButtonJammedAfterLeakStarted()
    {
        (GameEngine engine, InMemoryTerminal term, Room maintenance, _, _, _, _, _) = BuildAtDam();

        term.Reset();
        engine.Submit("push blue button");
        Assert.Contains("rumbling", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("push blue button");
        Assert.Contains("jammed", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
