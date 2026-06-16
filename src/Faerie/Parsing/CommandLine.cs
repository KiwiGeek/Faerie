using Faerie.Model;
using Faerie.Verbs;

namespace Faerie.Parsing;

/// <summary>
/// Splits one player line into several commands separated by periods or command-boundary commas.
/// Object-list commas (<c>drop key, lamp</c>) are preserved.
/// </summary>
public static class CommandLine
{
    /// <summary>
    /// Splits <paramref name="line"/> into individual commands. Returns a single element when no
    /// command separator is present.
    /// </summary>
    public static IReadOnlyList<string> SplitCommands(string line, VerbLibrary verbs)
    {
        string trimmed = line.Trim();
        if (trimmed.Length == 0) return [];

        string normalized = NormalizePunctuation(trimmed);
        var commands = new List<string>();

        foreach (string segment in normalized.Split('.', StringSplitOptions.RemoveEmptyEntries))
            SplitSegmentOnCommas(segment.Trim(), verbs, commands);

        if (commands.Count == 0)
            commands.Add(trimmed);

        return commands;
    }

    private static string NormalizePunctuation(string line) =>
        line.Replace('!', '.').Replace('?', '.');

    private static void SplitSegmentOnCommas(string segment, VerbLibrary verbs, List<string> commands)
    {
        int start = 0;
        for (int i = 0; i < segment.Length; i++)
        {
            if (segment[i] != ',') continue;

            string after = segment[(i + 1)..].TrimStart();
            if (after.Length == 0 || !StartsNewCommand(after, verbs)) continue;

            string cmd = segment[start..i].Trim();
            if (cmd.Length > 0) commands.Add(cmd);
            start = i + 1;
        }

        string last = segment[start..].Trim();
        if (last.Length > 0) commands.Add(last);
    }

    private static bool StartsNewCommand(string text, VerbLibrary verbs)
    {
        List<string> tokens = Tokenize(text);
        if (tokens.Count == 0) return false;

        if (DirectionExtensions.TryParse(tokens[0], out _)) return true;

        return verbs.TryMatchVerb(tokens, out _, out _);
    }

    private static List<string> Tokenize(string text) =>
        text.Trim().ToLowerInvariant()
            .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .ToList();
}
