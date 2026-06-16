using Nixie.Model;
using Nixie.Runtime;
using Nixie.Tests;
using TextAdventure.Sample.Zork;
using Xunit;

public class CombatTests
{
    private static (GameEngine engine, InMemoryTerminal term, Thing troll, Room trollRoom, Thing sword, Thing lantern)
        StartInTrollRoom(bool arm, bool light = true)
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing troll = game.World.Things.First(t => t.Name == "troll");
        Thing sword = game.World.Things.First(t => t.Name == "elvish sword");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        Room trollRoom = engine.State.RoomOf(troll)!;

        if (light) { engine.State.TakeIntoInventory(lantern); lantern.Set(Attr.Lit); }
        if (arm) engine.State.TakeIntoInventory(sword);

        engine.State.CurrentRoom = trollRoom;
        term.Reset();
        return (engine, term, troll, trollRoom, sword, lantern);
    }

    [Fact]
    public void Bare_handed_attack_is_refused_and_the_troll_survives()
    {
        var (engine, term, troll, trollRoom, _, _) = StartInTrollRoom(arm: false);

        engine.Submit("attack troll");

        Assert.Contains("bare hands", term.Output);
        Assert.Equal(trollRoom, engine.State.RoomOf(troll));   // your futile swing changed nothing
    }

    [Fact]
    public void The_troll_attacks_on_its_own_turn_while_you_share_the_room()
    {
        var (engine, term, _, _, _, _) = StartInTrollRoom(arm: true);

        engine.Submit("wait");   // you do nothing; the combat daemon still gives the troll its turn

        Assert.Contains("troll", term.Output);
    }

    [Fact]
    public void A_sword_fight_always_resolves_to_a_dead_troll_or_a_dead_player()
    {
        var (engine, _, troll, _, _, _) = StartInTrollRoom(arm: true);

        for (int i = 0; i < 100 && !engine.State.IsOver && engine.State.RoomOf(troll) is not null; i++)
            engine.Submit("attack troll with sword");

        // HP is bounded on both sides, so the exchange must terminate one way or the other.
        bool trollDead = engine.State.RoomOf(troll) is null;
        Assert.True(trollDead || engine.State.IsOver);
    }
}
