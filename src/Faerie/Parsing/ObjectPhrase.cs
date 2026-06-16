namespace Faerie.Parsing;

/// <summary>Splits object phrases on conjunctions and recognises bulk quantifiers.</summary>
internal static class ObjectPhrase
{
    private static readonly HashSet<string> BulkQuantifiers = ["all", "everything", "each"];

    /// <summary>Splits a token list into one or more noun phrases separated by "and".</summary>
    public static List<List<string>> SplitConjuncts(IReadOnlyList<string> tokens)
    {
        List<List<string>> phrases = [];
        List<string> current = [];

        foreach (string token in tokens)
        {
            if (token == "and")
            {
                if (current.Count > 0)
                {
                    phrases.Add(current);
                    current = [];
                }
            }
            else
                current.Add(token);
        }

        if (current.Count > 0)
            phrases.Add(current);

        return phrases;
    }

    public static bool IsBulkQuantifier(IReadOnlyList<string> tokens) =>
        tokens.Count == 1 && BulkQuantifiers.Contains(tokens[0]);
}
