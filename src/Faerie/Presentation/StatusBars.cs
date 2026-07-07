namespace Faerie.Presentation;

/// <summary>A single rendered character cell: a glyph plus its style.</summary>
public readonly record struct GlyphCell(char Glyph, TextStyle Style);

/// <summary>
/// The content of a title or status bar, expressed as three marked-up segments laid out across the
/// width of the screen. The game definition supplies these via callbacks; the engine composes them
/// into a fixed, non-scrolling row.
/// </summary>
public sealed class BarContent
{
    /// <summary>Marked-up text aligned to the left edge.</summary>
    public string Left { get; set; } = "";

    /// <summary>Marked-up text centred in the row.</summary>
    public string Center { get; set; } = "";

    /// <summary>Marked-up text aligned to the right edge.</summary>
    public string Right { get; set; } = "";

    /// <summary>The base/fill style for the whole row (the bar's background and default colour).</summary>
    public TextStyle Style { get; set; } = new(TerminalColor.Black, TerminalColor.LightGray);
}

/// <summary>Lays bar content out into a fixed-width row of <see cref="GlyphCell"/>.</summary>
public static class BarComposer
{
    public static GlyphCell[] Compose(BarContent content, int width, int leftInset = 0, int rightInset = 0)
    {
        GlyphCell[] row = new GlyphCell[width];
        for (int i = 0; i < width; i++) row[i] = new GlyphCell(' ', content.Style);

        // The usable region excludes any columns reserved for host window chrome (issue #116).
        leftInset = Math.Clamp(leftInset, 0, width);
        rightInset = Math.Clamp(rightInset, 0, width - leftInset);
        int regionStart = leftInset;
        int regionEnd = width - rightInset;                 // exclusive
        int regionWidth = Math.Max(0, regionEnd - regionStart);

        IReadOnlyList<StyledSpan> left = Markup.Parse(content.Left, content.Style);
        IReadOnlyList<StyledSpan> center = Markup.Parse(content.Center, content.Style);
        IReadOnlyList<StyledSpan> right = Markup.Parse(content.Right, content.Style);

        Place(row, left, regionStart, regionEnd);

        int centerLen = Length(center);
        Place(row, center, regionStart + Math.Max(0, (regionWidth - centerLen) / 2), regionEnd);

        int rightLen = Length(right);
        Place(row, right, Math.Max(regionStart, regionEnd - rightLen), regionEnd);

        return row;
    }

    private static int Length(IReadOnlyList<StyledSpan> spans)
    {
        int n = 0;
        foreach (StyledSpan s in spans) n += s.Text.Length;
        return n;
    }

    private static void Place(GlyphCell[] row, IReadOnlyList<StyledSpan> spans, int start, int endExclusive)
    {
        int col = start;
        foreach (StyledSpan span in spans)
        {
            foreach (char c in span.Text)
            {
                if (col < 0 || col >= endExclusive) { col++; continue; }
                row[col] = new GlyphCell(c, span.Style);
                col++;
            }
        }
    }
}
