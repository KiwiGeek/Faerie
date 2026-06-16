using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class SaveSlotTests
{
    [Fact]
    public void SuffixDash_DefaultAndNamedSlots()
    {
        var catalog = new SaveSlotCatalog("ignored", "zork", ".json");
        Assert.Equal("zork.json", catalog.FileNameForLabel(null));
        Assert.Equal("zork-abc.json", catalog.FileNameForLabel("abc"));
        Assert.Equal("ABC", catalog.NormalizeLabel("abcdef"));
    }

    [Fact]
    public void SierraPrefix_MatchesSoftpornNaming()
    {
        var catalog = new SaveSlotCatalog("ignored", "SOFTPORN", ".SAV", SaveSlotNaming.SierraPrefix);
        Assert.Equal("SOFTPORN.SAV", catalog.FileNameForLabel(null));
        Assert.Equal("SOFTPABC.SAV", catalog.FileNameForLabel("abc"));
    }

    [Fact]
    public void SaveRestore_WithNamedSlot_RoundTrips()
    {
        string dir = Path.Combine(Path.GetTempPath(), "faerie-save-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
            Room hall = b.Room("Hall").Describe("A plain hall.");
            Thing key = b.Item("brass key").Describe("A brass key.");
            key.StartsIn(hall);
            b.StartIn(hall);

            InMemoryTerminal term = new();
            GameEngine engine = new(b.Build(), term, randomSeed: 1);
            engine.SaveCatalog = new SaveSlotCatalog(dir, "test");
            engine.Start();
            term.Reset();

            engine.Submit("take brass key");
            engine.Submit("save a");
            Assert.Contains("test-a.json saved", term.Output);

            engine.State.MoveTo(key, hall);
            Assert.False(engine.State.IsCarried(key));

            term.Reset();
            engine.Submit("restore a");
            Assert.True(engine.State.IsCarried(key));
            Assert.Contains("Restoring from test-a.json", term.Output);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}
