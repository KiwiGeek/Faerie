using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;
using static Faerie.Runtime.Fluid;

namespace Faerie.Tests;

public sealed class FluidTests
{
    [Fact]
    public void CanPourFrom_RequiresOpenContainer()
    {
        GameBuilder b = GameBuilder.Create("F");
        Room room = b.Room("Room");
        Thing bottle = b.Item("bottle").Container(open: false);
        Thing water = b.Item("water");
        bottle.StartsIn(room);
        water.StartsInside(bottle);
        b.StartIn(room);

        GameState state = new(b.Build().World);
        Assert.False(CanPourFrom(state, bottle, water));

        bottle.Set(Attr.Open, true);
        Assert.True(CanPourFrom(state, bottle, water));
    }

    [Fact]
    public void TryConsume_RemovesFluidFromBottleOrLoose()
    {
        GameBuilder b = GameBuilder.Create("F").AddCoreVerbs();
        Room room = b.Room("Room");
        Thing bottle = b.Item("bottle").Container(open: true);
        Thing water = b.Item("water").Takeable();
        bottle.StartsIn(room);
        water.StartsInside(bottle);
        b.StartIn(room);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Assert.True(TryConsume(engine.Context, bottle, water, bottle));
        Assert.False(ContainerHolds(engine.State, bottle, water));
    }
}
