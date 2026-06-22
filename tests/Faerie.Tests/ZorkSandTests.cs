using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkSandTests
{
    private static (GameEngine engine, InMemoryTerminal term, Thing sand, Thing scarab, Thing shovel)
        Build()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing sand = game.World.Things.First(t => t.Nouns.Contains("sand"));
        Thing scarab = game.World.Things.First(t => t.Nouns.Contains("scarab"));
        Thing shovel = game.World.Things.First(t => t.Nouns.Contains("shovel"));
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        Room sandyCave = game.World.Rooms.First(r => r.Name == "Sandy Cave");

        engine.State.TakeIntoInventory(shovel);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.MovePlayerTo(sandyCave);

        return (engine, term, sand, scarab, shovel);
    }

    [Fact]
    public void Zork_Sand_FifthDigWithoutScarabTaken_Collapses()
    {
        (GameEngine engine, InMemoryTerminal term, Thing sand, Thing scarab, _) = Build();

        for (int i = 0; i < 5; i++)
            engine.Submit("dig sand");

        Assert.Contains("smothering you", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Sand_FifthDigAfterTakingScarab_Survives()
    {
        (GameEngine engine, _, Thing sand, Thing scarab, _) = Build();

        for (int i = 0; i < 4; i++)
            engine.Submit("dig sand");

        engine.Submit("take scarab");
        engine.Submit("dig sand");

        Assert.False(engine.State.IsOver);
        Assert.True(engine.State.IsCarried(scarab));
    }
}
