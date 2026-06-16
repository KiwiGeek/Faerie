using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class ScheduleInTests
{
    [Fact]
    public void ScheduleIn_FiresAfterRelativeTurns()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);
        b.ScheduleIn(2, ctx => ctx.Say("The fuse explodes."));
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("wait");
        Assert.DoesNotContain("fuse explodes", term.Output);
        engine.Submit("wait");
        Assert.Contains("fuse explodes", term.Output);
    }

    [Fact]
    public void ScheduleIn_WhenConditionBlocks_FiresOnlyWhileTrue()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        StateKey<bool> armed = b.State("armed", false);
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);
        b.ScheduleIn(1, ctx => ctx.Say("Boom."), when: ctx => ctx.Get(armed));
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("wait");
        Assert.DoesNotContain("Boom", term.Output);

        engine.State.Set(armed, true);
        term.Reset();
        engine.Submit("wait");
        Assert.Contains("Boom", term.Output);
    }

    [Fact]
    public void CancelSchedule_PreventsNamedTimer()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing button = b.Scenery("button").Describe("A red button.");
        button.StartsIn(hall);
        b.On(button).Before(b.Verbs.Examine!, ctx =>
        {
            ctx.CancelSchedule("fuse");
            return VerbResult.Pass;
        });
        b.StartIn(hall);
        b.ScheduleIn("fuse", 2, ctx => ctx.Say("Boom."));
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("examine button");
        engine.Submit("wait");
        Assert.DoesNotContain("Boom", term.Output);
    }

    [Fact]
    public void ContextScheduleIn_FiresRelativeToCurrentTurn()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing lever = b.Scenery("lever").Describe("A lever.");
        lever.StartsIn(hall);
        b.On(lever).Before(b.Verbs.Examine!, ctx =>
        {
            ctx.ScheduleIn(1, c => c.Say("The walls rumble."));
            return VerbResult.Pass;
        });
        b.StartIn(hall);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("examine lever");
        Assert.DoesNotContain("rumble", term.Output);
        engine.Submit("wait");
        Assert.Contains("rumble", term.Output);
    }

    [Fact]
    public void SaveRestore_PreservesTimerState()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);
        b.ScheduleIn(3, ctx => ctx.Say("Tick."));
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();

        engine.Submit("wait");
        string snapshot = engine.CreateSnapshot();
        engine.Submit("wait");
        engine.Submit("wait");
        term.Reset();

        engine.LoadSnapshot(snapshot);
        engine.Submit("wait");
        engine.Submit("wait");
        Assert.Contains("Tick.", term.Output);
    }
}
