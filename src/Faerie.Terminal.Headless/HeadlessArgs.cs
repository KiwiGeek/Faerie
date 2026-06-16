namespace Faerie.Terminal.Headless;

/// <summary>Parses command-line flags for headless script replay.</summary>
public static class HeadlessArgs
{
    /// <summary>
    /// When <paramref name="args"/> contains <c>--script</c>, returns headless options and
    /// <see langword="true"/>. Otherwise returns <see langword="false"/> and the caller should
    /// start the graphical host.
    /// </summary>
    public static bool TryParse(string[] args, out HeadlessOptions? options, out string? error)
    {
        options = null;
        error = null;

        string? script = null;
        string? transcript = null;
        int? seed = null;
        string? save = null;
        bool helpRequested = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "--script":
                    if (!TryReadValue(args, ref i, out script)) { error = "Missing value for --script."; return true; }
                    break;
                case "--transcript":
                case "-o":
                    if (!TryReadValue(args, ref i, out transcript)) { error = "Missing value for --transcript."; return true; }
                    break;
                case "--seed":
                    if (!TryReadValue(args, ref i, out string? seedText)) { error = "Missing value for --seed."; return true; }
                    if (!int.TryParse(seedText, out int parsed)) { error = $"Invalid --seed value: {seedText}"; return true; }
                    seed = parsed;
                    break;
                case "--save":
                    if (!TryReadValue(args, ref i, out save)) { error = "Missing value for --save."; return true; }
                    break;
                case "--help":
                case "-h":
                    helpRequested = true;
                    break;
            }
        }

        if (helpRequested && script is null)
        {
            error = FormatHelp();
            return true;
        }

        if (script is null) return false;

        bool scriptFromStdin = script == "-";
        if (!scriptFromStdin && !File.Exists(script))
        {
            error = $"Script file not found: {script}";
            return true;
        }

        transcript ??= scriptFromStdin ? "-" : Path.ChangeExtension(script, ".transcript.txt");

        string savePath = save ?? (scriptFromStdin
            ? "headless.save.json"
            : Path.ChangeExtension(script, ".save.json"));

        options = new HeadlessOptions
        {
            ScriptPath = script,
            TranscriptPath = transcript,
            RandomSeed = seed,
            SavePath = savePath
        };
        return true;
    }

    /// <summary>Usage text printed when <c>--help</c> is passed with headless flags.</summary>
    public static string FormatHelp() =>
        """
        Headless mode — replay commands from a script and write a plain-text transcript.

          --script <path>       Input script, or - for stdin (one command per line; # and ; comments)
          --transcript <path>   Output transcript, or - for stdout (default: <script>.transcript.txt, or - when script is -)
          -o <path>             Alias for --transcript
          --seed <n>            Fixed random seed for reproducible rolls
          --save <path>         Save-game file path (default: <script>.save.json, or headless.save.json when script is -)
          -h, --help            Show this help

        Pipe example:
          type commands.txt | dotnet run --project src/Faerie.Samples.Zork -- --script - -o -

        Omit --script to launch the graphical Avalonia window instead.
        """;

    private static bool TryReadValue(string[] args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Length) return false;
        value = args[++index];
        return true;
    }
}
