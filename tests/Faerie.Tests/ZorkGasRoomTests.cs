using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkGasRoomTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room gasRoom, Thing lantern, Thing candles) Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room gasRoom = game.World.Rooms.First(r => r.Name == "Gas Room");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        Thing candles = game.World.Things.First(t => t.Nouns.Contains("candles"));

        return (engine, term, gasRoom, lantern, candles);
    }

    [Fact]
    public void Zork_GasRoom_KillsOnEnterWithLitCandles()
    {
        (GameEngine engine, InMemoryTerminal term, Room gasRoom, _, Thing candles) = Build();

        engine.State.TakeIntoInventory(candles);
        candles.Set(Attr.Lit, true);
        term.Reset();

        engine.MovePlayerTo(gasRoom);

        Assert.False(engine.State.IsOver);
        Assert.Contains("coal gas", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BOOOOOOOOOOOM", term.Output);
        Assert.Contains("deserve another chance", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_GasRoom_AllowsLitLantern()
    {
        (GameEngine engine, InMemoryTerminal term, Room gasRoom, Thing lantern, _) = Build();

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        term.Reset();

        engine.MovePlayerTo(gasRoom);
        engine.Submit("look");

        Assert.False(engine.State.IsOver);
        Assert.Equal(gasRoom, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_GasRoom_KillsWhenLightingCandles()
    {
        (GameEngine engine, InMemoryTerminal term, Room gasRoom, _, Thing candles) = Build();

        engine.State.CurrentRoom = gasRoom;
        engine.State.TakeIntoInventory(candles);
        term.Reset();

        engine.Submit("light candles");

        Assert.False(engine.State.IsOver);
        Assert.Contains("reeks of gas", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BOOOOOOOOOOOM", term.Output);
        Assert.Contains("deserve another chance", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
