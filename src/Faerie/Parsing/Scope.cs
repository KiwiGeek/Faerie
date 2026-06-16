using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Parsing;

/// <summary>
/// Computes what the player can currently see and reach, and resolves typed noun phrases against
/// it. Lighting is respected: in a dark room with no light source the player can only sense what
/// they are carrying.
/// </summary>
public sealed class Scope(GameState state)
{
    private readonly GameState _state = state;

    /// <summary>True if the current room is lit (not dark, or a light source is present).</summary>
    public bool IsCurrentRoomLit => IsLit(_state.CurrentRoom);

    public bool IsLit(Room room)
    {
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

        foreach (Thing t in EnumerateRoom(_state.CurrentRoom))
            yield return t;
    }

    private IEnumerable<Thing> EnumerateRoom(Room room)
    {
        foreach (Thing t in _state.ContentsOf(room))
        {
            if (t.Has(Attr.Concealed)) continue;
            yield return t;
            foreach (Thing inner in EnumerateInside(t))
                yield return inner;
        }
    }

    private IEnumerable<Thing> EnumerateInside(Thing container)
    {
        // Contents of open containers and everything on supporters are visible.
        if (container.Has(Attr.Container) && container.Has(Attr.Open))
            foreach (Thing t in _state.ContentsOf(container))
            {
                if (t.Has(Attr.Concealed)) continue;
                yield return t;
                foreach (Thing inner in EnumerateInside(t)) yield return inner;
            }

        if (container.Has(Attr.Supporter))
            foreach (Thing t in _state.ContentsOf(container, onTop: true))
            {
                if (t.Has(Attr.Concealed)) continue;
                yield return t;
                foreach (Thing inner in EnumerateInside(t)) yield return inner;
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
