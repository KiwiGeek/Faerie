using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Zork;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args) =>
        AvaloniaLauncher.For(ZorkGame.Build)
            .WithHeadlessFromArgs()
            .WithDefaultAppDataSaveCatalog("Zork", "zork")
            .Run(args);
}
