using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Terminal;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Softporn;

public sealed class App : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Game game = SoftpornGame.Build();

            TerminalWindow window = new();
            window.Title = string.IsNullOrWhiteSpace(game.WindowTitle) ? game.Title : game.WindowTitle!;
            window.Terminal.CursorStyle = game.Cursor;
            window.Terminal.FontSpec = game.FontSpec;

            TerminalBuffer buffer = new(80, 25, new TextStyle(TerminalColor.LightGray, TerminalColor.Black));
            window.Terminal.Buffer = buffer;

            GameEngine engine = new(game, buffer);
            WireSaveSystem(engine);

            _ = new GameHost(engine, window.Terminal);
            engine.OnQuit = () => desktop.Shutdown();

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void WireSaveSystem(GameEngine engine)
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Softporn");
        engine.SaveCatalog = new SaveSlotCatalog(dir, "SOFTPORN", ".SAV", SaveSlotNaming.SierraPrefix);
    }
}
