using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkMagicPassageTests
{
    [Fact]
    public void Zork_GnomeGiveTreasure_SetsMagicFlag()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room gallery = game.World.Rooms.First(r => r.Name == "Gallery");
        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        Thing painting = game.World.Things.First(t => t.Name == "painting");

        engine.State.TakeIntoInventory(painting);
        engine.MovePlayerTo(gallery);
        term.Reset();

        engine.Submit("give painting");

        engine.MovePlayerTo(livingRoom);
        term.Reset();
        engine.Submit("west");

        Assert.Equal("Strange Passage", engine.State.CurrentRoom.Name);
    }

    [Fact]
    public void Zork_ChimneyUp_SetsChimneyFlagForKitchenDown()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room gallery = game.World.Rooms.First(r => r.Name == "Gallery");
        Room studio = game.World.Rooms.First(r => r.Name == "Studio");
        Room kitchen = game.World.Rooms.First(r => r.Name == "Kitchen");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.MovePlayerTo(gallery);
        engine.Submit("north");
        Assert.Equal(studio, engine.State.CurrentRoom);

        engine.MovePlayerTo(kitchen);
        term.Reset();
        engine.Submit("down");
        Assert.Contains("Santa Claus", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(kitchen, engine.State.CurrentRoom);

        engine.MovePlayerTo(studio);
        engine.State.TakeIntoInventory(lantern);
        engine.Submit("up");
        Assert.Equal(kitchen, engine.State.CurrentRoom);

        engine.MovePlayerTo(kitchen);
        term.Reset();
        engine.Submit("down");
        Assert.Equal(studio, engine.State.CurrentRoom);
    }
}
