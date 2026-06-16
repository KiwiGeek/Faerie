namespace Faerie.Terminal.Headless;

/// <summary>Configuration for a headless script replay session.</summary>
public sealed class HeadlessOptions
{
    /// <summary>Path to the input script (one command per line).</summary>
    public required string ScriptPath { get; init; }

    /// <summary>Path to the output transcript file.</summary>
    public required string TranscriptPath { get; init; }

    /// <summary>Optional fixed random seed for reproducible play.</summary>
    public int? RandomSeed { get; init; }

    /// <summary>Optional path for save-game JSON when the player types SAVE.</summary>
    public string? SavePath { get; init; }
}
