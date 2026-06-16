# Feature Backlog

**Tracked on [GitHub Issues](https://github.com/KiwiGeek/Faerie/issues).** This file is an index; detail lives in each issue body.

Filter by label:

| Label | Meaning |
| --- | --- |
| [`engine`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aengine+is%3Aopen) | Faerie core engine |
| [`zork1`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Azork1+is%3Aopen) | Zork I sample port fidelity |
| [`avalonia`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aavalonia+is%3Aopen) | Avalonia front end |
| [`console`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aconsole+is%3Aopen) | Console front end |
| [`deficiency`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Adeficiency+is%3Aopen) | Genuine engine gap |
| [`convenience`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aconvenience+is%3Aopen) | Engine helper (doable today, worth first-class API) |
| [`content`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Acontent+is%3Aopen) | Sample content — engine is sufficient |
| [`parser`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aparser+is%3Aopen) | Parser and command language |
| [`world-model`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aworld-model+is%3Aopen) | Rooms, things, exits |
| [`systems`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Asystems+is%3Aopen) | Daemons, scoring, combat, timers |
| [`presentation`](https://github.com/KiwiGeek/Faerie/issues?q=label%3Apresentation+is%3Aopen) | Terminal UI, graphics, sound |

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

### ENGINE — world model ([#4](https://github.com/KiwiGeek/Faerie/issues/4)–[#6](https://github.com/KiwiGeek/Faerie/issues/6), [#8](https://github.com/KiwiGeek/Faerie/issues/8)–[#9](https://github.com/KiwiGeek/Faerie/issues/9))

Encumbrance; vehicles and water rooms; `OnDrop`; paired mirrors; pass-through exits.

### ENGINE — systems ([#10](https://github.com/KiwiGeek/Faerie/issues/10)–[#21](https://github.com/KiwiGeek/Faerie/issues/21))

Scope queries; schedulers; NPC helpers; hazards; scoring; death hooks; soft-death; optional combat module; mood/fluids; random encounters.

### ZORK1 — port fidelity ([open `zork1` issues](https://github.com/KiwiGeek/Faerie/issues?q=label%3Azork1+is%3Aopen))

Infocom behaviors simplified in the sample. Each issue cross-links related ENGINE work where applicable.
See `src/Faerie.Samples.Zork/AGENTS.md` for agent guidance and `ZorkSimplifications` / `ENGINE-LIMIT` markers in source.

### AVALONIA — presentation ([#40](https://github.com/KiwiGeek/Faerie/issues/40)–[#42](https://github.com/KiwiGeek/Faerie/issues/42))

Save/load browser; in-window graphics; sound/music.

### CONSOLE ([#43](https://github.com/KiwiGeek/Faerie/issues/43))

`Faerie.Terminal.Console` front end.
