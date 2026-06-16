# Faerie

**Faerie** is a fluent, UI-agnostic C# engine for building text adventures / interactive fiction — the
*Fluent Adventure Engine*.

- Rooms, things, creatures and verbs wired together by C# reference — no magic strings.
- A flexible parser, verb reactions, and per-turn daemons.
- Output filters (rewrite/echo/suppress text by room or state).
- JSON save/load.

The engine renders through the abstract `Faerie.Presentation.ITerminal`. It has **no UI dependency**;
front ends ship separately:

- [`Faerie.Terminal`](https://www.nuget.org/packages/Faerie.Terminal) — shared character-cell terminal model.
- [`Faerie.Terminal.Avalonia`](https://www.nuget.org/packages/Faerie.Terminal.Avalonia) — Avalonia "fake terminal" front end.

```csharp
var b = GameBuilder.Create("My Adventure").AddStandardVerbs();
var hall = b.Room("Hall").Describe("A plain hall.");
b.StartIn(hall);
Game game = b.Build();
```

See the repository for the authoring guide and sample games.
