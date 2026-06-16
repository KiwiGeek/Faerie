using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Xunit;

namespace Faerie.Tests;

public sealed class DropHookTests
{
    [Fact]
    public void OnDrop_ReturnTrue_SkipsDefaultPlacement()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing vase = b.Item("vase").Describe("A fragile vase.");
        vase.StartsIn(hall);
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(vase);
        bool hookRan = false;
        vase.OnDrop = ctx =>
        {
            hookRan = true;
            ctx.Say("It shatters!");
            ctx.Remove(vase);
            return true;
        };

        engine.Submit("drop vase");

        Assert.True(hookRan);
        Assert.DoesNotContain(vase, engine.State.ContentsOf(hall));
        Assert.Contains("shatters", term.Output);
        Assert.DoesNotContain("Dropped.", term.Output);
    }

    [Fact]
    public void OnDrop_ReturnFalse_PerformsDefaultDrop()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing lamp = b.Item("lamp").Describe("A lamp.");
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(lamp);
        lamp.OnDrop = _ => false;

        engine.Submit("drop lamp");

        Assert.Contains(lamp, engine.State.ContentsOf(hall));
        Assert.Contains("Dropped.", term.Output);
    }

    [Fact]
    public void Room_OnAfterDrop_FiresAfterDefaultDrop()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing lamp = b.Item("lamp").Describe("A lamp.");
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(lamp);
        Thing? dropped = null;
        hall.OnAfterDrop = (ctx, thing) =>
        {
            dropped = thing;
            ctx.Say("It lands with a thud.");
        };

        engine.Submit("drop lamp");

        Assert.Same(lamp, dropped);
        Assert.Contains("lands with a thud", term.Output);
    }

    [Fact]
    public void Room_OnAfterDrop_NotFiredWhenOnDropHandlesDrop()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing vase = b.Item("vase").Describe("A vase.");
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.State.TakeIntoInventory(vase);
        bool afterRan = false;
        hall.OnAfterDrop = (_, _) => afterRan = true;
        vase.OnDrop = ctx =>
        {
            ctx.Remove(vase);
            return true;
        };

        engine.Submit("drop vase");

        Assert.False(afterRan);
    }

    [Fact]
    public void Zork_EggDropFromTree_BreaksAndReleasesCanary()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing egg = game.World.Things.First(t => t.Name == "jewel-encrusted egg");
        Thing canary = game.World.Things.First(t => t.Name == "golden clockwork canary");
        Room tree = game.World.Rooms.First(r => r.Name == "Up a Tree");

        engine.State.TakeIntoInventory(egg);
        engine.State.CurrentRoom = tree;
        term.Reset();
        engine.Submit("drop egg");

        Assert.DoesNotContain(egg, engine.State.Inventory);
        Assert.Contains(canary, engine.State.ContentsOf(tree));
        Assert.Contains("sickening crunch", term.Output);
    }
}
