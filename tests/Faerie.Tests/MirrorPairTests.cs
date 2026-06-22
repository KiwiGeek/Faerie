using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class MirrorPairTests
{
    [Fact]
    public void SwapContents_ExchangesPortableItems()
    {
        GameBuilder b = GameBuilder.Create("M").AddStandardVerbs();
        Room a = b.Room("North");
        Room bRoom = b.Room("South");
        MirrorPair pair = b.MirrorRooms(a, bRoom);
        Thing coin = b.Item("coin").Takeable();
        Thing gem = b.Item("gem").Takeable();
        a.Contains(coin);
        bRoom.Contains(gem);
        b.StartIn(a);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        GameContext ctx = new(engine, engine.State, engine.Out);

        pair.SwapContents(ctx);

        Assert.Equal(bRoom, engine.State.RoomOf(coin));
        Assert.Equal(a, engine.State.RoomOf(gem));
    }

    [Fact]
    public void SwapContents_LeavesFixedScenery()
    {
        GameBuilder b = GameBuilder.Create("M").AddStandardVerbs();
        Room a = b.Room("North");
        Room bRoom = b.Room("South");
        MirrorPair pair = b.MirrorRooms(a, bRoom);
        Thing mirror = b.Scenery("mirror").Called("mirror").MirrorIn(pair, a);
        Thing coin = b.Item("coin").Takeable();
        a.Contains(mirror);
        a.Contains(coin);
        b.StartIn(a);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        GameContext ctx = new(engine, engine.State, engine.Out);

        pair.SwapContents(ctx);

        Assert.Equal(a, engine.State.RoomOf(mirror));
        Assert.Equal(bRoom, engine.State.RoomOf(coin));
    }
}
