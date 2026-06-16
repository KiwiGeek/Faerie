using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Faerie.Presentation;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// Renders a <see cref="TerminalBuffer"/> as a fake text screen: a character grid that fills the
/// control, drawn at true pixel size with a monospaced font, with blinking text and a blinking
/// caret. It runs a simple line editor (raising <see cref="CommandEntered"/> on Enter), supports
/// scrollback (mouse wheel / PageUp-PageDown) and zoom (Ctrl + wheel, Ctrl +/-/0), and resizes the
/// grid to fit the window. Because the buffer stores logical lines and wraps on demand, zooming and
/// resizing re-paginate the text automatically. Font and caret come from the game definition via
/// <see cref="FontSpec"/> and <see cref="CursorStyle"/>.
/// </summary>
public sealed class TerminalControl : Control
{
    private const double MinFontSize = 8.0;
    private const double MaxFontSize = 48.0;
    private const double DefaultFontSize = 16.0;

    static TerminalControl()
    {
        AffectsRender<TerminalControl>(BackgroundProperty);
    }

    private static readonly Cursor HiddenCursor = new(StandardCursorType.None);

    public TerminalControl()
    {
        Focusable = true;
        Cursor = HiddenCursor;   // hide the OS pointer; we draw our own cell highlight instead
        (_typeface, _boldTypeface) = TerminalFont.Resolve(null);
        MeasureCell();

        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _blinkTimer.Tick += (_, _) => { _blinkOn = !_blinkOn; InvalidateVisual(); };
    }

    // ---- styled properties ----------------------------------------------------------------

    /// <summary>The control's backdrop brush (the colour outside the centred grid). Themeable.</summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TerminalControl, IBrush?>(nameof(Background), Brushes.Black);

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    // ---- fields ---------------------------------------------------------------------------

    private Typeface _typeface;
    private Typeface _boldTypeface;
    private readonly Dictionary<uint, IBrush> _brushCache = [];
    private readonly DispatcherTimer _blinkTimer;

    private double _fontSize = DefaultFontSize;
    private double _cellWidth;
    private double _cellHeight;

    private bool _blinkOn = true;
    private bool _inputActive;
    private string _input = "";
    private int _scrollOffset;   // rows scrolled up into history (0 = live/bottom)
    private bool _started;
    private string? _fontSpec;

    private double _gridOffsetX, _gridOffsetY;   // last render's centring offset (for pointer hit-testing)
    private int _pointerCol = -1, _pointerRow = -1;

    /// <summary>The font spec from the game definition (see <see cref="TerminalFont"/>). Null = monospace.</summary>
    public string? FontSpec
    {
        get => _fontSpec;
        set
        {
            _fontSpec = value;
            (_typeface, _boldTypeface) = TerminalFont.Resolve(value);
            MeasureCell();
            ApplyGrid(Bounds.Size);
            InvalidateVisual();
        }
    }

    /// <summary>The caret shape, chosen by the game.</summary>
    public TerminalCursor CursorStyle { get; set; } = TerminalCursor.Block;

    /// <summary>The current font size in device-independent pixels.</summary>
    public double FontSize
    {
        get => _fontSize;
        set => SetFontSize(value);
    }

    private TerminalBuffer? _buffer;
    public TerminalBuffer? Buffer
    {
        get => _buffer;
        set
        {
            if (_buffer is not null) _buffer.Invalidated -= OnBufferInvalidated;
            _buffer = value;
            if (_buffer is not null) _buffer.Invalidated += OnBufferInvalidated;
            InvalidateVisual();
        }
    }

    /// <summary>Raised on a UI thread when the player submits a line of input.</summary>
    public event Action<string>? CommandEntered;

    /// <summary>Raised once, after the control is laid out and its buffer first sized.</summary>
    public event Action? Ready;

    /// <summary>Raised when the grid dimensions change after the first layout (resize / font change).</summary>
    public event Action? Resized;

    // ---- layout / sizing ------------------------------------------------------------------

    private void ApplyGrid(Size size)
    {
        if (_buffer is null || _cellWidth <= 0 || _cellHeight <= 0) return;
        if (size.Width < _cellWidth * 4 || size.Height < _cellHeight * 2) return;

        int cols = Math.Max(1, (int)(size.Width / _cellWidth));
        int rows = Math.Max(1, (int)(size.Height / _cellHeight));
        bool changed = cols != _buffer.Columns || rows != _buffer.Rows;
        _buffer.Resize(cols, rows);

        if (!_started)
        {
            _started = true;
            Dispatcher.UIThread.Post(() => Ready?.Invoke());
        }
        else if (changed)
        {
            Dispatcher.UIThread.Post(() => Resized?.Invoke());
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Size result = base.ArrangeOverride(finalSize);
        ApplyGrid(finalSize);
        return result;
    }

    // ---- zoom -----------------------------------------------------------------------------

    private void SetFontSize(double size)
    {
        double clamped = Math.Clamp(size, MinFontSize, MaxFontSize);
        if (Math.Abs(clamped - _fontSize) < 0.01) return;
        _fontSize = clamped;
        MeasureCell();
        ApplyGrid(Bounds.Size);
        InvalidateVisual();
    }

    private void ZoomBy(int steps) => SetFontSize(_fontSize + steps);
    private void ResetZoom() => SetFontSize(DefaultFontSize);

    // ---- input editing --------------------------------------------------------------------

    public void BeginInput()
    {
        _input = "";
        _inputActive = true;
        Focus();
        InvalidateVisual();
    }

    public void EndInput()
    {
        _inputActive = false;
        InvalidateVisual();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _blinkTimer.Start();
        Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _blinkTimer.Stop();
    }

    private void OnBufferInvalidated() => Dispatcher.UIThread.Post(() =>
    {
        _scrollOffset = 0; // new output snaps the view back to the bottom
        InvalidateVisual();
    });

    // ---- mouse cell highlight -------------------------------------------------------------

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdatePointerCell(e.GetPosition(this));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_pointerCol != -1 || _pointerRow != -1)
        {
            _pointerCol = _pointerRow = -1;
            InvalidateVisual();
        }
    }

    private void UpdatePointerCell(Point p)
    {
        int col = -1, row = -1;
        if (_buffer is not null && _cellWidth > 0 && _cellHeight > 0)
        {
            col = (int)Math.Floor((p.X - _gridOffsetX) / _cellWidth);
            row = (int)Math.Floor((p.Y - _gridOffsetY) / _cellHeight);
            if (col < 0 || row < 0 || col >= _buffer.Columns || row >= _buffer.Rows)
            {
                col = row = -1;
            }
        }

        if (col != _pointerCol || row != _pointerRow)
        {
            _pointerCol = col;
            _pointerRow = row;
            InvalidateVisual();
        }
    }

    private int MaxScroll()
    {
        if (_buffer is null) return 0;
        return Math.Max(0, _buffer.WrappedRows().Count - _buffer.VisibleRows);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (_buffer is null) return;

        // Ctrl + wheel zooms the font (and re-flows the grid). Plain wheel scrolls history.
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ZoomBy(e.Delta.Y > 0 ? 1 : -1);
            e.Handled = true;
            return;
        }

        int step = e.Delta.Y > 0 ? 3 : (e.Delta.Y < 0 ? -3 : 0);
        _scrollOffset = Math.Clamp(_scrollOffset + step, 0, MaxScroll());
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        if (!_inputActive || e.Text is null) return;

        _scrollOffset = 0;
        foreach (char c in e.Text)
            if (!char.IsControl(c)) _input += c;

        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Alt+Enter (fullscreen) bubbles up to the window.
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            return;

        // Ctrl +/-/0 keyboard zoom.
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.OemPlus or Key.Add:
                    ZoomBy(1); e.Handled = true; return;
                case Key.OemMinus or Key.Subtract:
                    ZoomBy(-1); e.Handled = true; return;
                case Key.D0 or Key.NumPad0:
                    ResetZoom(); e.Handled = true; return;
            }
        }

        // Scrollback paging works whether or not input is active.
        if (_buffer is not null)
        {
            int page = _buffer.VisibleRows;
            switch (e.Key)
            {
                case Key.PageUp:
                    _scrollOffset = Math.Clamp(_scrollOffset + page, 0, MaxScroll());
                    InvalidateVisual(); e.Handled = true; return;
                case Key.PageDown:
                    _scrollOffset = Math.Clamp(_scrollOffset - page, 0, MaxScroll());
                    InvalidateVisual(); e.Handled = true; return;
            }
        }

        if (!_inputActive) return;

        _scrollOffset = 0;
        switch (e.Key)
        {
            case Key.Enter:
                EchoAndCommit(_input);
                e.Handled = true;
                break;
            case Key.Back:
                if (_input.Length > 0) _input = _input[..^1];
                InvalidateVisual();
                e.Handled = true;
                break;
            case Key.Escape:
                _input = "";
                InvalidateVisual();
                e.Handled = true;
                break;
        }
    }

    private void EchoAndCommit(string line)
    {
        if (_buffer is not null)
        {
            _buffer.Write(line, _buffer.DefaultStyle);
            _buffer.NewLine();
        }
        _input = "";
        _inputActive = false;
        CommandEntered?.Invoke(line);
    }

    // ---- rendering ------------------------------------------------------------------------

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Background ?? Brushes.Black, new Rect(Bounds.Size));

        if (_buffer is null || _cellWidth <= 0 || _cellHeight <= 0) return;

        IReadOnlyList<GlyphCell[]> wrapped = _buffer.WrappedRows();
        int total = wrapped.Count;
        int visible = _buffer.VisibleRows;
        int maxScroll = Math.Max(0, total - visible);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, maxScroll);
        int windowTop = Math.Max(0, total - visible - _scrollOffset);

        double gridW = _buffer.Columns * _cellWidth;
        double gridH = _buffer.Rows * _cellHeight;
        double offsetX = Math.Max(0, Math.Floor((Bounds.Width - gridW) / 2));
        double offsetY = Math.Max(0, Math.Floor((Bounds.Height - gridH) / 2));
        _gridOffsetX = offsetX;
        _gridOffsetY = offsetY;

        GlyphCell[] blankRow = BlankRow();

        using (context.PushTransform(Matrix.CreateTranslation(offsetX, offsetY)))
        {
            for (int screenRow = 0; screenRow < _buffer.Rows; screenRow++)
                RenderRow(context, screenRow, wrapped, total, windowTop, blankRow);

            if (_inputActive && _scrollOffset == 0)
                RenderInput(context, windowTop);

            if (_scrollOffset > 0)
                RenderScrollbackIndicator(context);

            // The mouse pointer inverts the colours of whatever cell it is over (old text-UI style).
            if (_pointerCol >= 0 && _pointerRow >= 0)
                RenderInvertedCell(context, _pointerRow, _pointerCol, wrapped, total, windowTop, blankRow);
        }
    }

    private void RenderInvertedCell(DrawingContext context, int screenRow, int col,
        IReadOnlyList<GlyphCell[]> wrapped, int total, int windowTop, GlyphCell[] blankRow)
    {
        GlyphCell[] cells = RowCells(screenRow, wrapped, total, windowTop, blankRow);
        if (col >= cells.Length) return;
        GlyphCell cell = cells[col];

        // Effective colours (after any Inverse attribute), then swap them for the cursor.
        (TerminalColor fg, TerminalColor bg) = Effective(cell.Style);
        double x = col * _cellWidth;
        double y = screenRow * _cellHeight;

        context.FillRectangle(BrushFor(fg), new Rect(x, y, _cellWidth, _cellHeight));
        if (cell.Glyph != ' ')
        {
            Typeface tf = cell.Style.Attributes.HasFlag(TextAttributes.Bold) ? _boldTypeface : _typeface;
            FormattedText ft = new(
                cell.Glyph.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                tf, _fontSize, BrushFor(bg));
            context.DrawText(ft, new Point(x, y));
        }
    }

    private GlyphCell[] BlankRow()
    {
        GlyphCell[] row = new GlyphCell[_buffer!.Columns];
        GlyphCell blank = new(' ', _buffer.DefaultStyle);
        for (int i = 0; i < row.Length; i++) row[i] = blank;
        return row;
    }

    private GlyphCell[] RowCells(int screenRow, IReadOnlyList<GlyphCell[]> wrapped, int total, int windowTop, GlyphCell[] blankRow)
    {
        TerminalBuffer buf = _buffer!;
        if (buf.TitleEnabled && screenRow == 0)
            return TitleOrStatus(buf.TitleCell, buf.DefaultStyle);
        if (buf.StatusEnabled && screenRow == buf.Rows - 1)
            return TitleOrStatus(buf.StatusCell, new TextStyle(TerminalColor.Black, TerminalColor.LightGray));

        int textIdx = screenRow - buf.TextTop;
        int wrappedIdx = windowTop + textIdx;
        return wrappedIdx >= 0 && wrappedIdx < total ? wrapped[wrappedIdx] : blankRow;
    }

    private GlyphCell[] TitleOrStatus(Func<int, GlyphCell?> source, TextStyle fill)
    {
        GlyphCell[] row = new GlyphCell[_buffer!.Columns];
        for (int c = 0; c < row.Length; c++)
            row[c] = source(c) ?? new GlyphCell(' ', fill);
        return row;
    }

    private void RenderRow(DrawingContext context, int screenRow, IReadOnlyList<GlyphCell[]> wrapped, int total, int windowTop, GlyphCell[] blankRow)
    {
        GlyphCell[] cells = RowCells(screenRow, wrapped, total, windowTop, blankRow);
        int columns = _buffer!.Columns;

        // Backgrounds in horizontal runs of identical colour (one rect per run = no seams).
        int col = 0;
        while (col < columns)
        {
            TerminalColor bg = Effective(cells[col].Style).bg;
            int start = col;
            do { col++; }
            while (col < columns && Effective(cells[col].Style).bg == bg);
            FillRun(context, screenRow, start, col, bg);
        }

        for (int c = 0; c < columns; c++)
            RenderGlyph(context, screenRow, c, cells[c]);
    }

    private static (TerminalColor fg, TerminalColor bg) Effective(TextStyle style) =>
        style.Attributes.HasFlag(TextAttributes.Inverse)
            ? (style.Background, style.Foreground)
            : (style.Foreground, style.Background);

    private void FillRun(DrawingContext context, int row, int startCol, int endColExclusive, TerminalColor bg)
    {
        double x = startCol * _cellWidth;
        double y = row * _cellHeight;
        double w = (endColExclusive - startCol) * _cellWidth;
        context.FillRectangle(BrushFor(bg), new Rect(x, y, w, _cellHeight));
    }

    private void RenderGlyph(DrawingContext context, int row, int col, GlyphCell cell)
    {
        TextStyle style = cell.Style;
        (TerminalColor fgColor, _) = Effective(style);

        double x = col * _cellWidth;
        double y = row * _cellHeight;

        bool hidden = style.Attributes.HasFlag(TextAttributes.Blink) && !_blinkOn;
        if (cell.Glyph != ' ' && !hidden)
        {
            Typeface tf = style.Attributes.HasFlag(TextAttributes.Bold) ? _boldTypeface : _typeface;
            FormattedText ft = new(
                cell.Glyph.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                tf, _fontSize, BrushFor(fgColor));
            context.DrawText(ft, new Point(x, y));
        }

        if (style.Attributes.HasFlag(TextAttributes.Underline) && !hidden)
        {
            double uy = y + _cellHeight - 1.5;
            context.DrawLine(new Pen(BrushFor(fgColor), 1.0), new Point(x, uy), new Point(x + _cellWidth, uy));
        }
    }

    private void RenderInput(DrawingContext context, int windowTop)
    {
        TerminalBuffer buf = _buffer!;
        (int caretRow, int caretCol) = buf.CursorPosition();

        int screenRow = buf.TextTop + (caretRow - windowTop);
        int col = caretCol;

        foreach (char c in _input)
        {
            if (col >= buf.Columns) { col = 0; screenRow++; }
            if (screenRow > buf.TextBottom) return;
            if (screenRow >= buf.TextTop)
                RenderGlyph(context, screenRow, col, new GlyphCell(c, buf.DefaultStyle));
            col++;
        }

        if (col >= buf.Columns) { col = 0; screenRow++; }
        if (_blinkOn && screenRow >= buf.TextTop && screenRow <= buf.TextBottom)
            RenderCaret(context, col, screenRow);
    }

    private void RenderCaret(DrawingContext context, int col, int row)
    {
        double x = col * _cellWidth;
        double y = row * _cellHeight;
        IBrush brush = BrushFor(_buffer!.DefaultStyle.Foreground);

        switch (CursorStyle)
        {
            case TerminalCursor.Underline:
                context.FillRectangle(brush, new Rect(x, y + _cellHeight - Math.Max(2, _cellHeight * 0.12), _cellWidth, Math.Max(2, _cellHeight * 0.12)));
                break;
            case TerminalCursor.Bar:
                context.FillRectangle(brush, new Rect(x, y, Math.Max(2, _cellWidth * 0.15), _cellHeight));
                break;
            case TerminalCursor.Outline:
                context.DrawRectangle(null, new Pen(brush, 1.0), new Rect(x + 0.5, y + 0.5, _cellWidth - 1, _cellHeight - 1));
                break;
            default:
                context.FillRectangle(brush, new Rect(x, y, _cellWidth, _cellHeight));
                break;
        }
    }

    private void RenderScrollbackIndicator(DrawingContext context)
    {
        TerminalBuffer buf = _buffer!;
        string label = " -- SCROLLBACK (PgDn / scroll down to resume) -- ";
        if (label.Length > buf.Columns) label = " -- SCROLLBACK -- ";

        int row = buf.TextBottom;
        int start = Math.Max(0, buf.Columns - label.Length);
        TextStyle style = new(TerminalColor.Black, TerminalColor.Amber, TextAttributes.Bold);

        FillRun(context, row, start, buf.Columns, TerminalColor.Amber);
        for (int i = 0; i < label.Length && start + i < buf.Columns; i++)
            RenderGlyph(context, row, start + i, new GlyphCell(label[i], style));
    }

    // ---- helpers --------------------------------------------------------------------------

    private IBrush BrushFor(TerminalColor color)
    {
        uint key = color.ToArgb();
        if (_brushCache.TryGetValue(key, out IBrush? brush)) return brush;
        SolidColorBrush created = new(Color.FromArgb(255, color.R, color.G, color.B));
        _brushCache[key] = created;
        return created;
    }

    private void MeasureCell()
    {
        try
        {
            FormattedText probe = new("M", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeface, _fontSize, Brushes.White);
            _cellWidth = Math.Ceiling(probe.WidthIncludingTrailingWhitespace);
            _cellHeight = Math.Ceiling(probe.Height);
        }
        catch
        {
            _cellWidth = 0;
            _cellHeight = 0;
        }

        if (_cellWidth <= 0) _cellWidth = Math.Ceiling(_fontSize * 0.6);
        if (_cellHeight <= 0) _cellHeight = Math.Ceiling(_fontSize * 1.2);
    }
}
