using Faerie.Runtime;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Softporn;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args) =>
        AvaloniaLauncher.For(SoftpornGame.Build)
            .WithHeadlessFromArgs()
            .WithDefaultAppDataSaveCatalog("Softporn", "SOFTPORN", ".SAV", SaveSlotNaming.SierraPrefix)
            .Run(args);
}
