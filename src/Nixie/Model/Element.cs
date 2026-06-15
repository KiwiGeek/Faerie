using Nixie.Runtime;

namespace Nixie.Model;

/// <summary>
/// Base type for everything in the world that has identity, a name and a description:
/// rooms, things, creatures and the player. Authors hold C# references to these objects
/// and wire the world together by reference — there are no magic strings.
/// </summary>
public abstract class Element
{
    private protected Element(string id, string name)
    {
        Id = id;
        Name = name;
        Nouns = [name.ToLowerInvariant()];
    }

    /// <summary>Stable identifier, used for save files and debugging.</summary>
    public string Id { get; }

    /// <summary>The display name shown to the player (e.g. "brass key").</summary>
    public string Name { get; set; }

    /// <summary>The article used in generated prose ("a", "an", "the", or "" for proper nouns).</summary>
    public string Article { get; set; } = "a";

    /// <summary>Words a player may type to refer to this element.</summary>
    public List<string> Nouns { get; }

    /// <summary>Optional adjectives that help disambiguate ("brass", "rusty").</summary>
    public List<string> Adjectives { get; } = [];

    /// <summary>The boolean attributes currently set on this element.</summary>
    public Attr Attributes { get; set; } = Attr.None;

    /// <summary>Static description, used when <see cref="DescriptionFactory"/> is null.</summary>
    public string? Description { get; set; }

    /// <summary>Dynamic description hook; when set it overrides <see cref="Description"/>.</summary>
    public Func<GameContext, string>? DescriptionFactory { get; set; }

    // ---- typed, magic-string-free per-element state --------------------------------------
    private readonly Dictionary<object, object?> _state = [];

    public bool Has(Attr attribute) => (Attributes & attribute) == attribute;

    public void Set(Attr attribute, bool value = true)
    {
        if (value) Attributes |= attribute;
        else Attributes &= ~attribute;
    }

    /// <summary>Reads typed custom state attached to this element, falling back to the key default.</summary>
    public T Get<T>(StateKey<T> key) =>
        _state.TryGetValue(key, out object? v) && v is T t ? t : key.Default;

    /// <summary>Writes typed custom state attached to this element.</summary>
    public void Set<T>(StateKey<T> key, T value) => _state[key] = value;

    internal IReadOnlyDictionary<object, object?> RawState => _state;

    /// <summary>Resolves the description for the current turn.</summary>
    public string ResolveDescription(GameContext context) =>
        DescriptionFactory?.Invoke(context) ?? Description ?? $"You see nothing special about the {Name}.";

    public override string ToString() => $"{GetType().Name}({Id})";
}

/// <summary>
/// A handle to a piece of typed game state. Created through the builder so that puzzle flags,
/// counters and arbitrary values are referenced by instance rather than by string.
/// </summary>
public sealed class StateKey<T>(string name, T @default)
{
    /// <summary>Identifier used when the value is persisted to a save file.</summary>
    public string Name { get; } = name;

    /// <summary>Value returned before anything has been written.</summary>
    public T Default { get; } = @default;

    public override string ToString() => $"StateKey<{typeof(T).Name}>({Name})";
}
