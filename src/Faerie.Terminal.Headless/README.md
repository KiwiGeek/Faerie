# Faerie.Terminal.Headless

Headless host for the [**Faerie**](https://www.nuget.org/packages/Faerie) text-adventure engine: replay
commands from a script file and write a plain-text session transcript. No window, no Avalonia — suitable
for automated playthroughs, regression tests, and CI.

```bash
dotnet add package Faerie.Terminal.Headless
```

## In your game or test project

```csharp
using Faerie.Terminal.Headless;

Game game = MyGame.Build();
int exit = HeadlessRunner.Run(game, new HeadlessOptions
{
    ScriptPath = "commands.txt",
    TranscriptPath = "session.txt",
    RandomSeed = 42   // optional, for reproducible rolls
});
```

Parse CLI flags with `HeadlessArgs.TryParse(args, out HeadlessOptions? options, out string? error)` — the
sample apps branch to headless mode when `--script` is present.

## Command-line usage (sample apps)

```bash
dotnet run --project src/Faerie.Samples.Zork -- --script walkthrough.txt --transcript session.txt
```

Script lines starting with `#` or `;` are comments; blank lines are skipped. Each command is logged as
`> command` in the transcript, followed by stripped game output.

This package depends only on `Faerie` (which pulls in the engine). It does not reference Avalonia.
