namespace Faerie.Presentation;

/// <summary>
/// High-level styled output built on top of an <see cref="ITerminal"/>. Parses the markup language
/// into styled runs and writes them; <b>word-wrapping is the terminal's job</b> (the terminal stores
/// logical lines and wraps to its current width), so this writer only emits runs and hard newlines.
/// </summary>
public sealed class OutputWriter(ITerminal terminal)
{
    private bool _filtering;

    public ITerminal Terminal { get; } = terminal;
    public TextStyle BaseStyle { get; set; } = terminal.DefaultStyle;

    /// <summary>
    /// Optional output filter applied to every piece of game text just before it reaches the terminal.
    /// Receives the marked-up string and returns a (possibly rewritten) string to print, or
    /// <c>null</c> to suppress it entirely (no text, no newline). This is the seam behind effects like
    /// an echoing "loud room", mirror text, or drunk vision; a game installs one via
    /// <c>GameBuilder.FilterOutput(...)</c>.
    /// <para>
    /// Title/status bars do <b>not</b> pass through here, and the filter is bypassed for any output a
    /// filter itself produces (so a filter that writes output can't recurse infinitely).
    /// </para>
    /// </summary>
    public Func<string, string?>? Transform { get; set; }

    /// <summary>Retained for source compatibility; wrapping no longer depends on a tracked column.</summary>
    public void SyncColumn(int column) { _ = column; }

    public void Clear() => Terminal.Clear();

    public void NewLine() => Terminal.NewLine();

    /// <summary>Writes marked-up text (no trailing newline). The terminal wraps it to the current width.</summary>
    public void Print(string markup) => Emit(markup, newLine: false);

    /// <summary>Writes marked-up text followed by a newline.</summary>
    public void PrintLine(string markup = "") => Emit(markup, newLine: true);

    /// <summary>Writes a blank line.</summary>
    public void Blank() => NewLine();

    private void Emit(string markup, bool newLine)
    {
        if (Transform is { } transform && !_filtering)
        {
            _filtering = true;
            string? filtered;
            try { filtered = transform(markup); }
            finally { _filtering = false; }
            if (filtered is null) return;   // filter suppressed this output entirely
            markup = filtered;
        }

        foreach (StyledSpan span in Markup.Parse(markup, BaseStyle))
            Terminal.Write(span.Text, span.Style);

        if (newLine) NewLine();
    }
}
