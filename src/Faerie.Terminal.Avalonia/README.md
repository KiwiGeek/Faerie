# Faerie.Terminal.Avalonia

Avalonia "fake terminal" front end for the [**Faerie**](https://www.nuget.org/packages/Faerie)
text-adventure engine.

Renders [`Faerie.Terminal`](https://www.nuget.org/packages/Faerie.Terminal)'s character-cell framebuffer
as an on-screen text display: true colour with blink/underline/bold/inverse, scrollback, zoom,
title/status bars, and a game-supplied or built-in retro font and cursor.

Install this package to build a desktop (Avalonia) text adventure; it pulls in `Faerie.Terminal` and
`Faerie` transitively. Bundled fonts: IBM PC video modes, a few PC compatibles, BBC Master/teletext, and 8-bit classics
(Apple II, C64, Spectrum, Atari, TRS-80) — see `BuiltInTerminalFont`. Use
`.WithFont(BuiltInTerminalFont.IbmVga8x16)` or pass a custom font spec string.

## Fluent launcher (minimal Program.cs)

`Application` is Avalonia's app-host type. `AvaloniaLauncher` removes the need to hand-roll that
boilerplate in each game:

```csharp
[STAThread]
public static int Main(string[] args) =>
    AvaloniaLauncher.For(MyGame.Build)
        .WithHeadlessFromArgs()
        .WithDefaultAppDataSaveCatalog()
        .Run(args);
```

Custom headless trigger (predicate + options factory):

```csharp
[STAThread]
public static int Main(string[] args) =>
    AvaloniaLauncher.For(MyGame.Build)
        .WithHeadlessRunnerWhen(
            shouldRunHeadless: a => a.Contains("--headless"),
            optionsFactory: _ => new HeadlessOptions
            {
                ScriptPath = "tests\\smoke.txt",
                TranscriptPath = "tests\\smoke.out.txt"
            })
        .Run(args);
```

## Fluent display host

`AvaloniaDisplay` wraps the usual Avalonia host boilerplate (window creation, terminal wiring, host
bridge, and quit hookup) while keeping the game engine itself separate and UI-agnostic.

From a game:

```csharp
Game game = MyGame.Build();
_ = AvaloniaDisplay.For(game)
    .WithDefaultAppDataSaveCatalog()
    .AttachToDesktop(desktop);
```

From an existing engine instance:

```csharp
TerminalBuffer buffer = new(80, 25, new TextStyle(TerminalColor.LightGray, TerminalColor.Black));
GameEngine engine = new(game, buffer);

_ = AvaloniaDisplay.For(engine)
    .WithDefaultAppDataSaveCatalog()
    .AttachToDesktop(desktop);
```

If you need a deterministic random seed, create the engine yourself (`new GameEngine(game, buffer, seed)`)
and use `AvaloniaDisplay.For(engine)`.

Advanced hooks:

- `WithWindow(...)` to supply your own `TerminalWindow`.
- `WithDefaultAppDataSaveCatalog(...)` to derive save-folder and base-name defaults from the game title.
- `WithPromptMarkup(...)` to override the fallback prompt.
- `ApplyGameWindowChrome(false)` / `ApplyGameTerminalStyle(false)` to opt out of game metadata defaults.
- `ConfigureEngine(...)` / `WithSaveCatalog(...)` for custom engine wiring before host build.
