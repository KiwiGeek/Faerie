using Faerie.Model;
using Faerie.Presentation;
using Faerie.Runtime;

namespace Faerie.Verbs;

/// <summary>
/// A <see cref="GameContext"/> enriched with everything the parser resolved for the current
/// command: the verb, the direct and indirect objects (already resolved to <see cref="Thing"/>
/// instances), any preposition, and a direction for movement verbs.
/// </summary>
public sealed class VerbContext : GameContext
{
    public VerbContext(GameEngine engine, GameState state, OutputWriter output) : base(engine, state, output) { }

    public required Verb Verb { get; init; }

    /// <summary>The original text the player typed.</summary>
    public required string RawInput { get; init; }

    /// <summary>The resolved direct object, if the command had one.</summary>
    public Thing? DirectObject { get; set; }

    /// <summary>The resolved indirect object (after a preposition), if any.</summary>
    public Thing? IndirectObject { get; set; }

    /// <summary>The preposition that joined the two objects ("to", "with", "in", ...), if any.</summary>
    public string? Preposition { get; set; }

    /// <summary>The direction, for movement verbs.</summary>
    public Direction? Direction { get; set; }

    /// <summary>The raw noun text for the direct object (useful for "I don't see any X" messages).</summary>
    public string? DirectObjectText { get; set; }

    /// <summary>The raw noun text for the indirect object.</summary>
    public string? IndirectObjectText { get; set; }

    public bool HasDirectObject => DirectObject is not null;
    public bool HasIndirectObject => IndirectObject is not null;
}
