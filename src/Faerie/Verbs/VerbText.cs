using Faerie.Model;

namespace Faerie.Verbs;

/// <summary>Small helpers for generating grammatical references to things in messages.</summary>
public static class VerbText
{
    /// <summary>"the brass key" (or just the name for proper nouns).</summary>
    public static string The(Thing thing) =>
        string.IsNullOrEmpty(thing.Article) ? thing.Name : $"the {thing.Name}";

    /// <summary>"a brass key" / "an apple" / proper name.</summary>
    public static string A(Thing thing) =>
        string.IsNullOrEmpty(thing.Article) ? thing.Name : $"{thing.Article} {thing.Name}";

    /// <summary>Capitalises the first character.</summary>
    public static string Cap(string text) =>
        string.IsNullOrEmpty(text) ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>"is" / "are" depending on plurality.</summary>
    public static string Is(Thing thing) => thing.Has(Attr.Plural) ? "are" : "is";

    /// <summary>"it" / "them" depending on plurality.</summary>
    public static string It(Thing thing) => thing.Has(Attr.Plural) ? "them" : "it";
}
