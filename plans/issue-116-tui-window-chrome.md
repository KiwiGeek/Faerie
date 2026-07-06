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
Status: Not started

- [ ] Add `HostPlatform` helper (enum `MacOS | Windows | Linux` + `Current` using
      `OperatingSystem.IsMacOS()/IsWindows()/IsLinux()`) under `WindowChrome/`, mirroring the existing
      `OperatingSystem.IsWindows()` usage in `Faerie.Terminal.Headless/ParentConsole.cs`.
- [ ] Add `WindowControlKind` enum (`Close | Minimize | Maximize`) and a `WindowControlLayout` type
      that, given platform + column count, returns the ordered button cells and their column ranges
      (macOS top-left close/min/max; Windows/Linux top-right min/max/close). No Avalonia types in
      this class so it is unit-testable.
- [ ] Add an opt-in `UseTuiWindowChrome` bool on `TerminalWindow` (default `true`) that sets
      `SystemDecorations = SystemDecorations.None` and `ExtendClientAreaToDecorationsHint` as needed
      when enabled; leave native chrome when disabled.
- [ ] Wire the flag through `AvaloniaDisplayBuilder` with a `.WithTuiWindowChrome(bool = true)` method
      and apply it in `Build()` alongside `ApplyGameWindowChrome`.
- [ ] Ensure `MinWidth`/`MinHeight` still enforced so the button clusters always fit.

### Verification Plan
- `dotnet build src/Faerie.Terminal.Avalonia/Faerie.Terminal.Avalonia.csproj -c Debug` → builds with
  no new warnings/errors.
- New unit test `HostPlatform_Current_MatchesOperatingSystem` and
  `WindowControlLayout_OrdersButtons_PerPlatform` pass: `dotnet test tests/Faerie.Tests/Faerie.Tests.csproj
  --filter FullyQualifiedName~WindowChrome` → all green.

### Phase Summary
_(write when phase completes)_

## Phase 2: Render TUI window controls in row 0
Status: Not started

- [ ] In `TerminalControl`, when TUI chrome is enabled, force row 0 to render as a chrome row even if
      `buf.TitleEnabled` is false (extend `RowCells`/`TitleOrStatus` path so chrome always occupies
      row 0).
- [ ] Draw the control glyphs from `WindowControlLayout`: macOS uses filled circles (`●`) colored
      red/yellow/green; Windows/Linux use minimize/maximize/close glyphs (e.g. `—` `▢` `✕`) in the
      title style. Reserve those columns and clip the game title text to the region between clusters.
- [ ] Add hover state: track pointer-over-button in `OnPointerMoved`/`OnPointerExited` and repaint the
      hovered button (e.g. brighten, or reveal macOS glyphs on hover) so controls feel interactive.
- [ ] Confirm background-run and glyph rendering (`RenderRow`/`RenderGlyph`) handle the per-cell button
      colors without seams.

### Verification Plan
- `dotnet build src/Faerie.Terminal.Avalonia/Faerie.Terminal.Avalonia.csproj -c Debug` → clean build.
- Unit test `WindowControlLayout_ClipsTitleBetweenClusters` (title text never overlaps button columns)
  passes.
- Manual GUI smoke test (author, Windows): run a sample (`dotnet run --project
  src/Faerie.Samples.Zork`) → window opens borderless; min/max/close glyphs visible top-right; game
  title visible and not overlapping buttons.

### Phase Summary
_(write when phase completes)_

## Phase 3: Interaction — click commands, drag-to-move, edge/corner resize
Status: Not started

- [ ] Route `OnPointerPressed` on a button cell to the matching command (Close → `Window.Close()`;
      Minimize → `WindowState.Minimized`; Maximize → toggle `Normal`/`Maximized`) via a chrome event
      exposed by `TerminalControl` and handled in `TerminalWindow`. Button clicks must not start text
      selection.
- [ ] Drag-to-move: `PointerPressed` on the title-row region outside the buttons calls
      `BeginMoveDrag(e)` on the hosting window.
- [ ] 8-direction resize: hit-test a few px inside each edge/corner; on press in a resize zone call
      `BeginResizeDrag(WindowEdge.*, e)`. Update the cursor (`Cursor`) per zone for affordance.
      (Cell-snapping of the resulting size is handled in Phase 4.)
- [ ] Ensure resize/move hit-testing does not interfere with in-grid text selection (selection only
      when press is inside the game text area, not the chrome row or resize border).

### Verification Plan
- `dotnet build` of the Avalonia project → clean.
- Unit test `WindowControlLayout_HitTest_ReturnsButtonAtColumn` and edge-zone hit-test math tests pass.
- Manual GUI smoke test (author): clicking close/min/max works; dragging the title row moves the
  window; dragging each edge/corner resizes; text selection still works inside the game area.

### Phase Summary
_(write when phase completes)_

## Phase 4: Cell-snapped window sizing (no black border)
Status: Not started

- [ ] Add a `SnapClientSize(Size proposed) -> Size` helper (pure, testable) that rounds a proposed
      client size to the nearest whole cell: `cols = round(width / _cellWidth)`,
      `rows = round(height / _cellHeight)`, returning `cols x _cellWidth` by `rows x _cellHeight`,
      clamped to `MinWidth`/`MinHeight`.
- [ ] Snap on interactive resize: after `BeginResizeDrag`, snap the window client size to cell
      multiples when the drag settles (handle window resize/size-changed), so no partial cells remain.
      Skip snapping while `WindowState` is `Maximized` or `FullScreen`.
- [ ] Snap on zoom: change `SetFontSize`/`ZoomBy`/`ResetZoom` so that in the normal state they hold the
      current `cols x rows` constant and resize the window to `cols x newCellWidth` by
      `rows x newCellHeight` (clamped to the working-area screen bounds), instead of keeping the window
      fixed and reflowing the grid. When clamped by the screen, fall back to reflowing.
- [ ] Gate the centered black border: keep the `offsetX`/`offsetY` centering in `Render` active only
      when `Maximized`/`FullScreen` (or transiently mid-resize); in the normal snapped state the offsets
      compute to ~0 so the grid meets the window edges with no border.
- [ ] Set the initial window size on open to a snapped size for the default `cols x rows` so the app
      launches border-free.

### Verification Plan
- `dotnet build src/Faerie.Terminal.Avalonia/Faerie.Terminal.Avalonia.csproj -c Debug` → clean build.
- Unit tests pass: `SnapClientSize_RoundsToWholeCells`, `SnapClientSize_RespectsMinimums`,
  and `Zoom_KeepsGridDimensions_AndResizesWindow` (drive cell metrics via injected values, no live
  window) → all green.
- Manual GUI smoke test (author, Windows): dragging any edge/corner leaves no black gap between the
  window edge and text in the normal state; zooming in/out grows/shrinks the window while keeping the
  same column/row count and no border; maximizing/fullscreen shows the centered black border and text
  stays crisp.

### Phase Summary
_(write when phase completes)_

## Phase 5: Fullscreen reconciliation, fallback, tests & docs
Status: Not started

- [ ] Reconcile F11/Alt+Enter fullscreen (currently in `TerminalWindow.OnKeyDown`) with borderless
      chrome: decide chrome visibility in fullscreen (recommend: keep row-0 chrome; Maximize button
      reflects state) and verify toggling in/out restores the borderless snapped normal state correctly.
- [ ] Verify `UseTuiWindowChrome=false` cleanly restores native decorations (regression path).
- [ ] Finalize unit tests in `tests/Faerie.Tests/` covering `HostPlatform`, layout ordering, title
      clipping, and hit-testing for all three platforms (drive platform via injected value, not the
      live OS, so tests are deterministic on CI).
- [ ] Update `src/Faerie.Terminal.Avalonia/README.md` and `docs/FEATURES.md` to describe the TUI
      window chrome and the `WithTuiWindowChrome` option.

### Verification Plan
- Full solution build: `dotnet build Faerie.slnx -c Debug` → clean.
- Full test run: `dotnet test tests/Faerie.Tests/Faerie.Tests.csproj` → all green (matches CI in
  `.github/workflows/ci.yml`).
- Manual GUI smoke test (author): F11 toggles fullscreen and back without losing chrome or controls;
  `WithTuiWindowChrome(false)` shows native title bar.

### Phase Summary
_(write when phase completes)_

## Final Recap
_(write when all phases complete)_

## Deployment Plan
_(write when all phases complete)_
