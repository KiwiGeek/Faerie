using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class HazardTests
{
    [Fact]
    public void HasCarriedOpenFlame_IgnoresBatteryLantern()
    {
        GameBuilder b = GameBuilder.Create("H").AddStandardVerbs();
        Room room = b.Room("Room");
        Thing lantern = b.Item("lantern").LightSource(lit: true);
        Thing candles = b.Item("candles").OpenFlame(lit: true);
        b.StartIn(room);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        GameContext ctx = new(engine, engine.State, engine.Out);
        engine.State.TakeIntoInventory(lantern);

        Assert.False(Hazards.HasCarriedOpenFlame(ctx));

        engine.State.TakeIntoInventory(candles);
        Assert.True(Hazards.HasCarriedOpenFlame(ctx));
    }

    [Fact]
    public void HazardOnEnter_TriggersWhenConditionMet()
    {
        GameBuilder b = GameBuilder.Create("H").AddStandardVerbs();
        Room safe = b.Room("Safe");
        Room gas = b.Room("Gas");
        safe.Connect(Direction.North, gas);
        Thing candles = b.Item("candles").OpenFlame(lit: true);
        b.StartIn(safe);

        gas.HazardOnEnter(Hazards.HasCarriedOpenFlame, ctx => ctx.Lose("Boom."));

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(candles);
        term.Reset();

        engine.MovePlayerTo(gas);

        Assert.True(engine.State.IsOver);
        Assert.Contains("Boom.", term.Output);
    }

    [Fact]
    public void HazardEveryTurn_TriggersInRoom()
    {
        GameBuilder b = GameBuilder.Create("H").AddStandardVerbs();
        Room gas = b.Room("Gas");
        Thing candles = b.Item("candles").OpenFlame(lit: false);
        b.StartIn(gas);
        b.HazardEveryTurn(gas, Hazards.HasCarriedOpenFlame, ctx => ctx.Lose("Boom."));

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(candles);
        candles.Set(Attr.Lit, true);
        term.Reset();

        engine.Submit("wait");

        Assert.True(engine.State.IsOver);
        Assert.Contains("Boom.", term.Output);
    }
}
