using Faerie.Presentation;
using Faerie.Terminal;
using Xunit;

namespace Faerie.Tests;

public sealed class TerminalBufferTests
{
    [Fact]
    public void OverwriteLine_ReplacesCurrentLineWithoutAppending()
    {
        TerminalBuffer buffer = new(columns: 40, rows: 25);

        buffer.Write("first", TextStyle.Default);
        buffer.NewLine();
        buffer.OverwriteLine("!  #  *", TextStyle.Default);
        buffer.OverwriteLine("^  $  !", TextStyle.Default);
        buffer.NewLine();
        buffer.Write("done", TextStyle.Default);

        IReadOnlyList<GlyphCell[]> rows = buffer.WrappedRows();
        string text = string.Concat(rows.SelectMany(r => r.Select(c => c.Glyph)));
        Assert.Contains("first", text);
        Assert.Contains("^  $  !", text);
        Assert.DoesNotContain("!  #  *", text);
        Assert.Contains("done", text);
    }

    [Fact]
    public void OutputWriter_OverwriteLine_FlattensMarkupOntoOneLine()
    {
        InMemoryTerminal terminal = new();
        OutputWriter writer = new(terminal);

        writer.PrintLine("before");
        writer.OverwriteLine("abc");
        writer.OverwriteLine("xyz");
        writer.NewLine();
        writer.PrintLine("after");

        Assert.Equal("before\nxyz\nafter\n", terminal.Output);
    }
}
