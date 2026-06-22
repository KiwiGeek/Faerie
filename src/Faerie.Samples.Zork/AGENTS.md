# AGENTS.md — Zork I Sample

Guidance for AI agents working on the Zork port in this project.

## Related projects

| Path | Role |
| --- | --- |
| `../Faerie` | The UI-agnostic engine: world model, fluent builder, verbs, parser, runtime, output filters, save/load, and the `Faerie.Presentation.ITerminal` contract (`Faerie.*`). No UI dependency. |
| `../Faerie.Terminal` | Shared, UI-agnostic terminal model — `TerminalBuffer` (the character-cell framebuffer); implements `ITerminal`. Reused by every front end. |
| `../Faerie.Terminal.Avalonia` | Avalonia "fake terminal" front end (the on-screen control, window, font loader, game host). |
| `../Faerie.Samples.HauntedHouse` | Original sample adventure |
| `../../docs/AUTHORING.md` | Authoring guide for new games |
| `../../docs/FEATURES.md` | Index of [GitHub Issues](https://github.com/KiwiGeek/Faerie/issues) |

When changing the engine to close a gap, prefer extending existing patterns (`GameBuilder`, `Attr`, `ReactionTable`, `Exit.Condition`) over ad-hoc logic in this sample.

---

## Zork I port: backlog on GitHub

Simplified Infocom behaviors are tracked in [#104](https://github.com/KiwiGeek/Faerie/issues/104) (fidelity tracker) and [open **ZORK1:** issues](https://github.com/KiwiGeek/Faerie/issues?q=label%3Azork1+is%3Aopen). In-source markers: `ZorkSimplifications` and `// ENGINE-LIMIT:` comments.

**Winpath oracle:** `scripts/zork1-winpath.txt` (`--seed 1`) reaches **350/350** and stone barrow win as of [#105](https://github.com/KiwiGeek/Faerie/pull/105). Regression: `ZorkWalkthroughTests`.

### Walkthrough failure triage

When `ZorkWalkthroughTests` or a headless replay fails:

1. **Read the transcript** at the failing command (`scripts/zork1-winpath.transcript.txt` if you ran with `-o`). Compare output to Infocom expectations, not to a guessed fix.
2. **Classify the gap** using the list above: `content` (missing puzzle/verb in this sample), `convenience` (awkward but doable with current APIs), or `deficiency` (needs an engine feature).
3. **Do not patch the walkthrough** to skip a puzzle unless the original Infocom game allows that shortcut. Prefer fixing the port.
4. **Add a focused test** when the fix is non-obvious (`Zork*Tests`, `DamPuzzleTests`, `ZorkConcealedRevealTests`, …) so the behavior stays locked without re-running the full script every time.
5. **Check `ENGINE-LIMIT` / `ZorkSimplifications`** before “fixing” something that is intentionally simplified; update those markers when fidelity improves.

Headless replay:

```powershell
dotnet run --project src/Faerie.Samples.Zork/Faerie.Samples.Zork.csproj -- --script scripts/zork1-winpath.txt -o scripts/zork1-winpath.transcript.txt --seed 1
```

### Reality check: engine gap vs unimplemented content

**Do not assume every simplification needs engine work.** After review, most items are either:

1. **Genuine engine deficiency** — hard to express cleanly without a new API (rare).
2. **Engine convenience** — fully doable today (`EveryTurn`, `StateKey`, `Exit.Condition`, …) but worth a reusable helper ([open `engine` issues](https://github.com/KiwiGeek/Faerie/issues?q=label%3Aengine+is%3Aopen) labelled `convenience` or `deficiency`).
3. **Unimplemented Zork content** — the engine can already express it; the sample has not written it yet (label `content`).

### Issue map (AGENTS § → GitHub)

| § | Topic | ZORK1 issue | ENGINE issue(s) |
| --- | --- | --- | --- |
| 1 | Encumbrance | **done** ([#22](https://github.com/KiwiGeek/Faerie/issues/22), [#4](https://github.com/KiwiGeek/Faerie/issues/4)) | — |
| 2 | Boat / river | **done** — [#23](https://github.com/KiwiGeek/Faerie/issues/23) | [#5](https://github.com/KiwiGeek/Faerie/issues/5) (optional engine polish) |
| 3 | Combat fidelity | **done** — [#24](https://github.com/KiwiGeek/Faerie/issues/24) | [#18](https://github.com/KiwiGeek/Faerie/issues/18) (optional module) |
| 5 | Thief | **done** — [#25](https://github.com/KiwiGeek/Faerie/issues/25) | [#12](https://github.com/KiwiGeek/Faerie/issues/12), [#13](https://github.com/KiwiGeek/Faerie/issues/13) |
| 6 | Dam / flood | [#26](https://github.com/KiwiGeek/Faerie/issues/26) | [#11](https://github.com/KiwiGeek/Faerie/issues/11) |
| 7 | Gas room | **done** — [#27](https://github.com/KiwiGeek/Faerie/issues/27) | [#14](https://github.com/KiwiGeek/Faerie/issues/14) |
| 8 | Mirrors | **done** — [#28](https://github.com/KiwiGeek/Faerie/issues/28) | [#8](https://github.com/KiwiGeek/Faerie/issues/8) |
| 9 | Hades ritual | **done** — [#29](https://github.com/KiwiGeek/Faerie/issues/29) | [#11](https://github.com/KiwiGeek/Faerie/issues/11) |
| 10 | Grating pass-through | **done** — [#30](https://github.com/KiwiGeek/Faerie/issues/30) | [#9](https://github.com/KiwiGeek/Faerie/issues/9) |
| 12 | Scoring | **done** — [#32](https://github.com/KiwiGeek/Faerie/issues/32) | [#15](https://github.com/KiwiGeek/Faerie/issues/15) |
| 13 | Death scatter | **done** — [#33](https://github.com/KiwiGeek/Faerie/issues/33) | [#16](https://github.com/KiwiGeek/Faerie/issues/16) |
| 16 | Cyclops | **done** — [#35](https://github.com/KiwiGeek/Faerie/issues/35) | [#20](https://github.com/KiwiGeek/Faerie/issues/20) |
| 17 | Bat | **done** — [#36](https://github.com/KiwiGeek/Faerie/issues/36) | (game code only) |
| 18 | Sand collapse | **done** — [#37](https://github.com/KiwiGeek/Faerie/issues/37) | (game code only) |
| 19 | Verbs | **done** — [#38](https://github.com/KiwiGeek/Faerie/issues/38) | [#19](https://github.com/KiwiGeek/Faerie/issues/19) |
| 20 | Magic / gnome / passage | **done** — [#39](https://github.com/KiwiGeek/Faerie/issues/39) | [#21](https://github.com/KiwiGeek/Faerie/issues/21), [#4](https://github.com/KiwiGeek/Faerie/issues/4) |

---

## Working on this sample

1. Read `ZorkWorld.Simplifications.cs` and follow `ENGINE-LIMIT` comments before "fixing" simplified behavior.
2. Check the linked GitHub issue for classification (`content` vs `deficiency` vs `convenience`).
3. Do **not** paper over gaps with huge bespoke logic unless the engine gains a reusable feature.
4. When an issue is closed, update `ZorkSimplifications`, remove or narrow `ENGINE-LIMIT` comments, and close the cross-linked issue if applicable.
5. Keep all Zork-specific content in this project unless explicitly asked to change the engine.

## Working on the engine

- This sample should remain the proof point: extend engine APIs, then **reduce** simplifications here.
- Preserve fluent, reference-wired style (no magic strings between objects).
- Run `dotnet test` and build both samples after engine changes.
