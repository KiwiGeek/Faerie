using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkScoringTests
{
    [Fact]
    public void Zork_PutTreasureInCase_AwardsCasePoints()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        engine.MovePlayerTo(livingRoom);

        Thing painting = game.World.Things.First(t => t.Name == "painting");
        engine.State.TakeIntoInventory(painting);
        term.Reset();

        engine.Submit("open case");
        term.Reset();
        engine.Submit("put painting in case");

        Assert.Contains("gained 6 points", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(6, engine.State.Score);
    }

    [Fact]
    public void Zork_TakeTreasureFromCase_RemovesCasePoints()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        engine.MovePlayerTo(livingRoom);

        Thing painting = game.World.Things.First(t => t.Name == "painting");
        engine.State.TakeIntoInventory(painting);
        engine.Submit("open case");
        engine.Submit("put painting in case");
        int afterPut = engine.State.Score;

        term.Reset();
        engine.Submit("take painting");

        Assert.True(engine.State.Score < afterPut);
        Assert.Equal(afterPut - 6, engine.State.Score);
    }

    [Fact]
    public void Zork_TakeTreasure_AwardsTouchPointsOnce()
    {
        Game game = ZorkGame.Build();
        GameEngine engine = new(game, new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Thing painting = game.World.Things.First(t => t.Name == "painting");
        Room gallery = game.World.Rooms.First(r => r.Name == "Gallery");
        engine.MovePlayerTo(gallery);

        engine.Submit("take painting");
        int afterFirst = engine.State.Score;

        engine.Submit("drop painting");
        engine.Submit("take painting");

        Assert.Equal(4, afterFirst);
        Assert.Equal(afterFirst, engine.State.Score);
    }
}
