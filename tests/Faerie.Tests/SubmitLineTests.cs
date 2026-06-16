using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class SubmitLineTests
{
    [Fact]
    public void ChainedMovement_AdvancesOneTurnPerCommand()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room east = b.Room("East").Describe("East room.");
        Room west = b.Room("West").Describe("West room.");
        east.West(west);
        west.East(east);
        b.StartIn(east);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.SubmitLine("west, east");

        Assert.Equal(east, engine.State.CurrentRoom);
        Assert.Equal(2, engine.State.TurnCount);
    }

    [Fact]
    public void CompoundObjectList_StillSingleCommand()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing key = b.Item("brass key").Called("key").Describe("A key.");
        Thing lamp = b.Item("lamp").Describe("A lamp.");
        key.StartsIn(hall);
        lamp.StartsIn(hall);
        b.StartIn(hall);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.SubmitLine("take key, lamp");

        Assert.True(engine.State.IsCarried(key));
        Assert.True(engine.State.IsCarried(lamp));
        Assert.Equal(1, engine.State.TurnCount);
    }

    [Fact]
    public void PeriodSeparated_RunsInOrder()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Room cellar = b.Room("Cellar").Describe("A cellar.");
        hall.Down(cellar);
        b.StartIn(hall);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.SubmitLine("down. up");

        Assert.Equal(hall, engine.State.CurrentRoom);
        Assert.Equal(2, engine.State.TurnCount);
    }
}
