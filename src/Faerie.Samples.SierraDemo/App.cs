using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Faerie.Runtime;
using Faerie.Terminal;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.SierraDemo;

public sealed class App : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Game game = SierraDemoGame.Build();

            TerminalWindow window = new() { Title = game.WindowTitle ?? game.Title };
            window.Terminal.CursorStyle = game.Cursor;
            window.Terminal.FontSpec = game.FontSpec;

            TerminalBuffer buffer = new(80, 25);
            window.Terminal.Buffer = buffer;

            GameEngine engine = new(game, buffer);
            _ = new GameHost(engine, window.Terminal);
            engine.OnQuit = () => desktop.Shutdown();

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
