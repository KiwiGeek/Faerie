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
- `WithTuiWindowChrome(false)` to keep the native OS title bar instead of the in-TUI window chrome.
- `ConfigureEngine(...)` / `WithSaveCatalog(...)` for custom engine wiring before host build.

## Borderless, fully-TUI window chrome

By default the window is borderless (`SystemDecorations="None"`): the whole window is the character
grid, and the window controls are drawn *inside* the TUI on row 0 (the title row). Placement follows
the host OS — a red/yellow/green traffic-light cluster at the top-left on macOS, and
minimize/maximize/close at the top-right on Windows and Linux.

- **Move**: drag the title row (anywhere except the buttons). Double-click the title row to
  maximize/restore.
- **Resize**: drag any edge or corner (the outer few pixels); the cursor changes to a resize arrow.
- **Controls**: click the drawn buttons to minimize, maximize/restore, or close.
- **Cell-snapped sizing**: in the normal (non-maximized) state the window snaps to a whole number of
  character cells, so there is never a black border between the window edge and the text. Zooming
  (`Ctrl`+`+`/`-`/`0`, or `Ctrl`+mouse wheel) keeps the window as close to its current size as possible
  and reflows the grid to the new font size, snapping back to the nearest whole cell. A centered border
  appears only when maximized or fullscreen (`F11`), to preserve font fidelity.
- **Title bar**: the controls sit on the title row and never overlap the game's title text (the engine
  composes the bar clear of the reserved button columns). If a game defines no title bar of its own,
  the window title is drawn there by default so the row still reads as a title bar.
- **Glyphs**: the buttons use `●` traffic-light dots (macOS) and `_`/`□`/`×` (Windows/Linux), each with
  an ASCII fallback (`O`/`#`/`x`) for retro fonts that lack the Unicode form.

Preview the other platform's styling without changing OS by setting the `FAERIE_WINDOW_CHROME`
environment variable to `macos`, `windows`, or `linux` before launching.

Opt out with `.WithTuiWindowChrome(false)` on the display builder, or `TerminalWindow.UseTuiWindowChrome
= false`, to restore the native OS title bar and borders.
