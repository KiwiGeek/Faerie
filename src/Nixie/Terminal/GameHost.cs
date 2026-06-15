using Nixie.Runtime;

namespace Nixie.Terminal;

/// <summary>
/// Glues a <see cref="GameEngine"/> to a <see cref="TerminalControl"/>: starts the game, prints the
/// prompt, feeds each submitted line to the engine, and stops accepting input when the game ends.
/// </summary>
public sealed class GameHost
{
    private readonly GameEngine _engine;
    private readonly TerminalControl _control;
    private readonly string _promptMarkup;
    private bool _started;

    public GameHost(GameEngine engine, TerminalControl control, string promptMarkup = "{fg:lightgreen}>{/} ")
    {
        _engine = engine;
        _control = control;
        _promptMarkup = promptMarkup;
        _control.CommandEntered += OnCommandEntered;

        // The game starts once the control has laid out and sized its buffer to the window.
        _control.Ready += Start;

        // On window-resize / font-change, the grid dimensions change; repaint the bars to fit.
        _control.Resized += () => _engine.RefreshStatusBars();
    }

    /// <summary>Starts the game (idempotent). Normally invoked automatically when the control is ready.</summary>
    public void Start()
    {
        if (_started) return;
        _started = true;
        _engine.Start();
        Prompt();
    }

    private void OnCommandEntered(string line)
    {
        // The control already echoed the line and moved to a fresh row; keep the wrapper in sync.
        _engine.Out.SyncColumn(0);
        _engine.Submit(line);
        Prompt();
    }

    private void Prompt()
    {
        // Keep accepting input after the game ends so the player can RESTORE or QUIT;
        // only stop once they have actually quit.
        if (_engine.QuitRequested)
        {
            _control.EndInput();
            return;
        }

        _engine.Out.Blank();
        _engine.Out.Print(_promptMarkup);
        _control.BeginInput();
    }
}
