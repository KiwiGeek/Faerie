# AGENTS.md — Haunted House Sample

Guidance for AI agents working on the Haunted House port in this project. This file is the
working backlog from a 2026-07-06 fidelity/architecture audit — read it before touching any of the
files it references, and update it as items are fixed (check the box, note the commit/PR if useful,
delete the item once it's genuinely done and covered by a test).

## Related projects

| Path | Role |
| --- | --- |
| `../Faerie` | The UI-agnostic engine: world model, fluent builder, verbs, parser, runtime, save/load (`Faerie.*`). |
| `../Faerie.Samples.Zork` | The other sample — much more mature use of the engine (declarative conditional exits, `AddCoreVerbs()` + `b.On(thing).Before/After(...)` reactions, carry limits, grue/darkness via `Scope`). Use it as "the idiomatic way to do X" whenever an item below says to switch to an engine built-in. Its own `AGENTS.md` documents the same kind of fidelity-audit process this file follows. |
| `../../docs/AUTHORING.md` | Authoring guide for new games — explains the fluent APIs referenced below. |
| `../../docs/FEATURES.md` | Index of the engine's GitHub issue backlog. |
| `scripts/haunted-house-walkthrough.txt` | The fidelity-oracle walkthrough script (Dorothy Millard's C64 walkthrough, transcribed verbatim). Replayed by `tests/Faerie.Tests/HauntedHouseWalkthroughTests.cs`. **Note:** this walkthrough reaches a *suboptimal* win (score 16→32, doubled) — it never picks up the rope. See item 2 below; don't treat "the walkthrough still passes" as proof a scoring fix is correct, also reason about the true 17-treasure win path. |

## Source of truth for "authentic"

The original is Usborne's *Write Your Own Adventure Programs for Your Microcomputer* (Jenny Tyler &
Les Howarth), a BASIC type-in listing. The authoritative cross-check used for this audit —
and already cited in this sample's existing code comments — is **`jbanes/haunted`**
(https://github.com/jbanes/haunted, file `haunted.c`), a line-for-line C89 port of the BASIC listing
by John Elliott. Fetch that file directly when verifying anything below; don't trust a paraphrase.
Room/object/direction constants referenced below (`ROOM_SLIPPERY`, `DIR_UP`, `OBJECT_COUNT`, etc.)
are from that file.

The developer intentionally deviated from the original in exactly two ways — **do not "fix" these**:
1. Room descriptions were expanded with original flavor prose (the original has only a terse
   one-line room name/status, e.g. `"SLIPPERY STEPS"`).
2. The three staircases (spiral staircase, marble stairs, slippery steps) use real `Up()`/`Down()`
   exits instead of the original's trick of aliasing the `U`/`D` keys to whichever compass direction
   happens to lead the right way at that specific room. This is a deliberate, documented improvement
   (see the comment block above the exits in `HauntedHouseWorld.Rooms.cs`) — the *destinations* must
   still match the original exactly (that's items where this went wrong, see item 1).

Everything else should match the original's geography, object placement, puzzle logic, and message
wording as closely as is reasonable in modern prose. Where this doc says a message is wrong, that's
because it changes actual game text the original player would have seen — not a style nitpick.

---

## Fidelity bugs (confirmed against `jbanes/haunted`)

### 1. [ ] Slippery Steps `Up`/`Down` destinations are swapped

`HauntedHouseWorld.Rooms.cs:221-224`:
```csharp
WidePassage.East(SlipperySteps, reciprocal: false);
SlipperySteps.Up(WidePassage, reciprocal: false);
BarredCellar.North(SlipperySteps, reciprocal: false);
SlipperySteps.Down(BarredCellar, reciprocal: false);
```
Original (`haunted.c`): `ROOM_SLIPPERY` (room 22) has route string `"WSUD"` on an 8-wide grid
(`N=-8, S=+8, W=-1, E=+1`). Literal `W` leads to `ROOM_WPASSAGE` (21, Wide Passage) and literal `S`
leads to `ROOM_CELLAR` (30, Barred Cellar) — both already correctly modeled as the plain compass
exits. The special case is:
```c
case ROOM_SLIPPERY: if (d==DIR_UP) d=DIR_SOUTH; if (d==DIR_DOWN) d=DIR_WEST; break;
```
i.e. `UP` is a redundant alias for `SOUTH` → **Barred Cellar**, and `DOWN` is a redundant alias for
`WEST` → **Wide Passage**. Faerie has it backwards.

**Fix:** swap the two lines — `SlipperySteps.Up(BarredCellar, ...)`, `SlipperySteps.Down(WidePassage, ...)`.
Add/update `HauntedHouseWalkthroughTests` or a small targeted nav test so this can't regress silently
(the existing walkthrough script does traverse this — `u` / `d` around lines 100-101 of
`scripts/haunted-house-walkthrough.txt` — so fixing this and re-running the walkthrough test is a
reasonable first check, but eyeball the transcript, don't just trust green).

### 2. [ ] Win-scoring threshold should be 17 (boat-only exclusion), not a hardcoded 16

`HauntedHouseWorld.State.cs:49-53` (`Treasures` — 18 items, includes `SmallBoat` and `Rope`) and
`HauntedHouseWorld.Verbs.cs:411-442` (`ScoreHandler`):
```csharp
const int allTreasuresExceptBoat = 16;
if (score == allTreasuresExceptBoat && !ctx.Carrying(SmallBoat) && ctx.CurrentRoom != IronGate) { ... }
if (score == allTreasuresExceptBoat && ctx.CurrentRoom == IronGate && !ctx.Carrying(SmallBoat)) {
    int doubled = score * 2;              // 32
    ...
    ctx.Win(...);
}
...
if (score > 18) ctx.Win(...);             // currently dead code — max possible score is 18
```
Original (`haunted.c`), full score loop and win check:
```c
for (s = 0, i = 1; i <= OBJECT_COUNT; i++)   // OBJECT_COUNT == 18 — loop includes the rope (object 14)
    if (gl_state.carried[i]) ++s;
...
if (s == 17 && gl_state.carried[OBJ_BOAT] == 0) {
    if (gl_state.rm == ROOM_GATE) { s *= 2; printf("DOUBLE SCORE FOR REACHING HERE!\n"); }
}
if (s > 18) /* win */
```
The original only excludes the **boat** from the winning count (17 = 18 minus the boat). It does
**not** assume the rope is uncarriable. See item 3 — the rope genuinely can be carried, so a player
who does so is currently unable to ever win (score sits at 17, matches neither the `==16` branch nor
`>18`).

**Fix:** change `allTreasuresExceptBoat` (rename it — the constant name is itself part of the bug,
it should just be "target score", 17) to `17`. This makes the `score > 18` branch live for the first
time (17 doubled = 34), which is correct — no other change needed there.

**Existing test/script note:** `scripts/haunted-house-walkthrough.txt` doesn't pick up the rope, so
it will still top out at 16/32 after this fix and the existing assertions
(`HauntedHouseWalkthroughTests`, expects `"32"` and `"DOUBLE SCORE"`) should still pass unchanged —
this fix doesn't break that path, it just also *unblocks* the true-max path. See item 3 for adding
coverage of that path.

### 3. [ ] Confirm/cover the "pick up the rope" path once item 2 is fixed

`HauntedHouseWorld.Verbs.cs` `ClimbHandler` (lines 289-311) and `SwingHandler` (lines 256-287), object
`Rope` (`HauntedHouseWorld.Things.cs:24`, `StartsIn(BlastedTree)`, a normal takeable treasure — not
`.Concealed()`).

Original (`haunted.c`), `CLIMB ROPE` case:
```c
if (gl_ob == OBJ_ROPE) {
    if (gl_state.carried[OBJ_ROPE]) gl_msg = "IT ISN'T ATTACHED TO ANYTHING";
    else if (gl_state.rm == ROOM_TREE) {
        if (gl_state.flag[OBJ_ROPE]) { gl_msg = "GOING DOWN!"; gl_state.flag[OBJ_ROPE] = 0; }
        else { gl_msg = "YOU SEE THICK FOREST AND CLIFF SOUTH"; gl_state.flag[OBJ_ROPE] = 1; }
    }
}
```
and in the move function, trying to leave the tree mid-climb (flag still set) causes
`"CRASH! YOU FELL OUT OF THE TREE!"` and resets the flag. None of this requires the rope to be
absent from the room or unobtainable afterward — a player can finish the climb sequence (as the
walkthrough does: `climb rope` / `climb rope`) and *then* `get rope` like any other treasure, reaching
the true 17/34 win. Current Faerie `ClimbHandler` mirrors this logic correctly already (two-step
ready/climbing flags, no interaction with carrying state) — the only known bug was the scoring
threshold in item 2, not the climb logic itself.

**Action:** after fixing item 2, manually or via a headless script verify: reach Blasted Tree,
`climb`, `climb`, `get rope`, then complete the walkthrough's remaining treasures, and confirm you
reach 17 and a doubled score of 34 at the gate. Consider adding a second script
(`scripts/haunted-house-walkthrough-maxscore.txt`) and matching test rather than editing the existing
oracle script (which should stay a verbatim transcription of the published walkthrough per the
comment at the top of that file).

### 4. [ ] `SAY` confirmation message text is wrong

`HauntedHouseWorld.Verbs.cs:222`: `ctx.Say($"Ready '{word}'");`
Original: `sprintf(gl_msgbuf, "OK '%s'", gl_noun);` — should be `"OK '{word}'"`.

### 5. [ ] XZANFAR magic-effect message doesn't match either branch

`HauntedHouseWorld.Verbs.cs:219-237` (`SayHandler`). Currently: teleport branch prints a custom
sentence ("You suddenly feel very faint and have to close your eyes...") and the ice-cold-chamber
barrier-lift branch prints nothing extra. Original prints the exact same line, `"*MAGIC OCCURS*"`,
in **both** cases (teleport and barrier-lift) — there is no original message distinguishing them.

**Fix:** print `"*MAGIC OCCURS*"` in both branches. (Judgment call: if you want to keep some of the
added flavor text, you could keep it as a *second* line after `"*MAGIC OCCURS*"` rather than
replacing it outright — but the literal original line should appear either way, since this is a
puzzle-critical message players would search a walkthrough for.)

### 6. [ ] Axe-swing message text is wrong

`HauntedHouseWorld.Verbs.cs:275`: `ctx.Say("Whoooosshhh!");`
Original: `gl_msg = "WHOOSH!";` — fix the text (capitalization doesn't need to match 1980s
all-caps-BASIC-output convention since the rest of this port uses normal case, just fix the spelling).

### 7. [ ] `HELP` verb list shows `CARRYING`, original requires `CARRYING?`

`HauntedHouseWorld.Verbs.cs:27` (`UsborneVerbList`). Original vocabulary table has the literal token
`"CARRYING?"` (the `?` is part of the word the parser expects). Faerie's own `CarryingHandler` is
registered under the word `"carrying"` (no `?`) at line 34, which is fine to leave as-is (Faerie's
parser presumably doesn't want a `?` in a command word) — this item is just about correcting the
**displayed** verb-list text in `HelpHandler` to read `CARRYING?` like the original screen did, purely
cosmetic/textual.

### 8. [ ] Minor: "It will burn your hands!" has an added exclamation point

`HauntedHouseWorld.Verbs.cs:320`. Original: `"IT WILL BURN YOUR HANDS"` (no `!`). Very low priority —
only fix this in the same pass as the other message-wording items above, not worth its own change.

---

## Engine-usage / architecture gaps

These aren't wrong game behavior — the walkthrough passes today — but they mean this sample doesn't
demonstrate (or benefit from) engine features it should, and in a couple of cases papers over an
actual missing feature (`LOOK`) or a latent bug (drifted darkness logic). `HauntedHouseGame.cs:20`
calls only `.AddMovement()` — none of `.AddCoreVerbs()` / `.AddMetaVerbs()` / `.AddStandardVerbs()`
are ever called, which is the root cause of most items below. Compare with `../Faerie.Samples.Zork`
throughout, which uses the mature pattern (`AddStandardVerbs()` + `b.On(thing).Before/After(...)`
reactions layered on top).

### 9. [ ] No `LOOK` verb exists at all

`Faerie.Verbs.StandardVerbs.cs:23` defines `Look` (`"look"`, `"l"`), but it's only registered by
`AddCoreVerbs()`, which this sample never calls. Confirmed by grep — there is no `"look"` command
anywhere in `HauntedHouseWorld.Verbs.cs`. Players cannot re-display the current room without moving.
**Fix:** either call `AddCoreVerbs()` (see item 12 for why that's a bigger, more deliberate change)
or, as a minimal fix, register just the `Look` verb directly the way the other custom verbs here are
registered, wired to call the same room-description path movement uses.

### 10. [ ] Open/Unlock are hand-rolled instead of using the engine's generic verbs + attributes

`HauntedHouseWorld.Verbs.cs:111-139` (`OpenHandler`) and `:380-399` (`UnlockHandler`) hardcode
`ctx.CurrentRoom == Study && (thing == Drawer || thing == Desk)`-style checks per puzzle. `Drawer` and
`Coffin` are already tagged `.Openable()` (`HauntedHouseWorld.Things.cs:34,38`), and the engine's
built-in `Open`/`Close`/`Lock`/`Unlock` verbs (`Faerie/Verbs/StandardVerbs.cs`, roughly lines
306-385 — recheck exact lines, the file may have shifted) already react to `Attr.Openable`,
`Attr.Locked`, and `Thing.Key` generically. `Faerie.Samples.Zork` uses the real verbs plus
`b.On(thing).Before/After(...)` for puzzle-specific reactions (see `ZorkWorld.Puzzles.cs` around
lines 100-151 for the pattern).

**Fix:** adopt `AddCoreVerbs()` (or otherwise register the standard Open/Unlock verbs), give
`Drawer`/`Coffin`/`FrontDoor` the right `Attr` flags (`Openable`, `Locked`, `LockedWith` via
`ThingFluent.LockedWith`, see `Building/FluentExtensions.cs`), and move today's bespoke messages
("The drawer is now open", "It is locked", "The key turns!") into `b.On(thing).Before/After(...)`
reactions on top of the generic verb. This item is intertwined with item 12 — probably do them
together.

### 11. [ ] Puzzle solutions rewire the map imperatively instead of declaring conditional exits

`HauntedHouseWorld.Verbs.cs:251` (`DigHandler`, digging the bars out), `:280` (`SwingHandler`,
breaking the study wall), `:393` (`UnlockHandler`, unlocking the front door) all call
`Room.SetExit(...)` / `.South(...)` / `.East(...)` live, inside a verb handler, to add an exit that
didn't exist before. The engine already has a declarative way to express "this exit exists once a
condition is true": `Exit.Condition` / `Exit.Gate` / `Exit.BlockedMessage`
(`Faerie/Model/Exit.cs:29-42`) plumbed through `ExitFluent.When(StateKey<bool>)` and
`Room.Connect(..., reciprocal:, when:, blocked:)` (`Faerie/Building/FluentExtensions.cs`, roughly
lines 25-26 and 149-163 — recheck line numbers). `Faerie.Samples.Zork` uses this for every equivalent
puzzle (trapdoor, grating, kitchen window, magic passage — see `ZorkWorld.Build.Map.cs` around lines
478-506).

**Fix:** declare these four exits statically in `HauntedHouseWorld.Rooms.cs` gated on the relevant
`StateKey<bool>` (`BarsDugOut`, `WeakWallIntact`, `FrontDoorLocked`) instead of mutating the room
graph from inside `DigHandler`/`SwingHandler`/`UnlockHandler`. The verb handlers should just flip the
state flag and print the message; the exit's existence becomes a pure function of that flag. This
also means the full room graph in `HauntedHouseWorld.Rooms.cs` stays the single source of truth for
navigation, rather than being partially defined by scattered verb handlers.

### 12. [ ] Darkness/light-source logic has drifted into two disagreeing implementations

The engine's own dark-room system (used correctly by `HauntedHousePresentation.cs:18-21` via
`Scope.IsLit`) already knows which rooms are dark (`.Dark()` in `HauntedHouseWorld.Rooms.cs`) and
whether the player has a light source. But movement-blocking is handled by a second, hand-rolled
system: `HasLight` (`HauntedHouseWorld.State.cs:46-47`, checks `Attr.Lit` on `ThingsHere` only) and
`IsDarkHall` (`HauntedHouseWorld.Movement.cs:110-111`, a hardcoded list: `ImpressiveHallway`,
`LockedDoorHall`, `TrophyRoom`). **`DarkAlcove` is also `.Dark()`**
(`HauntedHouseWorld.Rooms.cs:51`) **but is missing from `IsDarkHall`** — meaning movement into/around
that room is currently not being gated the way it should be (a real, if minor, latent bug caused by
the duplication, not just a style issue). `Faerie.Samples.Zork`'s grue daemon instead calls
`new Scope(ctx.State, ctx).IsCurrentRoomLit` directly (`ZorkWorld.Puzzles.cs:684`) rather than keeping
a parallel hand-rolled room list.

**Fix:** replace `IsDarkHall(...)`/`HasLight(...)`'s room-list approach with a direct
`Scope.IsCurrentRoomLit` (or equivalent) check in `CheckMovementBlock`
(`HauntedHouseWorld.Movement.cs`), so there is exactly one system deciding "is it dark here," and
`DarkAlcove` (and any future `.Dark()` room) is automatically covered.

### 13. [ ] `WithCarryLimit(...)` is never set, so the encumbrance hint is a lie

`HauntedHouseGame.cs` never calls `.WithCarryLimit(...)` (contrast `Faerie.Samples.Zork/ZorkGame.cs:23`,
`.WithCarryLimit(100)`). `Encumbrance.CanTake`/`TakeBlockedMessage`
(`Faerie/Runtime/Encumbrance.cs:29-38`) *is* correctly called from `TakeHandler`
(`HauntedHouseWorld.Verbs.cs:100`), but with no `Game.CarryLimit` set it's a permanent no-op — nothing
can ever be too heavy to carry. Meanwhile `HintLines` in `HauntedHouseWorld.Verbs.cs` includes: "Leave
objects with LEAVE when you have collected too many to carry" — a hint describing a mechanic that
currently can't trigger.

**Fix:** decide on a sensible carry limit (check whether the original BASIC game had a fixed
"you can only carry N objects" rule — if so, match it; `jbanes/haunted` is the place to check for an
`OBJECT_COUNT`/inventory-limit style check) and set it via `.WithCarryLimit(...)`, giving `Thing.Size`
values to the treasures if the engine's encumbrance model needs them (see `Encumbrance.cs` for what
it reads).

---

## Working on this sample

1. Fix items in numeric order where possible — 1-8 are small, independent, low-risk message/logic
   fixes; 9-13 are more structural and worth doing as their own reviewed changes.
2. After any fidelity fix (items 1-8), re-run `dotnet test` — specifically
   `HauntedHouseWalkthroughTests` — and read the transcript, don't just check the exit code.
3. When you complete an item, delete its checklist entry (or check the box and leave a one-line
   note with the commit) rather than leaving stale, already-fixed context for the next agent.
4. If you find a new discrepancy while working here, add it to this file in the same format
   (what the code does today, what the original does, cited from `jbanes/haunted`, and a suggested
   fix) rather than just fixing it silently — that citation trail is what makes future audits fast.
