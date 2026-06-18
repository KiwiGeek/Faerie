# Faerie

A fluent C# engine for building old-school **text adventures / interactive fiction**, with a retro
"fake terminal" front end (Avalonia, so it runs on Windows, macOS and Linux). Define a world in a few
lines of strongly-typed C#, and Faerie renders it as a glowing character-cell screen — blink, colour,
scrollback, zoom and all.

> *Faerie* — a water-faerie of folklore, and the warm orange glow of a Faerie display tube. Apt for an
> engine whose whole job is glowing retro text.

The engine ships as an *empty box*: it assumes nothing about your world and comes with **no** active
verbs until you ask for them. You switch on whole families of built-in behaviour with one-liners like
`.AddMovement()` or the omnibus `.AddStandardVerbs()`, then define rooms, items and creatures and
wire them together **by C# reference** — there are no magic strings anywhere in a game definition.

Two complete sample games are included: *The Haunted House* (an original retelling in the spirit of the
Usborne type-in classic) and a full *Zork I* port.

## Solution layout

| Project | What it is |
| --- | --- |
| `src/Faerie` | The whole package: the UI-agnostic engine (`Faerie.Model` / `.Building` / `.Verbs` / `.Parsing` / `.Runtime` / `.Presentation`) **and** the Avalonia fake terminal (`Faerie.Terminal`). |
| `src/Faerie.Terminal` | Shared terminal model (`TerminalBuffer`, `ITerminal`). |
| `src/Faerie.Terminal.Avalonia` | Avalonia fake-terminal front end. |
| `src/Faerie.Terminal.Headless` | Script replay + transcript logging (no UI). |
| `src/Faerie.Samples.HauntedHouse` | A small, fully-worked sample game + Avalonia app host. |
| `src/Faerie.Samples.Zork` | A complete Zork I port (see its `AGENTS.md` for the engine-gap list it tracks). |
| `tests/Faerie.Tests` | xUnit tests for the engine (parser, verbs, world, save/load). |

The engine half of Faerie knows nothing about Avalonia: it writes through an `ITerminal` interface and
is fed input by whatever host drives it, so the same game logic runs in the fake terminal or a
headless test. The engine and terminal live in **one NuGet package** for simplicity — referencing
`Faerie` brings in Avalonia, which is the intended (and only) front end.

## Building and running

Requires the **.NET 10 SDK**.

```
dotnet build Faerie.sln
dotnet run --project src/Faerie.Samples.HauntedHouse
dotnet run --project src/Faerie.Samples.Zork
dotnet test
```

### Headless script replay

Run a game without opening a window — feed commands from a file and capture a plain-text transcript:

```
dotnet run --project src/Faerie.Samples.Zork -- --script commands.txt --transcript session.txt
```

Use `-` for stdin/stdout:

```
type commands.txt | dotnet run --project src/Faerie.Samples.Zork -- --script - -o -
```

Blank lines and `#` / `;` comment lines are skipped. Each command is logged as `> command` in the transcript. Omit `--script` to launch the Avalonia UI as usual. See `src/Faerie.Terminal.Headless/README.md`.

> Package versions in the `.csproj` files (Avalonia 11.3.0, the xUnit/test-SDK versions) are
> reasonable defaults — if `restore` complains, bump them to the latest your SDK offers.

### Controls

Type commands and press Enter. `F11` (or `Alt+Enter`) toggles fullscreen. The mouse wheel (or
`PageUp`/`PageDown`) scrolls back through history; **`Ctrl` + mouse wheel** (or `Ctrl` `+` / `Ctrl` `-`
/ `Ctrl` `0`) zooms the font. The text grid fills the window — it is not a fixed 80×25, so a bigger or
fullscreen window gives more columns and rows, and a smaller font gives even more. Because the
terminal stores logical lines and wraps on demand, resizing and zooming **re-paginate** the text. The
mouse pointer is drawn as an inverted character cell rather than the OS cursor. Try `HELP` in-game.

### Fonts

`Faerie.Terminal.Avalonia` bundles **20** curated retro terminal fonts selectable via
`BuiltInTerminalFont` and `.WithFont(BuiltInTerminalFont.IbmVga8x16)`. License files live in
`Assets/Fonts/`. Games can still pass a custom `WithFont(spec)` string.

| Built-in | Machine / use | Source | License |
|----------|---------------|--------|---------|
| `IbmBios8x8` | PC BIOS 8×8 | [int10h.org](https://int10h.org/oldschool-pc-fonts/) | CC BY-SA 4.0 |
| `IbmCga8x8` | IBM CGA | int10h.org | CC BY-SA 4.0 |
| `IbmMda9x14` | IBM MDA 80-col | int10h.org | CC BY-SA 4.0 |
| `IbmEga8x14` | IBM EGA | int10h.org | CC BY-SA 4.0 |
| `IbmVga8x14` | IBM VGA 8×14 | int10h.org | CC BY-SA 4.0 |
| `IbmVga8x16` | IBM VGA 80×25 | int10h.org | CC BY-SA 4.0 |
| `IbmVga9x16` | IBM VGA 9-dot | int10h.org | CC BY-SA 4.0 |
| `Tandy10008x16` | Tandy 1000 | int10h.org | CC BY-SA 4.0 |
| `AmstradPc` | Amstrad PC1512 | int10h.org | CC BY-SA 4.0 |
| `DecRainbow80Col` | DEC Rainbow 80-col | int10h.org | CC BY-SA 4.0 |
| `Kaypro2k` | Kaypro 2000 | int10h.org | CC BY-SA 4.0 |
| `BbcMaster512` | BBC Master MOS 8×8 | int10h.org | CC BY-SA 4.0 |
| `BbcTeletext` | BBC Mode 7 teletext | [Teletext50](https://galax.xyz/Teletext50/) | Public domain |
| `AppleIIe` | Apple II 40-col | [Kreative Software](https://www.kreativekorp.com/software/fonts/apple2/) | Free Use 1.2f |
| `AppleII80Column` | Apple II 80-col | Kreative Software | Free Use 1.2f |
| `Commodore64` | C64 PETSCII | Kreative Software | Free Use 1.2f |
| `ZxSpectrum` | ZX Spectrum | Kreative Software | Free Use 1.2f |
| `Atari8Bit` | Atari 8-bit | Kreative Software | Free Use 1.2f |
| `AtariSt` | Atari ST | Kreative Software | Free Use 1.2f |
| `Trs80CoCo` | TRS-80 Color Computer | Kreative Software | Free Use 1.2f |

To swap fonts, edit `scripts/builtin-fonts.manifest.json` and run
`python scripts/fetch-builtin-fonts.py` (downloads, prunes extras, regenerates the enum).
See also [`src/Faerie.Terminal.Avalonia/Assets/Fonts/FONTS.md`](src/Faerie.Terminal.Avalonia/Assets/Fonts/FONTS.md)
for a per-font reference.

### Guides

- [`docs/AUTHORING.md`](docs/AUTHORING.md) — a complete, beginner-friendly, build-a-game-from-scratch
  walkthrough: rooms, items, scenery, containers, NPCs, custom verbs, synonyms, reactions, flags,
  timers, win/lose conditions, doors, dark rooms, the bars, fonts/cursor, the native window title and
  icon, and a full tiny game you can paste and run.
- [`docs/FEATURES.md`](docs/FEATURES.md) — index of the [GitHub issue backlog](https://github.com/KiwiGeek/Faerie/issues) (engine, Zork port, Avalonia, console).

## Quick start: defining a game

```csharp
var b = GameBuilder.Create("Cave of Wonders")
    .AddStandardVerbs()          // movement + core + meta verbs, all at once
    .WithDefaultTitleBar()
    .WithFont(BuiltInTerminalFont.IbmVga8x16)   // bundled IBM VGA, or pass a custom spec string
    .WithCursor(TerminalCursor.Block);

Room cave = b.Room("Cave Mouth").Describe("Daylight spills into a low cave. A tunnel leads east.");
Room tomb = b.Room("Tomb").Describe("A cold burial chamber.").Brief("The tomb. Exits west.").Dark();
cave.East(tomb);                 // reciprocal west exit is created automatically

Thing torch = b.Item("torch").Describe("A pitch-soaked torch.").LightSource(lit: true);
Thing gem   = b.Item("ruby").Adjectives("red").Describe("A fist-sized ruby.");
cave.Contains(torch);
tomb.Contains(gem);

b.On(gem).After(b.Verbs.Take!, ctx => { ctx.State.Score += 10; return VerbResult.Pass; });

b.StartIn(cave);
Game game = b.Build();
```

Everything that links objects together — exits, container contents, which key opens which lock, which
verb a thing reacts to — takes the actual object, never a name.

### What the verb modules give you

`AddMovement()` adds travel (and bare directions like `n`, `se`, `up`). `AddCoreVerbs()` adds look,
examine, search, inventory, take, drop, open/close, lock/unlock, put (in/on), push/move, read,
wear/remove, eat/drink, switch on/off, give and use. `AddMetaVerbs()` adds help, score, save, restore
and quit. `AddStandardVerbs()` is just all three. You can also `DefineVerb(...)` your own.

The parser is not limited to "verb noun": it understands bare verbs (`look`), bare directions (`n`),
transitive forms (`take key`, `give bone`) and ditransitive forms joined by a preposition (`give bone
to dog`, `put coin in slot`, `unlock door with key`). The pronoun `it` (and `them`) refers to the
last-mentioned thing; `take all` / `drop all` act on every applicable object; compound lists work
too (`drop sword and lamp`, or `drop sword, lamp`). Classic meta commands: `AGAIN` / `G` repeats
the last action, and `UNDO` reverses the last turn. The Avalonia front end also recalls prior
lines with the Up/Down arrow keys. A word that is both a movement and an object
verb disambiguates on its argument — `move north` travels, `move rug` acts on the rug. When several
object verbs share the same word (e.g. core `push` and your own `move` both accept `move`), the
**last-registered** verb wins — define custom verbs after `AddCoreVerbs()` to override standard ones.
Every room,
item and creature can declare any number of synonyms (`.Called(...)`) and adjectives (`.Adjectives(...)`).

### Reacting to actions and the passage of time

`b.On(thing).Before(verb, handler)` lets a thing intercept a verb (return `VerbResult.Done` to override
the default behaviour, `Pass` to let it proceed). `b.On(thing).After(...)` runs afterward. Rooms have
`OnEnter` / `OnFirstEnter` / `OnTurn` hooks, and `b.EveryTurn(...)` / `b.AtTurn(n, ...)` register world
daemons (a roaming monster, a candle burning down, a timed trap).

### Rooms: full and brief descriptions

`Describe(...)` is the full room text shown on the first visit and whenever the player `LOOK`s.
`Brief(...)` is an optional shorter line shown on re-entry to an already-visited room.

### The fake terminal

The screen is a true-colour character grid where every cell has its own foreground/background colour
plus **blink, underline, bold and inverse** attributes. Colours are full 24-bit RGB — the classic
16-colour VGA palette is provided for convenience but you are not limited to it. Styled output uses a
small markup language:

```
ctx.Say("You found the {fg:gold}{bold}idol{/}{/}! {blink}Run!{/}");
```

A non-scrolling **title bar** (top) and an optional **status bar** (bottom) are driven by callbacks
from the game definition and can be styled arbitrarily:

```csharp
b.WithDefaultTitleBar();                       // game name + score/turns
b.WithStatusBar(ctx => new BarContent {
    Left  = $" {ctx.CurrentRoom.Name}",
    Right = "F11 fullscreen ",
    Style = new TextStyle(TerminalColor.White, TerminalColor.Blue)
});
```

The scrolling text region resizes itself to fit whichever bars are enabled, and the whole grid
re-paginates when the window or font size changes.

### The native window

A game can set the OS window title and icon: `b.WithWindowTitle("...")` and
`b.WithWindowIcon("avares://MyGame/Assets/icon.ico")` (or a file path).

### Saving

`SaveSystem` serialises the full mutable game state to JSON (thing locations, mutable attributes, turn
count, score, current room, global puzzle flags, daemon firing, win/lose outcome). The samples wire
`SAVE`/`RESTORE` to a slot under the user's local app-data folder; a host can point the engine's
`WriteSave`/`ReadSave` hooks anywhere it likes. Keep puzzle flags as **global** `State<T>` keys (as the
samples do) so they are captured in saves.

## The samples

**The Haunted House** — night has caught you at the gates of a derelict mansion. Find a light, keep
the silver crucifix between you and what sleeps in the cellar, recover the golden idol, and escape
through the iron gate. It exercises most of the engine (dark rooms and light sources, locked doors,
containers, a deadly creature gated by an item, a roaming ghost daemon, scoring, both bars) in one
readable file — a good template to copy.

**Zork I** — a full port of the 1980 classic's map, treasures and win path. Its `AGENTS.md` and
[GitHub issues labelled `zork1`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Azork1) track where Infocom behaviours are simplified.

## Roadmap

See [`docs/FEATURES.md`](docs/FEATURES.md) and [GitHub Issues](https://github.com/KiwiGeek/Faerie/issues). Highlights: parser improvements, engine world-model APIs, Zork I fidelity, Avalonia graphics/sound/save browser, and `Faerie.Terminal.Console`.
