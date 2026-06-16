using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class MetaCommandTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room hall, Room cellar, Thing lamp) BuildWorld()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Room cellar = b.Room("Cellar").Describe("A cold cellar.");
        hall.Down(cellar);
        Thing lamp = b.Item("lamp").Describe("A brass lamp.");
        lamp.StartsIn(hall);
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();
        return (engine, term, hall, cellar, lamp);
    }

    [Fact]
    public void Again_RepeatsLastSuccessfulCommand()
    {
        (GameEngine engine, _, _, _, Thing lamp) = BuildWorld();
        engine.Submit("take lamp");
        engine.Submit("again");

        Assert.True(engine.State.IsCarried(lamp));
    }

    [Fact]
    public void G_IsAliasForAgain()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, Thing lamp) = BuildWorld();
        engine.Submit("examine lamp");
        term.Reset();
        engine.Submit("g");

        Assert.Contains(lamp.Name, term.Output);
    }

    [Fact]
    public void Again_WithNoPriorCommand_ReportsNothingDone()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, _) = BuildWorld();
        engine.Submit("again");

        Assert.Contains("haven't done anything", term.Output);
    }

    [Fact]
    public void Undo_RestoresStateBeforeLastTurn()
    {
        (GameEngine engine, InMemoryTerminal term, Room hall, Room cellar, _) = BuildWorld();
        engine.Submit("down");
        Assert.Equal(cellar, engine.State.CurrentRoom);
        term.Reset();
        engine.Submit("undo");

        Assert.Equal(hall, engine.State.CurrentRoom);
        Assert.Contains("undone", term.Output);
    }

    [Fact]
    public void Undo_WithNothingToUndo_ReportsMessage()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, _) = BuildWorld();
        engine.Submit("undo");

        Assert.Contains("Nothing to undo", term.Output);
    }
}
