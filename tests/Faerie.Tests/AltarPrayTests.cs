using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class AltarPrayTests
{
    [Fact]
    public void Zork_Altar_Pray_TeleportsToForest()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room altar = game.World.Rooms.First(r => r.Name == "Altar");
        Room forest = game.World.Rooms.First(r => r.Name == "Forest");

        engine.State.CurrentRoom = altar;
        engine.Submit("pray");

        Assert.Equal(forest, engine.State.CurrentRoom);
        Assert.Contains("forest", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("serenity", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Altar_Pray_KeepsInventory()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room altar = game.World.Rooms.First(r => r.Name == "Altar");
        Thing coffin = game.World.Things.First(t => t.Name == "gold coffin");

        engine.State.CurrentRoom = altar;
        engine.State.TakeIntoInventory(coffin);
        engine.Submit("pray");

        Assert.Contains(coffin, engine.State.Inventory);
    }

    [Fact]
    public void Zork_Pray_OutsideAltar_IsSerenityOnly()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room temple = game.World.Rooms.First(r => r.Name == "Temple");

        engine.State.CurrentRoom = temple;
        engine.Submit("pray");

        Assert.Equal(temple, engine.State.CurrentRoom);
        Assert.Contains("serenity", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
