using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkMirrorTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room mirrorRoom1, Room mirrorRoom2,
        Thing mirror1, Thing garlic, Thing lantern) Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        MirrorPair pair = game.MirrorPairs[0];
        Thing mirror1 = pair.MirrorA!;
        Room mirrorRoom1 = pair.RoomA;
        Room mirrorRoom2 = pair.RoomB;
        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);

        return (engine, term, mirrorRoom1, mirrorRoom2, mirror1, garlic, lantern);
    }

    private static void LightRoom(GameEngine engine, Thing lantern)
    {
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
    }

    [Fact]
    public void Zork_RubMirror_TeleportsAndSwapsItems()
    {
        (GameEngine engine, InMemoryTerminal term, Room mirrorRoom1, Room mirrorRoom2, _, Thing garlic, Thing lantern) = Build();

        engine.State.Move(garlic, Placement.InRoom(mirrorRoom2));
        engine.MovePlayerTo(mirrorRoom1);
        LightRoom(engine, lantern);
        term.Reset();

        engine.Submit("rub mirror");

        Assert.Equal(mirrorRoom2, engine.State.CurrentRoom);
        Assert.Equal(mirrorRoom1, engine.State.RoomOf(garlic));
        Assert.Contains("rumble", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_BreakMirror_SetsBadLuck()
    {
        (GameEngine engine, InMemoryTerminal term, Room mirrorRoom1, _, Thing mirror1, _, Thing lantern) = Build();

        engine.MovePlayerTo(mirrorRoom1);
        LightRoom(engine, lantern);
        term.Reset();

        engine.Submit("break mirror");

        Assert.True(mirror1.Has(Attr.Broken));
        Assert.Contains("seven years", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_ExamineMirror_ShowsReflection()
    {
        (GameEngine engine, InMemoryTerminal term, Room mirrorRoom1, _, _, _, Thing lantern) = Build();

        engine.MovePlayerTo(mirrorRoom1);
        LightRoom(engine, lantern);
        term.Reset();

        engine.Submit("examine mirror");

        Assert.Contains("ugly person", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
