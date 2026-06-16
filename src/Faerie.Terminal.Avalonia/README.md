# Faerie.Terminal.Avalonia

Avalonia "fake terminal" front end for the [**Faerie**](https://www.nuget.org/packages/Faerie)
text-adventure engine.

Renders [`Faerie.Terminal`](https://www.nuget.org/packages/Faerie.Terminal)'s character-cell framebuffer
as an on-screen text display: true colour with blink/underline/bold/inverse, scrollback, zoom,
title/status bars, and a game-supplied font and cursor.

Install this package to build a desktop (Avalonia) text adventure; it pulls in `Faerie.Terminal` and
`Faerie` transitively. Ships **no fonts** of its own — a game supplies its own font via the game
definition (`WithFont(...)`).
