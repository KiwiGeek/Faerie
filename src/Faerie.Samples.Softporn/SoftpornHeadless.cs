using Faerie.Samples.Softporn.Interpreter;
using Faerie.Terminal.Headless;

namespace Faerie.Samples.Softporn;

internal static class SoftpornHeadless
{
    public static int Run(HeadlessOptions options)
    {
        try
        {
            bool useStdout = options.TranscriptToStdout;
            if (useStdout || options.ScriptFromStdin)
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true) { AutoFlush = true });

            IEnumerable<string> script = options.ScriptFromStdin
                ? ScriptReader.ReadCommands(Console.In)
                : ScriptReader.ReadCommands(options.ScriptPath);

            StreamWriter? owned = null;
            TextWriter transcript = useStdout
                ? owned = new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true) { AutoFlush = true }
                : new StreamWriter(options.TranscriptPath, append: false) { AutoFlush = true };

            try
            {
                new SoftpornHost(TextReader.Null, transcript, script).Run();
                return 0;
            }
            finally
            {
                owned?.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Headless run failed: {ex.Message}");
            return 1;
        }
    }
}
