using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Softporn;

public sealed class App : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            TerminalWindow window = new();
            window.Title = "Softporn Adventure";
            window.Terminal.FontSpec = "avares://Softporn/Assets/Fonts#PxPlus IBM VGA 8x16";

            _ = new SoftpornTerminalSession(window.Terminal, () => desktop.Shutdown());
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
