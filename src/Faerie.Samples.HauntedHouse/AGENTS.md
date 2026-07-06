# AGENTS.md — Haunted House Sample

Guidance for AI agents working on the Haunted House port in this project.

## Related projects

| Path | Role |
| --- | --- |
| `../Faerie` | The UI-agnostic engine: world model, fluent builder, verbs, parser, runtime, save/load (`Faerie.*`). |
| `../Faerie.Samples.Zork` | The other sample — much more mature use of the engine (declarative conditional exits, `AddCoreVerbs()` + `b.On(thing).Before/After(...)` reactions, carry limits, grue/darkness via `Scope`). Use it as "the idiomatic way to do X" whenever an issue below says to switch to an engine built-in. Its own `AGENTS.md` documents the same kind of fidelity-audit process this file follows. |
| `../../docs/AUTHORING.md` | Authoring guide for new games — explains the fluent APIs referenced below. |
| `../../docs/FEATURES.md` | Index of the engine's GitHub issue backlog. |
| `scripts/haunted-house-walkthrough.txt` | The fidelity-oracle walkthrough script (Dorothy Millard's C64 walkthrough, transcribed verbatim). Replayed by `tests/Faerie.Tests/HauntedHouseWalkthroughTests.cs`. **Note:** this walkthrough reaches a *suboptimal* win (score 16→32, doubled) — it never picks up the rope. See [#109](https://github.com/KiwiGeek/Faerie/issues/109) — don't treat "the walkthrough still passes" as proof a scoring fix is correct; also reason about the true 17-treasure win path. |

## Source of truth for "authentic"

The original is Usborne's *Write Your Own Adventure Programs for Your Microcomputer* (Jenny Tyler &
Les Howarth), a BASIC type-in listing. The authoritative cross-check for fidelity work is
**`jbanes/haunted`** (https://github.com/jbanes/haunted, file `haunted.c`), a line-for-line C89 port
of the BASIC listing by John Elliott. Fetch that file directly when verifying a fidelity claim; don't
trust a paraphrase, and don't assume a filed issue's quoted snippet is exhaustive — re-check the
surrounding logic in the source before making a change.

The developer intentionally deviated from the original in exactly two ways — **do not "fix" these,
and don't file new issues about them**:
1. Room descriptions were expanded with original flavor prose (the original has only a terse
   one-line room name/status, e.g. `"SLIPPERY STEPS"`).
2. The three staircases (spiral staircase, marble stairs, slippery steps) use real `Up()`/`Down()`
   exits instead of the original's trick of aliasing the `U`/`D` keys to whichever compass direction
   happens to lead the right way at that specific room. The *destinations* those keys lead to must
   still match the original exactly — see #108 for a case where that went wrong.

Everything else should match the original's geography, object placement, puzzle logic, and message
wording as closely as is reasonable in modern prose.

---

## Backlog: GitHub issues (label `hhouse`)

A 2026-07-06 audit (fidelity vs. `jbanes/haunted`, and engine-feature usage vs. `Faerie.Samples.Zork`)
produced the following, all still open as of this writing. Each issue is self-contained — cited
file:line locations, the relevant original-source snippet, and a suggested fix — so start there
rather than re-deriving context here.

Live query: `gh issue list --repo KiwiGeek/Faerie --label hhouse --state open`

| # | Topic | Kind |
| --- | --- | --- |
| [#108](https://github.com/KiwiGeek/Faerie/issues/108) | Slippery Steps up/down destinations swapped | fidelity bug |
| [#109](https://github.com/KiwiGeek/Faerie/issues/109) | Win-score threshold hardcodes 16, should be 17 (boat-only exclusion) | fidelity bug |
| [#110](https://github.com/KiwiGeek/Faerie/issues/110) | Message wording drift (SAY, XZANFAR, axe swing, CARRYING?, punctuation) | fidelity bug (bundle) |
| [#111](https://github.com/KiwiGeek/Faerie/issues/111) | No `LOOK` verb registered | fidelity/content gap |
| [#112](https://github.com/KiwiGeek/Faerie/issues/112) | Open/Unlock hand-rolled instead of engine `Openable`/`Locked` verbs | engine-usage refactor |
| [#113](https://github.com/KiwiGeek/Faerie/issues/113) | Puzzle exits rewired imperatively instead of declared via `Exit.Condition` | engine-usage refactor |
| [#114](https://github.com/KiwiGeek/Faerie/issues/114) | Darkness/light logic duplicated and drifted (misses Dark Alcove) | engine-usage refactor + latent bug |
| [#115](https://github.com/KiwiGeek/Faerie/issues/115) | `WithCarryLimit` never set — encumbrance hint non-functional | engine-usage refactor |
| [#122](https://github.com/KiwiGeek/Faerie/issues/122) | Puzzle verb logic centralized in if-chains instead of per-thing reactions | engine-usage refactor (broader companion to #112/#113) |

None of #112–#115 or #122 require engine changes — in each case the engine already ships the needed feature
(see the closed engine issues cross-referenced from each one, e.g. #4 for carry limits, #9/#31 for the
declarative-exit pattern). These are sample-only fixes: adopt the existing API, don't add a new one.

Cross-referenced from the umbrella tracking issue [#53](https://github.com/KiwiGeek/Faerie/issues/53)
("SAMPLES: Usborne Adventures type-in ports").

---

## Working on this sample

1. Pick an open `hhouse` issue, read it in full — it has the file:line references and original-source
   citation you need.
2. Fidelity bugs (#108–#111) are small and independent; fix them individually. After any fidelity fix,
   re-run `dotnet test` — specifically `HauntedHouseWalkthroughTests` — and read the transcript, don't
   just check the exit code.
3. Engine-usage refactors (#112–#115) touch more surface area; do them as their own reviewed changes,
   one at a time, and re-run the full test suite after each.
4. When an issue is resolved, close it (referencing the commit/PR) rather than leaving it open with a
   stale description.
5. If you find a **new** discrepancy while working here, file a new issue labeled `hhouse` with the
   same structure as the existing ones (classification, summary, cited original-source snippet, file
   references, suggested fix) rather than fixing it silently or logging it only in this file — the
   citation trail in the issue is what makes future audits fast, and this file is meant to stay a thin
   index, not grow back into a full backlog.

## Working on the engine

- This sample should mostly be a *consumer* of existing engine features (see the issue table above) —
  extending the engine specifically for Haunted House should be rare. If you do find a genuine engine
  gap while working here, check `docs/FEATURES.md` and `Faerie.Samples.Zork/AGENTS.md`'s issue map
  first; the same gap may already be tracked (or already closed) from Zork fidelity work.
- Preserve the fluent, reference-wired style (no magic strings between objects).
- Run `dotnet test` and build both samples after any engine change.
