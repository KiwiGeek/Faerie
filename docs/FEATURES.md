# Feature Backlog

Planned and requested features, plus engine gaps discovered while porting larger games. Items are
grouped roughly by area. When a gap that is also listed in a sample's `AGENTS.md` (e.g. the Zork
sample) gets implemented, **update that `AGENTS.md`** to mark it done and trim the corresponding
in-sample simplification.

## Naming

The project needs a real name (working title: "Text Adventure Engine"). Candidates to consider:

- **Lumen** / **Lumin** — "illuminated" text; short, brandable.
- **Glyphwright** — evokes hand-crafted text screens.
- **Parsely** / **Parserly** — playful nod to the parser.
- **Scrivening** / **Scriv** — authorly.
- **Adventure Foundry**, **StoryForge**, **VerbForge** — "forge a world."
- **Fathom** — exploration + depth.
- **Grue** (taken/loaded by Zork connotations — avoid for a neutral engine).
- **Aventura**, **Questrel**, **Wayfarer**.
- User suggestion: **FluentAdventureEngine / FAE**. "FAE" is a nice short package prefix; "Faerie"
  spelling (**Fae**) reads better than the acronym.

Recommendation to decide on: a single short noun (e.g. **Lumen** or **Fathom**) for the brand, with
namespaces like `Fathom.Engine`, `Fathom.Terminal.Avalonia`. Renaming touches namespaces, assembly
names, csproj `PackageId`s, and the avares URIs in sample fonts — do it in one dedicated pass.

## Parser & language

- [ ] **Typo / fuzzy suggestions (Hamming/edit-distance).** When a word isn't understood, suggest
  the nearest known verb/noun: `Go Notrh` → "I don't understand that. Did you mean GO NORTH?"
  Use Levenshtein distance over the known vocabulary (verbs + every thing's nouns/adjectives), with
  a small max distance, and either auto-correct on a single close match or prompt.
- [x] **Disambiguate movement vs object for shared words** ("move north" vs "move rug"). *Done:* the
  parser now picks the movement verb when the argument is a direction and an object verb otherwise;
  a standard `push`/`move` object verb was added.
- [ ] **Pronoun / "it" robustness, "all", "and" lists** ("take all", "drop sword and lamp").
- [ ] **AGAIN / G, OOPS, undo** meta commands.

## World model

- [x] **Multiple room descriptions** — brief on re-entry, full on LOOK / first visit. *Done* via
  `Room.Brief(...)` and verbose/brief selection in `DescribeCurrentRoom`.
- [ ] **Inventory weight/size and carry limits** (`Thing.Size`, `GameState.TotalLoad`, take and
  movement guards, "empty hands to pass" exits). *(Zork gap #1)*
- [ ] **Vehicles & water rooms** (`Attr.Vehicle`, `Room.IsWater`, board/disembark, movement keyed on
  carried/active vehicle). *(Zork gap #2)*
- [ ] **`Thing.OnDrop` / after-drop reaction** (fragile items, drop-from-height). *(Zork gap #14)*
- [ ] **Dynamic room lighting** — `Room.IsLit`/light-level factory rather than only item-derived
  light; lets opening a grating light a room. *(Zork gaps #11)*
- [ ] **Paired/mirrored rooms** & "break scenery" verb. *(Zork gap #8)*
- [ ] **Pass-object-through-exit/door** API (push something through a grating to a linked room).
  *(Zork gap #10)*

## Systems / modules

- [ ] **Combat module** (rounds, weapon damage, creature strength/state, hooks on attack/throw).
  *(Zork gap #3)*
- [ ] **NPC movement / daemons** — pathfinding, creature inventory ("bag"), `Room.IsSacred`,
  relocate-without-teleport. *(Zork gap #5)*
- [ ] **First-class timers & staged rituals** — beyond `EveryTurn`; named countdowns, multi-step
  "ritual" state machine, button/switch groups, room "flooded" state. *(Zork gaps #6, #9)*
- [ ] **Area hazards** — flame-in-gas-room style on-enter/on-turn hazards (`Attr.Flammable`/flame
  detection). *(Zork gap #7)*
- [ ] **Per-room command/output filter** — `Room.OnCommand` or an output pipeline hook to rewrite
  text (e.g. an echoing "loud room"). *(Zork gap #4)*
- [ ] **Richer scoring** — `AdjustScore`, trophy-case helper, death penalty/counter, place-visit
  bonuses. *(Zork gap #12)*
- [ ] **Death/restore behaviors** — death hook to scatter items, randomize placements. *(Zork gap #13)*
- [ ] **Proximity sensing / numeric thing state surfaced to daemons** (sword glow near monsters).
  *(Zork gap #15)*
- [ ] **Creature mood counters & fluids** (cyclops stages; bottle/water as quantity). *(Zork gap #16)*
- [ ] **Standard "Infocom" verb bundle** — board, disembark, count, smell, listen, jump, swim, touch,
  wake, poke, lower, raise, fill, pour, burn, rub, shake, cross, enter, etc., as an optional module.
  *(Zork gap #19)*
- [ ] **Random NPC-encounter system** (gnome/magic-flag style). *(Zork gap #20)*

## Presentation / terminal

- [ ] **Graphics support.** (Not literally sixel — we render our own surface, not a real terminal.)
  Allow a game to draw bitmap/vector art into a region of the screen (splash images, room
  illustrations, a status panel). Likely an `IRegion`/layer API over the framebuffer plus an Avalonia
  image-draw path, with the text grid composited around/over it.
- [ ] **Sound & music.** Engine-agnostic audio hooks (play sound effect, loop background music,
  stop/fade), triggered from reactions/daemons; an Avalonia/host implementation. Keep the engine
  free of audio dependencies (interface in engine, impl in host).
- [ ] **Re-render/reflow backbuffer on zoom & resize.** Glyphs already redraw at the new font size;
  remaining work is to re-wrap stored scrollback/live text to the new column count (keep logical
  lines, re-wrap on resize) rather than leaving old wrapping. Today content is preserved top-left
  without reflow (DOS-style).
- [ ] **Mouse cursor as a cell highlight.** Over the game window, hide the system cursor and instead
  highlight the hovered character cell (inverse/background block), as old text UIs did. Track pointer
  → cell, draw a highlight in `Render`, restore the system cursor when the pointer leaves.
- [ ] **TUI file browser for SAVE/RESTORE.** Present an in-terminal (in-grid) file picker for choosing
  a save slot/file, styled like the rest of the UI. Native OS file dialogs are an acceptable fallback
  if the TUI picker proves too costly. Today save/restore use a single fixed slot.
- [ ] **CRT/scanline post-effect** (optional eye-candy toggle).

## Packaging / project shape

- [ ] **NuGet readiness pass.** Done so far: `IsPackable`, `PackageId`, `Description`, tags on the two
  libraries; the terminal library ships **no** fonts. Still to do: README/license/icon in the
  packages (`PackageReadmeFile`, `PackageLicenseExpression`, `PackageProjectUrl`,
  `RepositoryUrl`), SourceLink + deterministic builds, symbol packages (`snupkg`), decide on a
  minimum Avalonia version range rather than a pin, and validate `dotnet pack` output. Confirm the
  engine has zero non-BCL dependencies and the terminal package depends only on `Avalonia` (core),
  not `Avalonia.Desktop`/Themes (those belong to the app).
- [ ] **Sample app project** kept out of the packable set (it's an exe, already not packable).

## Docs

- [ ] Keep `docs/AUTHORING.md` in sync as features land (fonts/cursor section is current).
- [ ] Add a "porting a classic" note referencing the Zork sample's `AGENTS.md` gap list.
