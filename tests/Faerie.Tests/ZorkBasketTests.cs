using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkBasketTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room shaft, Room drafty, Thing basket, Thing coal)
        Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room shaft = game.World.Rooms.First(r => r.Name == "Shaft Room");
        Room drafty = game.World.Rooms.First(r => r.Name == "Drafty Room");
        Thing basket = game.World.Things.First(t => t.Nouns.Contains("basket"));
        Thing coal = game.World.Things.First(t => t.Name == "small lump of coal");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.State.CurrentRoom = shaft;

        return (engine, term, shaft, drafty, basket, coal);
    }

    [Fact]
    public void Zork_LowerBasket_MovesToDraftyRoom()
    {
        (GameEngine engine, InMemoryTerminal term, Room shaft, Room drafty, Thing basket, Thing coal) = Build();

        engine.State.TakeIntoInventory(coal);
        engine.Submit("put coal in basket");
        term.Reset();

        engine.Submit("lower basket");

        Assert.Contains("lowered", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(drafty, engine.State.RoomOf(basket));
        Assert.Contains(coal, engine.State.ContentsOf(basket));
    }

    [Fact]
    public void Zork_RaiseBasket_ReturnsToShaftRoom()
    {
        (GameEngine engine, InMemoryTerminal term, Room shaft, Room drafty, Thing basket, Thing coal) = Build();

        engine.Submit("lower basket");
        engine.State.CurrentRoom = drafty;
        term.Reset();

        engine.Submit("raise basket");

        Assert.Contains("raised", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(shaft, engine.State.RoomOf(basket));
    }
}
