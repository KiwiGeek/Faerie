# Feature Backlog

**Tracked on [GitHub Issues](https://github.com/KiwiGeek/Faerie/issues).** This file is an index; detail lives in each issue body.

Filter by label:

| Label | Meaning |
| --- | --- |
| [`engine`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aengine) | Faerie core engine |
| [`zork1`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Azork1) | Zork I sample port fidelity |
| [`avalonia`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aavalonia) | Avalonia front end |
| [`console`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aconsole) | Console front end |
| [`deficiency`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Adeficiency) | Genuine engine gap |
| [`convenience`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aconvenience) | Engine helper (doable today, worth first-class API) |
| [`content`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Acontent) | Sample content — engine is sufficient |
| [`parser`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aparser) | Parser and command language |
| [`world-model`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aworld-model) | Rooms, things, exits |
| [`systems`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Asystems) | Daemons, scoring, combat, timers |
| [`presentation`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Apresentation) | Terminal UI, graphics, sound |

Title prefixes: **ENGINE:**, **ZORK1:**, **AVALONIA:**, **CONSOLE:**

When an issue is closed, trim the matching `ZorkSimplifications` entry and `ENGINE-LIMIT` comments in the Zork sample (see `AGENTS.md`).

---

## Naming — resolved: **Faerie**

The project is named **Faerie** — a nod to **F**luent **A**dventure **E**ngine ("FAE"). Tagline: *the Fluent Adventure Engine*. NuGet/package layout:

- `Faerie` — the engine / core (UI-agnostic; emits through the `Faerie.Presentation.ITerminal` abstraction).
- `Faerie.Terminal` — shared, UI-agnostic terminal model (`TerminalBuffer`, the character-cell framebuffer);
  implements `ITerminal` and is reused by every front end.
- `Faerie.Terminal.Avalonia` — the Avalonia "fake terminal" front end.
- `Faerie.Terminal.Console` — planned stdout/console front end ([#43](https://github.com/KiwiGeek/Faerie/issues/43)).
- `Faerie.Samples.Zork`, `Faerie.Samples.HauntedHouse`, `Faerie.Tests` — not published.

The bare `Faerie` id is the engine (Serilog/Polly-style discoverability); front ends are `Faerie.Terminal.*`.
Sample apps keep short `AssemblyName`s (`Zork`, `HauntedHouse`) so the avares font URIs (`avares://Zork/...`)
stay stable.

---

## Open issues (by area)

### ENGINE — parser ([#1](https://github.com/KiwiGeek/Faerie/issues/1)–[#3](https://github.com/KiwiGeek/Faerie/issues/3), [#19](https://github.com/KiwiGeek/Faerie/issues/19))

Fuzzy typo suggestions; pronoun / "all" / compound commands; AGAIN/G/OOPS/undo; optional Infocom verb bundle.

### ENGINE — world model ([#4](https://github.com/KiwiGeek/Faerie/issues/4)–[#9](https://github.com/KiwiGeek/Faerie/issues/9))

Encumbrance; vehicles and water rooms; `OnDrop`; dynamic lighting; paired mirrors; pass-through exits.

### ENGINE — systems ([#10](https://github.com/KiwiGeek/Faerie/issues/10)–[#21](https://github.com/KiwiGeek/Faerie/issues/21))

Scope queries; schedulers; NPC helpers; hazards; scoring; death hooks; soft-death; optional combat module; mood/fluids; random encounters.

### ZORK1 — port fidelity ([#22](https://github.com/KiwiGeek/Faerie/issues/22)–[#39](https://github.com/KiwiGeek/Faerie/issues/39))

Infocom behaviors simplified in the sample. Each issue cross-links related ENGINE work where applicable.
See `src/Faerie.Samples.Zork/AGENTS.md` for agent guidance and `ZorkSimplifications` / `ENGINE-LIMIT` markers in source.

### AVALONIA — presentation ([#40](https://github.com/KiwiGeek/Faerie/issues/40)–[#42](https://github.com/KiwiGeek/Faerie/issues/42))

Save/load browser; in-window graphics; sound/music.

### CONSOLE ([#43](https://github.com/KiwiGeek/Faerie/issues/43))

`Faerie.Terminal.Console` front end.

---

## Completed (not open issues)

| Feature | Notes |
| --- | --- |
| Movement vs object disambiguation | Parser picks movement when argument is a direction |
| Multiple room descriptions | `Room.Brief(...)` + verbose/brief in `DescribeCurrentRoom` |
| Turn-by-turn melee combat | Built in Zork game code; `ctx.RoomOf` only engine change |
| Per-room output filter | `OutputWriter.Transform` + `GameBuilder.FilterOutput` |
| `GameContext.RoomOf(Thing)` | Sword glow, combat, proximity daemons |
| Sword proximity glow | `ZorkWorld.DefineSwordGlow` |
| Loud Room echo | `ZorkWorld.DefineLoudRoom` via output filter |
