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
    public Thing? IndirectObject { get; init; }
    public string? Preposition { get; init; }

    public string? DirectObjectText { get; init; }
    public string? IndirectObjectText { get; init; }

    public IReadOnlyList<Thing> Ambiguities { get; init; } = [];

    public static ParsedCommand Empty() => new() { Status = ParseStatus.Empty };
    public static ParsedCommand Unknown(string message) => new() { Status = ParseStatus.UnknownVerb, Message = message };
    public static ParsedCommand NoObject(string message) => new() { Status = ParseStatus.UnknownObject, Message = message };
    public static ParsedCommand Ambiguous(string message, IReadOnlyList<Thing> candidates) =>
        new() { Status = ParseStatus.Ambiguous, Message = message, Ambiguities = candidates };
}
