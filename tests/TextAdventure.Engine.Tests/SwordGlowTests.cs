using Nixie.Model;
using Nixie.Runtime;
using Nixie.Tests;
using TextAdventure.Sample.Zork;
using Xunit;

public class SwordGlowTests
{
    [Fact]
    public void Sword_Glows_By_Villain_Proximity()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        // Things are looked up by display name; Thing.Id is an auto-slug (e.g. "thing:elvish-sword").
        Thing sword = game.World.Things.First(t => t.Name == "elvish sword");
        Thing troll = game.World.Things.First(t => t.Name == "troll");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        Room trollRoom = engine.State.RoomOf(troll)!;
        Room adjacent = game.World.Rooms.First(r => r.Exits.Values.Any(e => e.Destination == trollRoom));

        engine.State.TakeIntoInventory(sword);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);                 // keep rooms lit so the turn isn't dark-gated

        // One room away -> faint
        engine.State.CurrentRoom = adjacent;
        term.Reset();
        engine.Submit("look");
        Assert.Contains("faint blue glow", term.Output);

        // Same room -> bright
        engine.State.CurrentRoom = trollRoom;
        term.Reset();
        engine.Submit("look");
        Assert.Contains("very brightly", term.Output);

        // Villain gone -> stops glowing
        engine.State.Move(troll, Placement.Offstage);
        term.Reset();
        engine.Submit("look");
        Assert.Contains("no longer glowing", term.Output);
    }
}