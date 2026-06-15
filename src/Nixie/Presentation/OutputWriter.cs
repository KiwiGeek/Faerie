namespace Nixie.Presentation;

/// <summary>
/// High-level styled output built on top of an <see cref="ITerminal"/>. Parses the markup language
/// into styled runs and writes them; <b>word-wrapping is the terminal's job</b> (the terminal stores
/// logical lines and wraps to its current width), so this writer only emits runs and hard newlines.
/// </summary>
public sealed class OutputWriter(ITerminal terminal)
{
    public ITerminal Terminal { get; } = terminal;
    public TextStyle BaseStyle { get; set; } = terminal.DefaultStyle;

    /// <summary>Retained for source compatibility; wrapping no longer depends on a tracked column.</summary>
    public void SyncColumn(int column) { _ = column; }

    public void Clear() => Terminal.Clear();

    public void NewLine() => Terminal.NewLine();

    /// <summary>Writes marked-up text (no trailing newline). The terminal wraps it to the current width.</summary>
    public void Print(string markup)
    {
        foreach (StyledSpan span in Markup.Parse(markup, BaseStyle))
            Terminal.Write(span.Text, span.Style);
    }

    /// <summary>Writes marked-up text followed by a newline.</summary>
    public void PrintLine(string markup = "")
    {
        Print(markup);
        NewLine();
    }

    /// <summary>Writes a blank line.</summary>
    public void Blank() => NewLine();
}
