using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Terminal.Headless;
using Xunit;

namespace Faerie.Tests;

public sealed class HeadlessRunnerTests
{
    [Fact]
    public void ScriptReader_SkipsBlanksAndComments()
    {
        using StringReader reader = new("""
            take lamp

            # inventory check
            ; another comment
            look
            """);

        Assert.Equal(["take lamp", "look"], ScriptReader.ReadCommands(reader).ToArray());
    }

    [Fact]
    public void Run_WritesTranscriptWithCommandsAndStrippedOutput()
    {
        string dir = Path.Combine(Path.GetTempPath(), "faerie-headless-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string script = Path.Combine(dir, "script.txt");
        string transcript = Path.Combine(dir, "out.txt");
        File.WriteAllText(script, """
            # walk in
            examine lamp
            take lamp
            """);

        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A {fg:yellow}plain{/} hall.");
        Thing lamp = b.Item("lamp").Describe("A brass lamp.");
        lamp.StartsIn(hall);
        b.StartIn(hall);

        int exit = HeadlessRunner.Run(b.Build(), new HeadlessOptions
        {
            ScriptPath = script,
            TranscriptPath = transcript,
            RandomSeed = 1
        });

        Assert.Equal(0, exit);
        string log = File.ReadAllText(transcript);
        Assert.Contains("> examine lamp", log);
        Assert.Contains("> take lamp", log);
        Assert.Contains("brass lamp", log);
        Assert.DoesNotContain("{fg:yellow}", log);

        Directory.Delete(dir, recursive: true);
    }

    [Fact]
    public void TryParse_RequiresExistingScriptFile()
    {
        bool parsed = HeadlessArgs.TryParse(["--script", "missing-file.txt"], out _, out string? error);
        Assert.True(parsed);
        Assert.Contains("not found", error);
    }

    [Fact]
    public void TryParse_StdinScript_DefaultsTranscriptToStdout()
    {
        bool parsed = HeadlessArgs.TryParse(["--script", "-"], out HeadlessOptions? options, out string? error);
        Assert.True(parsed);
        Assert.Null(error);
        Assert.NotNull(options);
        Assert.Equal("-", options!.TranscriptPath);
        Assert.Equal("headless.save.json", options.SavePath);
    }

    [Fact]
    public void Run_WithStreamOverrides_WritesTranscript()
    {
        using StringReader script = new("look\n");
        using StringWriter transcript = new();

        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);

        int exit = HeadlessRunner.Run(b.Build(), new HeadlessOptions
        {
            ScriptPath = "-",
            TranscriptPath = "-"
        }, script, transcript);

        Assert.Equal(0, exit);
        string log = transcript.ToString();
        Assert.Contains("> look", log);
        Assert.Contains("A hall", log);
    }

    [Fact]
    public void TryParse_WithoutScript_ReturnsFalse()
    {
        bool parsed = HeadlessArgs.TryParse(["--help"], out HeadlessOptions? options, out string? error);
        Assert.True(parsed);
        Assert.Null(options);
        Assert.Contains("Headless mode", error);
    }
}
