using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public class RoomPresentationTests
{
    [Fact]
    public void Default_UsesInfocomPresentationOnReEnter()
    {
        GameBuilder b = GameBuilder.Create("Infocom").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A grand hall.").Brief("Back in the hall.");
        Room cellar = b.Room("Cellar").Describe("A cold cellar.");
        hall.Down(cellar);
        b.StartIn(hall);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("down");
        term.Reset();
        engine.Submit("up");

        Assert.Contains("Back in the hall.", term.Output);
        Assert.DoesNotContain("A grand hall.", term.Output);
    }

    [Fact]
    public void CustomPresentation_ReceivesDescribeMoments()
    {
        List<RoomDescribeMoment> moments = [];

        GameBuilder b = GameBuilder.Create("Custom").AddStandardVerbs()
            .WithRoomPresentation(new RoomPresentation
            {
                DescribeRoom = ctx => moments.Add(ctx.Moment),
                RefreshRoomDisplay = ctx => ctx.Say($"REFRESH:{ctx.CurrentRoom.Name}")
            });

        Room a = b.Room("Alpha").Describe("Room A.");
        Room bRoom = b.Room("Beta").Describe("Room B.");
        a.East(bRoom);
        b.StartIn(a);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("east");
        term.Reset();
        engine.Submit("west");
        term.Reset();
        engine.Submit("look");
        term.Reset();
        engine.Submit("wait");

        Assert.Equal(
            [
                RoomDescribeMoment.FirstEnter,
                RoomDescribeMoment.FirstEnter,
                RoomDescribeMoment.ReEnter,
                RoomDescribeMoment.Look
            ],
            moments);
        Assert.Contains("REFRESH:Alpha", term.Output);
    }

    [Fact]
    public void SierraPreset_MatchesWithSierraRoomBannerSugar()
    {
        GameBuilder direct = GameBuilder.Create("A").AddStandardVerbs()
            .WithRoomPresentation(RoomPresentations.Sierra(new SierraRoomPresentationOptions { SeparatorWidth = 12 }));
        Room hall = direct.Room("Hall").ShortTitle("HALL").Describe("Spacious.");
        direct.StartIn(hall);

        GameBuilder sugar = GameBuilder.Create("B").AddStandardVerbs()
            .WithRoomBannerSeparatorWidth(12)
            .WithSierraRoomBanner();
        sugar.Room("Hall").ShortTitle("HALL").Describe("Spacious.");
        sugar.StartIn(hall);

        InMemoryTerminal termA = new();
        InMemoryTerminal termB = new();
        new GameEngine(direct.Build(), termA, randomSeed: 1).Start();
        new GameEngine(sugar.Build(), termB, randomSeed: 1).Start();

        Assert.Equal(termA.Output, termB.Output);
    }

    [Fact]
    public void PresentRoom_LightingChanged_UsesCustomHook()
    {
        List<RoomDescribeMoment> moments = [];

        GameBuilder b = GameBuilder.Create("Light").AddStandardVerbs()
            .WithRoomPresentation(new RoomPresentation
            {
                DescribeRoom = ctx => moments.Add(ctx.Moment)
            });

        Room cave = b.Room("Cave").Describe("A dark cave.").Dark();
        b.Item("lamp").Describe("A lamp.").LightSource(lit: false).StartsCarried();
        b.StartIn(cave);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("light lamp");

        Assert.Contains(RoomDescribeMoment.LightingChanged, moments);
    }
}
