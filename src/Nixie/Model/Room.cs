using Nixie.Runtime;

namespace Nixie.Model;

/// <summary>
/// A location in the world. Rooms hold exits to other rooms and (via the game state) the things
/// currently present. Rooms can be dark, requiring the player to bring a light source.
/// </summary>
public sealed class Room : Element
{
    public Room(string id, string name) : base(id, name)
    {
        Article = "the";
    }

    private readonly Dictionary<Direction, Exit> _exits = [];

    /// <summary>The exits leading out of this room, keyed by direction.</summary>
    public IReadOnlyDictionary<Direction, Exit> Exits => _exits;

    /// <summary>
    /// An optional shorter description shown on re-entry to an already-visited room. The full
    /// <see cref="Element.Description"/> is used on the first visit and whenever the player LOOKs.
    /// When null, the full description is always shown.
    /// </summary>
    public string? BriefDescription { get; set; }

    /// <summary>Dynamic brief description; overrides <see cref="BriefDescription"/> when set.</summary>
    public Func<GameContext, string>? BriefDescriptionFactory { get; set; }

    /// <summary>Resolves the brief description, falling back to the full one.</summary>
    public string ResolveBrief(GameContext context) =>
        BriefDescriptionFactory?.Invoke(context) ?? BriefDescription ?? ResolveDescription(context);

    /// <summary>
    /// When true the room is unlit; the player sees nothing useful unless carrying (or the room
    /// contains) an active light source.
    /// </summary>
    public bool IsDark { get; set; }

    /// <summary>Optional hook fired the first time the player enters this room.</summary>
    public Action<GameContext>? OnFirstEnter { get; set; }

    /// <summary>Optional hook fired every time the player enters this room.</summary>
    public Action<GameContext>? OnEnter { get; set; }

    /// <summary>Optional hook fired at the start of every turn while the player is in this room.</summary>
    public Action<GameContext>? OnTurn { get; set; }

    /// <summary>Adds or replaces an exit. Returns the created exit so callers can refine it.</summary>
    public Exit SetExit(Direction direction, Room destination)
    {
        Exit exit = new(direction, destination);
        _exits[direction] = exit;
        return exit;
    }

    internal void SetExit(Exit exit) => _exits[exit.Direction] = exit;

    public Exit? ExitTo(Direction direction) => _exits.GetValueOrDefault(direction);

    public override string ToString() => $"Room({Id})";
}
