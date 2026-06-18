# Faerie.Terminal.Avalonia

Avalonia "fake terminal" front end for the [**Faerie**](https://www.nuget.org/packages/Faerie)
text-adventure engine.

Renders [`Faerie.Terminal`](https://www.nuget.org/packages/Faerie.Terminal)'s character-cell framebuffer
as an on-screen text display: true colour with blink/underline/bold/inverse, scrollback, zoom,
title/status bars, and a game-supplied or built-in retro font and cursor.

Install this package to build a desktop (Avalonia) text adventure; it pulls in `Faerie.Terminal` and
`Faerie` transitively. Bundled fonts: IBM PC video modes, a few PC compatibles, BBC Master/teletext, and 8-bit classics
(Apple II, C64, Spectrum, Atari, TRS-80) — see `BuiltInTerminalFont`. Use
`.WithFont(BuiltInTerminalFont.IbmVga8x16)` or pass a custom font spec string.
