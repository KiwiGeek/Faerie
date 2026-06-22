using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkSkeletonTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room maze5, Thing skeleton, Thing skeletonKey, Thing lantern) Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room maze5 = game.World.Rooms.First(r =>
            r.Description.Contains("skeleton", StringComparison.OrdinalIgnoreCase));
        Thing skeleton = game.World.Things.First(t => t.Nouns.Contains("bones"));
        Thing skeletonKey = game.World.Things.First(t => t.Name == "skeleton key");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);

        return (engine, term, maze5, skeleton, skeletonKey, lantern);
    }

    [Fact]
    public void Zork_SearchSkeleton_RevealsKey()
    {
        (GameEngine engine, InMemoryTerminal term, Room maze5, _, Thing skeletonKey, _) = Build();

        engine.State.CurrentRoom = maze5;
        Assert.True(skeletonKey.Has(Attr.Concealed));
        term.Reset();

        engine.Submit("search bones");

        Assert.Contains("skeleton key", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(skeletonKey.Has(Attr.Concealed));
        term.Reset();

        engine.Submit("take key");

        Assert.True(engine.State.IsCarried(skeletonKey));
    }

    [Fact]
    public void Zork_MoveSkeleton_RevealsKey()
    {
        (GameEngine engine, InMemoryTerminal term, Room maze5, _, Thing skeletonKey, _) = Build();

        engine.State.CurrentRoom = maze5;
        term.Reset();

        engine.Submit("move bones");

        Assert.Contains("skeleton key", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(skeletonKey.Has(Attr.Concealed));
    }

    [Fact]
    public void Zork_TakeSkeleton_CursesTreasures()
    {
        (GameEngine engine, InMemoryTerminal term, Room maze5, _, _, _) = Build();

        Thing painting = engine.Game.World.Things.First(t => t.Name == "painting");
        Room landOfDead = engine.Game.World.Rooms.First(r => r.Name == "Land of the Dead");

        engine.State.CurrentRoom = maze5;
        engine.State.TakeIntoInventory(painting);
        term.Reset();

        engine.Submit("take bones");

        Assert.Contains("ghost appears", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(landOfDead, engine.State.RoomOf(painting));
    }
}
