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

Simplified Infocom behaviors are tracked as **ZORK1:** issues ([#22](https://github.com/KiwiGeek/Faerie/issues/22)–[#39](https://github.com/KiwiGeek/Faerie/issues/39)), with related **ENGINE:** issues cross-linked in each body. In-source markers: `ZorkSimplifications` (`ZorkWorld.Simplifications.cs`) and `// ENGINE-LIMIT:` comments.

### Reality check: engine gap vs unimplemented content

**Do not assume every simplification needs engine work.** After review, most items are either:

1. **Genuine engine deficiency** — hard to express cleanly without a new API (rare; output filtering was the main example and is **done**).
2. **Engine convenience** — fully doable today (`EveryTurn`, `StateKey`, `Exit.Condition`, …) but worth a reusable helper ([#4](https://github.com/KiwiGeek/Faerie/issues/4)–[#21](https://github.com/KiwiGeek/Faerie/issues/21) label `convenience` or `deficiency`).
3. **Unimplemented Zork content** — the engine can already express it; the sample has not written it yet (label `content`).

Combat ([#24](https://github.com/KiwiGeek/Faerie/issues/24) + lingering fidelity), Loud Room, and sword glow are **implemented**. Do not re-open them as engine gaps.

### Issue map (AGENTS § → GitHub)

| § | Topic | ZORK1 issue | ENGINE issue(s) |
| --- | --- | --- | --- |
| 1 | Encumbrance | [#22](https://github.com/KiwiGeek/Faerie/issues/22) | [#4](https://github.com/KiwiGeek/Faerie/issues/4) |
| 2 | Boat / river | [#23](https://github.com/KiwiGeek/Faerie/issues/23) | [#5](https://github.com/KiwiGeek/Faerie/issues/5) |
| 3 | Combat | [#24](https://github.com/KiwiGeek/Faerie/issues/24) (fidelity) | [#18](https://github.com/KiwiGeek/Faerie/issues/18) (optional module); core **done** |
| 4 | Loud Room | — | **done** (output filter) |
| 5 | Thief | [#25](https://github.com/KiwiGeek/Faerie/issues/25) | [#12](https://github.com/KiwiGeek/Faerie/issues/12), [#13](https://github.com/KiwiGeek/Faerie/issues/13) |
| 6 | Dam / flood | [#26](https://github.com/KiwiGeek/Faerie/issues/26) | [#11](https://github.com/KiwiGeek/Faerie/issues/11) |
| 7 | Gas room | [#27](https://github.com/KiwiGeek/Faerie/issues/27) | [#14](https://github.com/KiwiGeek/Faerie/issues/14) |
| 8 | Mirrors | [#28](https://github.com/KiwiGeek/Faerie/issues/28) | [#8](https://github.com/KiwiGeek/Faerie/issues/8) |
| 9 | Hades ritual | [#29](https://github.com/KiwiGeek/Faerie/issues/29) | [#11](https://github.com/KiwiGeek/Faerie/issues/11) |
| 10 | Grating pass-through | [#30](https://github.com/KiwiGeek/Faerie/issues/30) | [#9](https://github.com/KiwiGeek/Faerie/issues/9) |
| 11 | Grating light | [#31](https://github.com/KiwiGeek/Faerie/issues/31) | [#7](https://github.com/KiwiGeek/Faerie/issues/7) |
| 12 | Scoring | [#32](https://github.com/KiwiGeek/Faerie/issues/32) | [#15](https://github.com/KiwiGeek/Faerie/issues/15) |
| 13 | Death scatter | [#33](https://github.com/KiwiGeek/Faerie/issues/33) | [#16](https://github.com/KiwiGeek/Faerie/issues/16) |
| 14 | Egg / tree drop | [#34](https://github.com/KiwiGeek/Faerie/issues/34) | [#6](https://github.com/KiwiGeek/Faerie/issues/6) |
| 15 | Sword glow | — | **done** (`ctx.RoomOf`) |
| 16 | Cyclops | [#35](https://github.com/KiwiGeek/Faerie/issues/35) | [#20](https://github.com/KiwiGeek/Faerie/issues/20) |
| 17 | Bat | [#36](https://github.com/KiwiGeek/Faerie/issues/36) | (game code only) |
| 18 | Sand collapse | [#37](https://github.com/KiwiGeek/Faerie/issues/37) | (game code only) |
| 19 | Verbs | [#38](https://github.com/KiwiGeek/Faerie/issues/38) | [#19](https://github.com/KiwiGeek/Faerie/issues/19) |
| 20 | Magic / gnome / passage | [#39](https://github.com/KiwiGeek/Faerie/issues/39) | [#21](https://github.com/KiwiGeek/Faerie/issues/21), [#4](https://github.com/KiwiGeek/Faerie/issues/4) |

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
