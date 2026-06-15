using Nixie.Building;
using Nixie.Model;
using Nixie.Runtime;
using Xunit;

namespace Nixie.Tests;

public class EngineTests
{
    /// <summary>Builds a tiny two-room world used across the tests.</summary>
    private static (GameEngine engine, InMemoryTerminal term, World world,
        Room hall, Room cellar, Thing key, Thing chest, Thing lamp) BuildWorld()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();

        Room hall = b.Room("Hall").Describe("A plain hall.");
        Room cellar = b.Room("Cellar").Describe("A cold cellar.").Dark();
        hall.Down(cellar);

        Thing key = b.Item("brass key").Adjectives("brass").Describe("A brass key.");
        Thing chest = b.Scenery("chest").Describe("A wooden chest.").Container(open: false);
        Thing lamp = b.Item("lamp").Describe("A brass lamp.").LightSource(lit: false);

        key.StartsInside(chest);
        chest.StartsIn(hall);
        lamp.StartsIn(hall);

        b.StartIn(hall);
        Game game = b.Build();

        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        term.Reset();
        return (engine, term, game.World, hall, cellar, key, chest, lamp);
    }

    [Fact]
    public void Movement_MovesBetweenRooms()
    {
        var (engine, term, _, _, cellar, _, _, lamp) = BuildWorld();
        engine.State.TakeIntoInventory(lamp);
        lamp.Set(Attr.Lit);

        engine.Submit("down");

        Assert.Equal(cellar, engine.State.CurrentRoom);
        Assert.Contains("Cellar", term.Output);
    }

    [Fact]
    public void Synonyms_GetIsTake()
    {
        var (engine, _, _, _, _, key, chest, _) = BuildWorld();
        chest.Set(Attr.Open);

        engine.Submit("get brass key");

        Assert.True(engine.State.IsCarried(key));
    }

    [Fact]
    public void Container_MustBeOpenToTakeContents()
    {
        var (engine, term, _, _, _, key, _, _) = BuildWorld();

        engine.Submit("take key");   // chest is closed, key not visible

        Assert.False(engine.State.IsCarried(key));
        Assert.Contains("can't see", term.Output);
    }

    [Fact]
    public void Open_RevealsAndAllowsTake()
    {
        var (engine, _, _, _, _, key, _, _) = BuildWorld();

        engine.Submit("open chest");
        engine.Submit("take brass key");

        Assert.True(engine.State.IsCarried(key));
    }

    [Fact]
    public void Darkness_BlocksSightVerbs()
    {
        var (engine, term, _, _, _, _, _, _) = BuildWorld();
        engine.Submit("down");   // enter dark cellar with no light
        term.Reset();

        engine.Submit("look");   // objectless verb, so the darkness gate is what fires

        Assert.Contains("pitch black", term.Output);
    }

    [Fact]
    public void Light_IlluminatesDarkRoom()
    {
        var (engine, term, _, _, cellar, _, _, lamp) = BuildWorld();
        engine.State.TakeIntoInventory(lamp);
        engine.Submit("light lamp");
        engine.Submit("down");
        term.Reset();

        engine.Submit("look");

        Assert.Equal(cellar, engine.State.CurrentRoom);
        Assert.Contains("cold cellar", term.Output);
    }

    [Fact]
    public void Ditransitive_PutInContainer()
    {
        var (engine, _, _, _, _, key, chest, _) = BuildWorld();
        chest.Set(Attr.Open);
        engine.Submit("open chest");
        engine.Submit("take key");

        engine.Submit("put key in chest");

        Assert.Equal(chest, engine.State.PlacementOf(key).Container);
        Assert.Equal(Anchor.Inside, engine.State.PlacementOf(key).Anchor);
    }

    [Fact]
    public void UnknownVerb_IsReported()
    {
        var (engine, term, _, _, _, _, _, _) = BuildWorld();
        engine.Submit("flibber the widget");
        Assert.Contains("I don't know how to", term.Output);
    }

    [Fact]
    public void SaveRestore_RoundTripsState()
    {
        var (engine, _, _, hall, _, key, chest, _) = BuildWorld();
        chest.Set(Attr.Open);
        engine.Submit("take brass key");   // the item's only noun is the full "brass key" (no "key" alias)
        engine.State.Score = 42;
        string snapshot = engine.CreateSnapshot();

        // Mutate away from the saved state.
        engine.State.MoveTo(key, hall);
        engine.State.Score = 0;

        engine.LoadSnapshot(snapshot);

        Assert.Equal(42, engine.State.Score);
        Assert.True(engine.State.IsCarried(key));
    }
}
