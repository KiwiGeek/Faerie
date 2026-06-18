using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkEncumbranceTests
{
    private static (GameEngine engine, InMemoryTerminal term, Thing lantern, Thing sword, Thing garlic,
        Thing coffin, Room timber, Room drafty, Room studio, Room kitchen)
        Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        Thing sword = game.World.Things.First(t => t.Name == "elvish sword");
        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        Thing coffin = game.World.Things.First(t => t.Name == "gold coffin");
        Room timber = game.World.Rooms.First(r => r.Name == "Timber Room");
        Room drafty = game.World.Rooms.First(r => r.Name == "Drafty Room");
        Room studio = game.World.Rooms.First(r => r.Name == "Studio");
        Room kitchen = game.World.Rooms.First(r => r.Name == "Kitchen");

        return (engine, term, lantern, sword, garlic, coffin, timber, drafty, studio, kitchen);
    }

    [Fact]
    public void Zork_Take_BlockedWhenOverCarryLimit()
    {
        (GameEngine engine, InMemoryTerminal term, Thing lantern, Thing sword, _, Thing coffin, _, _, _, _) = Build();

        Room egypt = engine.Game.World.Rooms.First(r => r.Name == "Egyptian Room");
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        engine.State.TakeIntoInventory(sword);
        engine.State.CurrentRoom = egypt;
        term.Reset();

        engine.Submit("take coffin");

        Assert.Contains("too heavy", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(engine.State.IsCarried(coffin));
    }

    [Fact]
    public void Zork_NarrowPassage_BlocksHeavyItem()
    {
        (GameEngine engine, InMemoryTerminal term, _, Thing sword, _, _, Room timber, Room drafty, _, _) = Build();

        engine.State.TakeIntoInventory(sword);
        engine.State.CurrentRoom = timber;
        term.Reset();

        engine.Submit("go west");

        Assert.Contains("cannot fit through this passage", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(timber, engine.State.CurrentRoom);

        engine.State.CurrentRoom = drafty;
        term.Reset();
        engine.Submit("go east");

        Assert.Contains("cannot fit through this passage", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(drafty, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_NarrowPassage_AllowsLightLoad()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, Thing garlic, _, Room timber, Room drafty, _, _) = Build();

        engine.State.TakeIntoInventory(garlic);
        engine.State.CurrentRoom = timber;
        term.Reset();

        engine.Submit("go west");

        Assert.Equal(drafty, engine.State.CurrentRoom);

        engine.Submit("go east");
        Assert.Equal(timber, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_ChimneyUp_RequiresLanternAndLightLoad()
    {
        (GameEngine engine, InMemoryTerminal term, Thing lantern, Thing sword, Thing garlic, _, _, _, Room studio, Room kitchen) = Build();

        Room gallery = engine.Game.World.Rooms.First(r => r.Name == "Gallery");
        engine.State.CurrentRoom = gallery;
        engine.Submit("north");
        Assert.Equal(studio, engine.State.CurrentRoom);
        term.Reset();

        engine.Submit("go up");
        Assert.Contains("empty-handed", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(studio, engine.State.CurrentRoom);

        engine.State.TakeIntoInventory(lantern);
        term.Reset();
        engine.Submit("go up");
        Assert.Equal(kitchen, engine.State.CurrentRoom);

        engine.State.CurrentRoom = studio;
        engine.State.TakeIntoInventory(sword);
        engine.State.TakeIntoInventory(garlic);
        term.Reset();
        engine.Submit("go up");
        Assert.Contains("can't get up there", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(studio, engine.State.CurrentRoom);
    }
}
