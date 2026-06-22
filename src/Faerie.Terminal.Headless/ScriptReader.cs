namespace Faerie.Terminal.Headless;

/// <summary>Reads player commands from a script file, skipping blanks and comment lines.</summary>
public static class ScriptReader
{
    /// <summary>
    /// Yields each command line from <paramref name="reader"/>. Blank lines and lines whose first
    /// non-whitespace character is <c>#</c> or <c>;</c> are ignored.
    /// </summary>
    public static IEnumerable<string> ReadCommands(TextReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0) continue;
            if (trimmed[0] is '#' or ';') continue;
            yield return trimmed;
        }
    }

    /// <summary>Reads commands from a file path.</summary>
    public static IEnumerable<string> ReadCommands(string path)
    {
        using StreamReader reader = File.OpenText(path);
        foreach (string command in ReadCommands(reader))
            yield return command;
    }

    /// <summary>
    /// Reads commands from <paramref name="path"/> through the line
    /// <c># CHECKPOINT: {checkpoint}</c> (exclusive). Earlier checkpoints in the same file are skipped.
    /// </summary>
    public static IEnumerable<string> ReadCommandsUntilCheckpoint(string path, string checkpoint)
    {
        using StreamReader reader = File.OpenText(path);
        foreach (string command in ReadCommandsUntilCheckpoint(reader, checkpoint))
            yield return command;
    }

    /// <summary>
    /// Like <see cref="ReadCommandsUntilCheckpoint(string, string)"/> but reads from an open
    /// <paramref name="reader"/>.
    /// </summary>
    public static IEnumerable<string> ReadCommandsUntilCheckpoint(TextReader reader, string checkpoint)
    {
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            if (trimmed.StartsWith("# CHECKPOINT:", StringComparison.OrdinalIgnoreCase))
            {
                string name = trimmed["# CHECKPOINT:".Length..].Trim();
                if (name.Equals(checkpoint, StringComparison.OrdinalIgnoreCase))
                    yield break;
                continue;
            }

            if (trimmed[0] is '#' or ';') continue;
            yield return trimmed;
        }
    }
}
