using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Tests;
using Xunit;

public class OutputFilterTests
{
    [Fact]
    public void A_filter_rewrites_output_before_it_reaches_the_terminal()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        b.StartIn(hall);
        b.FilterOutput((ctx, text) => text.Replace("plain", "MARVELLOUS"));

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();   // prints the opening room description through the filter

        Assert.Contains("MARVELLOUS", term.Output);
        Assert.DoesNotContain("plain", term.Output);
    }

    [Fact]
    public void A_filter_returning_null_suppresses_a_line_entirely()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        b.StartIn(hall);
        b.OnStart(ctx => { ctx.Say("keep this line"); ctx.Say("a secret line"); });
        b.FilterOutput((ctx, text) => text.Contains("secret") ? null : text);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();

        Assert.Contains("keep this line", term.Output);
        Assert.DoesNotContain("secret", term.Output);
    }

    [Fact]
    public void A_filter_can_be_scoped_to_state_so_it_only_acts_when_active()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        b.StartIn(hall);
        StateKey<bool> shouting = b.State("shouting", false);
        b.FilterOutput((ctx, text) => ctx.Get(shouting) ? text.ToUpperInvariant() : text);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        Assert.Contains("plain hall", term.Output);   // filter inactive: unchanged

        engine.State.Set(shouting, true);
        term.Reset();
        engine.Submit("look");
        Assert.Contains("PLAIN HALL", term.Output);    // filter active: rewritten
    }
}
