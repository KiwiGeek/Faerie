# AGENTS.md — Softporn Adventure Sample

Guidance for AI agents working on this port.

## Goal

**Exact recreation** of Chuck Benton's 1981 *Softporn Adventure* (On-Line Systems / Sierra). Player-facing text is **verbatim** from the original message file (`Data/softporn.txt`). Puzzle logic follows the Pascal original; implementation uses **Faerie `GameBuilder` / `GameEngine`** like `Faerie.Samples.Zork`.

**Do not paraphrase** room descriptions, object names, responses, or help text. Original typos and adult commands are intentional.

## Architecture

| Path | Role |
| --- | --- |
| `SoftpornGame.cs` | `GameBuilder` entry: Sierra banner, intro, save slots, start room |
| `SoftpornWorld.cs` + partials | Rooms, things, map, state keys, puzzles |
| `SoftpornMessages.cs` | Parses embedded `softporn.txt` (M1–M72) |
| `SoftpornIds.cs` | Stable room/thing ids for puzzle wiring |
| `Data/softporn.txt` | Original long prose |
| `App.cs` / `Program.cs` | Avalonia host + `HeadlessRunner` |

Engine features demonstrated: `WithSierraRoomBanner()`, `PromptLine` / `PromptKey`, multi-command lines, `SaveSlotCatalog` + `SierraPrefix`, `EveryTurn` daemons, verb/reaction hooks.

The original 4-character parser is **not** replicated; standard Faerie commands (`look`, `go north`, `take wallet`, …) are used instead.

## Running

```bash
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj -- --script scripts/softporn-smoketest.txt -o -
```

## Related

- [#58](https://github.com/KiwiGeek/Faerie/issues/58) — sample tracking issue
- `../Faerie.Samples.Zork` — sibling `GameBuilder` port
