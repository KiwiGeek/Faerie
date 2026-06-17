using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public class RoomBannerTests
{
    private static (GameEngine engine, InMemoryTerminal term, Room bar, Room back, Thing beer, Thing bartender)
        BuildSierraWorld(bool doorBlocksBack = false, bool resetAfterStart = true)
    {
        GameBuilder b = GameBuilder.Create("Softporn Test")
            .AddStandardVerbs()
            .WithSierraRoomBanner()
            .WithRoomBannerSeparatorWidth(20);

        Room bar = b.Room("The Bar")
            .ShortTitle("BAR")
            .Describe("A smoky bar with a long counter.");
        Room back = b.Room("Back Room")
            .ShortTitle("BACK ROOM")
            .Describe("A private room behind the bar.");

        Exit east = bar.Connect(Direction.East, back);
        if (doorBlocksBack)
        {
            Thing door = b.Scenery("door").Describe("A heavy door.").Openable(open: false);
            door.StartsIn(bar);
            east.Door = door;
        }

        Thing beer = b.Item("beer").Describe("A frosty mug.").StartsIn(bar);
        b.Scenery("counter").Describe("A polished bar counter.").StartsIn(bar);
        Thing bartender = b.Creature("bartender").Describe("A bored bartender.").StartsIn(bar);

        b.StartIn(bar);
        Game game = b.Build();

        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        if (resetAfterStart)
            term.Reset();
        return (engine, term, bar, back, beer, bartender);
    }

    [Fact]
    public void Start_PrintsLongDescriptionThenSierraBanner()
    {
        var (_, term, _, _, _, _) = BuildSierraWorld(resetAfterStart: false);

        Assert.Contains("A smoky bar with a long counter.", term.Output);
        Assert.Contains("BAR", term.Output);
        Assert.Contains("Items in sight: bartender, beer", term.Output);
        Assert.Contains("Other exits: EAST", term.Output);
        Assert.Contains(new string('=', 20), term.Output);
        Assert.DoesNotContain("You can see", term.Output);
        Assert.DoesNotContain("Exits:", term.Output);
    }

    [Fact]
    public void TurnEnd_ReprintsBannerWithoutLongDescription()
    {
        var (engine, term, _, _, _, _) = BuildSierraWorld();

        engine.Submit("wait");

        Assert.DoesNotContain("A smoky bar with a long counter.", term.Output);
        Assert.Contains("BAR", term.Output);
        Assert.Contains("Items in sight:", term.Output);
        Assert.Contains(new string('=', 20), term.Output);
    }

    [Fact]
    public void ReEntry_SkipsLongDescriptionButShowsBanner()
    {
        var (engine, term, bar, _, _, _) = BuildSierraWorld();

        engine.Submit("east");
        term.Reset();
        engine.Submit("west");

        Assert.DoesNotContain("A smoky bar with a long counter.", term.Output);
        Assert.Contains("BAR", term.Output);
        Assert.Equal(bar, engine.State.CurrentRoom);
    }

    [Fact]
    public void Look_ShowsLongDescriptionAgain()
    {
        var (engine, term, _, _, _, _) = BuildSierraWorld();

        engine.Submit("look");

        Assert.Contains("A smoky bar with a long counter.", term.Output);
        Assert.Contains("BAR", term.Output);
    }

    [Fact]
    public void BlockedExit_OmittedFromOtherAreas()
    {
        var (engine, term, _, _, _, _) = BuildSierraWorld(doorBlocksBack: true);

        engine.Submit("wait");

        Assert.Contains("Other exits: none", term.Output);
    }

    [Fact]
    public void TakenItem_OmittedFromItemsInSight()
    {
        var (engine, term, _, _, beer, _) = BuildSierraWorld();

        engine.Submit("take beer");

        Assert.True(engine.State.IsCarried(beer));
        Assert.Contains("Items in sight: bartender", term.Output);
        Assert.DoesNotContain("beer", term.Output);
    }

    [Fact]
    public void FirstVisitToDarkRoom_PrintsDarknessMessageNotLongDescription()
    {
        GameBuilder b = GameBuilder.Create("Dark")
            .AddStandardVerbs()
            .WithSierraRoomBanner()
            .WithRoomBannerSeparatorWidth(20);

        Room bar = b.Room("Bar").ShortTitle("BAR").Describe("A lit bar.");
        Room restroom = b.Room("Men's Room")
            .ShortTitle("MEN'S ROOM")
            .Describe("Fluorescent tubes buzz over cracked tiles.")
            .Dark();
        bar.West(restroom);
        b.StartIn(bar);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("west");

        Assert.Contains("pitch dark", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Fluorescent tubes", term.Output);
        Assert.Contains("MEN'S ROOM", term.Output);
    }

    [Fact]
    public void LookInDarkRoom_WithLight_ShowsLongDescription()
    {
        GameBuilder b = GameBuilder.Create("Dark")
            .AddStandardVerbs()
            .WithSierraRoomBanner();

        Room bar = b.Room("Bar").ShortTitle("BAR").Describe("A lit bar.");
        Room restroom = b.Room("Men's Room")
            .ShortTitle("MEN'S ROOM")
            .Describe("Fluorescent tubes buzz over cracked tiles.")
            .Dark();
        b.Item("lantern").Describe("A lantern.").LightSource(lit: true).StartsIn(bar);
        bar.West(restroom);
        b.StartIn(bar);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("take lantern");
        term.Reset();
        engine.Submit("west");
        term.Reset();
        engine.Submit("look");

        Assert.Contains("Fluorescent tubes buzz over cracked tiles.", term.Output);
    }

    [Fact]
    public void GameContext_PrintRoomBanner_MatchesEngine()
    {
        GameContext? ctx = null;
        GameBuilder b = GameBuilder.Create("Manual")
            .AddStandardVerbs()
            .WithSierraRoomBanner()
            .WithRoomBannerSeparatorWidth(10)
            .OnStart(c => ctx = c);
        Room hall = b.Room("Hall").ShortTitle("HALL").Describe("Spacious.");
        b.StartIn(hall);
        Game game = b.Build();

        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        term.Reset();

        ctx!.PrintRoomBanner();

        Assert.Contains("HALL", term.Output);
        Assert.Contains(new string('=', 10), term.Output);
    }
}
