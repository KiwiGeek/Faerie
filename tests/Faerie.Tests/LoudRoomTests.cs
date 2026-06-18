using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public class LoudRoomTests
{
    [Fact]
    public void Zork_LoudRoom_Echo_RevealsPlatinumBar()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room loudRoom = game.World.Rooms.First(r => r.Name == "Loud Room");
        Thing bar = game.World.Things.First(t => t.Name == "platinum bar");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        engine.State.CurrentRoom = loudRoom;
        Assert.True(bar.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("echo");

        Assert.False(bar.Has(Attr.Concealed));
        Assert.Contains(bar, engine.State.ContentsOf(loudRoom));
        Assert.Contains("acoustics of the room change subtly", term.Output);

        term.Reset();
        engine.Submit("take bar");
        Assert.Contains(bar, engine.State.Inventory);
        Assert.DoesNotContain("can't see", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_LoudRoom_OutputFilter_IgnoresPromptMarkup()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room loudRoom = game.World.Rooms.First(r => r.Name == "Loud Room");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        engine.State.CurrentRoom = loudRoom;

        term.Reset();
        engine.Out.Print("{fg:lightgreen}>{/} ");

        Assert.DoesNotContain("lightgreen", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_LoudRoom_OutputFilter_EchoesLastWordOfGameText()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room loudRoom = game.World.Rooms.First(r => r.Name == "Loud Room");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);
        engine.State.CurrentRoom = loudRoom;

        term.Reset();
        engine.Out.PrintLine("This is a {fg:yellow}loud{/} room.");

        Assert.Contains("room... room...", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("yellow", term.Output, StringComparison.OrdinalIgnoreCase);
    }
}
