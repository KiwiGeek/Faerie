using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

/// <summary>
/// Audit: every treasure or puzzle object that starts <see cref="Attr.Concealed"/> must have a
/// source-accurate reveal trigger. See also <c>LoudRoomTests</c>, <c>ZorkSkeletonTests</c>,
/// <c>ZorkTrapDoorTests</c>, and <c>ZorkGratingTests</c> for overlapping coverage.
/// </summary>
public sealed class ZorkConcealedRevealTests
{
    private static void CarryLitLantern(GameEngine engine, Game game)
    {
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
    }

    [Fact]
    public void Zork_MoveRug_RevealsTrapDoor()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room livingRoom = game.World.Rooms.First(r => r.Name == "Living Room");
        Thing trapDoor = game.World.Things.First(t => t.Name == "trap door");

        engine.State.CurrentRoom = livingRoom;
        Assert.True(trapDoor.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("move rug");

        Assert.False(trapDoor.Has(Attr.Concealed));
        Assert.Contains("trap door", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_MoveLeaves_RevealsGrating()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room clearing = game.World.Rooms.First(r =>
            r.Description.Contains("path leads south", StringComparison.Ordinal));
        Thing grating = game.World.Things.First(t => t.Nouns.Contains("grate"));

        engine.State.CurrentRoom = clearing;
        Assert.True(grating.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("move leaves");

        Assert.False(grating.Has(Attr.Concealed));
        Assert.Contains("grating", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_FourthDig_RevealsScarab()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing scarab = game.World.Things.First(t => t.Nouns.Contains("scarab"));
        Thing shovel = game.World.Things.First(t => t.Nouns.Contains("shovel"));
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        Room sandyCave = game.World.Rooms.First(r => r.Name == "Sandy Cave");

        engine.State.TakeIntoInventory(shovel);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit, true);
        engine.State.CurrentRoom = sandyCave;
        Assert.True(scarab.Has(Attr.Concealed));

        for (int i = 0; i < 3; i++)
            engine.Submit("dig sand");

        term.Reset();
        engine.Submit("dig sand");

        Assert.False(scarab.Has(Attr.Concealed));
        Assert.Contains(scarab, engine.State.ContentsOf(sandyCave));
    }

    [Fact]
    public void Zork_WaveSceptre_RevealsPotOfGold()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room rainbow = game.World.Rooms.First(r => r.Name == "End of Rainbow");
        Thing sceptre = game.World.Things.First(t => t.Nouns.Contains("sceptre"));
        Thing pot = game.World.Things.First(t => t.Name == "pot of gold");

        engine.State.TakeIntoInventory(sceptre);
        engine.State.CurrentRoom = rainbow;
        Assert.True(pot.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("wave sceptre");

        Assert.False(pot.Has(Attr.Concealed));
        Assert.Contains("pot of gold", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_MachineCoal_RevealsDiamond()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room machineRoom = game.World.Rooms.First(r => r.Name == "Machine Room");
        Thing coal = game.World.Things.First(t => t.Name == "small lump of coal");
        Thing screwdriver = game.World.Things.First(t => t.Name == "screwdriver");
        Thing machine = game.World.Things.First(t => t.Nouns.Contains("machine"));
        Thing diamond = game.World.Things.First(t => t.Name == "huge diamond");

        CarryLitLantern(engine, game);
        engine.State.TakeIntoInventory(coal);
        engine.State.TakeIntoInventory(screwdriver);
        engine.State.CurrentRoom = machineRoom;

        term.Reset();
        engine.Submit("put coal in machine");

        Assert.False(diamond.Has(Attr.Concealed));
        Assert.Contains(diamond, engine.State.ContentsOf(machineRoom));
        Assert.Contains("diamond", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_BlueButton_RevealsLeak()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room maintenance = game.World.Rooms.First(r => r.Name == "Maintenance Room");
        Thing leak = game.World.Things.First(t => t.Nouns.Contains("leak"));

        CarryLitLantern(engine, game);
        engine.State.CurrentRoom = maintenance;
        Assert.True(leak.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("push blue button");

        Assert.False(leak.Has(Attr.Concealed));
        Assert.Contains("leak", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_DamDrain_RevealsTrunkInReservoir()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room maintenance = game.World.Rooms.First(r => r.Name == "Maintenance Room");
        Room dam = game.World.Rooms.First(r => r.Name == "Dam");
        Room reservoir = game.World.Rooms.First(r => r.Name == "Reservoir");
        Thing wrench = game.World.Things.First(t => t.Name == "wrench");
        Thing trunk = game.World.Things.First(t => t.Name == "trunk of jewels");

        CarryLitLantern(engine, game);
        engine.State.TakeIntoInventory(wrench);
        engine.State.CurrentRoom = maintenance;
        engine.Submit("push yellow button");
        engine.State.CurrentRoom = dam;
        engine.Submit("turn bolt");

        for (int i = 0; i < 8; i++)
            engine.Submit("wait");

        engine.State.CurrentRoom = reservoir;
        Assert.False(trunk.Has(Attr.Concealed));
        Assert.Contains(trunk, engine.State.ContentsOf(reservoir));
    }
}
