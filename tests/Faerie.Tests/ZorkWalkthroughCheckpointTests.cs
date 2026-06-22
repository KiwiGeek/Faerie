using Faerie.Samples.Zork;
using Faerie.Terminal.Headless;
using Xunit;

namespace Faerie.Tests;

/// <summary>
/// Partial winpath replays for faster CI localization. Checkpoints are marked in
/// <c>scripts/zork1-winpath.txt</c> with <c># CHECKPOINT: name</c>.
/// </summary>
public sealed class ZorkWalkthroughCheckpointTests
{
    private static string WinpathScript =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "zork1-winpath.txt"));

    private static string RunToCheckpoint(string checkpoint)
    {
        Assert.True(File.Exists(WinpathScript), $"Missing script: {WinpathScript}");

        string transcript = Path.Combine(Path.GetTempPath(), "zork-checkpoint-" + Guid.NewGuid().ToString("N") + ".txt");
        var commands = ScriptReader.ReadCommandsUntilCheckpoint(WinpathScript, checkpoint).ToList();
        Assert.NotEmpty(commands);

        using var script = new StringReader(string.Join(Environment.NewLine, commands));
        int exit = HeadlessRunner.Run(ZorkGame.Build(), new HeadlessOptions
        {
            ScriptPath = WinpathScript,
            TranscriptPath = transcript,
            RandomSeed = 1
        }, scriptOverride: script, transcriptOverride: null);

        Assert.Equal(0, exit);
        return File.ReadAllText(transcript);
    }

    [Fact]
    public void Zork_Winpath_Checkpoint_Egg_PlacesFirstTrophy()
    {
        string output = RunToCheckpoint("egg");
        Assert.Contains("gained 5 points", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Winpath_Checkpoint_DamDrain_EmptiesReservoir()
    {
        string output = RunToCheckpoint("dam-drain");
        Assert.Contains("sluice gates open", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("water pours through the dam", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Winpath_Checkpoint_Pot_PlacesRainbowTreasure()
    {
        string output = RunToCheckpoint("pot");
        Assert.Contains("gained 10 points", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zork_Winpath_Checkpoint_Barrow_WinsGame()
    {
        string output = RunToCheckpoint("barrow");
        Assert.Contains("You have mastered Zork!", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("350/350", output, StringComparison.OrdinalIgnoreCase);
    }
}
