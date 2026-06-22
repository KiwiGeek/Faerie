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

    [Fact]
    public void Zork_CanaryInCase_AwardsBirdTreasureCasePoints()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        engine.MovePlayerTo(livingRoom);

        Thing canary = game.World.Things.First(t => t.Name == "golden clockwork canary");
        engine.State.TakeIntoInventory(canary);

        engine.Submit("open case");
        term.Reset();
        engine.Submit("put canary in case");

        Assert.Contains("gained 4 points", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4, engine.State.Score);
    }

    [Fact]
    public void Zork_EggAndCanaryInCase_ScoresHighestCaseSlotOnce()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        engine.MovePlayerTo(livingRoom);

        Thing egg = game.World.Things.First(t => t.Name == "jewel-encrusted egg");
        Thing canary = game.World.Things.First(t => t.Name == "golden clockwork canary");
        engine.State.TakeIntoInventory(egg);
        engine.State.TakeIntoInventory(canary);

        engine.Submit("open case");
        engine.Submit("put egg in case");
        Assert.Equal(5, engine.State.Score);

        term.Reset();
        engine.Submit("put canary in case");

        Assert.Equal(5, engine.State.Score);
        Assert.DoesNotContain("gained", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_TakeEggAndCanary_AwardsBothTouchScores()
    {
        Game game = ZorkGame.Build();
        GameEngine engine = new(game, new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Thing egg = game.World.Things.First(t => t.Name == "jewel-encrusted egg");
        Thing canary = game.World.Things.First(t => t.Name == "golden clockwork canary");
        Room tree = game.World.Rooms.First(r => r.Name == "Up a Tree");
        engine.MovePlayerTo(tree);

        engine.Submit("take egg");
        engine.Submit("open egg");
        engine.Submit("take canary");

        Assert.Equal(11, engine.State.Score);
    }
}
