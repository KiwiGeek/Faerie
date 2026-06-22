using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;
using static Faerie.Runtime.Fluid;

namespace Faerie.Tests;

public sealed class ZorkVerbTests
{
    [Fact]
    public void Zork_SmellGarlic_ReportsPungent()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        engine.State.TakeIntoInventory(garlic);
        term.Reset();

        engine.Submit("smell garlic");

        Assert.Contains("strongly of garlic", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_ListenAtFalls_HearsWater()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room falls = game.World.Rooms.First(r => r.Name == "Aragain Falls");
        engine.State.CurrentRoom = falls;
        term.Reset();

        engine.Submit("listen");

        Assert.Contains("water crashing", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_BrandishSword_AttacksTroll()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room trollRoom = game.World.Rooms.First(r => r.Name == "The Troll Room");
        Thing sword = game.World.Things.First(t => t.Nouns.Contains("sword"));
        Thing lantern = game.World.Things.First(t => t.Nouns.Contains("lantern"));

        engine.State.TakeIntoInventory(sword);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.State.CurrentRoom = trollRoom;
        term.Reset();

        engine.Submit("brandish troll");

        Assert.Contains("troll", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            term.Output.Contains("miss", StringComparison.OrdinalIgnoreCase) ||
            term.Output.Contains("strike", StringComparison.OrdinalIgnoreCase) ||
            term.Output.Contains("parry", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Zork_CountCoins_ReportsQuantity()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing coins = game.World.Things.First(t => t.Name == "leather bag of coins");
        engine.State.TakeIntoInventory(coins);
        term.Reset();

        engine.Submit("count coins");

        Assert.Contains("large number of coins", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_FillBottle_AtStream()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room stream = game.World.Rooms.First(r => r.Name == "Stream");
        Thing bottle = game.World.Things.First(t => t.Name == "glass bottle");
        Thing water = game.World.Things.First(t => t.Name == "water");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.State.TakeIntoInventory(bottle);
        bottle.Set(Attr.Open, true);
        if (ContainerHolds(engine.State, bottle, water))
            engine.State.Move(water, Placement.Offstage);
        engine.State.CurrentRoom = stream;
        term.Reset();

        engine.Submit("fill bottle");

        Assert.Contains("full of water", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(water, engine.State.ContentsOf(bottle));
    }
}
