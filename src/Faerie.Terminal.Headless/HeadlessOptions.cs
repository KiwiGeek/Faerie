namespace Faerie.Terminal.Headless;

/// <summary>Configuration for a headless script replay session.</summary>
public sealed class HeadlessOptions
{
    /// <summary>
    /// Path to the input script (one command per line), or <c>-</c> to read commands from standard input.
    /// </summary>
    public required string ScriptPath { get; init; }

    /// <summary>
    /// Path to the output transcript file, or <c>-</c> to write to standard output.
    /// </summary>
    public required string TranscriptPath { get; init; }

    /// <summary>Optional fixed random seed for reproducible play.</summary>
    public int? RandomSeed { get; init; }

    /// <summary>Optional directory for named save slots when the player types SAVE/RESTORE.</summary>
    public string? SaveDirectory { get; init; }

    /// <summary>Legacy single-file save path when <see cref="SaveDirectory"/> is not set.</summary>
    public string? SavePath { get; init; }

    /// <summary>True when <see cref="ScriptPath"/> is <c>-</c> (read from stdin).</summary>
    public bool ScriptFromStdin => ScriptPath == "-";

    /// <summary>True when <see cref="TranscriptPath"/> is <c>-</c> (write to stdout).</summary>
    public bool TranscriptToStdout => TranscriptPath == "-";
}
