using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkBatTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room batRoom, Room shaftRoom, Thing garlic, Thing figurine)
        Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room batRoom = game.World.Rooms.First(r => r.Name == "Bat Room");
        Room shaftRoom = game.World.Rooms.First(r => r.Name == "Shaft Room");
        Thing garlic = game.World.Things.First(t => t.Nouns.Contains("garlic"));
        Thing figurine = game.World.Things.First(t => t.Name == "jade figurine");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);

        return (engine, term, batRoom, shaftRoom, garlic, figurine);
    }

    [Fact]
    public void Zork_BatRoom_WithGarlic_RepelsBat()
    {
        (GameEngine engine, InMemoryTerminal term, Room batRoom, _, Thing garlic, _) = Build();

        engine.State.TakeIntoInventory(garlic);
        term.Reset();
        engine.MovePlayerTo(batRoom);

        Assert.Equal(batRoom, engine.State.CurrentRoom);
        Assert.Contains("garlic", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_BatRoom_WithoutGarlic_StealsTreasureToShaft()
    {
        (GameEngine engine, _, Room batRoom, Room shaftRoom, _, _) = Build();

        Thing coins = engine.Game.World.Things.First(t => t.Nouns.Contains("coins"));
        engine.State.TakeIntoInventory(coins);
        engine.MovePlayerTo(batRoom);

        Assert.Equal(shaftRoom, engine.State.CurrentRoom);
        Assert.Equal(shaftRoom, engine.State.RoomOf(coins));
        Assert.False(engine.State.IsCarried(coins));
    }

    [Fact]
    public void Zork_BatRoom_WithoutGarlicOrTreasure_KillsPlayer()
    {
        (GameEngine engine, InMemoryTerminal term, Room batRoom, Room forest, _, _) = Build();

        forest = engine.Game.World.Rooms.First(r => r.Name == "Forest" && r.Description.Contains("sunlight"));
        term.Reset();
        engine.MovePlayerTo(batRoom);

        Assert.Contains("bat swoops", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(forest, engine.State.CurrentRoom);
    }
}
