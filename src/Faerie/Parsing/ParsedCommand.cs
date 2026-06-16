using Faerie.Model;
using Faerie.Verbs;

namespace Faerie.Parsing;

public enum ParseStatus
{
    /// <summary>Successfully parsed into a verb (and possibly objects).</summary>
    Ok,

    /// <summary>The player typed nothing.</summary>
    Empty,

    /// <summary>The first word was not a known verb or direction.</summary>
    UnknownVerb,

    /// <summary>A referenced object could not be found in scope.</summary>
    UnknownObject,

    /// <summary>A noun phrase matched more than one visible thing.</summary>
    Ambiguous
}

/// <summary>The structured result of parsing a line of player input.</summary>
public sealed class ParsedCommand
{
    public ParseStatus Status { get; init; }
    public string? Message { get; init; }

    public Verb? Verb { get; init; }
    public Direction? Direction { get; init; }

    public Thing? DirectObject { get; init; }
    /// <summary>When the player names several direct objects ("drop sword and lamp"), every resolved target.</summary>
    public IReadOnlyList<Thing> DirectObjects { get; init; } = [];
    /// <summary>True when the player used a bulk quantifier such as "all" or "everything".</summary>
    public bool IsAll { get; init; }
    public Thing? IndirectObject { get; init; }
    public string? Preposition { get; init; }

    public string? DirectObjectText { get; init; }
    public string? IndirectObjectText { get; init; }

    /// <summary>
    /// When exactly one corrected command can be inferred, the UI may pre-fill the input line so
    /// the player can accept it with Enter.
    /// </summary>
    public string? SuggestedInput { get; init; }

    public IReadOnlyList<Thing> Ambiguities { get; init; } = [];

    public static ParsedCommand Empty() => new() { Status = ParseStatus.Empty };
    public static ParsedCommand Unknown(string message, string? suggestedInput = null) =>
        new() { Status = ParseStatus.UnknownVerb, Message = message, SuggestedInput = suggestedInput };
    public static ParsedCommand NoObject(string message, string? suggestedInput = null) =>
        new() { Status = ParseStatus.UnknownObject, Message = message, SuggestedInput = suggestedInput };
    public static ParsedCommand Ambiguous(string message, IReadOnlyList<Thing> candidates) =>
        new() { Status = ParseStatus.Ambiguous, Message = message, Ambiguities = candidates };
}
