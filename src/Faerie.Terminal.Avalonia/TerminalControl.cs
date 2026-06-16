using System.Globalization;
using System.Text;
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
///
/// Text can be selected with a click-drag (double-click selects a word, triple-click a paragraph,
/// Alt+drag a rectangular block) and copied (Ctrl+C / Ctrl+Insert, or right-click); the clipboard
/// can be pasted into the current input line (Ctrl+V / Shift+Insert).
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
    private TaskCompletionSource<string?>? _slotPickTcs;
    private IReadOnlyList<SaveSlotPicker.SlotOption>? _slotPickOptions;
    private int _slotPickIndex;
    private string _slotPickTyped = "";
    private TaskCompletionSource<string>? _linePromptTcs;
    private TaskCompletionSource<char>? _keyPromptTcs;
    private ReadOnlyMemory<char> _keyPromptValid;
    private readonly List<string> _commandHistory = [];
    private int _historyIndex = -1;   // index into _commandHistory, or -1 for the live draft line
    private string _historyDraft = "";
    private int _scrollOffset;   // rows scrolled up into history (0 = live/bottom)
    private bool _started;
    private string? _fontSpec;

    private double _gridOffsetX, _gridOffsetY;   // last render's centring offset (for pointer hit-testing)
    private int _pointerCol = -1, _pointerRow = -1;
    private int _windowTop;                      // first visible wrapped-row index (set during Render)

    // Text selection, stored in wrapped-row coordinates so it survives scrolling/resizing.
    private bool _selecting;                     // a left-drag is in progress
    private bool _hasSelection;                  // a non-empty selection exists
    private bool _blockSelect;                   // Alt+drag: column-aligned rectangle instead of linear span
    private int _selAnchorRow, _selAnchorCol;    // where the drag began
    private int _selEndRow, _selEndCol;          // where it currently ends
    private static readonly IBrush SelectionBrush = new SolidColorBrush(Color.FromArgb(96, 120, 170, 255));

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

    public void BeginInput(string? prefilled = null)
    {
        _input = prefilled ?? "";
        _inputActive = true;
        _historyIndex = -1;
        _historyDraft = "";
        Focus();
        InvalidateVisual();
    }

    public void EndInput()
    {
        _inputActive = false;
        InvalidateVisual();
    }

    /// <summary>Blocking in-terminal slot picker; must run on the UI thread.</summary>
    internal string? RunSlotPicker(IReadOnlyList<SaveSlotPicker.SlotOption> options, bool forSave)
    {
        _slotPickTcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _slotPickOptions = options;
        _slotPickIndex = 0;
        _slotPickTyped = "";
        _inputActive = false;
        Focus();
        UpdateSlotPickStatus();
        InvalidateVisual();

        Task<string?> pick = _slotPickTcs.Task;
        if (!pick.IsCompleted)
        {
            var frame = new DispatcherFrame();
            pick.ContinueWith(static (_, state) => ((DispatcherFrame)state!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }

        return pick.GetAwaiter().GetResult();
    }

    /// <summary>Blocking mid-turn line read; must run on the UI thread.</summary>
    internal string RunLinePrompt()
    {
        _linePromptTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _input = "";
        _inputActive = true;
        _historyIndex = -1;
        _historyDraft = "";
        Focus();
        InvalidateVisual();

        Task<string> prompt = _linePromptTcs.Task;
        if (!prompt.IsCompleted)
        {
            var frame = new DispatcherFrame();
            prompt.ContinueWith(static (_, state) => ((DispatcherFrame)state!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }

        return prompt.GetAwaiter().GetResult();
    }

    /// <summary>Blocking mid-turn single-key read; must run on the UI thread.</summary>
    internal char RunKeyPrompt(ReadOnlySpan<char> validKeys)
    {
        _keyPromptTcs = new TaskCompletionSource<char>(TaskCreationOptions.RunContinuationsAsynchronously);
        _keyPromptValid = validKeys.ToArray();
        _inputActive = false;
        Focus();
        InvalidateVisual();

        Task<char> prompt = _keyPromptTcs.Task;
        if (!prompt.IsCompleted)
        {
            var frame = new DispatcherFrame();
            prompt.ContinueWith(static (_, state) => ((DispatcherFrame)state!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }

        return prompt.GetAwaiter().GetResult();
    }

    private void CompleteLinePrompt(string line)
    {
        if (_buffer is not null)
        {
            _buffer.Write(line, _buffer.DefaultStyle);
            _buffer.NewLine();
        }

        _linePromptTcs?.TrySetResult(line);
        _linePromptTcs = null;
        _input = "";
        _inputActive = false;
        InvalidateVisual();
    }

    private void CompleteKeyPrompt(char key)
    {
        if (_buffer is not null)
        {
            _buffer.Write(key.ToString(), _buffer.DefaultStyle);
            _buffer.NewLine();
        }

        _keyPromptTcs?.TrySetResult(key);
        _keyPromptTcs = null;
        InvalidateVisual();
    }

    private void HandleKeyPromptKey(KeyEventArgs e)
    {
        if (e.Key is >= Key.A and <= Key.Z)
        {
            char c = (char)('a' + ((int)e.Key - (int)Key.A));
            if (KeyIsValid(c))
            {
                CompleteKeyPrompt(c);
                e.Handled = true;
            }
            return;
        }

        if (e.Key is >= Key.D0 and <= Key.D9)
        {
            char c = (char)('0' + ((int)e.Key - (int)Key.D0));
            if (KeyIsValid(c))
            {
                CompleteKeyPrompt(c);
                e.Handled = true;
            }
            return;
        }

        if (e.Key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            char c = (char)('0' + ((int)e.Key - (int)Key.NumPad0));
            if (KeyIsValid(c))
            {
                CompleteKeyPrompt(c);
                e.Handled = true;
            }
            return;
        }

        if (e.Key == Key.Y && KeyIsValid('y')) { CompleteKeyPrompt('y'); e.Handled = true; return; }
        if (e.Key == Key.N && KeyIsValid('n')) { CompleteKeyPrompt('n'); e.Handled = true; return; }
    }

    private bool KeyIsValid(char c)
    {
        ReadOnlySpan<char> valid = _keyPromptValid.Span;
        for (int i = 0; i < valid.Length; i++)
        {
            if (char.ToLowerInvariant(valid[i]) == char.ToLowerInvariant(c))
                return true;
        }

        return false;
    }

    private void CompleteSlotPick(string? label)
    {
        _slotPickTcs?.TrySetResult(label);
        _slotPickTcs = null;
        _slotPickOptions = null;
        _slotPickTyped = "";
        _buffer?.SetStatusBar(null);
        InvalidateVisual();
    }

    private void UpdateSlotPickStatus()
    {
        if (_buffer is null || _slotPickOptions is null || _slotPickOptions.Count == 0) return;
        var opt = _slotPickOptions[_slotPickIndex];
        string hint = opt.Label is null
            ? (_slotPickTyped.Length > 0 ? $"New slot: {_slotPickTyped}" : "Type new slot name")
            : opt.Display.Trim();
        _buffer.SetStatusBar(StatusLine($"[ {_slotPickIndex + 1}/{_slotPickOptions.Count} ] {hint}"));
    }

    private static IReadOnlyList<GlyphCell> StatusLine(string text)
    {
        var cells = new GlyphCell[80];
        for (int i = 0; i < cells.Length; i++)
            cells[i] = new GlyphCell(i < text.Length ? text[i] : ' ', new TextStyle(TerminalColor.Black, TerminalColor.LightGray));
        return cells;
    }

    private void HandleSlotPickKey(KeyEventArgs e)
    {
        if (_slotPickOptions is null || _slotPickOptions.Count == 0)
        {
            CompleteSlotPick(null);
            e.Handled = true;
            return;
        }

        var opt = _slotPickOptions[_slotPickIndex];

        switch (e.Key)
        {
            case Key.Escape:
                CompleteSlotPick(null);
                e.Handled = true;
                return;
            case Key.Enter:
                if (opt.Label is null)
                {
                    if (_slotPickTyped.Length > 0)
                        CompleteSlotPick(_slotPickTyped.ToUpperInvariant());
                }
                else
                    CompleteSlotPick(opt.Label);
                e.Handled = true;
                return;
            case Key.Up:
                _slotPickIndex = (_slotPickIndex + _slotPickOptions.Count - 1) % _slotPickOptions.Count;
                _slotPickTyped = "";
                UpdateSlotPickStatus();
                InvalidateVisual();
                e.Handled = true;
                return;
            case Key.Down:
                _slotPickIndex = (_slotPickIndex + 1) % _slotPickOptions.Count;
                _slotPickTyped = "";
                UpdateSlotPickStatus();
                InvalidateVisual();
                e.Handled = true;
                return;
            case >= Key.D1 and <= Key.D9:
                int n = (int)e.Key - (int)Key.D1;
                if (n < _slotPickOptions.Count)
                {
                    _slotPickIndex = n;
                    var chosen = _slotPickOptions[n];
                    if (chosen.Label is null)
                    {
                        UpdateSlotPickStatus();
                        InvalidateVisual();
                    }
                    else
                        CompleteSlotPick(chosen.Label);
                }
                e.Handled = true;
                return;
            case >= Key.NumPad1 and <= Key.NumPad9:
                int np = (int)e.Key - (int)Key.NumPad1;
                if (np < _slotPickOptions.Count)
                {
                    _slotPickIndex = np;
                    var chosen = _slotPickOptions[np];
                    if (chosen.Label is null)
                    {
                        UpdateSlotPickStatus();
                        InvalidateVisual();
                    }
                    else
                        CompleteSlotPick(chosen.Label);
                }
                e.Handled = true;
                return;
        }

        if (opt.Label is null && e.Key is >= Key.A and <= Key.Z)
        {
            if (_slotPickTyped.Length < 3)
            {
                _slotPickTyped += (char)('A' + ((int)e.Key - (int)Key.A));
                UpdateSlotPickStatus();
                InvalidateVisual();
            }
            e.Handled = true;
            return;
        }

        if (opt.Label is null && e.Key == Key.Back && _slotPickTyped.Length > 0)
        {
            _slotPickTyped = _slotPickTyped[..^1];
            UpdateSlotPickStatus();
            InvalidateVisual();
            e.Handled = true;
        }
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
        _scrollOffset = 0;     // new output snaps the view back to the bottom
        _hasSelection = false; // ...and invalidates any selection (wrapped rows have shifted)
        _blockSelect = false;
        InvalidateVisual();
    });

    // ---- mouse cell highlight -------------------------------------------------------------

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        Point p = e.GetPosition(this);
        UpdatePointerCell(p);

        if (_selecting && HitTestText(p, out int row, out int col)
            && (row != _selEndRow || col != _selEndCol))
        {
            _selEndRow = row;
            _selEndCol = col;
            _hasSelection = !(row == _selAnchorRow && col == _selAnchorCol);
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PointerPointProperties props = e.GetCurrentPoint(this).Properties;

        // Right-click copies the current selection (and does nothing if there isn't one).
        if (props.IsRightButtonPressed)
        {
            if (_hasSelection) { CopySelectionAsync(); e.Handled = true; }
            return;
        }

        if (!props.IsLeftButtonPressed) return;
        Focus();
        if (!HitTestText(e.GetPosition(this), out int row, out int col)) return;

        switch (e.ClickCount)
        {
            case 2:                                    // double-click selects the word
                SelectWord(row, col);
                break;
            case 3:                                    // triple-click selects the paragraph (logical line)
                SelectParagraph(row);
                break;
            default:                                   // single click begins a drag selection
                _selecting = true;
                _hasSelection = false;
                _blockSelect = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
                _selAnchorRow = _selEndRow = row;
                _selAnchorCol = _selEndCol = col;
                e.Pointer.Capture(this);
                InvalidateVisual();
                break;
        }
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        bool wasSelecting = _selecting;
        _selecting = false;
        e.Pointer.Capture(null);                       // always release (double/triple clicks capture on the first press)
        if (wasSelecting)
        {
            if (!_hasSelection) InvalidateVisual();     // a plain click cleared the selection
            e.Handled = true;
        }
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

    /// <summary>
    /// Maps a pointer position to a cell in the scrolling text area, expressed in wrapped-row
    /// coordinates (an index into <see cref="TerminalBuffer.WrappedRows"/>) so selections stay put
    /// when the view scrolls. Rows above/below the text area clamp to its first/last row.
    /// </summary>
    private bool HitTestText(Point p, out int wrappedRow, out int col)
    {
        wrappedRow = col = 0;
        if (_buffer is null || _cellWidth <= 0 || _cellHeight <= 0) return false;

        int screenCol = (int)Math.Floor((p.X - _gridOffsetX) / _cellWidth);
        int screenRow = (int)Math.Floor((p.Y - _gridOffsetY) / _cellHeight);

        col = Math.Clamp(screenCol, 0, _buffer.Columns);
        screenRow = Math.Clamp(screenRow, _buffer.TextTop, _buffer.TextBottom);
        wrappedRow = _windowTop + (screenRow - _buffer.TextTop);
        return true;
    }

    private (int r0, int c0, int r1, int c1) NormalizedSelection()
    {
        int r0 = _selAnchorRow, c0 = _selAnchorCol, r1 = _selEndRow, c1 = _selEndCol;
        if (r1 < r0 || (r1 == r0 && c1 < c0))
            (r0, c0, r1, c1) = (r1, c1, r0, c0);
        return (r0, c0, r1, c1);
    }

    private GlyphCell[] RowAt(int wrappedRow)
    {
        IReadOnlyList<GlyphCell[]> wrapped = _buffer!.WrappedRows();
        return wrappedRow >= 0 && wrappedRow < wrapped.Count ? wrapped[wrappedRow] : [];
    }

    private void SetSelection(int r0, int c0, int r1, int c1, bool block = false)
    {
        _selAnchorRow = r0; _selAnchorCol = c0;
        _selEndRow = r1; _selEndCol = c1;
        _selecting = false;
        _blockSelect = block;
        _hasSelection = true;
        InvalidateVisual();
    }

    /// <summary>Selects the run of like characters (word, or whitespace) around the clicked cell.</summary>
    private void SelectWord(int row, int col)
    {
        GlyphCell[] cells = RowAt(row);
        if (cells.Length == 0) { _hasSelection = false; InvalidateVisual(); return; }

        col = Math.Clamp(col, 0, cells.Length - 1);
        bool space = char.IsWhiteSpace(cells[col].Glyph);
        int start = col, end = col;
        while (start > 0 && char.IsWhiteSpace(cells[start - 1].Glyph) == space) start--;
        while (end < cells.Length - 1 && char.IsWhiteSpace(cells[end + 1].Glyph) == space) end++;
        SetSelection(row, start, row, end);
    }

    /// <summary>
    /// Selects the whole logical line (paragraph) containing the clicked wrapped row, including every
    /// wrapped segment from the first column through the last non-blank cell on the final segment.
    /// </summary>
    private void SelectParagraph(int wrappedRow)
    {
        if (_buffer is null) { _hasSelection = false; InvalidateVisual(); return; }

        IReadOnlyList<int> rowLine = _buffer.WrappedRowLines();
        if (wrappedRow < 0 || wrappedRow >= rowLine.Count)
        {
            _hasSelection = false;
            InvalidateVisual();
            return;
        }

        int logical = rowLine[wrappedRow];
        int start = wrappedRow, end = wrappedRow;
        while (start > 0 && rowLine[start - 1] == logical) start--;
        while (end < rowLine.Count - 1 && rowLine[end + 1] == logical) end++;

        GlyphCell[] lastCells = RowAt(end);
        int lastCol = -1;
        for (int i = 0; i < lastCells.Length; i++)
            if (!char.IsWhiteSpace(lastCells[i].Glyph)) lastCol = i;

        if (lastCol < 0)
        {
            _hasSelection = false;
            InvalidateVisual();
            return;
        }

        SetSelection(start, 0, end, lastCol);
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

        if (_slotPickTcs is not null)
        {
            HandleSlotPickKey(e);
            return;
        }

        if (_keyPromptTcs is not null)
        {
            HandleKeyPromptKey(e);
            return;
        }

        // Ctrl-based shortcuts: copy, paste, and zoom.
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.C or Key.Insert:                 // Ctrl+C / Ctrl+Insert -> copy selection
                    if (_hasSelection) { CopySelectionAsync(); e.Handled = true; return; }
                    break;                                // nothing selected: fall through (no-op)
                case Key.V:                               // Ctrl+V -> paste into the input line
                    if (_inputActive) { PasteAsync(); e.Handled = true; return; }
                    break;
                case Key.OemPlus or Key.Add:
                    ZoomBy(1); e.Handled = true; return;
                case Key.OemMinus or Key.Subtract:
                    ZoomBy(-1); e.Handled = true; return;
                case Key.D0 or Key.NumPad0:
                    ResetZoom(); e.Handled = true; return;
            }
        }

        // Shift+Insert -> paste (classic terminal binding).
        if (e.Key == Key.Insert && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && _inputActive)
        {
            PasteAsync(); e.Handled = true; return;
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
        if (_linePromptTcs is not null)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    CompleteLinePrompt(_input);
                    e.Handled = true;
                    return;
                case Key.Back:
                    if (_input.Length > 0) _input = _input[..^1];
                    InvalidateVisual();
                    e.Handled = true;
                    return;
            }
        }

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
                _historyIndex = -1;
                _historyDraft = "";
                _hasSelection = false;
                _blockSelect = false;
                InvalidateVisual();
                e.Handled = true;
                break;
            case Key.Up:
                RecallHistory(-1);
                e.Handled = true;
                break;
            case Key.Down:
                RecallHistory(1);
                e.Handled = true;
                break;
        }
    }

    private void RecallHistory(int direction)
    {
        if (_commandHistory.Count == 0) return;

        if (_historyIndex < 0 && direction < 0)
        {
            _historyDraft = _input;
            _historyIndex = _commandHistory.Count - 1;
        }
        else if (direction < 0)
        {
            if (_historyIndex > 0) _historyIndex--;
        }
        else if (_historyIndex >= 0)
        {
            if (_historyIndex < _commandHistory.Count - 1)
                _historyIndex++;
            else
            {
                _historyIndex = -1;
                _input = _historyDraft;
                _historyDraft = "";
                InvalidateVisual();
                return;
            }
        }
        else
        {
            return;
        }

        _input = _commandHistory[_historyIndex];
        InvalidateVisual();
    }

    private void PushHistory(string line)
    {
        if (line.Length == 0) return;
        if (_commandHistory.Count > 0 && _commandHistory[^1] == line) return;
        _commandHistory.Add(line);
    }

    private void EchoAndCommit(string line)
    {
        if (_buffer is not null)
        {
            _buffer.Write(line, _buffer.DefaultStyle);
            _buffer.NewLine();
        }
        PushHistory(line);
        _input = "";
        _historyIndex = -1;
        _historyDraft = "";
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
        _windowTop = windowTop;

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

            if (_hasSelection)
                RenderSelection(context, windowTop);

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

    // ---- selection & clipboard ------------------------------------------------------------

    private void RenderSelection(DrawingContext context, int windowTop)
    {
        var (r0, c0, r1, c1) = NormalizedSelection();
        TerminalBuffer buf = _buffer!;

        for (int screenRow = buf.TextTop; screenRow <= buf.TextBottom; screenRow++)
        {
            int wrappedIdx = windowTop + (screenRow - buf.TextTop);
            if (wrappedIdx < r0 || wrappedIdx > r1) continue;

            int startCol = _blockSelect ? c0 : (wrappedIdx == r0 ? c0 : 0);
            int endCol = _blockSelect
                ? Math.Min(c1 + 1, buf.Columns)
                : (wrappedIdx == r1 ? Math.Min(c1 + 1, buf.Columns) : buf.Columns);
            if (endCol <= startCol) continue;

            double x = startCol * _cellWidth;
            double y = screenRow * _cellHeight;
            double w = (endCol - startCol) * _cellWidth;
            context.FillRectangle(SelectionBrush, new Rect(x, y, w, _cellHeight));
        }
    }

    private string? GetSelectedText()
    {
        if (!_hasSelection || _buffer is null) return null;

        var (r0, c0, r1, c1) = NormalizedSelection();
        IReadOnlyList<GlyphCell[]> wrapped = _buffer.WrappedRows();
        StringBuilder all = new();

        for (int r = r0; r <= r1; r++)
        {
            GlyphCell[] cells = r >= 0 && r < wrapped.Count ? wrapped[r] : [];
            int start = _blockSelect ? c0 : (r == r0 ? c0 : 0);
            int end = _blockSelect
                ? Math.Min(c1 + 1, _buffer.Columns)
                : (r == r1 ? Math.Min(c1 + 1, _buffer.Columns) : _buffer.Columns);

            StringBuilder line = new();
            for (int c = start; c < end; c++)
                line.Append(c < cells.Length ? cells[c].Glyph : ' ');

            all.Append(_blockSelect ? line.ToString() : line.ToString().TrimEnd());
            if (r < r1) all.Append('\n');
        }

        return all.ToString();
    }

    private async void CopySelectionAsync()
    {
        try
        {
            string? text = GetSelectedText();
            if (string.IsNullOrEmpty(text)) return;
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(text);
        }
        catch { /* clipboard unavailable -> ignore */ }
    }

    private async void PasteAsync()
    {
        if (!_inputActive) return;
        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard) return;
            string? text = await clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(text)) return;

            // The line editor only appends, so paste a single line: stop at the first newline and
            // drop any other control characters.
            int newline = text.IndexOfAny(['\n', '\r']);
            if (newline >= 0) text = text[..newline];

            StringBuilder sb = new(_input);
            foreach (char ch in text)
                if (!char.IsControl(ch)) sb.Append(ch);

            _input = sb.ToString();
            _scrollOffset = 0;
            InvalidateVisual();
        }
        catch { /* clipboard unavailable -> ignore */ }
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
