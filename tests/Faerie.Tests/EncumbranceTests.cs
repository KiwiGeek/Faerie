using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class EncumbranceTests
{
    [Fact]
    public void TotalLoad_SumsCarriedAndNestedSizes()
    {
        GameBuilder b = GameBuilder.Create("Load").WithCarryLimit(100).AddCoreVerbs();
        Room room = b.Room("Room");
        Thing bag = b.Item("bag").Size(10).Container(open: true);
        Thing coin = b.Item("coin").Size(5);
        room.Contains(bag);
        coin.StartsInside(bag);
        b.StartIn(room);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Submit("take bag");

        Assert.Equal(15, engine.State.TotalLoad);
    }

    [Fact]
    public void Take_BlockedWhenOverCarryLimit()
    {
        GameBuilder b = GameBuilder.Create("Limit").WithCarryLimit(10).AddCoreVerbs();
        Room room = b.Room("Room");
        Thing brick = b.Item("brick").Size(15);
        room.Contains(brick);
        b.StartIn(room);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("take brick");

        Assert.Contains("too heavy", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(engine.State.IsCarried(brick));
    }

    [Fact]
    public void Take_AllowedWhenUnderCarryLimit()
    {
        GameBuilder b = GameBuilder.Create("Ok").WithCarryLimit(10).AddCoreVerbs();
        Room room = b.Room("Room");
        Thing pebble = b.Item("pebble").Size(3);
        room.Contains(pebble);
        b.StartIn(room);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Submit("take pebble");

        Assert.True(engine.State.IsCarried(pebble));
        Assert.Equal(3, engine.State.TotalLoad);
    }

    [Fact]
    public void RequiresEmptyHands_BlocksWhileHoldingItems()
    {
        GameBuilder b = GameBuilder.Create("Hands").AddMovement().AddCoreVerbs().WithCarryLimit(100);
        Room a = b.Room("A");
        Room bRoom = b.Room("B");
        Thing key = b.Item("key").Size(1).Takeable();
        a.Contains(key);
        a.Connect(Direction.East, bRoom).RequiresEmptyHands("You need both hands free.");
        b.StartIn(a);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("take key");
        engine.Submit("go east");

        Assert.Contains("both hands", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(a, engine.State.CurrentRoom);
    }

    [Fact]
    public void RequiresNoLoad_BlocksWhenCarryingWeight()
    {
        GameBuilder b = GameBuilder.Create("NoLoad").AddMovement().AddCoreVerbs().WithCarryLimit(100);
        Room a = b.Room("A");
        Room shaft = b.Room("Shaft");
        Thing brick = b.Item("brick").Size(5).Takeable();
        a.Contains(brick);
        a.Connect(Direction.Down, shaft).RequiresNoLoad("Your load is too heavy.");
        b.StartIn(a);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("take brick");
        engine.Submit("go down");

        Assert.Contains("too heavy", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(a, engine.State.CurrentRoom);

        engine.Submit("drop brick");
        engine.Submit("go down");
        Assert.Equal(shaft, engine.State.CurrentRoom);
    }

    [Fact]
    public void Thing_OnTakeBlockedMessage_OverridesDefault()
    {
        GameBuilder b = GameBuilder.Create("Msg").WithCarryLimit(1).AddCoreVerbs();
        Room room = b.Room("Room");
        Thing anvil = b.Item("anvil").Size(50);
        anvil.OnTakeBlockedMessage = "The anvil will not budge.";
        room.Contains(anvil);
        b.StartIn(room);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("take anvil");

        Assert.Contains("will not budge", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
