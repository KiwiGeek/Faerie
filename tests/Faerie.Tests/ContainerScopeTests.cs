using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class ContainerScopeTests
{
    private static (GameEngine engine, InMemoryTerminal term, Thing garlic) BuildKitchenSack()
    {
        GameBuilder b = GameBuilder.Create("Kitchen").AddStandardVerbs();
        Room kitchen = b.Room("Kitchen").Describe("A kitchen.");
        Thing sack = b.Item("brown sack").Called("sack", "bag").Adjectives("brown")
            .Describe("The brown sack is closed.").Container(open: false);
        Thing garlic = b.Item("clove of garlic").Called("garlic", "clove").Adjectives("pungent")
            .Describe("A clove of garlic.");
        sack.StartsIn(kitchen);
        garlic.StartsInside(sack);
        b.StartIn(kitchen);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        return (engine, term, garlic);
    }

    [Theory]
    [InlineData("take garlic")]
    [InlineData("take clove")]
    [InlineData("take clove of garlic")]
    public void TakeFromOpenContainer_Works(string command)
    {
        (GameEngine engine, _, Thing garlic) = BuildKitchenSack();
        engine.Submit("open sack");
        engine.Submit(command);

        Assert.True(engine.State.IsCarried(garlic));
    }

    [Fact]
    public void OpenContainer_ContentsVisibleToScope()
    {
        (GameEngine engine, _, Thing garlic) = BuildKitchenSack();
        engine.Submit("open sack");

        Scope scope = new(engine.State);
        Assert.Contains(garlic, scope.VisibleThings());
    }

    [Fact]
    public void TakeGarlik_SuggestsGarlic()
    {
        (GameEngine engine, InMemoryTerminal term, _) = BuildKitchenSack();
        term.Reset();
        engine.Submit("open sack");
        term.Reset();
        engine.Submit("take garlik");

        Assert.Equal("take garlic", engine.SuggestedInput);
        Assert.Contains("Did you mean", term.Output);
    }

    [Fact]
    public void Zork_Kitchen_OpenSack_TakeGarlic()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        Thing sack = game.World.Things.First(t => t.Name == "brown sack");
        Room kitchen = game.World.Rooms.First(r => r.Name == "Kitchen");
        engine.State.CurrentRoom = kitchen;
        term.Reset();

        engine.Submit("open sack");
        Assert.True(sack.Has(Attr.Open));
        Assert.Contains(garlic, new Scope(engine.State).VisibleThings());

        term.Reset();
        engine.Submit("take garlic");
        Assert.True(engine.State.IsCarried(garlic));
    }

    [Fact]
    public void Zork_Kitchen_ClosedSack_Garlik_NoSuggestionUntilOpen()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Room kitchen = game.World.Rooms.First(r => r.Name == "Kitchen");
        engine.State.CurrentRoom = kitchen;
        term.Reset();

        engine.Submit("take garlik");
        Assert.Null(engine.SuggestedInput);

        engine.Submit("open sack");
        term.Reset();
        engine.Submit("take garlik");
        Assert.Equal("take garlic", engine.SuggestedInput);
    }

    [Fact]
    public void Zork_WalkFromLivingRoom_TakeGarlicFromOpenSack()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");
        Room living = game.World.Rooms.First(r => r.Name == "Living Room");

        engine.State.CurrentRoom = living;
        term.Reset();
        engine.Submit("east");
        engine.Submit("open sack");
        engine.Submit("take garlic");

        Assert.Equal("Kitchen", engine.State.CurrentRoom.Name);
        Assert.True(engine.State.IsCarried(garlic));
    }

    [Fact]
    public void Zork_FromStart_ViaWindowToKitchen_TakeGarlic()
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing garlic = game.World.Things.First(t => t.Name == "clove of garlic");

        engine.Submit("north");
        engine.Submit("east");
        engine.Submit("open window");
        engine.Submit("in");
        engine.Submit("open sack");
        engine.Submit("take garlic");

        Assert.True(engine.State.IsCarried(garlic));
    }

    [Fact]
    public void Resolve_PhraseWithOf_MatchesSynonymsWithoutFullName()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing coin = b.Item("coin").Called("piece").Describe("A gold coin.");
        coin.StartsIn(hall);
        b.StartIn(hall);
        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Scope scope = new(engine.State);
        NounResolution r = scope.Resolve(["piece", "of", "coin"]);
        Assert.Equal(NounResolution.Kind.Single, r.Status);
        Assert.Same(coin, r.Thing);
    }
}
