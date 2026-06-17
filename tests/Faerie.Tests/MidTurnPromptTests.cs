using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Terminal.Headless;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class MidTurnPromptTests
{
    [Fact]
    public void PromptLine_DuringVerb_CompletesBeforeTurnEnds()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing button = b.Scenery("button").Describe("A button.");
        button.StartsIn(hall);
        b.StartIn(hall);

        b.DefineVerb("push", ["push"], VerbForms.Transitive, ctx =>
        {
            if (ctx.DirectObject != button) return VerbResult.Done;
            string code = ctx.PromptLine("Password: ");
            ctx.Say(code.Equals("BELLYBUTTON", StringComparison.OrdinalIgnoreCase)
                ? "Click!"
                : "Wrong.");
            return VerbResult.Done;
        });

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.PlayerInput = new QueuedPlayerInput("BELLYBUTTON");
        engine.Start();
        term.Reset();

        engine.Submit("push button");

        Assert.Contains("Click!", term.Output);
        Assert.Equal(1, engine.State.TurnCount);
    }

    [Fact]
    public void PromptKey_AcceptsSingleLetter()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);

        b.DefineVerb("ask", ["ask"], VerbForms.Intransitive, ctx =>
        {
            char answer = ctx.PromptKey("Continue? (y/n) ", "yn");
            ctx.Say(answer == 'y' ? "Yes." : "No.");
            return VerbResult.Done;
        });

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.PlayerInput = new QueuedPlayerInput("y");
        engine.Start();
        term.Reset();

        engine.Submit("ask");

        Assert.Contains("Yes.", term.Output);
    }

    [Fact]
    public void PromptKey_NormalizesToValidKeyCasing()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);

        b.DefineVerb("ask", ["ask"], VerbForms.Intransitive, ctx =>
        {
            char answer = ctx.PromptKey("Continue? (y/n) ", "YN");
            ctx.Say(answer == 'Y' ? "Yes." : "No.");
            return VerbResult.Done;
        });

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.PlayerInput = new QueuedPlayerInput("y");
        engine.Start();
        term.Reset();

        engine.Submit("ask");

        Assert.Contains("Yes.", term.Output);
    }

    [Fact]
    public void HeadlessScript_IncludesPromptAnswersInOrder()
    {
        string dir = Path.Combine(Path.GetTempPath(), "faerie-prompt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string script = Path.Combine(dir, "script.txt");
        string transcript = Path.Combine(dir, "out.txt");
        File.WriteAllText(script, """
            push button
            BELLYBUTTON
            """);

        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing button = b.Scenery("button").Describe("A button.");
        button.StartsIn(hall);
        b.StartIn(hall);
        b.DefineVerb("push", ["push"], VerbForms.Transitive, ctx =>
        {
            if (ctx.DirectObject != button) return VerbResult.Done;
            string code = ctx.PromptLine("Password: ");
            ctx.Say(code.Equals("BELLYBUTTON", StringComparison.OrdinalIgnoreCase) ? "Click!" : "Wrong.");
            return VerbResult.Done;
        });

        int exit = HeadlessRunner.Run(b.Build(), new HeadlessOptions
        {
            ScriptPath = script,
            TranscriptPath = transcript,
            RandomSeed = 1
        });

        Assert.Equal(0, exit);
        string log = File.ReadAllText(transcript);
        Assert.Contains("> push button", log);
        Assert.Contains("> BELLYBUTTON", log);
        Assert.Contains("Click!", log);

        Directory.Delete(dir, recursive: true);
    }
}
