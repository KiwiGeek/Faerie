using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkTrapDoorTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room livingRoom, Room cellar, Thing trapDoor) Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        Room cellar = game.World.Rooms.First(r => r.Name == "Cellar");
        Thing trapDoor = game.World.Things.First(t => t.Name == "trap door");

        return (engine, term, livingRoom, cellar, trapDoor);
    }

    [Fact]
    public void Zork_FirstCellarEntry_SlamsTrapDoor()
    {
        (GameEngine engine, InMemoryTerminal term, Room livingRoom, Room cellar, Thing trapDoor) = Build();

        engine.State.CurrentRoom = livingRoom;
        Thing rug = engine.Game.World.Things.First(t => t.Nouns.Contains("rug"));
        engine.Submit("move rug");
        trapDoor.Set(Attr.Concealed, false);
        engine.Submit("open trap door");
        term.Reset();

        engine.Submit("down");

        Assert.Equal(cellar, engine.State.CurrentRoom);
        Assert.Contains("crashes shut", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(trapDoor.Has(Attr.Open));
        term.Reset();

        engine.Submit("up");

        Assert.Contains("door at the top of the stairs is closed", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_SecondCellarEntry_DoesNotReSlamTrapDoor()
    {
        (GameEngine engine, InMemoryTerminal term, Room livingRoom, Room cellar, Thing trapDoor) = Build();

        engine.State.CurrentRoom = livingRoom;
        Thing rug = engine.Game.World.Things.First(t => t.Nouns.Contains("rug"));
        engine.Submit("move rug");
        trapDoor.Set(Attr.Concealed, false);
        engine.Submit("open trap door");
        engine.Submit("down");
        term.Reset();

        engine.Submit("open trap door");
        engine.Submit("down");

        Assert.Equal(cellar, engine.State.CurrentRoom);
        Assert.DoesNotContain("crashes shut", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
