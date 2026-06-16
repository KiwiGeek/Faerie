using Faerie.Runtime;

namespace Faerie.Terminal.Headless;

/// <summary>Runs a game headlessly: script in, transcript out.</summary>
public static class HeadlessRunner
{
    /// <summary>
    /// Plays <paramref name="game"/> by submitting each line from the script. Returns process exit code
    /// (0 on success, 1 on error).
    /// </summary>
    public static int Run(Game game, HeadlessOptions options)
    {
        try
        {
            using StreamWriter transcript = new(options.TranscriptPath, append: false);
            TranscriptTerminal terminal = new(transcript);
            GameEngine engine = new(game, terminal, options.RandomSeed);
            WireSaveSystem(engine, options.SavePath!);

            engine.Start();

            foreach (string command in ScriptReader.ReadCommands(options.ScriptPath))
            {
                transcript.WriteLine($"> {command}");
                transcript.Flush();
                engine.Submit(command);
                if (engine.QuitRequested) break;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Headless run failed: {ex.Message}");
            return 1;
        }
    }

    private static void WireSaveSystem(GameEngine engine, string savePath)
    {
        engine.SaveProvider = () => savePath;
        engine.WriteSave = json =>
        {
            string? dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(savePath, json);
        };
        engine.RestoreProvider = () => File.Exists(savePath) ? savePath : null;
        engine.ReadSave = p => File.Exists(p) ? File.ReadAllText(p) : null;
    }
}
