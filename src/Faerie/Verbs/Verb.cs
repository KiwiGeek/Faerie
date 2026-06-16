using Faerie.Runtime;

namespace Faerie.Verbs;

/// <summary>The grammatical forms a verb is willing to accept. A verb may accept several at once.</summary>
[Flags]
public enum VerbForms
{
    None = 0,

    /// <summary>"look", "wait", "inventory" — no object.</summary>
    Intransitive = 1 << 0,

    /// <summary>"take key", "give condom" — one object.</summary>
    Transitive = 1 << 1,

    /// <summary>"give condom to girl", "put key in lock" — two objects joined by a preposition.</summary>
    Ditransitive = 1 << 2
}

/// <summary>
/// A command the player can type. A verb owns its synonyms, the grammatical forms it accepts, the
/// prepositions that introduce an indirect object, and the handler that carries out the action.
/// Verbs are referenced by instance (so things can react to a specific verb without magic strings).
/// </summary>
public sealed class Verb
{
    public Verb(string id, IEnumerable<string> words, VerbForms forms, Func<VerbContext, VerbResult> handler)
    {
        Id = id;
        Words = [.. words.Select(w => w.ToLowerInvariant())];
        Forms = forms;
        Handler = handler;
    }

    public string Id { get; }

    /// <summary>The words/phrases that invoke this verb (e.g. "take", "get", "pick up").</summary>
    public List<string> Words { get; }

    /// <summary>The grammatical forms accepted.</summary>
    public VerbForms Forms { get; set; }

    /// <summary>
    /// Prepositions that separate a direct object from an indirect object for this verb
    /// (e.g. "to", "with", "in", "on"). Empty means the verb's default set is used.
    /// </summary>
    public List<string> Prepositions { get; } = [];

    /// <summary>When true, the verb cannot be performed in a dark room.</summary>
    public bool RequiresLight { get; set; }

    /// <summary>The action carried out when the command is understood.</summary>
    public Func<VerbContext, VerbResult> Handler { get; set; }

    public bool Accepts(VerbForms form) => (Forms & form) == form;

    public override string ToString() => $"Verb({Id})";
}
