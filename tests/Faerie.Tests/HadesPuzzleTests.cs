using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class HadesPuzzleTests
{
    [Fact]
    public void Zork_Hades_RingBell_DropsCandlesSoTheyCanBeTakenAgain()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room entrance = game.World.Rooms.First(r => r.Name == "Entrance to Hades");
        Thing bell = game.World.Things.First(t => t.Name == "brass bell");
        Thing candles = game.World.Things.First(t => t.Name == "pair of candles");
        Thing book = game.World.Things.First(t => t.Name == "black book");
        Thing matchbook = game.World.Things.First(t => t.Name == "matchbook");

        engine.State.CurrentRoom = entrance;
        engine.State.TakeIntoInventory(bell);
        engine.State.TakeIntoInventory(candles);
        engine.State.TakeIntoInventory(book);
        engine.State.TakeIntoInventory(matchbook);

        engine.Submit("ring bell");

        Assert.Contains("red hot", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("candles drop", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(candles.Has(Attr.Lit));
        Assert.Contains(candles, engine.State.ContentsOf(entrance));
        Assert.DoesNotContain(candles, engine.State.Inventory);

        term.Reset();
        engine.Submit("take candles");
        Assert.Contains(candles, engine.State.Inventory);
    }

    [Fact]
    public void Zork_Hades_FullRitual_OpensGate()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room entrance = game.World.Rooms.First(r => r.Name == "Entrance to Hades");
        Room landOfDead = game.World.Rooms.First(r => r.Name == "Land of the Dead");
        Thing bell = game.World.Things.First(t => t.Name == "brass bell");
        Thing candles = game.World.Things.First(t => t.Name == "pair of candles");
        Thing book = game.World.Things.First(t => t.Name == "black book");
        Thing matchbook = game.World.Things.First(t => t.Name == "matchbook");

        engine.State.CurrentRoom = entrance;
        engine.State.TakeIntoInventory(bell);
        engine.State.TakeIntoInventory(candles);
        engine.State.TakeIntoInventory(book);
        engine.State.TakeIntoInventory(matchbook);

        engine.Submit("ring bell");
        engine.Submit("take candles");
        engine.Submit("light candles with matchbook");
        engine.Submit("read book");

        Assert.Contains("Begone, fiends", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("south");
        Assert.Equal(landOfDead, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_Hades_ReadBookBeforeRelightingCandles_DoesNotOpenGate()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room entrance = game.World.Rooms.First(r => r.Name == "Entrance to Hades");
        Thing bell = game.World.Things.First(t => t.Name == "brass bell");
        Thing candles = game.World.Things.First(t => t.Name == "pair of candles");
        Thing book = game.World.Things.First(t => t.Name == "black book");

        engine.State.CurrentRoom = entrance;
        engine.State.TakeIntoInventory(bell);
        engine.State.TakeIntoInventory(candles);
        engine.State.TakeIntoInventory(book);

        engine.Submit("ring bell");
        engine.Submit("take candles");
        engine.Submit("read book");

        Assert.Contains("unknown tongue", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("south");
        Assert.Contains("invisible force", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(entrance, engine.State.CurrentRoom);
    }
}
