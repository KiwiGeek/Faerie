using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Nixie.Presentation;
using Nixie.Runtime;
using Nixie.Terminal;

namespace Nixie.Sample.HauntedHouse;

public sealed class App : Application
{
    public override void Initialize()
    {
        // Code-only theme setup (no XAML required for this single-window app).
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Game game = HauntedHouseGame.Build();

            TerminalWindow window = new();
            ApplyWindowChrome(window, game);

            // Font and caret come from the game definition (the engine ships no fonts).
            window.Terminal.CursorStyle = game.Cursor;
            window.Terminal.FontSpec = game.FontSpec;

            // Initial size is provisional; the control resizes the buffer to fill the window.
            TerminalBuffer buffer = new(80, 25, new TextStyle(TerminalColor.LightGray, TerminalColor.Black));
            window.Terminal.Buffer = buffer;

            GameEngine engine = new(game, buffer);
            WireSaveSystem(engine);

            // The host auto-starts once the control is laid out (see TerminalControl.Ready).
            _ = new GameHost(engine, window.Terminal);
            engine.OnQuit = () => desktop.Shutdown();

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ApplyWindowChrome(Window window, Game game)
    {
        window.Title = string.IsNullOrWhiteSpace(game.WindowTitle) ? game.Title : game.Wi