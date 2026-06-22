using Faerie.Samples.Zork;
using Faerie.Terminal.Headless;
using Xunit;

namespace Faerie.Tests;

public sealed class ZorkWalkthroughTests
{
    private static string WinpathScript =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "zork1-winpath.txt"));

    [Fact]
    public void Zork_Winpath_Seed1_ReachesMaxScoreAndWins()
    {
        Assert.True(File.Exists(WinpathScript), $"Missing script: {WinpathScript}");

        string transcript = Path.Combine(Path.GetTempPath(), "zork-winpath-" + Guid.NewGuid().ToString("N") + ".txt");
        int exit = HeadlessRunner.Run(ZorkGame.Build(), new HeadlessOptions
        {
            ScriptPath = WinpathScript,
            TranscriptPath = transcript,
            RandomSeed = 1
        });

        string output = File.ReadAllText(transcript);
        Assert.Equal(0, exit);
        Assert.Contains("You have mastered Zork!", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("350/350", output, StringComparison.OrdinalIgnoreCase);
    }
}
