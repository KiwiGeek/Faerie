using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class DomeRopeTests
{
    [Fact]
    public void Zork_TieRopeToCeiling_OpensDescentToTorchRoom()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room domeRoom = game.World.Rooms.First(r => r.Name == "Dome Room");
        Room torchRoom = game.World.Rooms.First(r => r.Name == "Torch Room");
        Thing rope = game.World.Things.First(t => t.Name == "rope");
        Thing lantern = game.World.Things.First(t => t.Name == "brass lantern");

        engine.State.CurrentRoom = domeRoom;
        engine.State.TakeIntoInventory(rope);
        engine.State.TakeIntoInventory(lantern);
        lantern.Set(Attr.Lit);

        engine.Submit("tie rope to ceiling");

        Assert.Contains("tied to the railing", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.True(domeRoom.Exits.ContainsKey(Direction.Down));

        term.Reset();
        engine.Submit("down");

        Assert.Equal(torchRoom, engine.State.CurrentRoom);
    }
}
