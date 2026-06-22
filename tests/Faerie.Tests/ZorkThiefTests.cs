using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkThiefTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room treasureRoom, Thing thief, Thing sword, Thing chalice, Thing lantern)
        Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room treasureRoom = game.World.Rooms.First(r => r.Name == "Treasure Room");
        Thing thief = game.World.Things.First(t => t.Nouns.Contains("robber"));
        Thing sword = game.World.Things.First(t => t.Name == "elvish sword");
        Thing chalice = game.World.Things.First(t => t.Name == "silver chalice");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        return (engine, term, treasureRoom, thief, sword, chalice, lantern);
    }

    [Fact]
    public void Zork_TreasureRoom_ThiefStaysToFight_OnSeed1()
    {
        (GameEngine engine, InMemoryTerminal term, Room treasureRoom, Thing thief, Thing sword, _, _) = Build();

        engine.State.TurnCount = 50;
        engine.State.TakeIntoInventory(sword);
        term.Reset();

        engine.MovePlayerTo(treasureRoom);
        engine.Submit("wait");

        Assert.Equal(treasureRoom, engine.State.CurrentRoom);
        Assert.True(engine.State.IsLocatedIn(thief, treasureRoom));
        Assert.False(thief.Has(Attr.Concealed));
        Assert.DoesNotContain("just left", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_TreasureRoom_EnterMovesChaliceIntoThiefLoot()
    {
        (GameEngine engine, _, Room treasureRoom, Thing thief, _, Thing chalice, _) = Build();

        engine.State.TurnCount = 50;
        engine.MovePlayerTo(treasureRoom);

        Thing bag = engine.Game.World.Things.First(t => t.Nouns.Contains("bag") && t.Adjectives.Contains("large"));
        Assert.Contains(chalice, engine.State.ContentsOf(bag));
        Assert.True(engine.State.IsLocatedIn(thief, treasureRoom));
    }

    [Fact]
    public void Zork_TreasureRoom_KillThief_OnSeed1_AfterDisarm()
    {
        (GameEngine engine, InMemoryTerminal term, Room treasureRoom, Thing thief, Thing sword, _, Thing lantern) = Build();

        engine.State.TurnCount = 50;
        engine.State.TakeIntoInventory(sword);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        term.Reset();

        engine.MovePlayerTo(treasureRoom);
        engine.Submit("take sword");

        for (int i = 0; i < 20 && engine.State.IsLocatedIn(thief, treasureRoom) && !engine.State.IsOver; i++)
            engine.Submit("kill thief with sword");

        Assert.False(engine.State.IsLocatedIn(thief, treasureRoom));
        Assert.Contains("falls to the floor, dead", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_GiveThiefTreasure_EngrossesThief()
    {
        (GameEngine engine, InMemoryTerminal term, _, Thing thief, _, _, Thing lantern) = Build();

        Room roundRoom = engine.Game.World.Rooms.First(r => r.Name == "Round Room");
        Thing painting = engine.Game.World.Things.First(t => t.Name == "painting");

        engine.State.TurnCount = 50;
        engine.State.TakeIntoInventory(painting);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.MovePlayerTo(roundRoom);
        engine.State.Move(thief, Placement.InRoom(roundRoom));
        thief.Set(Attr.Concealed, false);
        term.Reset();

        engine.Submit("give painting to thief");

        Assert.Contains("thanks you politely", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(painting, engine.State.ContentsOf(
            engine.Game.World.Things.First(t => t.Nouns.Contains("bag") && t.Adjectives.Contains("large"))));
    }
}
