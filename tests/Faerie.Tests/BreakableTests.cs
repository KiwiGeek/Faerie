using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class BreakableTests
{
    [Fact]
    public void Break_SetsBrokenFlag()
    {
        GameBuilder b = GameBuilder.Create("B").AddCoreVerbs();
        Room room = b.Room("Room");
        Thing vase = b.Scenery("vase").Called("vase").Breakable();
        room.Contains(vase);
        b.StartIn(room);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("break vase");

        Assert.True(vase.Has(Attr.Broken));
        Assert.Contains("break", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Break_AlreadyBroken_UsesCustomMessage()
    {
        GameBuilder b = GameBuilder.Create("B").AddCoreVerbs();
        Room room = b.Room("Room");
        Thing vase = b.Scenery("vase").Called("vase").Breakable(alreadyBrokenMessage: "Enough damage.");
        vase.Set(Attr.Broken);
        room.Contains(vase);
        b.StartIn(room);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("smash vase");

        Assert.Contains("Enough damage.", term.Output);
    }
}
