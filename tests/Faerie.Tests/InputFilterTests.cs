using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class InputFilterTests
{
    [Fact]
    public void FilterInput_can_reject_without_advancing_turn()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Thing lamp = b.Item("lamp").Takeable();
        hall.Contains(lamp);
        b.StartIn(hall);
        b.FilterInput((_, line) =>
            line.Contains("lamp", StringComparison.OrdinalIgnoreCase)
                ? InputFilterResult.Reject("Blocked.")
                : InputFilterResult.Continue);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        int turns = engine.State.TurnCount;

        term.Reset();
        engine.Submit("take lamp");

        Assert.Equal(turns, engine.State.TurnCount);
        Assert.Contains("Blocked.", term.Output);
        Assert.False(engine.State.IsCarried(lamp));
    }

    [Fact]
    public void FilterInput_continue_allows_normal_execution()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Thing lamp = b.Item("lamp").Takeable();
        hall.Contains(lamp);
        b.StartIn(hall);
        b.FilterInput((_, _) => InputFilterResult.Continue);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Submit("take lamp");

        Assert.True(engine.State.IsCarried(lamp));
        Assert.Equal(1, engine.State.TurnCount);
    }
}
