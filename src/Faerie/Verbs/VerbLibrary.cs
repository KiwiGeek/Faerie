namespace Faerie.Verbs;

/// <summary>
/// The set of verbs a game understands. Supports multi-word verb phrases ("pick up", "look at")
/// by matching the longest phrase against the leading tokens of a command.
/// </summary>
public sealed class VerbLibrary
{
    private readonly List<Verb> _verbs = [];

    /// <summary>Prepositions recognised by default when a verb does not specify its own.</summary>
    public static readonly string[] DefaultPrepositions =
        ["to", "with", "in", "into", "inside", "on", "onto", "at", "from", "using", "under", "behind", "about"];

    public IReadOnlyList<Verb> All => _verbs;

    public void Add(Verb verb) => _verbs.Add(verb);

    public bool Contains(string word) =>
        _verbs.Any(v => v.Words.Contains(word.ToLowerInvariant()));

    /// <summary>All verbs that respond to a given (already lower-cased) word/phrase.</summary>
    public IEnumerable<Verb> VerbsFor(string phrase) => _verbs.Where(v => v.Words.Contains(phrase));

    public Verb? FindById(string id) => _verbs.FirstOrDefault(v => v.Id == id);

    /// <summary>
    /// Tries to match a verb at the start of the token list, preferring the longest phrase
    /// (so "pick up key" matches the verb "pick up", not "pick").
    /// </summary>
    public bool TryMatchVerb(IReadOnlyList<string> tokens, out Verb verb, out int wordsConsumed)
    {
        verb = null!;
        wordsConsumed = 0;

        // Try phrases of decreasing length (up to 3 words).
        for (int len = Math.Min(3, tokens.Count); len >= 1; len--)
        {
            string phrase = string.Join(' ', tokens.Take(len));
            foreach (Verb candidate in _verbs)
            {
                if (candidate.Words.Contains(phrase))
                {
                    verb = candidate;
                    wordsConsumed = len;
                    return true;
                }
            }
        }

        return false;
    }
}
