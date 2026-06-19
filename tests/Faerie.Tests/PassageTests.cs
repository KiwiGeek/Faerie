using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class PassageTests
{
    [Fact]
    public void Put_PassesCarriedItemThroughOpenPassage()
    {
        GameBuilder b = GameBuilder.Create("Pass").AddStandardVerbs();
        Room above = b.Room("Above").Describe("Above.");
        Room below = b.Room("Below").Describe("Below.");
        Thing grate = b.Scenery("grating").Called("grate").Openable(open: true).PassObjectsTo(below, maxSize: 20);
        above.Contains(grate);
        Thing coin = b.Item("coin").Size(5);
        b.StartIn(above);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(coin);
        term.Reset();
        engine.Submit("put coin in grate");

        Assert.Contains("goes through", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(engine.State.IsCarried(coin));
        Assert.Equal(below, engine.State.RoomOf(coin));
    }

    [Fact]
    public void Put_BlockedWhenPassageClosed()
    {
        GameBuilder b = GameBuilder.Create("Pass").AddStandardVerbs();
        Room above = b.Room("Above");
        Room below = b.Room("Below");
        Thing grate = b.Scenery("grating").Called("grate").Openable(open: false).PassObjectsTo(below);
        above.Contains(grate);
        Thing coin = b.Item("coin").Size(5);
        b.StartIn(above);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(coin);
        engine.Submit("put coin in grate");

        Assert.Contains("closed", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.True(engine.State.IsCarried(coin));
    }

    [Fact]
    public void Put_BlockedWhenItemTooLarge()
    {
        GameBuilder b = GameBuilder.Create("Pass").AddStandardVerbs();
        Room above = b.Room("Above");
        Room below = b.Room("Below");
        Thing grate = b.Scenery("grating").Called("grate").Openable(open: true)
            .PassObjectsTo(below, maxSize: 4, tooLargeMessage: "Too big.");
        above.Contains(grate);
        Thing chest = b.Item("chest").Size(10);
        b.StartIn(above);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(chest);
        engine.Submit("put chest in grate");

        Assert.Contains("Too big.", term.Output);
        Assert.True(engine.State.IsCarried(chest));
    }

    [Fact]
    public void PassageVisibleFromDestinationRoom()
    {
        GameBuilder b = GameBuilder.Create("Pass").AddStandardVerbs();
        Room above = b.Room("Above");
        Room below = b.Room("Below");
        Thing grate = b.Scenery("grating").Called("grate").Openable(open: true).PassObjectsTo(below);
        above.Contains(grate);
        b.StartIn(below);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Assert.Contains(grate, new Scope(engine.State).VisibleThings());
    }
}
