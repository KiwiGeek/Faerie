using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Terminal;
using System.Text;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// Fluent entry point for composing the Avalonia display host around a game engine.
/// </summary>
public static class AvaloniaDisplay
{
    /// <summary>
    /// Starts a display-host builder around an existing engine. The engine terminal must be a
    /// <see cref="TerminalBuffer"/> so the UI can render it.
    /// </summary>
    public static AvaloniaDisplayBuilder For(GameEngine engine) => AvaloniaDisplayBuilder.From(engine);

    /// <summary>
    /// Starts a display-host builder by creating a <see cref="GameEngine"/> and backing
    /// <see cref="TerminalBuffer"/> for <paramref name="game"/>.
    /// </summary>
    public static AvaloniaDisplayBuilder For(Game game) =>
        AvaloniaDisplayBuilder.From(game);
}

/// <summary>
/// Fluent builder that wires a <see cref="GameEngine"/> to a <see cref="TerminalWindow"/>.
/// </summary>
public sealed class AvaloniaDisplayBuilder
{
    private readonly GameEngine _engine;
    private readonly TerminalBuffer _buffer;
    private TerminalWindow? _window;
    private Action? _onQuit;
    private Action<GameEngine>? _engineConfig;
    private string _promptMarkup = "{fg:lightgreen}>{/} ";
    private bool _applyGameWindowChrome = true;
    private bool _applyGameTerminalStyle = true;

    private AvaloniaDisplayBuilder(GameEngine engine, TerminalBuffer buffer)
    {
        _engine = engine;
        _buffer = buffer;
    }

    internal static AvaloniaDisplayBuilder From(GameEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        if (engine.Terminal is not TerminalBuffer buffer)
            throw new InvalidOperationException(
                "AvaloniaDisplay.For(GameEngine) requires the engine to be created with a TerminalBuffer.");
        return new AvaloniaDisplayBuilder(engine, buffer);
    }

    internal static AvaloniaDisplayBuilder From(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        TerminalBuffer buffer = new(80, 25, new TextStyle(TerminalColor.LightGray, TerminalColor.Black));
        GameEngine engine = new(game, buffer);
        return new AvaloniaDisplayBuilder(engine, buffer);
    }

    /// <summary>Uses an existing terminal window instead of creating a default one.</summary>
    public AvaloniaDisplayBuilder WithWindow(TerminalWindow window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        return this;
    }

    /// <summary>Sets the default prompt markup for games that do not define <see cref="Game.InputPrompt"/>.</summary>
    public AvaloniaDisplayBuilder WithPromptMarkup(string promptMarkup)
    {
        _promptMarkup = promptMarkup ?? throw new ArgumentNullException(nameof(promptMarkup));
        return this;
    }

    /// <summary>Applies game title/icon metadata to the native window.</summary>
    public AvaloniaDisplayBuilder ApplyGameWindowChrome(bool enabled = true)
    {
        _applyGameWindowChrome = enabled;
        return this;
    }

    /// <summary>Applies game font/cursor metadata to the terminal control.</summary>
    public AvaloniaDisplayBuilder ApplyGameTerminalStyle(bool enabled = true)
    {
        _applyGameTerminalStyle = enabled;
        return this;
    }

    /// <summary>Runs additional engine setup just before the display host is created.</summary>
    public AvaloniaDisplayBuilder ConfigureEngine(Action<GameEngine> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _engineConfig += configure;
        return this;
    }

    /// <summary>Sets the save-slot catalog used by SAVE/RESTORE.</summary>
    public AvaloniaDisplayBuilder WithSaveCatalog(SaveSlotCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        return ConfigureEngine(engine => engine.SaveCatalog = catalog);
    }

    /// <summary>
    /// Configures save slots under the current user's local app-data folder.
    /// </summary>
    public AvaloniaDisplayBuilder WithAppDataSaveCatalog(
        string appDataFolder,
        string baseName,
        string extension = ".json",
        SaveSlotNaming naming = SaveSlotNaming.SuffixDash,
        int maxLabelLength = 3)
    {
        if (string.IsNullOrWhiteSpace(appDataFolder))
            throw new ArgumentException("App-data folder name is required.", nameof(appDataFolder));
        if (string.IsNullOrWhiteSpace(baseName))
            throw new ArgumentException("Save base name is required.", nameof(baseName));

        return ConfigureEngine(engine =>
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appDataFolder);
            engine.SaveCatalog = new SaveSlotCatalog(dir, baseName, extension, naming, maxLabelLength);
        });
    }

    /// <summary>
    /// Configures save slots in local app-data using defaults from the game title. When omitted,
    /// both <paramref name="appDataFolder"/> and <paramref name="baseName"/> are derived from a
    /// slug of <see cref="Game.Title"/>.
    /// </summary>
    public AvaloniaDisplayBuilder WithDefaultAppDataSaveCatalog(
        string? appDataFolder = null,
        string? baseName = null,
        string extension = ".json",
        SaveSlotNaming naming = SaveSlotNaming.SuffixDash,
        int maxLabelLength = 3)
    {
        string fallback = Slugify(_engine.Game.Title);
        return WithAppDataSaveCatalog(
            string.IsNullOrWhiteSpace(appDataFolder) ? fallback : appDataFolder,
            string.IsNullOrWhiteSpace(baseName) ? fallback : baseName,
            extension,
            naming,
            maxLabelLength);
    }

    /// <summary>Sets the action run when QUIT is requested.</summary>
    public AvaloniaDisplayBuilder WithOnQuit(Action onQuit)
    {
        _onQuit = onQuit ?? throw new ArgumentNullException(nameof(onQuit));
        return this;
    }

    /// <summary>Builds a display session but does not assign a desktop main window.</summary>
    public AvaloniaDisplaySession Build()
    {
        _engineConfig?.Invoke(_engine);

        TerminalWindow window = _window ?? new TerminalWindow();
        TerminalControl terminal = window.Terminal;

        if (_applyGameWindowChrome)
            ApplyWindowChrome(window, _engine.Game);
        if (_applyGameTerminalStyle)
        {
            terminal.CursorStyle = _engine.Game.Cursor;
            terminal.FontSpec = _engine.Game.FontSpec;
        }

        terminal.Buffer = _buffer;
        GameHost host = new(_engine, terminal, _promptMarkup);

        if (_onQuit is not null)
            _engine.OnQuit = _onQuit;

        return new AvaloniaDisplaySession(_engine, _buffer, window, host);
    }

    /// <summary>
    /// Builds a display session, wires QUIT to close the desktop lifetime (unless overridden), and sets
    /// the main window.
    /// </summary>
    public AvaloniaDisplaySession AttachToDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        AvaloniaDisplaySession session = Build();
        session.Engine.OnQuit ??= () => desktop.Shutdown();
        desktop.MainWindow = session.Window;
        return session;
    }

    private static void ApplyWindowChrome(Window window, Game game)
    {
        window.Title = string.IsNullOrWhiteSpace(game.WindowTitle) ? game.Title : game.WindowTitle!;

        if (game.WindowIconUri is not { } iconUri)
            return;

        if (iconUri.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(iconUri, UriKind.Absolute, out Uri? uri) || !AssetLoader.Exists(uri))
                return;

            using Stream stream = AssetLoader.Open(uri);
            window.Icon = new WindowIcon(stream);
            return;
        }

        if (!File.Exists(iconUri))
            return;

        using Stream file = File.OpenRead(iconUri);
        window.Icon = new WindowIcon(file);
    }

    private static string Slugify(string value)
    {
        StringBuilder slug = new();
        bool pendingDash = false;
        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (pendingDash && slug.Length > 0)
                    slug.Append('-');
                slug.Append(char.ToLowerInvariant(c));
                pendingDash = false;
            }
            else if (slug.Length > 0)
            {
                pendingDash = true;
            }
        }

        return slug.Length == 0 ? "game" : slug.ToString();
    }
}

/// <summary>
/// The composed Avalonia display wiring: engine, backing buffer, window and host bridge.
/// </summary>
public sealed record AvaloniaDisplaySession(
    GameEngine Engine,
    TerminalBuffer Buffer,
    TerminalWindow Window,
    GameHost Host);
