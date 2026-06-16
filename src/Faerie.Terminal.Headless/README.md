# Faerie.Terminal.Headless

No-UI host for the Faerie engine: read commands from a script file, write a plain-text transcript.

```bash
dotnet run --project src/Faerie.Samples.Zork -- --script walkthrough.txt --transcript session.txt
```

Script lines starting with `#` or `;` are comments; blank lines are skipped. Each command is logged
as `> command` in the transcript, followed by stripped game output.
