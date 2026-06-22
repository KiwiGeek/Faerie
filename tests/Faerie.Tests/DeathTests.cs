using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class DeathTests
{
    [Fact]
    public void Die_WithoutHandler_EndsGame()
    {
        GameBuilder b = GameBuilder.Create("D").AddCoreVerbs();
        Room room = b.Room("Room");
        b.StartIn(room);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Context.Die("You died.");

        Assert.True(engine.State.IsOver);
    }

    [Fact]
    public void Die_WithRevivingHandler_ContinuesGame()
    {
        GameBuilder b = GameBuilder.Create("D").AddCoreVerbs();
        Room room = b.Room("Room");
        Room forest = b.Room("Forest");
        b.StartIn(room);
        b.OnDeath(death =>
        {
            death.Context.MovePlayerTo(forest);
            death.Revived = true;
        });

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Context.Die("You died.");

        Assert.False(engine.State.IsOver);
        Assert.Equal(forest, engine.State.CurrentRoom);
    }

    [Fact]
    public void ScatterCarried_PlacesTreasuresInDarkRooms()
    {
        GameBuilder b = GameBuilder.Create("D").AddCoreVerbs();
        Room start = b.Room("Start");
        Room dark = b.Room("Dark").Dark();
        Room lit = b.Room("Lit");
        Thing coin = b.Item("coin").Takeable();
        Thing rock = b.Item("rock").Takeable();
        b.StartIn(start);
        b.OnDeath(_ => { });

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        GameContext ctx = engine.Context;
        ctx.State.TakeIntoInventory(coin);
        ctx.State.TakeIntoInventory(rock);

        Death.ScatterCarried(ctx, t => t == coin, [dark], [lit]);

        Assert.Equal(dark, ctx.State.RoomOf(coin));
        Assert.Equal(lit, ctx.State.RoomOf(rock));
    }
}
