using Faerie.Presentation;

namespace Faerie.Terminal.Headless;

/// <summary>
/// An <see cref="ITerminal"/> that writes plain text to a <see cref="TextWriter"/>, stripping markup tags.
/// </summary>
public sealed class TranscriptTerminal(TextWriter writer) : ITerminal
{
    public int Columns => 80;
    public int Rows => 25;
    public TextStyle DefaultStyle => TextStyle.Default;

    public void Write(string text, TextStyle style)
    {
        writer.Write(Markup.Strip(text));
        writer.Flush();
    }

    public void NewLine()
    {
        writer.WriteLine();
        writer.Flush();
    }

    public void Clear() { }
}
