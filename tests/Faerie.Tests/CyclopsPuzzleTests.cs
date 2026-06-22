using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class CyclopsPuzzleTests
{
    private static (GameEngine engine, InMemoryTerminal term, Game game, Room cyclopsRoom, Thing cyclops, Thing lantern)
        BuildAtCyclops()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room cyclopsRoom = game.World.Rooms.First(r => r.Name == "Cyclops Room");
        Thing cyclops = game.World.Things.First(t => t.Name == "cyclops");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        engine.State.CurrentRoom = cyclopsRoom;

        return (engine, term, game, cyclopsRoom, cyclops, lantern);
    }

    [Fact]
    public void Zork_Cyclops_Odysseus_RemovesCyclopsAndOpensEastWall()
    {
        (GameEngine engine, InMemoryTerminal term, Game game, Room cyclopsRoom, Thing cyclops, _) = BuildAtCyclops();
        Room strangePassage = game.World.Rooms.First(r => r.Name == "Strange Passage");

        term.Reset();
        engine.Submit("odysseus");

        Assert.Contains("father's deadly nemesis", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(cyclops, engine.State.ContentsOf(cyclopsRoom));

        term.Reset();
        engine.Submit("east");
        Assert.Equal(strangePassage, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_Cyclops_OdysseusElsewhere_IsSailorQuip()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        term.Reset();
        engine.Submit("odysseus");

        Assert.Contains("sailor", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Cyclops_LunchThenWater_PutsCyclopsToSleep()
    {
        (GameEngine engine, InMemoryTerminal term, Game game, Room cyclopsRoom, _, _) = BuildAtCyclops();
        Thing lunch = game.World.Things.First(t => t.Name == "lunch");
        Thing bottle = game.World.Things.First(t => t.Name == "glass bottle");
        Room treasureRoom = game.World.Rooms.First(r => r.Name == "Treasure Room");

        engine.State.TakeIntoInventory(lunch);
        engine.State.TakeIntoInventory(bottle);

        term.Reset();
        engine.Submit("give lunch to cyclops");
        Assert.Contains("hot peppers", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("open bottle");
        term.Reset();
        engine.Submit("give bottle to cyclops");
        Assert.Contains("falls fast asleep", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("up");
        Assert.Equal(treasureRoom, engine.State.CurrentRoom);
    }

    [Fact]
    public void Zork_Cyclops_AttackEscalatesTowardDeath()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, _, _) = BuildAtCyclops();

        for (int i = 0; i < 5; i++)
            engine.Submit("attack cyclops");

        term.Reset();
        engine.Submit("attack cyclops");

        Assert.Contains("Become dinner", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("wait");

        Assert.False(engine.State.IsOver);
        Assert.Contains("trickery", term.Output, StringComparison.OrdinalIgnoreCase);

        term.Reset();
        engine.Submit("inventory");

        Assert.False(engine.State.IsOver);
        Assert.Contains("Forest", engine.State.CurrentRoom.Name, StringComparison.OrdinalIgnoreCase);
    }
}
