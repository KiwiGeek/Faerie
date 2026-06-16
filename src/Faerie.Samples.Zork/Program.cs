using Avalonia;
using Faerie.Terminal.Headless;

namespace Faerie.Samples.Zork;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (HeadlessArgs.TryParse(args, out HeadlessOptions? options, out string? error))
        {
            if (options is null)
            {
                Console.WriteLine(error);
                Environment.Exit(error == HeadlessArgs.FormatHelp() ? 0 : 1);
            }

            Environment.Exit(HeadlessRunner.Run(ZorkGame.Build(), options));
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
