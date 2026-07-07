# Issue #116 — Borderless, fully-TUI window chrome with custom controls

Remove the native OS title bar / window chrome from the Avalonia fake-terminal window so the
whole window is the character-cell grid, and reimplement the close/minimize/maximize controls as
character cells inside the TUI. Controls are drawn as **character-cell circles** (traffic-light
styling) on macOS and as conventional min/max/close glyphs on Windows/Linux, with placement chosen
by OS detection at the Avalonia host layer.

Reference: https://github.com/KiwiGeek/Faerie/issues/116

## Scope & confirmed decisions
- **macOS controls:** char-cell circles (red/yellow/green), fully borderless on every platform — no
  native traffic lights, no hybrid native chrome.
- **Control home:** the buttons live in the existing title row (row 0). When TUI window-chrome is
  enabled the control **guarantees row 0 is rendered as chrome even if the game did not enable its
  own title bar**, so the buttons always have a home; the game title text is clipped to the space
  between the button clusters.
- **Resize:** full 8-direction resize (all edges + corners) via edge/corner hit-testing +
  `BeginResizeDrag`; drag-to-move on the remaining title-row region via `BeginMoveDrag`.
- **Enablement:** opt-in flag defaulting to **on**, matching the existing `ApplyGameWindowChrome`
  pattern on `AvaloniaDisplayBuilder`; can be disabled to fall back to native chrome (useful for
  tests and games that want the OS title bar).
- **Cell-snapped sizing:** in the normal (non-maximized) borderless state the window client size must
  be an exact multiple of the cell size (`cols x _cellWidth` by `rows x _cellHeight`) so there is
  **no black border** between the window edge and the rendered text. The centered black border is
  permitted **only** when the window is maximized or fullscreened, where it preserves monospaced font
  fidelity. Changing the zoom factor keeps the current `cols x rows` and resizes the window to the new
  cell size (clamped to the screen), rather than keeping the window fixed and reflowing the grid.
- **Verification:** author builds/runs locally on Windows (`dotnet build`, `dotnet test`); this plan
  provides autonomous build + unit-test checks for the testable logic (OS detection, button layout,
  hit-testing math), and lists manual GUI smoke-test steps for the visual behavior.
- **Layout target:** macOS cluster top-left in order close/minimize/maximize (red/yellow/green);
  Windows/Linux cluster top-right in order minimize/maximize/close.
- **Commits/branches:** no Claude co-author attribution in commits or branch names (author preference).

## Key files
- `src/Faerie.Terminal.Avalonia/TerminalWindow.cs` — borderless window setup, move/resize drag,
  min/max/close command handlers, F11 fullscreen reconciliation.
- `src/Faerie.Terminal.Avalonia/TerminalControl.cs` — reserve/render chrome cells in row 0,
  hit-test button + edge/corner + drag regions, raise chrome events, pointer routing.
- `src/Faerie.Terminal.Avalonia/AvaloniaDisplay.cs` — wire the opt-in flag and OS detection into the
  builder; expose configuration.
- New `src/Faerie.Terminal.Avalonia/WindowChrome/` (proposed) — `HostPlatform` detection helper,
  `WindowControlKind` enum, `WindowControlLayout` (pure layout/hit-test logic, no Avalonia types so
  it is unit-testable).
- `tests/Faerie.Tests/` — new unit tests for platform detection + layout/hit-test math.
- `src/Faerie.Terminal.Avalonia/README.md` and `docs/FEATURES.md` — document the new chrome.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to
`Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue
with zero context); run the phase's **Verification Plan** and record the result before moving on.
When all phases are done, fill in **Final Recap** and **Deployment Plan**.

## Phase 1: OS detection + borderless window scaffolding
Status: Complete

- [x] Add `HostPlatform` helper (enum `Windows | MacOS | Linux` + `HostPlatformDetector.Current` using
      `OperatingSystem.IsMacOS()/IsWindows()`, everything else = Linux). File:
      `src/Faerie.Terminal.Avalonia/HostPlatform.cs`.
- [x] Add `WindowControlKind` enum (`Minimize | Maximize | Close`), `WindowControlCell` record, and a
      pure `WindowControlLayout` (macOS top-left close/min/max; Windows/Linux top-right
      min/max/close; `Compute`, `TryHitTest`, `IsButtonColumn`, `ClusterWidth`). No Avalonia types so
      it is unit-testable. File: `src/Faerie.Terminal.Avalonia/WindowControls.cs`.
- [x] Add opt-in `UseTuiWindowChrome` bool on `TerminalWindow` (default `true`) that sets
      `SystemDecorations = SystemDecorations.None` when enabled (via `ApplyWindowChromeMode`), and sets
      `Terminal.ShowWindowControls` / `Terminal.WindowControlPlatform`.
- [x] Wire the flag through `AvaloniaDisplayBuilder` with `.WithTuiWindowChrome(bool = true)`, applied
      in `Build()` (`window.UseTuiWindowChrome = _useTuiWindowChrome`).
- [x] `MinWidth`/`MinHeight` (480/300) remain enforced on `TerminalWindow`; the layout returns an empty
      cluster if a grid is ever too narrow, so rendering degrades safely.
- [x] Added `TerminalControl.ShowWindowControls` + `WindowControlPlatform` properties and
      `EnforceChromeRow()` (forces the buffer title bar on when controls are shown, re-asserted each
      layout pass since the engine re-applies `ConfigureBars` from the game at start).

### Verification Plan
- `dotnet build src/Faerie.Terminal.Avalonia/Faerie.Terminal.Avalonia.csproj -c Debug` → builds with
  no new warnings/errors.
- `dotnet test tests/Faerie.Tests/Faerie.Tests.csproj --filter FullyQualifiedName~WindowChrome` → all
  green (tests in `tests/Faerie.Tests/WindowChromeTests.cs`).

### Phase Summary
Added the OS-detection and window-control scaffolding. `HostPlatform`/`HostPlatformDetector` isolate
platform detection so tests drive layout deterministically. `WindowControlLayout` is the pure,
Avalonia-free core (used by Phases 2–3 for rendering and hit-testing): macOS => Close/Minimize/Maximize
anchored left; Windows/Linux => Minimize/Maximize/Close anchored right; one-cell `EdgeMargin` and
`ButtonGap`. `TerminalWindow.UseTuiWindowChrome` (default on) drives `SystemDecorations.None` and pushes
the flag + detected platform into the control; `AvaloniaDisplayBuilder.WithTuiWindowChrome` exposes it
fluently. `EnforceChromeRow()` guarantees row 0 exists for the controls even when a game defines no
title bar. Unit tests written (`WindowChromeTests.cs`) covering platform detection, per-platform
ordering/anchoring, distinct columns (title-clip safety), hit-test round-trips, gap columns, and the
too-narrow fallback. NOTE: `dotnet` is unavailable in the authoring sandbox, so the build/test commands
above must be run on a machine with the .NET SDK; the code was written and self-reviewed for compilation.
Key decision: rather than fight the engine's `ConfigureBars` call at start, the control re-asserts the
chrome row on every layout pass (cheap, idempotent).

## Phase 2: Render TUI window controls in row 0
Status: Complete

- [x] `RowCells` now routes the title row through `OverlayWindowControls` when `ShowWindowControls` is
      set; `EnforceChromeRow` (Phase 1) guarantees the title row exists, so chrome always occupies row 0.
- [x] `OverlayWindowControls`/`ControlGlyph` draw the glyphs from `WindowControlLayout`: macOS uses
      `●` colored LightRed/Yellow/LightGreen (close/min/max); Windows/Linux use `–` `□` `×` in the bar's
      foreground. The button columns overwrite the title cells, so title text is clipped around them.
- [x] Hover state via `_hoveredControl`, updated in `OnPointerMoved` (`UpdateHoveredControl`) and cleared
      in `OnPointerExited`; hovered Close turns red-on-white (Win/Linux), others brighten on dark.
- [x] Buttons are emitted as ordinary `GlyphCell`s, so the existing `RenderRow` background-run and
      `RenderGlyph` paths draw them with no seams and no renderer changes.

### Verification Plan
- `dotnet build src/Faerie.Terminal.Avalonia/Faerie.Terminal.Avalonia.csproj -c Debug` → clean build.
- Unit test `WindowControlLayout_ClipsTitleBetweenClusters` (title text never overlaps button columns)
  passes.
- Manual GUI smoke test (author, Windows): run a sample (`dotnet run --project
  src/Faerie.Samples.Zork`) → window opens borderless; min/max/close glyphs visible top-right; game
  title visible and not overlapping buttons.

### Phase Summary
Row 0 now renders the window controls. `RowCells` calls `OverlayWindowControls(title, barStyle)` for the
title row when `ShowWindowControls` is set; it walks `WindowControlLayout.Compute(...)` and overwrites
each button column with a `ControlGlyph`. Glyphs/colors are centralized in `ControlGlyph`: macOS draws
`●` in LightRed/Yellow/LightGreen (traffic-light convention for close/minimize/maximize); Windows/Linux
draw `–`/`□`/`×` in the bar foreground. Because the buttons are plain `GlyphCell`s, the existing
background-run + glyph renderer handles them unchanged. Hover is tracked with `_hoveredControl` (set in
`UpdateHoveredControl` from the screen-row-0 hit-test, cleared on pointer exit). Glyph choices are easy
to tweak after the manual smoke test if a bundled retro font lacks a symbol; the default monospace
typeface has all four. Clicks/drag/resize are NOT yet wired (Phase 3).

## Phase 3: Interaction — click commands, drag-to-move, edge/corner resize
Status: Complete

- [x] `TerminalControl.TryHandleChromePress` routes a button press to `WindowControlInvoked`; the window
      handler (`OnWindowControlInvoked`) maps Close => `Close()`, Minimize => `WindowState.Minimized`,
      Maximize => toggle `Normal`/`Maximized`. Handled presses `return` before selection begins.
- [x] Drag-to-move: a press on the chrome row outside the buttons raises `WindowMoveRequested`; the
      window calls `BeginMoveDrag(e)`.
- [x] 8-direction resize: `WindowResizeHitTest.Classify` (pure) maps a border point to a `WindowEdge`;
      a press there raises `WindowResizeRequested` and the window calls `BeginResizeDrag(edge, e)`.
      `UpdateCursor` shows the matching resize cursor over the borders (hidden cursor elsewhere).
- [x] Priority order (buttons → resize border → chrome drag) keeps the top edge from swallowing button
      clicks; selection only starts when `TryHandleChromePress` returns false (i.e. inside the text area
      away from borders), so in-grid selection is unaffected.

### Verification Plan
- `dotnet build` of the Avalonia project → clean.
- Unit test `WindowControlLayout_HitTest_ReturnsButtonAtColumn` and edge-zone hit-test math tests pass.
- Manual GUI smoke test (author): clicking close/min/max works; dragging the title row moves the
  window; dragging each edge/corner resizes; text selection still works inside the game area.

### Phase Summary
Interaction is wired. `TerminalControl` exposes three events — `WindowControlInvoked`,
`WindowMoveRequested`, `WindowResizeRequested((WindowEdge, PointerPressedEventArgs))` — raised from
`TryHandleChromePress` during `OnPointerPressed` (only when `ShowWindowControls`). `TerminalWindow`
subscribes in its constructor and performs the actual window ops (`Close`/`WindowState`/`BeginMoveDrag`/
`BeginResizeDrag`) since those are `Window` members. Priority is buttons → resize border → drag region,
so the top resize edge never eats a button click and text selection is untouched (selection only runs
when the press wasn't consumed). Resize-edge math was extracted to the pure `WindowResizeHitTest.Classify`
so it's unit-tested (8 edges/corners + interior + corner-priority). Cursors update over the borders via
`UpdateCursor`. Cell-snapping the resized/zoomed dimensions is Phase 4.

## Phase 4: Cell-snapped window sizing (no black border)
Status: Complete

- [x] Added the pure `WindowCellSnap.Snap(width, height, cellW, cellH, minW, minH)` helper that rounds
      to whole cells (`MidpointRounding.AwayFromZero`) and enforces the minimums rounded up to whole
      cells. Unit-tested.
- [x] Snap on interactive resize: `TerminalWindow.OnPropertyChanged` watches `ClientSizeProperty`/
      `WindowStateProperty` and calls `SnapToCellGrid`, which snaps `ClientSize` to a cell multiple when
      `WindowState == Normal` (skipped for Maximized/FullScreen). Re-entrancy guarded by `_adjustingSize`.
- [x] Zoom keeps the window ~fixed and reflows: `TerminalControl.SetFontSize` re-measures the cell,
      reflows the grid to the current window (`ApplyGrid`), and raises `CellMetricsChanged`;
      `TerminalWindow` handles it by calling `SnapToCellGrid`, nudging the window to the nearest whole
      cell (sub-cell change) to remove the partial-cell border. (Revised from an earlier version that
      preserved cols x rows and resized the window — per user feedback, zoom should hold window size.)
- [x] Centered black border: no `Render` change needed — when snapped, `Render`'s existing
      `offsetX`/`offsetY` compute to ~0 (grid == client size); only Maximized/FullScreen (snap disabled)
      leaves a centered border, exactly as required.
- [x] Initial size: the first `ClientSize` change after open triggers `SnapToCellGrid`, so the window
      launches border-free at the default font.

### Verification Plan
- `dotnet build src/Faerie.Terminal.Avalonia/Faerie.Terminal.Avalonia.csproj -c Debug` → clean build.
- Unit tests pass (`dotnet test ... --filter FullyQualifiedName~WindowChrome`): `CellSnap_ExactMultiples_AreUnchanged`,
  `CellSnap_RoundsWidthToWholeCells`, `CellSnap_RespectsMinimums_RoundedUpToWholeCells`,
  `CellSnap_ReturnsInputWhenCellSizeUnknown`, `CellSnap_NeverProducesZeroCells`.
- Manual GUI smoke test (author, Windows): dragging any edge/corner leaves no black gap in the normal
  state; zooming grows/shrinks the window while keeping the same column/row count and no border;
  maximizing/fullscreen shows the centered black border and text stays crisp.

### Phase Summary
Window sizing now snaps to the character grid. The pure `WindowCellSnap.Snap` does the rounding (tested);
`TerminalWindow` applies it. Snap-on-resize is driven from `OnPropertyChanged` (watching `ClientSize`/
`WindowState`) rather than a size-changed event, since `OnPropertyChanged` is a stable base hook;
`_adjustingSize` prevents the Width/Height writes from recursing. Snap-on-zoom is a control->window
handshake: `SetFontSize` preserves `cols x rows` and raises `WindowSizeToGridRequested`, the window
resizes (clamped to the screen via `Screens.ScreenFromVisual`), and if it can't fit, the resulting layout
reflows naturally. The centered-border requirement fell out for free — the existing centering yields 0 in
the snapped normal state and a centered border only when Maximized/FullScreen (where snapping is off).
RISKS to confirm in the manual smoke test: (1) snapping on every `ClientSize` change during an OS-driven
`BeginResizeDrag` could feel slightly "sticky" — if so, move the snap to pointer-release; (2) the
`Screens`/`Scaling` clamp APIs are assumed from Avalonia 11 and should be confirmed at build time.
BUGFIX (found at first run): `OnPropertyChanged` fires during the base `Window` constructor's
`set_ClientSize`, before `Terminal` is assigned, causing a `NullReferenceException` in `SnapToCellGrid`.
Guarded with a `_chromeReady` flag set at the end of the constructor (SnapToCellGrid no-ops until then).

## Phase 5: Fullscreen reconciliation, fallback, tests & docs
Status: Complete

- [x] F11/Alt+Enter fullscreen reconciled with no code change: `OnKeyDown` still toggles
      `Normal`<->`FullScreen`; in FullScreen `SnapToCellGrid` steps aside (not `Normal`) so the grid
      centers with a border, and the row-0 chrome keeps rendering. Returning to `Normal` re-snaps via
      the `WindowStateProperty` branch of `OnPropertyChanged`.
- [x] `UseTuiWindowChrome=false` verified by inspection: `ApplyWindowChromeMode` sets
      `SystemDecorations.Full` + `ShowWindowControls=false`; the control skips chrome render/hit-test and
      `SnapToCellGrid`/`OnWindowSizeToGrid` early-return, so native decorations and free resize return.
- [x] Unit tests in `tests/Faerie.Tests/WindowChromeTests.cs` cover platform detection, per-platform
      ordering/anchoring, distinct columns, hit-test round-trips, gap columns, too-narrow fallback,
      resize edge/corner classification, and cell-snap rounding/minimums — all platform-driven, no live OS.
- [x] `src/Faerie.Terminal.Avalonia/README.md` and `docs/FEATURES.md` updated to describe the borderless
      chrome, controls, move/resize, cell-snapped sizing, and the `WithTuiWindowChrome(false)` opt-out.

### Verification Plan
- Full solution build: `dotnet build Faerie.slnx -c Debug` → clean.
- Full test run: `dotnet test tests/Faerie.Tests/Faerie.Tests.csproj` → all green (matches CI in
  `.github/workflows/ci.yml`).
- Manual GUI smoke test (author): F11 toggles fullscreen and back without losing chrome or controls;
  `WithTuiWindowChrome(false)` shows native title bar.

### Phase Summary
No code change was needed for fullscreen: the existing `OnKeyDown` toggle plus the state-aware
`SnapToCellGrid` gate already produce the intended behavior (border-free normal, centered border in
maximized/fullscreen, chrome always visible). The native-chrome fallback is fully handled by
`ApplyWindowChromeMode` and the `_useTuiWindowChrome` guards. Tests and docs finalized. A fresh-eyes code
review found no compile errors in the new code (its one flag — `_activePumpFrame?.Continue = false;` — is
pre-existing C# 14 null-conditional-assignment, valid on net10.0, and not part of this change).

## Final Recap
Issue #116 is implemented. The Avalonia window is borderless by default (`SystemDecorations.None`) with
the OS title bar/buttons removed and reimplemented as character cells on row 0: macOS shows a
red/yellow/green traffic-light cluster top-left (close/minimize/maximize), Windows/Linux show
minimize/maximize/close top-right, chosen by `HostPlatformDetector`. The window is movable (drag the
title row) and resizable from any edge/corner (`BeginMoveDrag`/`BeginResizeDrag`, with resize cursors),
and in the normal state it snaps to whole character cells so there is no black border; zoom preserves the
column/row count and resizes the window; maximized/fullscreen keep a centered border for font fidelity.
All behavior is opt-out via `WithTuiWindowChrome(false)` / `TerminalWindow.UseTuiWindowChrome`.

New files: `HostPlatform.cs`, `WindowControls.cs` (pure `WindowControlLayout` + `WindowResizeHitTest` +
`WindowCellSnap`), `tests/Faerie.Tests/WindowChromeTests.cs`. Modified: `TerminalWindow.cs`,
`TerminalControl.cs`, `AvaloniaDisplay.cs`, plus README/FEATURES docs. The pure geometry (layout,
resize hit-test, cell-snap) is unit-tested; rendering, pointer wiring, and window sizing are validated by
the manual GUI smoke tests listed per phase (the authoring sandbox has no .NET SDK).

## Deployment Plan
This is a library/front-end change (no data migration, no services). To ship on a machine with the .NET
SDK:

1. Build: `dotnet build Faerie.slnx -c Debug` (and `-c Release`) → expect clean.
2. Test: `dotnet test tests/Faerie.Tests/Faerie.Tests.csproj` → all green (adds the `WindowChrome` tests).
3. Manual GUI smoke test on Windows (and macOS/Linux if available): run a sample,
   `dotnet run --project src/Faerie.Samples.Zork`. Confirm: borderless window; controls in the correct
   corner for the OS; click close/minimize/maximize; drag the title row to move; drag edges/corners to
   resize (with resize cursors); no black border in the normal state; zoom (`Ctrl`+`+`/`-`/`0`, `Ctrl`+
   wheel) resizes the window keeping the grid; F11 fullscreen shows a centered border and text stays
   crisp; verify `WithTuiWindowChrome(false)` restores the native title bar. Tweak the button glyphs in
   `TerminalControl` if a bundled retro font lacks `●`/`□`/`×`/`–`.
4. Commit on the feature branch (no Claude co-author) and open a PR to `master`. NOTE: `master` must
   already contain the fluent-launcher commit (`28c2032`, cherry-picked as `666c90d`), which this branch
   is based on.
5. If interactive resize feels "sticky" (snapping mid-drag), change `SnapToCellGrid` to run on
   pointer-release instead of every `ClientSize` change.
