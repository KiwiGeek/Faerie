# Faerie.Terminal

Shared, UI-agnostic terminal model for the [**Faerie**](https://www.nuget.org/packages/Faerie)
text-adventure engine.

Provides `TerminalBuffer` — a true-colour character-cell framebuffer with a non-scrolling title row,
an optional status row, scrollback, and styled cells. It implements Faerie's `ITerminal`, so it is the
shared foundation every front end builds on (e.g.
[`Faerie.Terminal.Avalonia`](https://www.nuget.org/packages/Faerie.Terminal.Avalonia) and a future
`Faerie.Terminal.Console`).

This package has **no UI-toolkit dependency** — it depends only on `Faerie`.
