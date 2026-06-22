using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class ScoringTests
{
    [Fact]
    public void AdjustScore_ClampsToMax()
    {
        GameState state = new(CreateWorld());
        state.AdjustScore(400, maxScore: 350);
        Assert.Equal(350, state.Score);
    }

    [Fact]
    public void AdjustScore_DoesNotGoBelowZero()
    {
        GameState state = new(CreateWorld());
        state.Score = 5;
        state.AdjustScore(-20);
        Assert.Equal(0, state.Score);
    }

    [Fact]
    public void AwardOnce_OnlyAwardsFirstTime()
    {
        GameBuilder b = GameBuilder.Create("S").AddCoreVerbs();
        Room room = b.Room("Room");
        b.StartIn(room);
        StateKey<int> mask = b.State("mask", 0);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Assert.True(Scoring.AwardOnce(engine.Context, mask, 0, 10));
        Assert.False(Scoring.AwardOnce(engine.Context, mask, 0, 10));
        Assert.Equal(10, engine.State.Score);
    }

    [Fact]
    public void SyncTrophyCase_AddsAndRemovesPoints()
    {
        GameBuilder b = GameBuilder.Create("S").AddCoreVerbs().WithMaxScore(100);
        Room room = b.Room("Room");
        Thing caseBox = b.Scenery("case").Container();
        Thing coin = b.Item("coin").Takeable();
        caseBox.StartsIn(room);
        b.StartIn(room);
        StateKey<int> caseScore = b.State("case-score", 0);
        StateKey<int> touched = b.State("touched", 0);
        Scoring.TrophyEntry[] entries = [new(coin, 0, CasePoints: 5, TouchPoints: 2)];

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        GameContext ctx = engine.Context;

        ctx.State.Move(coin, Placement.Inside(caseBox));
        Scoring.SyncTrophyCase(ctx, caseBox, caseScore, touched, entries);
        Assert.Equal(5, engine.State.Score);

        ctx.Take(coin);
        Scoring.SyncTrophyCase(ctx, caseBox, caseScore, touched, entries);
        Assert.Equal(0, engine.State.Score);
    }

    private static World CreateWorld()
    {
        GameBuilder b = GameBuilder.Create("S");
        Room room = b.Room("Room");
        b.StartIn(room);
        return b.Build().World;
    }
}
