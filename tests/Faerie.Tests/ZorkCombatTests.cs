using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkCombatTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room trollRoom, Thing troll, Thing sword)
        BuildAtTroll()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room trollRoom = game.World.Rooms.First(r => r.Name == "The Troll Room");
        Thing troll = game.World.Things.First(t => t.Nouns.Contains("troll"));
        Thing sword = game.World.Things.First(t => t.Nouns.Contains("sword"));
        Thing lantern = game.World.Things.First(t => t.Nouns.Contains("lantern"));

        engine.State.TakeIntoInventory(sword);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.MovePlayerTo(trollRoom);

        return (engine, term, trollRoom, troll, sword);
    }

    [Fact]
    public void Zork_ThrowSwordAtTroll_CatchesWeapon()
    {
        (GameEngine engine, InMemoryTerminal term, _, _, _) = BuildAtTroll();

        term.Reset();
        engine.SubmitLine("throw sword at troll");

        Assert.Contains("catches", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.False(engine.State.IsCarried(engine.Game.World.Things.First(t => t.Nouns.Contains("sword"))));
    }
}
