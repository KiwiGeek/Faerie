namespace Faerie.Parsing;

/// <summary>Finds close vocabulary matches for mistyped player input.</summary>
public static class WordSuggest
{
    /// <summary>Maximum edit distance allowed for a word of the given length.</summary>
    public static int MaxDistance(string word) => word.Length switch
    {
        <= 3 => 1,
        _ => 2
    };

    /// <summary>
    /// Returns the closest vocabulary entries within <see cref="MaxDistance"/> of
    /// <paramref name="input"/>, ordered by distance then word. Excludes exact matches.
    /// </summary>
    public static IReadOnlyList<string> FindCloseMatches(string input, IEnumerable<string> vocabulary, int? maxDistance = null)
    {
        input = input.Trim().ToLowerInvariant();
        if (input.Length == 0) return [];

        int limit = maxDistance ?? MaxDistance(input);
        List<(string word, int dist)> hits = [];

        foreach (string raw in vocabulary)
        {
            string candidate = raw.Trim().ToLowerInvariant();
            if (candidate.Length == 0) continue;

            int dist = Levenshtein.Distance(input, candidate);
            if (dist > 0 && dist <= limit)
                hits.Add((raw, dist));
        }

        if (hits.Count == 0) return [];

        hits.Sort((a, b) =>
        {
            int cmp = a.dist.CompareTo(b.dist);
            return cmp != 0 ? cmp : string.Compare(a.word, b.word, StringComparison.OrdinalIgnoreCase);
        });

        int best = hits[0].dist;
        return hits.TakeWhile(h => h.dist == best).Select(h => h.word).ToList();
    }

    /// <summary>Returns the sole match, or null if there are zero or many.</summary>
    public static string? SingleOrNull(IReadOnlyList<string> matches) =>
        matches.Count == 1 ? matches[0] : null;

    /// <summary>Appends a suggestion clause; returns a pre-fill command when the match is unique.</summary>
    public static (string message, string? suggestedInput) AppendSuggestion(
        string message, string mistyped, IEnumerable<string> vocabulary, string? commandPrefix = null,
        bool uppercaseSuggestion = false, int? maxDistance = null)
    {
        IReadOnlyList<string> matches = FindCloseMatches(mistyped, vocabulary, maxDistance);
        if (matches.Count == 0)
        {
            string[] parts = mistyped.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                matches = FindCloseMatches(parts[^1], vocabulary, maxDistance);
        }

        string? tail = DidYouMean(matches, uppercaseSuggestion);
        if (tail is null) return (message, null);

        string? suggestedInput = null;
        if (matches.Count == 1)
        {
            string fix = matches[0];
            suggestedInput = commandPrefix is null ? fix : $"{commandPrefix} {fix}";
        }

        return ($"{message} {tail}", suggestedInput);
    }

    /// <summary>Formats zero or more close matches as a trailing "Did you mean …?" clause.</summary>
    public static string? DidYouMean(IReadOnlyList<string> matches, bool uppercase = false)
    {
        if (matches.Count == 0) return null;

        string Format(string s) => uppercase ? s.ToUpperInvariant() : s;

        string suggestion = matches.Count switch
        {
            1 => Format(matches[0]),
            2 => $"{Format(matches[0])} or {Format(matches[1])}",
            _ => string.Join(", ", matches.Take(matches.Count - 1).Select(Format)) +
                 $", or {Format(matches[^1])}"
        };

        return $"Did you mean {suggestion}?";
    }

    /// <summary>Appends a suggestion clause when close matches exist.</summary>
    public static string WithSuggestion(string message, IEnumerable<string> vocabulary, string mistyped,
        bool uppercaseSuggestion = false, int? maxDistance = null) =>
        AppendSuggestion(message, mistyped, vocabulary, uppercaseSuggestion: uppercaseSuggestion, maxDistance: maxDistance).message;
}
