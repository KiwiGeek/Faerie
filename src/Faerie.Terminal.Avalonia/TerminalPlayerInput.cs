using Faerie.Presentation;

namespace Faerie.Terminal.Avalonia;

/// <summary>Blocking mid-turn input on the Avalonia terminal control.</summary>
public sealed class TerminalPlayerInput(TerminalControl terminal) : IPlayerInput
{
    public string ReadLine() => terminal.RunLinePrompt();

    public char ReadKey(ReadOnlySpan<char> validKeys) => terminal.RunKeyPrompt(validKeys);
}
