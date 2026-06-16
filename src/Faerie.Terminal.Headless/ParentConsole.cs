using System.Runtime.InteropServices;

namespace Faerie.Terminal.Headless;

/// <summary>
/// WinExe hosts (e.g. Avalonia sample apps) have no console on Windows; attach to the parent
/// terminal so <c>-o -</c> and <c>--script -</c> work under <c>dotnet run</c> or PowerShell.
/// </summary>
internal static class ParentConsole
{
    private const int AttachParentProcess = -1;

    public static void AttachIfNeeded(bool useStdin, bool useStdout)
    {
        if (!useStdin && !useStdout) return;
        if (!OperatingSystem.IsWindows()) return;
        if (GetConsoleWindow() != IntPtr.Zero) return;

        if (!AttachConsole(AttachParentProcess)) return;

        if (useStdout)
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        }

        if (useStdin)
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);
}
