using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class CommandLineTests
{
    private static VerbLibrary Verbs()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);
        return b.Build().Verbs;
    }

    [Theory]
    [InlineData("look", new[] { "look" })]
    [InlineData("look.", new[] { "look" })]
    [InlineData("look!", new[] { "look" })]
    [InlineData("north. take lamp", new[] { "north", "take lamp" })]
    [InlineData("north, take lamp", new[] { "north", "take lamp" })]
    [InlineData("go north, take lamp", new[] { "go north", "take lamp" })]
    [InlineData("drop key, lamp", new[] { "drop key, lamp" })]
    [InlineData("take key, lamp", new[] { "take key, lamp" })]
    [InlineData("save, look", new[] { "save", "look" })]
    public void SplitCommands(string line, string[] expected)
    {
        IReadOnlyList<string> parts = CommandLine.SplitCommands(line, Verbs());
        Assert.Equal(expected, parts);
    }
}
