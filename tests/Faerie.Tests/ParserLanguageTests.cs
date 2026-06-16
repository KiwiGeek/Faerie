using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class ParserLanguageTests
{
    private static (GameEngine engine, InMemoryTerminal term, Thing key, Thing lamp, Thing coin) BuildRoom()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Thing key = b.Item("brass key").Called("key").Adjectives("brass").Describe("A brass key.");
        Thing lamp = b.Item("brass lamp").Called("lamp").Adjectives("brass").Describe("A brass lamp.");
        Thing coin = b.Item("gold coin").Called("coin").Adjectives("gold").Describe("A gold coin.");
        key.StartsIn(hall);
        lamp.StartsIn(hall);
        coin.StartsIn(hall);
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();
        return (engine, term, key, lamp, coin);
    }

    [Fact]
    public void PronounIt_RefersToLastMentionedThing()
    {
        (GameEngine engine, _, Thing key, Thing lamp, _) = BuildRoom();
        engine.Submit("examine lamp");
        engine.Submit("take it");

        Assert.True(engine.State.IsCarried(lamp));
        Assert.False(engine.State.IsCarried(key));
    }

    [Fact]
    public void PronounIt_WorksAfterTake()
    {
        (GameEngine engine, _, Thing key, _, _) = BuildRoom();
        engine.Submit("take key");
        engine.Submit("drop it");

        Assert.False(engine.State.IsCarried(key));
        Assert.Contains(key, engine.State.ContentsOf(engine.State.CurrentRoom));
    }

    [Fact]
    public void TakeAll_TakesEveryVisibleTakeable()
    {
        (GameEngine engine, _, Thing key, Thing lamp, Thing coin) = BuildRoom();
        engine.Submit("take all");

        Assert.True(engine.State.IsCarried(key));
        Assert.True(engine.State.IsCarried(lamp));
        Assert.True(engine.State.IsCarried(coin));
    }

    [Fact]
    public void DropAll_DropsEntireInventory()
    {
        (GameEngine engine, _, Thing key, Thing lamp, Thing coin) = BuildRoom();
        engine.Submit("take all");
        engine.Submit("drop all");

        Assert.Empty(engine.State.Inventory);
        Room hall = engine.State.CurrentRoom;
        Assert.Contains(key, engine.State.ContentsOf(hall));
        Assert.Contains(lamp, engine.State.ContentsOf(hall));
        Assert.Contains(coin, engine.State.ContentsOf(hall));
    }

    [Fact]
    public void CompoundAnd_DropsNamedObjects()
    {
        (GameEngine engine, _, Thing key, Thing lamp, Thing coin) = BuildRoom();
        engine.Submit("take key");
        engine.Submit("take lamp");
        engine.Submit("drop key and lamp");

        Assert.False(engine.State.IsCarried(key));
        Assert.False(engine.State.IsCarried(lamp));
        Assert.True(engine.State.IsCarried(coin) == false);
        Assert.DoesNotContain(coin, engine.State.Inventory);
    }

    [Fact]
    public void CompoundComma_DropsNamedObjects()
    {
        (GameEngine engine, _, Thing key, Thing lamp, _) = BuildRoom();
        engine.Submit("take key");
        engine.Submit("take lamp");
        engine.Submit("drop key, lamp");

        Assert.False(engine.State.IsCarried(key));
        Assert.False(engine.State.IsCarried(lamp));
    }

    [Fact]
    public void TakeAll_EmptyRoom_ReportsNothingToTake()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("An empty hall.");
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("take all");

        Assert.Contains("nothing here to take", term.Output);
    }
}
