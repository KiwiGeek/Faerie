using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class MetaVerbTurnTests
{
    private static (GameEngine engine, InMemoryTerminal term, int[] daemonTicks, int[] onTurnTicks) BuildTrackedWorld()
    {
        int[] daemonTicks = [0];
        int[] onTurnTicks = [0];
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        hall.OnTurn = _ => onTurnTicks[0]++;
        b.StartIn(hall);
        b.EveryTurn(_ => daemonTicks[0]++);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();
        return (engine, term, daemonTicks, onTurnTicks);
    }

    [Theory]
    [InlineData("save")]
    [InlineData("help")]
    [InlineData("score")]
    public void MetaVerb_DoesNotAdvanceTurnOrTickHooks(string command)
    {
        (GameEngine engine, _, int[] daemonTicks, int[] onTurnTicks) = BuildTrackedWorld();
        engine.SaveCatalog = new SaveSlotCatalog(Path.GetTempPath(), "meta-verb-test");

        int turnBefore = engine.State.TurnCount;
        engine.Submit(command);

        Assert.Equal(turnBefore, engine.State.TurnCount);
        Assert.Equal(0, daemonTicks[0]);
        Assert.Equal(0, onTurnTicks[0]);
    }

    [Fact]
    public void Save_DoesNotPushUndoSnapshot()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Room cellar = b.Room("Cellar").Describe("A cellar.");
        hall.Down(cellar);
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.SaveCatalog = new SaveSlotCatalog(Path.GetTempPath(), "meta-verb-test");

        engine.Submit("down");
        Assert.Equal(cellar, engine.State.CurrentRoom);
        engine.Submit("save");
        engine.Submit("undo");

        Assert.Equal(hall, engine.State.CurrentRoom);
    }

    [Fact]
    public void Wait_StillAdvancesTurnAndTicksDaemons()
    {
        (GameEngine engine, _, int[] daemonTicks, int[] onTurnTicks) = BuildTrackedWorld();

        engine.Submit("wait");

        Assert.Equal(1, engine.State.TurnCount);
        Assert.Equal(1, daemonTicks[0]);
        Assert.Equal(1, onTurnTicks[0]);
    }
}
