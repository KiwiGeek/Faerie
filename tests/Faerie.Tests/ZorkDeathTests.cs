using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkDeathTests
{
    [Fact]
    public void Zork_FirstDeath_RevivesAndPenalizesScore()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        engine.State.Score = 25;
        int startScore = engine.State.Score;

        term.Reset();
        engine.Context.Die("Testing death.");

        Assert.False(engine.State.IsOver);
        Assert.Equal(startScore - 10, engine.State.Score);
        Assert.Contains("deserve another chance", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Forest", engine.State.CurrentRoom.Name);
    }

    [Fact]
    public void Zork_Death_ScatterTreasureToDarkRoom()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        Thing painting = game.World.Things.First(t => t.Name == "painting");
        engine.State.TakeIntoInventory(garlic);
        engine.State.TakeIntoInventory(painting);
        Room forest = engine.State.CurrentRoom;

        term.Reset();
        engine.Context.Die("Testing death.");

        Assert.False(engine.State.IsCarried(painting));
        Assert.False(engine.State.IsCarried(garlic));
        Room paintingRoom = engine.State.RoomOf(painting)!;
        Room garlicRoom = engine.State.RoomOf(garlic)!;
        Assert.True(paintingRoom.IsDark);
        Assert.False(garlicRoom.IsDark);
        Assert.NotEqual(forest, garlicRoom);
    }

    [Fact]
    public void Zork_ThirdDeath_EndsGame()
    {
        Game game = ZorkGame.Build();
        GameEngine engine = new(game, new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        engine.Context.Die("Once.");
        engine.Context.Die("Twice.");
        engine.Context.Die("Thrice.");

        Assert.True(engine.State.IsOver);
    }
}
