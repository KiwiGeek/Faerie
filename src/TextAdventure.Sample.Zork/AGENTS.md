# AGENTS.md — Zork I Sample

Guidance for AI agents working on the Zork port in this project.

## Related projects

| Path | Role |
| --- | --- |
| `../Nixie` | The engine **and** Avalonia fake-terminal host, merged into one package: world model, fluent builder, verbs, parser, runtime, save/load (`Nixie.*`) plus the terminal (`Nixie.Terminal`) |
| `../TextAdventure.Sample.HauntedHouse` | Original sample adventure |
| `../../docs/AUTHORING.md` | Authoring guide for new games |

When changing the engine to close a gap below, prefer extending existing patterns (`GameBuilder`, `Attr`, `ReactionTable`, `Exit.Condition`) over ad-hoc logic in this sample.

---

## Zork I port: engine gaps

This sample implements the full **map**, **treasures**, and **win path** of Zork I, but several Infocom behaviors are **simplified or stubbed** because the engine lacks the underlying systems. Each gap below lists what the original game does, what the sample does instead, and where to look in sample source.

In-game source references use `ZorkSimplifications` (`ZorkWorld.Simplifications.cs`) and inline `// ENGINE-LIMIT:` comments.

### 1. Inventory weight and encumbrance

**Original:** Every object has a `size`; total carried weight is capped (~100). Heavy items (gold coffin) cannot be taken from the altar. The drafty-room exit requires empty hands. Climbing the kitchen chimney with a load fails.

**Engine gap:** No weight/size property, no carry limit, no “empty hands” check on exits.

**Sample simplification:** Coffin take is blocked only when on the altar (`DefineDaemons`). Drafty-room and chimney load rules are not enforced.

**Engine improvement:** Add `Thing.Size`, `GameState.TotalLoad`, take/movement guards, optional `Exit.Condition` helpers for encumbrance.

---

### 2. Vehicles, water rooms, and river travel

**Original:** Frigid River and Reservoir are `water_room`s. The player boards a deflated/inflated boat; upstream travel requires the boat in inventory; beaches block passage while carrying the boat. `LAND`/`board`/`disembark` verbs move between river and shore.

**Engine gap:** No vehicle attribute, no water-room type, no boarding, movement does not inspect carried boat state.

**Sample simplification:** Boat inflate/deflate only (`DefineBoat`). Reservoir crossing uses a `_lowTide` flag after the dam bolt turns; no river navigation loop.

**Engine improvement:** `Attr.Vehicle`, `Room.IsWater`, custom movement hooks or exit conditions keyed on vehicle state.

---

### 3. Melee combat (troll, thief)

**Original:** Turn-based fight with strength, disarm, wake-on-leave, troll recovers axe, thief stiletto and special disengage rules.

**Engine gap:** No combat system, no creature strength, no `fightbit` / out-of-combat recovery.

**Sample simplification:** `attack` with sword = instant kill; lunch/garlic = instant troll defeat; thief dodges without sword (`DefineCombat`, `DefineTrollAndCyclops`).

**Engine improvement:** Optional combat module: rounds, weapon damage, creature state machine, hooks on `Attack`/`Throw`.

---

### 4. Loud Room command echo

**Original:** In the Loud Room, **every** command’s output is garbled (except `verbose`/`look`). “Take bar” echoes as “bar bar bar” and reveals the platinum bar.

**Engine gap:** No per-room output filter, no hook to rewrite parser/verb messages globally. `ReactionTable.BeforeAny` can block but not mutate standard verb text.

**Sample simplification:** Only the custom `echo` verb distorts text; `echo bar` reveals the bar (`EchoHandler`). `DefineLoudRoom` is intentionally empty.

**Engine improvement:** `Room.OnCommand` or `GameEngine` output pipeline hook: `(room, verb, rawInput) => string?`.

---

### 5. Thief daemon and NPC movement

**Original:** Thief starts in Round Room, roams (maze, above-ground), steals treasures to his bag, conceals loot in Treasure Room, becomes engrossed, has complex fight/rob logic, respects sacred rooms.

**Engine gap:** No NPC pathfinding, no creature inventory/bag, no `sacred` room flag, no steal-from-player API beyond manual `State.Move`.

**Sample simplification:** Random steal every few turns after turn 20; stolen items go to Treasure Room floor; thief teleports into player room (`DefineThief`).

**Engine improvement:** Creature placement/bag model, `Room.IsSacred`, structured daemon helpers, relocate-to-room without teleport messaging.

---

### 6. Dam, maintenance room, and flooding timer

**Original:** Green bubble must glow (maintenance buttons) before bolt turns. Opening sluice gates drains reservoir over turns; failing to open causes flooding, maintenance room underwater, rising reservoir while player is on the lake.

**Engine gap:** No timed world events beyond `EveryTurn` daemons (sample uses these manually), no room flooding state, no button-panel puzzle primitives.

**Sample simplification:** Turning bolt with wrench immediately sets `_damOpened` and `_lowTide` and reveals trunk (`DefineDamAndReservoir`). Maintenance `blast` = instant death only.

**Engine improvement:** First-class timers, room attribute “flooded”, switch/button groups.

---

### 7. Gas room explosion

**Original:** Open flame (lit lantern, matches, candles) in Gas Room causes explosion and death.

**Engine gap:** No `Attr.Flammable` / flame detection in room, no area hazard on enter/turn.

**Sample simplification:** Not implemented.

**Engine improvement:** `Attr.Flame` or `LightSource` + `Room.OnTurn` hazard helper; or `EveryTurn(when: in GasRoom && HasFlame)`.

---

### 8. Mirror rooms

**Original:** Two mirror rooms; breaking a mirror duplicates/swaps items between north and south mirrors; bad luck flag.

**Engine gap:** No paired-room object sync, no “break scenery” verb.

**Sample simplification:** Map connections only; no mirror interaction.

---

### 9. Hades exorcism sequence

**Original:** Multi-step ritual at Entrance to Hades: light candles, ring bell, read book (order and timing matter); spirits and crystal skull.

**Engine gap:** No multi-step ritual state machine; candles/matches fuel partially modeled.

**Sample simplification:** Single check — at Hades with bell, book, and candles in inventory (`DefineHades`).

**Engine improvement:** `Ritual` builder or staged `StateKey` pattern in engine docs; optional `Attr.Flame` fuel.

---

### 10. Grating two-way object pass-through

**Original:** Objects can be pushed through the grating between Clearing and Grating Room; opening grating from below drops leaves.

**Engine gap:** No “pass object through exit/door to linked room” API.

**Sample simplification:** Grating is a normal locked door between rooms; leaves only reveal grating (`DefineGrating`).

---

### 11. Dynamic room lighting (grating)

**Original:** Opening grating sets `light` on Grating Room (still `rlandbit` but lit).

**Engine gap:** `Room.IsDark` is static; lighting is derived only from carried/room light **items**, not room state.

**Sample simplification:** Manually sets `GratingRoom.IsDark = false` when grating opens.

**Engine improvement:** `Room.IsLit` property or dynamic `DescriptionFactory` for light level.

---

### 12. Scoring fidelity

**Original:** Trophy case adds/subtracts `trophy_value`; place visits add `value`; task array; **−10 per death**; max 350.

**Engine gap:** Score is a single int; no built-in death penalty; no trophy removal penalty.

**Sample simplification:** One-time treasure bits on `Put` in case; five place bonuses; no death penalty; no score loss when removing treasures (`DefineTrophyScoring`, `DefineExplorationScore`).

**Engine improvement:** `GameState.AdjustScore`, trophy-case helper, death counter hook in `GameEngine`.

---

### 13. Death, restore, and randomize

**Original:** On death, treasures scatter to random dark rooms; optional restore; “undo” feel.

**Engine gap:** Save/restore exists but no death handler hook to randomize placements.

**Sample simplification:** Standard lose message; no scatter on death.

---

### 14. Fragile egg and tree drop

**Original:** Egg breaks if dropped from tree; yields broken canary; wind-up bird puzzle on Forest Path.

**Engine gap:** No `OnDrop` hook on things (only `OnTake`); drop uses standard verb without per-thing after hook in all paths.

**Sample simplification:** `open egg` releases canary; `wind canary` on Forest Path spawns bauble (`DefineEggAndCanary`).

**Engine improvement:** `Thing.OnDrop`, `Room.OnAfterDrop` in reaction table.

---

### 15. Sword proximity glow — IMPLEMENTED

**Original:** Sword `number` increases near spirits/troll; messages in Torch Room.

**Former engine gap:** Adjacency wasn't reachable from daemons — `GameContext` only exposed
presence in the *current* room (`Here`/`InRoom`).

**Resolution:** Added `GameContext.RoomOf(Thing)` (wrapping the existing `GameState.RoomOf`), so a
daemon can locate any thing. `ZorkWorld.DefineSwordGlow` is an `EveryTurn` daemon (gated on the
sword being present) that grades the glow off living-villain proximity — **bright** when a troll or
thief shares the room, **faint** when one is in an adjacent room (`CurrentRoom.Exits`), **none**
otherwise — and reports only on change. Level is held in the `sword-glow` state key, so it survives
save/restore. No remaining engine gap.

---

### 16. Cyclops mood and water

**Original:** Multi-stage cyclops (`number`), lunch then water from **opened bottle**, ODYSSEUS yell, daemon turns until fight or sleep.

**Engine gap:** No creature mood counter in engine; bottle/water as fluid quantity not modeled.

**Sample simplification:** Lunch, any yell, or giving bottle sets sleep; sword kills (`DefineTrollAndCyclops`).

---

### 17. Bat and garlic

**Original:** Bat steals valuables and deposits in Shaft Room; garlic repels; complex bat daemon.

**Engine gap:** No flying creature steal behavior.

**Sample simplification:** Enter Bat Room without garlic = instant death (`DefineBatAndGarlic`).

---

### 18. Sand collapse

**Original:** Fifth dig without scarab retrieved causes collapse death.

**Engine gap:** None critical — doable with state; partially implemented.

**Sample simplification:** Four digs reveal scarab; no collapse on over-dig (see `DefineSandAndScarab`).

---

### 19. Missing standard verbs

**Original uses many verbs not in `AddStandardVerbs()`:** e.g. `board`, `disembark`, `count`, `smell`, `listen`, `jump`, `swim`, `touch`, `wake`, `poke`, `lower`, `raise`, `fill`, `pour`, `burn`, `rub`, `shake`, `cross`, `enter` (vehicle).

**Engine gap:** Samples must `DefineVerb` each; no Infocom verb bundle.

**Sample simplification:** Custom verbs only where minimally needed (`DefineCustomVerbs` in `ZorkWorld.Puzzles.cs`).

---

### 20. Magic flag, chimney, strange passage

**Original:** `Magic_flag` from gnome after gallery visit (random); cyclops breaks wall into Strange Passage; chimney flag from studio visit with load check.

**Engine gap:** No random NPC encounter system.

**Sample simplification:** `_magicFlag` set on first gallery enter; `_chimneyFlag` on studio enter; strange passage east wall not tied to cyclops break (`DefineMagicAndChimney`, `ConfigureConditionalExits`).

---

## Working on this sample

1. Read `ZorkWorld.Simplifications.cs` and follow `ENGINE-LIMIT` comments before “fixing” simplified behavior.
2. Do **not** paper over gaps with huge bespoke logic unless the engine gains a reusable feature.
3. When implementing an engine feature from the list above, update this file and trim the corresponding simplification in the sample.
4. Keep all Zork-specific content in this project unless explicitly asked to change the engine.

## Working on the engine

- This sample should remain the proof point: extend engine APIs, then **reduce** simplifications here.
- Preserve fluent, reference-wired style (no magic strings between objects).
- Run `dotnet test` and build both samples after engine changes.
