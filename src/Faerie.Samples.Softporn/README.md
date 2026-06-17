# Softporn Adventure (sample)

A port of Chuck Benton's 1981 *Softporn Adventure* (On-Line Systems / Sierra) on the Faerie
`GameBuilder` engine — the same fluent, reference-wired style as `Faerie.Samples.Zork`.

**This is not an exhaustive recreation.** The author has never played the original game; the sample
exists to exercise Faerie features (Sierra room banner, mid-turn prompts, casino minigames, save
slots, asymmetric exits, and so on) and to provide a second large `GameBuilder` example. Room
descriptions, puzzle logic, and walkthrough fidelity may be incomplete or wrong compared to the
1981 release.

Player-facing text is authored **inline in C#** at the point of definition — there is no external
data file. Rooms and things are first-class fields on `SoftpornWorld`, wired by reference rather
than string ids.

## Layout

| File | Role |
| --- | --- |
| `SoftpornGame.cs` | Entry point: intro, Sierra banner, status bar, start room |
| `SoftpornWorld.cs` | Rooms, things, map connections, state keys |
| `SoftpornWorld.Puzzles*.cs` | Custom verbs and puzzle reactions |
| `SoftpornWorld.Casino.cs` | Slots and blackjack minigames |
| `SoftpornWorld.Helpers.cs` | Shared helpers (money, purgatory, long text, etc.) |
| `App.cs` / `Program.cs` | Avalonia host and headless script runner |

The original four-character parser is not replicated; standard Faerie commands (`look`, `go north`,
`take wallet`, …) are used instead.

## Running

**Graphical (Avalonia):**

```bash
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj
```

**Headless smoke test:**

```bash
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj -- \
  --script scripts/softporn-smoketest.txt -o -
```

**Full win-path script** (for regression; not guaranteed to match the original game):

```bash
dotnet run --project src/Faerie.Samples.Softporn/Faerie.Samples.Softporn.csproj -- \
  --script scripts/softporn-winpath.txt -o -
```

Use the **Softporn Adventure (Avalonia)** launch configuration in `.vscode/launch.json` for
process-attached debugging (requires the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)).

## Related

- [#58](https://github.com/KiwiGeek/Faerie/issues/58) — sample tracking issue
- `../Faerie.Samples.Zork` — sibling port with fuller fidelity goals
