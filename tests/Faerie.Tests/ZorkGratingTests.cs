using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkGratingTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room clearing, Room gratingRoom,
        Thing leaves, Thing grating, Thing garlic, Thing sword, Thing skeletonKey, Thing lantern) Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room clearing = game.World.Rooms.First(r =>
            r.Description.Contains("path leads south", StringComparison.Ordinal));
        Room gratingRoom = game.World.Rooms.First(r => r.Name == "Grating Room");
        Thing leaves = game.World.Things.First(t => t.Nouns.Contains("leaves"));
        Thing grating = game.World.Things.First(t => t.Nouns.Contains("grate"));
        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        Thing sword = game.World.Things.First(t => t.Name == "elvish sword");
        Thing skeletonKey = game.World.Things.First(t => t.Name == "skeleton key");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        return (engine, term, clearing, gratingRoom, leaves, grating, garlic, sword, skeletonKey, lantern);
    }

    private static void LightGratingRoom(GameEngine engine, Thing lantern)
    {
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
    }

    [Fact]
    public void Zork_MoveLeaves_RevealsGrating()
    {
        (GameEngine engine, InMemoryTerminal term, Room clearing, _, Thing leaves, Thing grating, _, _, _, _) = Build();

        engine.State.CurrentRoom = clearing;
        term.Reset();
        engine.Submit("move leaves");

        Assert.Contains("grating is revealed", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(grating.Has(Attr.Concealed));
    }

    [Fact]
    public void Zork_PutSmallItemThroughOpenGrate()
    {
        (GameEngine engine, InMemoryTerminal term, Room clearing, Room gratingRoom, _, Thing grating, Thing garlic, _, Thing skeletonKey, _) = Build();

        engine.State.CurrentRoom = clearing;
        grating.Set(Attr.Concealed, false);
        grating.Set(Attr.Open);
        grating.Set(Attr.Locked, false);
        engine.State.TakeIntoInventory(garlic);
        term.Reset();

        engine.Submit("put garlic in grate");

        Assert.Contains("goes through", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(gratingRoom, engine.State.RoomOf(garlic));
    }

    [Fact]
    public void Zork_PutLargeItemBlockedByGrate()
    {
        (GameEngine engine, InMemoryTerminal term, Room clearing, _, _, Thing grating, _, Thing sword, _, _) = Build();

        engine.State.CurrentRoom = clearing;
        grating.Set(Attr.Concealed, false);
        grating.Set(Attr.Open);
        grating.Set(Attr.Locked, false);
        engine.State.TakeIntoInventory(sword);
        term.Reset();

        engine.Submit("put sword in grate");

        Assert.Contains("won't fit through the grating", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.True(engine.State.IsCarried(sword));
    }

    [Fact]
    public void Zork_UnlockGrate_FromClearingBlocked()
    {
        (GameEngine engine, InMemoryTerminal term, Room clearing, _, _, Thing grating, _, _, Thing skeletonKey, _) = Build();

        engine.State.CurrentRoom = clearing;
        grating.Set(Attr.Concealed, false);
        engine.State.TakeIntoInventory(skeletonKey);
        term.Reset();

        engine.Submit("unlock grate with skeleton key");

        Assert.Contains("can't reach the lock", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.True(grating.Has(Attr.Locked));
    }

    [Fact]
    public void Zork_UnlockGrate_FromGratingRoomSucceeds()
    {
        (GameEngine engine, InMemoryTerminal term, _, Room gratingRoom, _, Thing grating, _, _, Thing skeletonKey, Thing lantern) = Build();

        engine.State.CurrentRoom = gratingRoom;
        LightGratingRoom(engine, lantern);
        engine.State.TakeIntoInventory(skeletonKey);
        term.Reset();

        engine.Submit("unlock grate with skeleton key");

        Assert.False(grating.Has(Attr.Locked));
    }

    [Fact]
    public void Zork_OpenGrateFromBelow_DropsLeaves()
    {
        (GameEngine engine, InMemoryTerminal term, _, Room gratingRoom, Thing leaves, Thing grating, _, _, _, Thing lantern) = Build();

        engine.State.CurrentRoom = gratingRoom;
        LightGratingRoom(engine, lantern);
        grating.Set(Attr.Locked, false);
        grating.Set(Attr.Open, false);
        term.Reset();

        engine.Submit("open grate");

        Assert.Contains("leaves falls", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(gratingRoom, engine.State.RoomOf(leaves));
        Assert.True(grating.Has(Attr.Open));
    }
}
