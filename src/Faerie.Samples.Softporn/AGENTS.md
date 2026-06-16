# AGENTS.md — Softporn Adventure Sample

Guidance for AI agents working on this port.

## Goal

**Exact recreation** of Chuck Benton's 1981 *Softporn Adventure* (On-Line Systems / Sierra). Player-facing text is **verbatim** from the original message file (`Data/softporn.txt`, derived from the Applesoft / Paul Schlyter Pascal tree). Puzzle logic is ported from [xandark/softporn-modern-port](https://github.com/xandark/softporn-modern-port).

Do not paraphrase room descriptions, responses, or help text. Do not replace `FUCK` with euphemisms.

## Architecture

This sample does **not** use `GameBuilder` / `GameEngine` for gameplay. It runs a standalone Pascal-faithful interpreter:

| Path | Role |
| --- | --- |
| `Interpreter/SoftpornHost.cs` | Main loop (`softporn_adventure.pas`) |
| `Interpreter/SoftpornVerbs.cs` | Verb handlers (`softporn-4/5/6.inc.pas`) |
| `Interpreter/SoftpornHelpers.cs` | `look_around`, parser, casino, death (`softporn-2/3.inc.pas`) |
| `Interpreter/SoftpornEnums.cs` | Places, objects, verbs, map tables (`softporn-1.inc.pas`) |
| `Interpreter/SoftpornState.cs` | `gamepos` state + JSON `.SAV` files |
| `Interpreter/SoftpornMessages.cs` | Long messages from embedded `softporn.txt` |
| `Data/softporn.txt` | Original long prose (messages 1–72) |
| `SoftpornHeadless.cs` | `--script` / `-o` replay |
| `SoftpornTerminalIo.cs` | Avalonia `TerminalBuffer` bridge |

Faerie projects referenced only for **terminal UI** (`Faerie.Terminal.Avalonia`) and **headless replay** (`Faerie.Terminal.Headless`).

## Engine gaps (GitHub issues)

Fidelity required a custom loop. If this were built on `GameEngine`, these would block:

| Issue | Gap |
| --- | --- |
| [#71](https://github.com/KiwiGeek/Faerie/issues/71) | Mid-turn `readln` / `read_key` prompts (password, taxi, TV, blackjack, rubber options, purgatory) |
| [#72](https://github.com/KiwiGeek/Faerie/issues/72) | Multiple commands per line (comma/period separated) |
| [#73](https://github.com/KiwiGeek/Faerie/issues/73) | Named `SAVE` / `RESTORE` slots (`SOFTP*.SAV`) |
| [#74](https://github.com/KiwiGeek/Faerie/issues/74) | Sierra per-turn room banner (title, items in sight, exits, `===`) |

## Known interpreter deviations

Document any new deviation here when fixing bugs:

- **CRT layout** — no `clrscr` / `gotoXY`; linear scrollback terminal (Pascal `$define omit_extra_newlines` behaviour).
- **RNG** — `System.Random`, not Turbo Pascal `random` sequence.
- **Save format** — JSON state files named `SOFTP*.SAV`, not binary `gamepos` records.
- **Messages** — plain `softporn.txt`, not obfuscated `softporn.msg` / `pred(c)` encoding.

## Running

```bash
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj -- --script scripts/softporn-smoketest.txt -o -
```

Headless scripts must include **prompt answers** in order (instructions y/n, bellybutton password, taxi destination, blackjack hits, etc.).

## Related

- [#58](https://github.com/KiwiGeek/Faerie/issues/58) — sample tracking issue
- `../Faerie.Samples.Zork` — full `GameBuilder` port (different style)
