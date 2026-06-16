using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Parsing;

/// <summary>
/// Computes what the player can currently see and reach, and resolves typed noun phrases against
/// it. Lighting is respected: in a dark room with no light source the player can only sense what
/// they are carrying.
/// </summary>
public sealed class Scope(GameState state, GameContext? context = null)
{
    private static readonly HashSet<string> FillerWords = ["of", "from", "with", "in", "on", "at", "the"];

    private readonly GameState _state = state;
    private readonly GameContext? _context = context;

    /// <summary>True if the current room is lit (ambient, not dark, or a light source is present).</summary>
    public bool IsCurrentRoomLit => IsLit(_state.CurrentRoom);

    public bool IsLit(Room room)
    {
        if (_context is not null && room.IsLitFactory?.Invoke(_context) == true)
            return true;

        if (!room.IsDark) return true;

        // Any active light source carried or present in the room lights it.
        foreach (Thing t in _state.Inventory.Concat(_state.Worn))
            if (t.Has(Attr.LightSource) && t.Has(Attr.Lit)) return true;

        foreach (Thing t in _state.ContentsOf(room))
            if (t.Has(Attr.LightSource) && t.Has(Attr.Lit)) return true;

        return false;
    }

    /// <summary>Everything the player can currently refer to.</summary>
    public IEnumerable<Thing> VisibleThings()
    {
        // Always able to refer to carried / worn items.
        foreach (Thing t in _state.Inventory) yield return t;
        foreach (Thing t in _state.Worn) yield return t;

        if (!IsCurrentRoomLit) yield break;

        Room room = _state.CurrentRoom;
        foreach (Thing thing in _state.World.Things)
        {
            if (thing.Has(Attr.Concealed)) continue;
            if (_state.RoomOf(thing) != room) continue;
            if (IsInsideClosedOpenable(thing)) continue;
            yield return thing;
        }
    }

    /// <summary>True when the thing sits inside a closed, openable container (matches Search verb rules).</summary>
    private bool IsInsideClosedOpenable(Thing thing)
    {
        for (Placement p = _state.PlacementOf(thing);;)
        {
            switch (p.Anchor)
            {
                case Anchor.Inside when p.Container is { } container:
                    if (container.Has(Attr.Concealed)) return true;
                    if (container.Has(Attr.Container) && container.Has(Attr.Openable) && !container.Has(Attr.Open))
                        return true;
                    p = _state.PlacementOf(container);
                    break;
                case Anchor.On when p.Container is { } supporter:
                    if (supporter.Has(Attr.Concealed)) return true;
                    p = _state.PlacementOf(supporter);
                    break;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Resolves a noun phrase (already tokenised, articles stripped) to a single visible thing.
    /// Returns the match status plus any candidates for disambiguation.
    /// </summary>
    public NounResolution Resolve(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0) return NounResolution.None;

        // Pronoun support.
        if (tokens is ["it"] or ["them"] && _state.LastReferencedThing is { } last && _state.IsPresent(last))
            return NounResolution.Single(last);

        List<Thing> matches = [];
        foreach (Thing thing in VisibleThings().Distinct())
            if (Matches(thing, tokens))
                matches.Add(thing);

        return matches.Count switch
        {
            0 => NounResolution.NotFound,
            1 => NounResolution.Single(matches[0]),
            _ => NounResolution.Ambiguous(matches)
        };
    }

    private static bool Matches(Thing thing, IReadOnlyList<string> tokens)
    {
        HashSet<string> nouns = [.. thing.Nouns];
        HashSet<string> vocab = [.. thing.Nouns, .. thing.Adjectives];

        // The whole phrase joined (handles multi-word nouns like "front door").
        string joined = string.Join(' ', tokens);
        if (nouns.Contains(joined)) return true;

        bool headNoun = false;
        foreach (string token in tokens)
        {
            if (FillerWords.Contains(token)) continue;
            if (!vocab.Contains(token)) return false;
            if (nouns.Contains(token)) headNoun = true;
        }

        return headNoun;
    }
}

/// <summary>The result of resolving a noun phrase.</summary>
public readonly struct NounResolution
{
    public enum Kind { Empty, NotFound, Single, Ambiguous }

    public Kind Status { get; private init; }
    public Thing? Thing { get; private init; }
    public IReadOnlyList<Thing> Candidates { get; private init; }

    public static readonly NounResolution None = new() { Status = Kind.Empty, Candidates = [] };
    public static readonly NounResolution NotFound = new() { Status = Kind.NotFound, Candidates = [] };
    public static NounResolution Single(Thing thing) => new() { Status = Kind.Single, Thing = thing, Candidates = [thing] };
    public static NounResolution Ambiguous(IReadOnlyList<Thing> things) => new() { Status = Kind.Ambiguous, Candidates = things };
}
