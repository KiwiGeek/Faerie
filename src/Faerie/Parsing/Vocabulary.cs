using Faerie.Model;
using Faerie.Verbs;

namespace Faerie.Parsing;

/// <summary>Collects the words the parser can suggest when the player mistypes.</summary>
public static class Vocabulary
{
    public static IEnumerable<string> VerbWords(VerbLibrary verbs)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (Verb verb in verbs.All)
        {
            foreach (string phrase in verb.Words)
            {
                if (seen.Add(phrase)) yield return phrase;
                foreach (string part in phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    if (seen.Add(part)) yield return part;
            }
        }
    }

    /// <summary>First word of each verb phrase — used when matching a lone mistyped verb token.</summary>
    public static IEnumerable<string> VerbLeadingWords(VerbLibrary verbs)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (Verb verb in verbs.All)
        {
            foreach (string phrase in verb.Words)
            {
                string first = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                if (seen.Add(first)) yield return first;
            }
        }
    }

    public static IEnumerable<string> DirectionWords()
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (Direction dir in Enum.GetValues<Direction>())
        {
            foreach (string word in dir.Words())
                if (seen.Add(word)) yield return word;
        }
    }

    public static IEnumerable<string> VisibleNouns(Scope scope)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (Thing thing in scope.VisibleThings().Distinct())
        {
            foreach (string noun in thing.Nouns)
                if (seen.Add(noun)) yield return noun;
            foreach (string adj in thing.Adjectives)
                if (seen.Add(adj)) yield return adj;
        }
    }
}
