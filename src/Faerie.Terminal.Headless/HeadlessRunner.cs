using Faerie.Runtime;

namespace Faerie.Terminal.Headless;

/// <summary>Runs a game headlessly: script in, transcript out.</summary>
public static class HeadlessRunner
{
    /// <summary>
    /// Plays <paramref name="game"/> by submitting each line from the script. Returns process exit code
    /// (0 on success, 1 on error).
    /// </summary>
    public static int Run(Game game, HeadlessOptions options) =>
        Run(game, options, scriptOverride: null, transcriptOverride: null);

    /// <summary>
    /// Like <see cref="Run(Faerie.Runtime.Game, HeadlessOptions)"/> but allows overriding the script
    /// input and transcript output streams (for tests). When null, <paramref name="options"/> paths are
    /// used (<c>-</c> = stdin/stdout).
    /// </summary>
    public static int Run(Game game, HeadlessOptions options, TextReader? scriptOverride, TextWriter? transcriptOverride)
    {
        try
        {
            bool useStdin = scriptOverride is null && options.ScriptFromStdin;
            bool useStdout = transcriptOverride is null && options.TranscriptToStdout;
            ParentConsole.AttachIfNeeded(useStdin, useStdout);

            StreamWriter? ownedWriter = null;
            TextWriter transcriptWriter = transcriptOverride ?? OpenTranscript(options, out ownedWriter);
            try
            {
                TranscriptTerminal terminal = new(transcriptWriter);
                GameEngine engine = new(game, terminal, options.RandomSeed);
                WireSaveSystem(engine, options);

                var script = new Queue<string>(ReadCommands(options, scriptOverride));
                void LogScriptLine(string line)
                {
                    transcriptWriter.WriteLine($"> {line}");
                    transcriptWriter.Flush();
                }

                engine.PlayerInput = new ScriptPlayerInput(script, LogScriptLine);
                engine.Start();

                while (script.Count > 0 && !engine.QuitRequested)
                {
                    string command = script.Dequeue();
                    LogScriptLine(command);
                    engine.SubmitLine(command);
                }

                return 0;
            }
            finally
            {
                ownedWriter?.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Headless run failed: {ex.Message}");
            return 1;
        }
    }

    private static TextWriter OpenTranscript(HeadlessOptions options, out StreamWriter? ownedWriter)
    {
        if (options.TranscriptToStdout)
        {
            // leaveOpen: true — stdout must stay open; Dispose() still flushes before Environment.Exit.
            ownedWriter = new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true) { AutoFlush = true };
            return ownedWriter;
        }

        ownedWriter = new StreamWriter(options.TranscriptPath, append: false);
        return ownedWriter;
    }

    private static IEnumerable<string> ReadCommands(HeadlessOptions options, TextReader? scriptOverride)
    {
        if (scriptOverride is not null)
            return ScriptReader.ReadCommands(scriptOverride);
        return options.ScriptFromStdin
            ? ScriptReader.ReadCommands(Console.In)
            : ScriptReader.ReadCommands(options.ScriptPath);
    }

    private static void WireSaveSystem(GameEngine engine, HeadlessOptions options)
    {
        if (options.SaveDirectory is { } dir)
        {
            engine.SaveCatalog = new SaveSlotCatalog(dir, "headless");
            return;
        }

        string savePath = options.SavePath!;
        engine.SaveCatalog = new SaveSlotCatalog(
            Path.GetDirectoryName(savePath) ?? ".",
            Path.GetFileNameWithoutExtension(savePath));
    }
}
