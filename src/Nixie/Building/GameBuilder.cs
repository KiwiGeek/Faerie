using System.Text;
using Nixie.Model;
using Nixie.Presentation;
using Nixie.Runtime;
using Nixie.Verbs;

namespace Nixie.Building;

/// <summary>
/// The fluent entry point for assembling a game. Create one with <see cref="Create"/>, add the
/// verbs you want ("the empty box" ships with none until you ask for them), define rooms, things
/// and creatures — wiring them together by reference — then call <see cref="Build"/>.
/// </summary>
/// <example>
/// <code>
/// var builder = GameBuilder.Create("The Haunted House").AddStandardVerbs();
/// var hall    = builder.Room("Entrance Hall").Describe("A cold, panelled hall.");
/// var kitchen = builder.Room("Kitchen").Describe("Grease-blackened and silent.");
/// hall.Connect(Direction.North, kitchen);
/// var key = builder.Item("brass key").Describe("A heavy brass key.").Takeable();
/// hall.Contains(key);
/// builder.StartIn(hall);
/// Game game = builder.Build();
/// </code>
/// </example>
public sealed class GameBuilder
{
    private readonly List<TurnDaemon> _daemons = [];
    private readonly HashSet<string> _modules = [];
    private readonly Dictionary<string, int> _idCounters = [];

    private string? _intro;
    private string? _author;
    private int _maxScore;
    private string? _windowTitle;
    private string? _windowIconUri;
    private string? _fontSpec;
    private TerminalCursor _cursor = TerminalCursor.Block;
    private Action<GameContext>? _onStart;
    private Func<GameContext, BarContent>? _titleBar;
    private Func<GameContext, BarContent>? _statusBar;

    private GameBuilder(string title) => Title = title;

    public static GameBuilder Create(string title) => new(title);

    public string Title { get; }

    public World World { get; } = new();
    public VerbLibrary Library { get; } = new();
    public ReactionTable Reactions { get; } = new();

    /// <summary>Named handles to the built-in verbs (populated as modules are added).</summary>
    public StandardVerbSet Verbs { get; } = new();

    // ---- world authoring ------------------------------------------------------------------

    /// <summary>Creates and registers a room.</summary>
    public Room Room(string name)
    {
        Room room = new(NextId("room", name), name);
        World.Register(room);
        return room;
    }

    /// <summary>Creates a takeable item.</summary>
    public Thing Item(string name)
    {
        Thing thing = CreateThing(name);
        thing.Set(Attr.Takeable);
        return thing;
    }

    /// <summary>Creates a fixed piece of scenery (examined but not taken, not auto-listed).</summary>
    public Thing Scenery(string name)
    {
        Thing thing = CreateThing(name);
        thing.Set(Attr.Fixed);
        thing.Set(Attr.Scenery);
        return thing;
    }

    /// <summary>Creates a living being (NPC / creature).</summary>
    public Thing Creature(string name)
    {
        Thing thing = CreateThing(name);
        thing.Set(Attr.Animate);
        thing.Set(Attr.Fixed);
        thing.Article = "the";
        return thing;
    }

    /// <summary>Creates a bare thing with no attributes set.</summary>
    public Thing Thing(string name) => CreateThing(name);

    private Thing CreateThing(string name)
    {
        Thing thing = new(NextId("thing", name), name);
        World.Register(thing);
        return thing;
    }

    /// <summary>Defines a handle to a piece of typed game state (a puzzle flag, counter, etc).</summary>
    public StateKey<T> State<T>(string name, T defaultValue) => new(name, defaultValue);

    // ---- start / metadata -----------------------------------------------------------------

    public GameBuilder StartIn(Room room)
    {
        World.StartRoom = room;
        return this;
    }

    public GameBuilder By(string author) { _author = author; return this; }
    public GameBuilder WithIntro(string markup) { _intro = markup; return this; }
    public GameBuilder WithMaxScore(int max) { _maxScore = max; return this; }
    public GameBuilder OnStart(Action<GameContext> action) { _onStart = action; return this; }

    /// <summary>Sets the native OS window title bar text (defaults to the game title if not set).</summary>
    public GameBuilder WithWindowTitle(string title) { _windowTitle = title; return this; }

    /// <summary>
    /// Sets the native window icon. Accepts an Avalonia resource URI
    /// (e.g. "avares://MyGame/Assets/icon.ico") or a file path. The host loads it; if it can't be
    /// found, the platform default icon is used.
    /// </summary>
    public GameBuilder WithWindowIcon(string uriOrPath) { _windowIconUri = uriOrPath; return this; }

    /// <summary>
    /// Chooses the terminal font. The font lives in <em>your</em> game assembly, not the engine, so
    /// the engine ships with no font dependencies. Accepts:
    /// a system family name ("Consolas"); an Avalonia resource font with explicit family
    /// ("avares://MyGame/Assets/Fonts#Apple ][" ); or a URI/folder pointing at an embedded font file
    /// or folder ("avares://MyGame/Assets/Fonts" or ".../MyFont.ttf"), in which case the family name
    /// is read from the font automatically. Null (the default) uses the host's monospace font.
    /// </summary>
    public GameBuilder WithFont(string fontSpec) { _fontSpec = fontSpec; return this; }

    /// <summary>Chooses the caret shape (block, underline, bar, outline).</summary>
    public GameBuilder WithCursor(TerminalCursor cursor) { _cursor = cursor; return this; }

    // ---- presentation ---------------------------------------------------------------------

    /// <summary>Supplies a callback that builds the title bar each turn.</summary>
    public GameBuilder WithTitleBar(Func<GameContext, BarContent> builder) { _titleBar = builder; return this; }

    /// <summary>Supplies a callback that builds the (optional) status bar each turn.</summary>
    public GameBuilder WithStatusBar(Func<GameContext, BarContent> builder) { _statusBar = builder; return this; }

    /// <summary>Installs a sensible default title bar (game name on the left, score/turns on the right).</summary>
    public GameBuilder WithDefaultTitleBar()
    {
        _titleBar = ctx =>
        {
            string right = _maxScore > 0
                ? $"Score: {ctx.State.Score}/{_maxScore}  Turns: {ctx.State.TurnCount}"
                : $"Turns: {ctx.State.TurnCount}";
            return new BarContent
            {
                Left = $" {Title}",
                Right = right + " ",
                Style = new TextStyle(TerminalColor.Black, TerminalColor.LightGray, TextAttributes.Bold)
            };
        };
        return this;
    }

    // ---- daemons & hooks ------------------------------------------------------------------

    /// <summary>Registers an action that runs at the start of every turn.</summary>
    public GameBuilder EveryTurn(Action<GameContext> action, Func<GameContext, bool>? when = null)
    {
        _daemons.Add(new TurnDaemon { Action = action, EveryTurn = true, Condition = when });
        return this;
    }

    /// <summary>Registers an action that fires once, the first time the turn count reaches <paramref name="turn"/>.</summary>
    public GameBuilder AtTurn(int turn, Action<GameContext> action)
    {
        _daemons.Add(new TurnDaemon { Action = action, AtTurn = turn });
        return this;
    }

    /// <summary>Begins a fluent reaction registration for a thing.</summary>
    public ReactionScope On(Thing thing) => new(this, thing);

    internal void RegisterBefore(Thing thing, Verb verb, Func<VerbContext, VerbResult> handler) => Reactions.Before(thing, verb, handler);
    internal void RegisterAfter(Thing thing, Verb verb, Func<VerbContext, VerbResult> handler) => Reactions.After(thing, verb, handler);

    // ---- verb modules ---------------------------------------------------------------------

    /// <summary>Adds the movement verb (and bare-direction handling). Part of <see cref="AddStandardVerbs"/>.</summary>
    public GameBuilder AddMovement()
    {
        if (_modules.Add("movement")) StandardVerbs.InstallMovement(this);
        return this;
    }

    /// <summary>Adds the observation and manipulation verbs (look, examine, take, open, put, ...).</summary>
    public GameBuilder AddCoreVerbs()
    {
        if (_modules.Add("core")) StandardVerbs.InstallCore(this);
        return this;
    }

    /// <summary>Adds the meta verbs (help, quit, save, restore, score).</summary>
    public GameBuilder AddMetaVerbs()
    {
        if (_modules.Add("meta")) StandardVerbs.InstallMeta(this);
        return this;
    }

    /// <summary>The omnibus: movement + core + meta verbs in one call.</summary>
    public GameBuilder AddStandardVerbs() => AddMovement().AddCoreVerbs().AddMetaVerbs();

    /// <summary>Defines and registers a custom verb, returning the instance for later reference.</summary>
    public Verb DefineVerb(string id, IEnumerable<string> words, VerbForms forms, Func<VerbContext, VerbResult> handler)
    {
        Verb verb = new(id, words, forms, handler);
        Library.Add(verb);
        return verb;
    }

    /// <summary>Registers an already-constructed verb.</summary>
    public Verb AddVerb(Verb verb)
    {
        Library.Add(verb);
        return verb;
    }

    // ---- build ----------------------------------------------------------------------------

    public Game Build()
    {
        if (World.StartRoom is null)
            throw new InvalidOperationException("No start room set. Call StartIn(room) before Build().");

        // Resolve initial placements recorded on things.
        foreach (Thing thing in World.Things)
            if (thing.InitialPlacement.Anchor != Anchor.Offstage)
                World.PlaceInitially(thing, thing.InitialPlacement);

        return new Game
        {
            Title = Title,
            Author = _author,
            WindowTitle = _windowTitle,
            WindowIconUri = _windowIconUri,
            FontSpec = _fontSpec,
            Cursor = _cursor,
            Intro = _intro,
            World = World,
            Verbs = Library,
            Reactions = Reactions,
            Daemons = _daemons,
            MaxScore = _maxScore,
            OnStart = _onStart,
            TitleBar = _titleBar,
            StatusBar = _statusBar
        };
    }

    private string NextId(string kind, string name)
    {
        StringBuilder slug = new();
        foreach (char c in name.ToLowerInvariant())
            slug.Append(char.IsLetterOrDigit(c) ? c : '-');
        string baseId = $"{kind}:{slug.ToString().Trim('-')}";

        int n = _idCounters.GetValueOrDefault(baseId);
        _idCounters[baseId] = n + 1;
        return n == 0 ? baseId : $"{baseId}-{n}";
    }
}

/// <summary>Fluent helper for attaching reactions to a specific thing.</summary>
public sealed class ReactionScope(GameBuilder builder, Thing thing)
{
    /// <summary>Intercepts a verb before its default behaviour. Return <see cref="VerbResult.Done"/> to override it.</summary>
    public ReactionScope Before(Verb verb, Func<VerbContext, VerbResult> handler)
    {
        builder.RegisterBefore(thing, verb, handler);
        return this;
    }

    /// <summary>Runs after a verb's default behaviour has completed.</summary>
    public ReactionScope After(Verb verb, Func<VerbContext, VerbResult> handler)
    {
        builder.RegisterAfter(thing, verb, handler);
        return this;
    }
}
