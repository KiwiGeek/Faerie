using System.Text;

namespace Faerie.Terminal.Headless;

/// <summary>Writes every line to two <see cref="TextWriter"/> sinks (e.g. file + console).</summary>
internal sealed class TeeTextWriter(TextWriter primary, TextWriter mirror) : TextWriter
{
    public override Encoding Encoding => primary.Encoding;

    public override void Write(char value)
    {
        primary.Write(value);
        mirror.Write(value);
    }

    public override void Write(string? value)
    {
        primary.Write(value);
        mirror.Write(value);
    }

    public override void WriteLine(string? value)
    {
        primary.WriteLine(value);
        mirror.WriteLine(value);
    }

    public override void Flush()
    {
        primary.Flush();
        mirror.Flush();
    }
}
