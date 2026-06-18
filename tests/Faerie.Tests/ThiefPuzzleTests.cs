using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ThiefPuzzleTests
{
    [Fact]
    public void Zork_Thief_DoesNotStealFromPlayerInSacredLivingRoom()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 42);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        Thing painting = game.World.Things.First(t => t.Name == "painting");
        engine.State.TakeIntoInventory(painting);
        engine.State.CurrentRoom = livingRoom;

        for (int i = 0; i < 40; i++)
            engine.Submit("wait");

        Assert.Contains(painting, engine.State.Inventory);
        Assert.DoesNotContain("robbed you blind", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Thief_GiveTreasure_GoesIntoBag()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room roundRoom = game.World.Rooms.First(r => r.Name == "Round Room");
        Thing thief = game.World.Things.First(t => t.Name == "thief");
        Thing bag = game.World.Things.First(t => t.Name == "large bag");
        Thing painting = game.World.Things.First(t => t.Name == "painting");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.CurrentRoom = roundRoom;
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        thief.Set(Attr.Concealed, false);
        engine.State.Move(thief, Placement.InRoom(roundRoom));
        engine.State.TakeIntoInventory(painting);

        term.Reset();
        engine.Submit("give painting to thief");

        Assert.Contains("thanks you politely", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(painting, engine.State.ContentsOf(bag));
        Assert.DoesNotContain(painting, engine.State.Inventory);
    }

    [Fact]
    public void Zork_Thief_DepositsBootyInTreasureRoomWhenAlone()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room treasureRoom = game.World.Rooms.First(r => r.Name == "Treasure Room");
        Room gallery = game.World.Rooms.First(r => r.Name == "Gallery");
        Thing thief = game.World.Things.First(t => t.Name == "thief");
        Thing bag = game.World.Things.First(t => t.Name == "large bag");
        Thing painting = game.World.Things.First(t => t.Name == "painting");

        painting.Set(Attr.Concealed, true);
        engine.State.Move(painting, Placement.Inside(bag));
        engine.State.Move(thief, Placement.InRoom(treasureRoom));
        engine.State.CurrentRoom = gallery;
        engine.State.TurnCount = 25;

        engine.Submit("wait");

        Assert.Contains(painting, engine.State.ContentsOf(treasureRoom));
        Assert.DoesNotContain(painting, engine.State.ContentsOf(bag));
    }

    [Fact]
    public void Zork_Thief_KillSpillsBagContents()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room roundRoom = game.World.Rooms.First(r => r.Name == "Round Room");
        Thing thief = game.World.Things.First(t => t.Name == "thief");
        Thing bag = game.World.Things.First(t => t.Name == "large bag");
        Thing painting = game.World.Things.First(t => t.Name == "painting");
        Thing sword = game.World.Things.First(t => t.Name == "elvish sword");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.CurrentRoom = roundRoom;
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        thief.Set(Attr.Concealed, false);
        engine.State.Move(thief, Placement.InRoom(roundRoom));
        painting.Set(Attr.Concealed, true);
        engine.State.Move(painting, Placement.Inside(bag));
        engine.State.TakeIntoInventory(sword);

        for (int i = 0; i < 100 && engine.State.RoomOf(thief) is not null; i++)
            engine.Submit("attack thief with sword");

        Assert.Null(engine.State.RoomOf(thief));
        Assert.Contains(painting, engine.State.ContentsOf(roundRoom));
    }

    [Fact]
    public void Zork_Thief_NeverRoamIntoSacredLivingRoom()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing thief = game.World.Things.First(t => t.Name == "thief");
        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        engine.State.CurrentRoom = livingRoom;
        engine.State.TurnCount = 25;

        for (int i = 0; i < 200; i++)
            engine.Submit("wait");

        Assert.NotEqual(livingRoom, engine.State.RoomOf(thief));
    }
}
