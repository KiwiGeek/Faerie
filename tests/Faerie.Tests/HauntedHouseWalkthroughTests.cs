using Faerie.Samples.HauntedHouse;
using Faerie.Terminal.Headless;
using Xunit;

namespace Faerie.Tests;

/// <summary>
/// Plays the published Usborne <em>Haunted House</em> walkthrough (Dorothy Millard, C64 version;
/// solutionarchive.com file id 9320) verbatim through this port, as a fidelity oracle. The script
/// under <c>scripts/haunted-house-walkthrough.txt</c> uses the original wording (including object
/// names/verbs from the original game) rather than this sample's current vocabulary, so any
/// divergence between the two shows up as a failing assertion instead of being silently patched
/// away by rewording the walkthrough.
/// </summary>
public sealed class HauntedHouseWalkthroughTests
{
    private static string WalkthroughScript =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "haunted-house-walkthrough.txt"));

    [Fact]
    public void Walkthrough_ReachesDoubleScoreAndWins()
    {
        Assert.True(File.Exists(WalkthroughScript), $"Missing script: {WalkthroughScript}");

        string transcript = Path.Combine(Path.GetTempPath(), "haunted-house-walkthrough-" + Guid.NewGuid().ToString("N") + ".txt");
        int exit = HeadlessRunner.Run(HauntedHouseGame.Build(), new HeadlessOptions
        {
            ScriptPath = WalkthroughScript,
            TranscriptPath = transcript,
            RandomSeed = 1
        });

        string output = File.ReadAllText(transcript);
        Assert.Equal(0, exit);
        Assert.Contains("DOUBLE SCORE FOR REACHING HERE", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("32", output);
        Assert.Contains("Well done", output, StringComparison.OrdinalIgnoreCase);
    }
}
