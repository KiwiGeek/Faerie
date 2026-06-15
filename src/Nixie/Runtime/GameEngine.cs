using Nixie.Model;
using Nixie.Parsing;
using Nixie.Presentation;
using Nixie.Verbs;
using static Nixie.Verbs.VerbText;

namespace Nixie.Runtime;

/// <summary>
/// Drives a <see cref="Game"/>: prints the intro, describes rooms, parses each line the player
/// types, dispatches verbs (with reaction hooks), runs daemons, and keeps the title/status bars up
/// to date. The engine is UI-agnostic — it writes through an <see cref="ITerminal"/> and is fed
/// input by whatever host is driving it (the Avalonia fake terminal, a console, or a test).
/// </summary>
public sealed class GameEngine
{
    // Verbs that still work when the room is dark.
    private static readonly HashSet<string> DarkSafe =
    [
        StandardVerbIds.Go, StandardVerbIds.Inventory, StandardVerbIds.Wait, StandardVerbIds.Drop,
        StandardVerbIds.PushButton, StandardVerbIds.SwitchOff,
        StandardVerbIds.Help, StandardVerbIds.Quit, StandardVerbIds.Save, StandardVerbIds.Restore, StandardVerbIds.Score
    ];

    // Verbs still accepted after the game has ended (so the player can recover or leave).
    private static readonly HashSet<string> EndSafe =
    [
        StandardVerbIds.Help, StandardVerbIds.Score, StandardVerbIds.Restore, StandardVerbIds.Quit
    ];

    private readonly Game _game;
    private readonly GameContext _context;

    public GameEngine(Game game, ITerminal terminal, int? randomSeed = null)
    {
        _game = game;
        State = new GameState(game.World);
        Out = new OutputWriter(terminal) { BaseStyle = terminal.DefaultStyle };
        Parser = new Parser(game.Verbs);
        Random = randomSeed is { } s ? new Random(s) : new Random();
        _context = new GameContext(this, State, Out);

        terminal.ConfigureBars(game.TitleBar is not null, game.StatusBar is not null);
    }

    public GameState State { get; }
    public OutputWriter Out { get; }
    public Parser Parser { get; }
    public Random Random { get; }
    public ITerminal Terminal => Out.Terminal;
    public int MaxScore => _game.MaxScore;

    /// <summary>True once the player has quit or the game has ended.</summary>
    public bool IsFinished => State.IsOver || QuitRequested;
    public bool QuitRequested { get; private set; }

    // Hooks the host wires up for platform-specific behaviour (file dialogs, window close, ...).
    public Action? OnQuit { get; set; }
    public Func<string>? SaveProvider { get; set; }      // returns a path/slot to save to
    public Func<string?>? RestoreProvider { get; set; }  // returns a path/slot to restore from, or null to cancel
    public Action<string>? WriteSave { get; set; }       // persist json to the given path
    public Func<string, string?>? ReadSave { get; set; } // read json from the given path

    // ---- lifecycle ------------------------------------------------------------------------

    /// <summary>Prints the intro and the opening room, runs the start hook, and paints the bars.</summary>
    public void Start()
    {
        Out.Clear();
        if (_game.Intro is { } intro)
        {
            Out.PrintLine(intro);
            Out.Blank();
        }

        EnterRoom(State.CurrentRoom, firstDescription: true);
        _game.OnStart?.Invoke(_context);
        RefreshBars();
    }

    /// <summary>Processes one line of player input, advancing the world a turn if a command runs.</summary>
    public void Submit(string input)
    {
        if (QuitRequested) return;
        bool wasOver = State.IsOver;

        ParsedCommand command = Parser.Parse(input, new Scope(State), State);

        switch (command.Status)
        {
            case ParseStatus.Empty:
                Out.PrintLine("I beg your pardon?");
                RefreshBars();
                return;
            case ParseStatus.UnknownVerb:
            case ParseStatus.UnknownObject:
            case ParseStatus.Ambiguous:
                Out.PrintLine(command.Message ?? "That doesn't make sense.");
                RefreshBars();
                return;
        }

        Verb verb = command.Verb!;

        // Once the game has ended, only meta verbs (restore/quit/help/score) are accepted.
        if (wasOver)
        {
            if (!EndSafe.Contains(verb.Id))
            {
                Out.PrintLine("The game is over. Type RESTORE to load a saved game, or QUIT.");
                RefreshBars();
                return;
            }

            RunCommand(verb, command, input);
            RefreshBars();
            return;
        }

        // Darkness gate.
        if (!new Scope(State).IsCurrentRoomLit && !DarkSafe.Contains(verb.Id))
        {
            Out.PrintLine("It's pitch black down here. You can't see a thing.");
            RefreshBars();
            return;
        }

        RunCommand(verb, command, input);

        // A turn has passed.
        State.TurnCount++;
        if (!IsFinished)
        {
            State.CurrentRoom.OnTurn?.Invoke(_context);
            foreach (TurnDaemon daemon in _game.Daemons)
            {
                if (IsFinished) break;
                daemon.Tick(_context);
            }
        }

        RefreshBars();

        // Announce the ending only on the turn it actually happens.
        if (State.IsOver && !wasOver)
            AnnounceEnding();
    }

    private void RunCommand(Verb verb, ParsedCommand command, string input)
    {
        VerbContext vctx = new(this, State, Out)
        {
            Verb = verb,
            RawInput = input,
            DirectObject = command.DirectObject,
            IndirectObject = command.IndirectObject,
            Preposition = command.Preposition,
            Direction = command.Direction,
            DirectObjectText = command.DirectObjectText,
            IndirectObjectText = command.IndirectObjectText
        };

        // before-reactions can fully handle (and override) the verb
        if (_game.Reactions.RunBefore(vctx) != VerbResult.Done)
            verb.Handler(vctx);

        _game.Reactions.RunAfter(vctx);
    }

    // ---- movement & description -----------------------------------------------------------

    public void MovePlayerTo(Room room)
    {
        State.CurrentRoom = room;
        bool first = !room.Has(Attr.Visited);
        EnterRoom(room, firstDescription: first);
    }

    private void EnterRoom(Room room, bool firstDescription)
    {
        room.OnEnter?.Invoke(_context);
        if (!room.Has(Attr.Visited))
        {
            room.OnFirstEnter?.Invoke(_context);
            room.Set(Attr.Visited);
        }

        if (!IsFinished)
            DescribeCurrentRoom(verbose: firstDescription);
    }

    /// <summary>Prints the current room's heading, description, contents and exits.</summary>
    public void DescribeCurrentRoom(bool verbose)
    {
        Room room = State.CurrentRoom;
        Scope scope = new(State);

        Out.Blank();
        Out.PrintLine($"{{bold}}{{fg:white}}{room.Name}{{/}}{{/}}");

        if (!scope.IsLit(room))
        {
            Out.PrintLine("It is pitch dark, and you can see nothing.");
            return;
        }

        // Full description on LOOK / first visit; the shorter brief on re-entry to a known room.
        Out.PrintLine(verbose ? room.ResolveDescription(_context) : room.ResolveBrief(_context));

        DescribeContents(room);
        DescribeExits(room);
    }

    private void DescribeContents(Room room)
    {
        List<Thing> loose = State.ContentsOf(room)
            .Where(t => !t.Has(Attr.Concealed) && !t.Has(Attr.Scenery) && !t.Has(Attr.Animate))
            .ToList();

        List<Thing> creatures = State.ContentsOf(room)
            .Where(t => !t.Has(Attr.Concealed) && t.Has(Attr.Animate))
            .ToList();

        List<Thing> plain = [];
        foreach (Thing thing in loose)
        {
            // A custom initial line is shown only while the thing sits in its original spot.
            if (thing.InitialDescription is { } line &&
                State.PlacementOf(thing) == thing.InitialPlacement)
                Out.PrintLine(line);
            else
                plain.Add(thing);
        }

        if (plain.Count > 0)
        {
            string list = JoinWithAnd(plain.Select(A));
            Out.PrintLine($"You can see {list} here.");
        }

        foreach (Thing creature in creatures)
            Out.PrintLine(creature.InitialDescription ?? $"{Cap(A(creature))} is here.");
    }

    private void DescribeExits(Room room)
    {
        if (room.Exits.Count == 0) return;
        string exits = JoinWithAnd(room.Exits.Keys.Select(d => d.ToDisplayString()));
        Out.PrintLine($"{{fg:darkgray}}Exits: {exits}.{{/}}");
    }

    // ---- meta-verb plumbing ---------------------------------------------------------------

    public void ShowHelp()
    {
        Out.PrintLine("{bold}How to play{/}");
        Out.PrintLine("Type short commands such as LOOK, EXAMINE LANTERN, TAKE KEY, GO NORTH (or just N),");
        Out.PrintLine("OPEN DOOR, PUT COIN IN SLOT, GIVE BONE TO DOG, INVENTORY (I), WAIT (Z).");
        Out.PrintLine("Movement: N S E W NE NW SE SW UP DOWN IN OUT.");
        Out.PrintLine("Other commands: SAVE, RESTORE, SCORE, QUIT. Press F11 to toggle fullscreen.");
    }

    public void RequestQuit()
    {
        Out.PrintLine("Until next time...");
        QuitRequested = true;
        OnQuit?.Invoke();
    }

    public void RequestSave()
    {
        if (SaveProvider is null || WriteSave is null)
        {
            Out.PrintLine("Saving isn't available in this build.");
            return;
        }

        try
        {
            string slot = SaveProvider();
            WriteSave(SaveSystem.Capture(State, _game.Daemons));
            Out.PrintLine("Game saved.");
            _ = slot;
        }
        catch (Exception ex)
        {
            Out.PrintLine($"{{fg:lightred}}Save failed: {ex.Message}{{/}}");
        }
    }

    public void RequestRestore()
    {
        if (RestoreProvider is null || ReadSave is null)
        {
            Out.PrintLine("Restoring isn't available in this build.");
            return;
        }

        try
        {
            if (RestoreProvider() is not { } slot) { Out.PrintLine("Restore cancelled."); return; }
            if (ReadSave(slot) is not { } json) { Out.PrintLine("No save found."); return; }

            SaveSystem.Restore(json, State, _game.Daemons);
            Out.PrintLine("Game restored.");
            DescribeCurrentRoom(verbose: true);
        }
        catch (Exception ex)
        {
            Out.PrintLine($"{{fg:lightred}}Restore failed: {ex.Message}{{/}}");
        }
    }

    /// <summary>Captures the current game to a JSON string (for hosts that manage their own files).</summary>
    public string CreateSnapshot() => SaveSystem.Capture(State, _game.Daemons);

    /// <summary>Restores game state from a JSON snapshot.</summary>
    public void LoadSnapshot(string json) => SaveSystem.Restore(json, State, _game.Daemons);

    private void AnnounceEnding()
    {
        Out.Blank();
        Out.PrintLine(State.PlayerWon
            ? "{bold}{fg:lightgreen}*** You have won ***{/}{/}"
            : "{bold}{fg:lightred}*** Game over ***{/}{/}");
        if (MaxScore > 0)
            Out.PrintLine($"Final score: {State.Score}/{MaxScore} in {State.TurnCount} turns.");
        Out.PrintLine("Type RESTORE to load a saved game, or QUIT.");
    }

    // ---- bars -----------------------------------------------------------------------------

    /// <summary>Recomputes the title/status bars for the current terminal width. Call after a resize.</summary>
    public void RefreshStatusBars() => RefreshBars();

    private void RefreshBars()
    {
        if (!Terminal.SupportsBars) return;

        if (_game.TitleBar is { } title)
            Terminal.SetTitleBar(BarComposer.Compose(title(_context), Terminal.Columns));

        if (_game.StatusBar is { } status)
            Terminal.SetStatusBar(BarComposer.Compose(status(_context), Terminal.Columns));
    }

    // ---- helpers --------------------------------------------------------------------------

    private static string JoinWithAnd(IEnumerable<string> items)
    {
        List<string> list = items.ToList();
        return list.Count switch
        {
            0 => "",
            1 => list[0],
            2 => $"{list[0]} and {list[1]}",
            _ => string.Join(", ", list.Take(list.Count - 1)) + $", and {list[^1]}"
        };
    }
}
