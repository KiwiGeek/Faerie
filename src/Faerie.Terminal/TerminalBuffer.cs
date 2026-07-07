using Faerie.Presentation;

namespace Faerie.Terminal;

/// <summary>
/// A text screen model that stores <b>logical lines</b> (runs of styled text separated only by hard
/// newlines) and word-wraps them to the current width on demand. Because wrapping is dynamic, the
/// display re-paginates automatically when the width changes (window resize or font zoom). It
/// implements the engine's <see cref="ITerminal"/> and adds a non-scrolling title row and an optional
/// status row. Scrollback is simply the wrapped rows above the visible window -- no separate store.
/// </summary>
public sealed class TerminalBuffer : ITerminal
{
    /// <summary>Maximum retained logical lines (older lines are trimmed).</summary>
    public const int MaxLines = 5000;

    private readonly record struct Run(string Text, TextStyle Style);

    private readonly List<List<Run>> _lines = [];
    private GlyphCell[]? _titleRow;
    private GlyphCell[]? _statusRow;
    private bool _titleEnabled;
    private bool _statusEnabled;

    // Wrap cache (rebuilt when content or width changes).
    private List<GlyphCell[]>? _wrapCache;
    private List<int> _rowLine = [];   // logical-line index each wrapped row belongs to (paragraph map)
    private int _wrapWidth = -1;
    private int _cursorRow;
    private int _cursorCol;

    public TerminalBuffer(int columns = 80, int rows = 25, TextStyle? defaultStyle = null)
    {
        Columns = Math.Max(1, columns);
        Rows = Math.Max(1, rows);
        DefaultStyle = defaultStyle ?? new TextStyle(TerminalColor.LightGray, TerminalColor.Black);
        _lines.Add([]);
    }

    public int Columns { get; private set; }
    public int Rows { get; private set; }
    public TextStyle DefaultStyle { get; }

    public int TextTop => _titleEnabled ? 1 : 0;
    public int TextBottom => _statusEnabled ? Rows - 2 : Rows - 1;
    public int VisibleRows => Math.Max(1, TextBottom - TextTop + 1);

    public bool SupportsBars => true;
    public bool TitleEnabled => _titleEnabled;
    public bool StatusEnabled => _statusEnabled;

    /// <summary>Columns reserved for host window chrome at the left of the title bar (issue #116).</summary>
    public int TitleBarInsetLeft { get; set; }

    /// <summary>Columns reserved for host window chrome at the right of the title bar (issue #116).</summary>
    public int TitleBarInsetRight { get; set; }

    /// <summary>True once a title-bar row has been set (i.e. the game defined a title bar).</summary>
    public bool HasTitleContent => _titleRow is not null;

    public event Action? Invalidated;

    // ---- ITerminal ------------------------------------------------------------------------

    public void Write(string text, TextStyle style)
    {
        foreach (string part in SplitKeepingNewlines(text))
        {
            if (part == "\n") _lines.Add([]);
            else if (part.Length > 0) _lines[^1].Add(new Run(part, style));
        }
        TrimAndInvalidate();
    }

    public void NewLine()
    {
        _lines.Add([]);
        TrimAndInvalidate();
    }

    public void OverwriteLine(string text, TextStyle style)
    {
        if (_lines.Count == 0) _lines.Add([]);
        _lines[^1] = text.Length > 0 ? [new Run(text, style)] : [];
        TrimAndInvalidate();
    }

    public void Clear()
    {
        _lines.Clear();
        _lines.Add([]);
        Dirty();
    }

    public void ConfigureBars(bool titleBar, bool statusBar)
    {
        _titleEnabled = titleBar;
        _statusEnabled = statusBar;
        Dirty();
    }

    public void SetTitleBar(IReadOnlyList<GlyphCell> cells) { _titleRow = Fit(cells); Invalidated?.Invoke(); }
    public void SetStatusBar(IReadOnlyList<GlyphCell>? cells) { _statusRow = cells is null ? null : Fit(cells); Invalidated?.Invoke(); }

    /// <summary>Changes the grid dimensions. Wrapping (and thus pagination) recomputes for the new width.</summary>
    public void Resize(int columns, int rows)
    {
        columns = Math.Max(1, columns);
        rows = Math.Max(1, rows);
        if (columns == Columns && rows == Rows) return;
        Columns = columns;
        Rows = rows;
        _titleRow = null;
        _statusRow = null;
        Dirty();
    }

    // ---- display --------------------------------------------------------------------------

    public GlyphCell? TitleCell(int col) => _titleRow is null || col >= _titleRow.Length ? null : _titleRow[col];
    public GlyphCell? StatusCell(int col) => _statusRow is null || col >= _statusRow.Length ? null : _statusRow[col];

    /// <summary>All wrapped display rows for the current width (cached).</summary>
    public IReadOnlyList<GlyphCell[]> WrappedRows()
    {
        if (_wrapCache is not null && _wrapWidth == Columns) return _wrapCache;
        Rewrap();
        return _wrapCache!;
    }

    /// <summary>
    /// Logical-line index for each wrapped row (parallel to <see cref="WrappedRows"/>). Hard newlines
    /// delimit logical lines; a long line may span several wrapped rows sharing the same index.
    /// </summary>
    public IReadOnlyList<int> WrappedRowLines()
    {
        WrappedRows();
        return _rowLine;
    }

    /// <summary>The caret position (wrapped row index and column) at the end of all content.</summary>
    public (int row, int col) CursorPosition()
    {
        WrappedRows();
        return (_cursorRow, _cursorCol);
    }

    // ---- internals ------------------------------------------------------------------------

    private void Dirty()
    {
        _wrapCache = null;
        Invalidated?.Invoke();
    }

    private void TrimAndInvalidate()
    {
        if (_lines.Count > MaxLines)
            _lines.RemoveRange(0, _lines.Count - MaxLines);
        Dirty();
    }

    private void Rewrap()
    {
        int w = Columns;
        List<GlyphCell[]> rows = [];
        List<int> rowLine = [];
        GlyphCell blank = new(' ', DefaultStyle);

        List<GlyphCell> cur = [];
        int lastRowLen = 0;
        int logicalLine = 0;

        void FlushRow()
        {
            GlyphCell[] arr = new GlyphCell[w];
            for (int i = 0; i < w; i++) arr[i] = i < cur.Count ? cur[i] : blank;
            rows.Add(arr);
            rowLine.Add(logicalLine);
            lastRowLen = cur.Count;
            cur.Clear();
        }

        foreach (List<Run> line in _lines)
        {
            List<GlyphCell> word = [];

            void EmitWord()
            {
                if (word.Count == 0) return;
                if (word.Count > w)
                {
                    // Word longer than a row: hard-break it.
                    foreach (GlyphCell c in word)
                    {
                        if (cur.Count >= w) FlushRow();
                        cur.Add(c);
                    }
                    word.Clear();
                    return;
                }
                if (cur.Count + word.Count > w) FlushRow();
                cur.AddRange(word);
                word.Clear();
            }

            foreach (Run run in line)
            {
                foreach (char ch in run.Text)
                {
                    if (ch == ' ')
                    {
                        EmitWord();
                        if (cur.Count >= w) FlushRow();
                        else if (cur.Count > 0) cur.Add(new GlyphCell(' ', run.Style));
                    }
                    else
                    {
                        word.Add(new GlyphCell(ch, run.Style));
                    }
                }
            }

            EmitWord();
            FlushRow(); // each logical line ends with a hard break -> at least one display row
            logicalLine++;
        }

        // The caret sits at the end of the final logical line's last display row.
        _cursorRow = rows.Count - 1;
        _cursorCol = lastRowLen;

        _wrapCache = rows;
        _rowLine = rowLine;
        _wrapWidth = w;
    }

    private GlyphCell[] Fit(IReadOnlyList<GlyphCell> cells)
    {
        GlyphCell[] row = new GlyphCell[Columns];
        TextStyle fill = cells.Count > 0 ? cells[0].Style : DefaultStyle;
        for (int c = 0; c < Columns; c++)
            row[c] = c < cells.Count ? cells[c] : new GlyphCell(' ', fill);
        return row;
    }

    private static IEnumerable<string> SplitKeepingNewlines(string text)
    {
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                if (i > start) yield return text[start..i];
                yield return "\n";
                start = i + 1;
            }
            else if (text[i] == '\r')
            {
                if (i > start) yield return text[start..i];
                start = i + 1;
            }
        }
        if (start < text.Length) yield return text[start..];
    }
}
