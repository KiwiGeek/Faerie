using System.Text;
using Faerie.Presentation;

namespace Faerie.Tests;

/// <summary>A headless <see cref="ITerminal"/> that accumulates plain text for assertions.</summary>
public sealed class InMemoryTerminal : ITerminal
{
    private readonly StringBuilder _sb = new();

    public int Columns => 80;
    public int Rows => 25;
    public TextStyle DefaultStyle => new(TerminalColor.LightGray, TerminalColor.Black);

    public void Write(string text, TextStyle style) => _sb.Append(text);

    public void OverwriteLine(string text, TextStyle style)
    {
        int lastNewline = _sb.ToString().LastIndexOf('\n');
        _sb.Length = lastNewline >= 0 ? lastNewline + 1 : 0;
        _sb.Append(text);
    }

    public void NewLine() => _sb.Append('\n');
    public void Clear() => _sb.Clear();

    public string Output => _sb.ToString();

    /// <summary>Clears captured output without resetting the screen (handy between turns in tests).</summary>
    public void Reset() => _sb.Clear();
}
