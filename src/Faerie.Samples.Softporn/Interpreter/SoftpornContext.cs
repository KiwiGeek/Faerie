namespace Faerie.Samples.Softporn.Interpreter;

public sealed class SoftpornContext
{
    public GameState State { get; }
    public SoftpornIo Io { get; }
    public SoftpornMessages Messages { get; }
    public Random Rng { get; } = Random.Shared;

    public string LineFromKbd { get; set; } = "";
    public string FullVerb { get; set; } = "";
    public string FullNoun { get; set; } = "";
    public string TaxiDestination { get; set; } = "";
    public string Password { get; set; } = "";
    public bool VerbOnly { get; set; }
    public bool NoVerb { get; set; }
    public bool NoObject { get; set; }
    public bool NoDirection { get; set; }
    public bool AnythingCarried { get; set; }

    public SoftpornContext(GameState state, SoftpornIo io, SoftpornMessages messages)
    {
        State = state;
        Io = io;
        Messages = messages;
    }

    public TextWriter Out => Io.Writer;
}
