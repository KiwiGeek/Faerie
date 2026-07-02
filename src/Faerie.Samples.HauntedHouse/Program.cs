using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.HauntedHouse;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args) =>
        AvaloniaLauncher.For(HauntedHouseGame.Build)
            .WithHeadlessFromArgs()
            .WithDefaultAppDataSaveCatalog("HauntedHouse", "haunted-house")
            .Run(args);
}
