using Faerie.Presentation;

namespace Faerie.Terminal.Headless;

/// <summary>
/// An <see cref="ITerminal"/> that writes plain text to a <see cref="TextWriter"/>, stripping markup tags.
/// </summary>
public sealed class TranscriptTerminal(TextWriter writer) : ITerminal
{
    private string? _pendingOverwriteLine;

    public int Columns => 80;
    public int Rows => 25;
    public TextStyle DefaultStyle => TextStyle.Default;

    public void Write(string text, TextStyle style)
    {
        FlushPendingOverwrite(asLine: false);
        writer.Write(Markup.Strip(text));
        writer.Flush();
    }

    public void OverwriteLine(string text, TextStyle style) =>
        _pendingOverwriteLine = Markup.Strip(text);

    public void NewLine()
    {
        if (_pendingOverwriteLine is not null)
        {
            writer.WriteLine(_pendingOverwriteLine);
            _pendingOverwriteLine = null;
        }
        else
        {
            writer.WriteLine();
        }

        writer.Flush();
    }

    public void Clear() => _pendingOverwriteLine = null;

    private void FlushPendingOverwrite(bool asLine)
    {
        if (_pendingOverwriteLine is null) return;
        if (asLine) writer.WriteLine(_pendingOverwriteLine);
        else writer.Write(_pendingOverwriteLine);
        _pendingOverwriteLine = null;
        writer.Flush();
    }
}
