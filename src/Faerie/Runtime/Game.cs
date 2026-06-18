using Faerie.Model;
using Faerie.Presentation;
using Faerie.Verbs;

namespace Faerie.Runtime;

/// <summary>
/// A fully assembled, runnable game: its world, vocabulary of verbs, reaction hooks, daemons,
/// presentation callbacks and metadata. Produced by the builder and consumed by the engine.
/// </summary>
public sealed class Game
{
    public required string Title { get; init; }
    public string? Author { get; init; }

    /// <summary>
    /// The native OS window title. If null, the host falls back to <see cref="Title"/>. This is a
    /// plain string so the engine stays UI-agnostic; the host applies it to its window.
    /// </summary>
    public string? WindowTitle { get; init; }

    /// <summary>
    /// A URI/path to the native window icon (e.g. "avares://MyGame/Assets/icon.ico" or a file path).
    /// The host loads and applies it; null means use the platform default.
    /// </summary>
    public string? WindowIconUri { get; init; }

    /// <summary>
    /// The terminal font, as a spec the host resolves. May be a built-in retro font (see
    /// <c>Faerie.Terminal.Avalonia.BuiltInTerminalFont</c>), a system family name ("Consolas"),
    /// an Avalonia resource font with an explicit family ("avares://MyGame/Assets/Fonts#Apple ][")
    /// , or a URI/folder pointing at an embedded font file. Null means the host's default monospace.
    /// </summary>
    public string? FontSpec { get; init; }

    /// <summary>The caret shape. Defaults to a solid block.</summary>
    public TerminalCursor Cursor { get; init; } = TerminalCursor.Block;

    /// <summary>Marked-up text shown once when the game begins.</summary>
    public string? Intro { get; init; }

    public required World World { get; init; }
    public required VerbLibrary Verbs { get; init; }
    public required ReactionTable Reactions { get; init; }

    public IReadOnlyList<TurnDaemon> Daemons { get; init; } = [];

    /// <summary>One-shot timers scheduled relative to the turn count.</summary>
    public IReadOnlyList<ScheduledTimer> Timers { get; init; } = [];

    /// <summary>
    /// Output filters applied, in order, to every line of game text before it reaches the terminal.
    /// Each receives the context and the marked-up text and returns a rewritten string, or null to
    /// suppress it. Installed via <see cref="Building.GameBuilder.FilterOutput"/>. See
    /// <see cref="Presentation.OutputWriter.Transform"/>.
    /// </summary>
    public IReadOnlyList<Func<GameContext, string, string?>> OutputFilters { get; init; } = [];

    /// <summary>Maximum achievable score (0 if the game does not use scoring).</summary>
    public int MaxScore { get; init; }

    /// <summary>Runs once after the intro and the first room description.</summary>
    public Action<GameContext>? OnStart { get; init; }

    /// <summary>Builds the title bar content for the current turn. Null disables the title bar.</summary>
    public Func<GameContext, BarContent>? TitleBar { get; init; }

    /// <summary>Builds the status bar content for the current turn. Null disables the status bar.</summary>
    public Func<GameContext, BarContent>? StatusBar { get; init; }

    /// <summary>
    /// Optional room describe / per-turn refresh hooks. Null uses built-in Infocom-style presentation.
    /// </summary>
    public RoomPresentation? RoomPresentation { get; init; }

    /// <summary>
    /// Optional input prompt shown by the host (e.g. Sierra's "What shall I do?"). Null keeps the
    /// host default. Usually set via <see cref="RoomPresentation.InputPrompt"/> or
    /// <see cref="Building.GameBuilder.WithSierraRoomBanner"/>.
    /// </summary>
    public string? InputPrompt { get; init; }
}
