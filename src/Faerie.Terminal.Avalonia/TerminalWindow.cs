using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// A ready-made window that hosts a <see cref="TerminalControl"/> filling the whole client area,
/// with F11 (or Alt+Enter) toggling fullscreen. Games can use this directly or host the control in
/// their own window.
/// </summary>
public class TerminalWindow : Window
{
    private bool _useTuiWindowChrome = true;
    private bool _adjustingSize;   // guards re-entrant Width/Height writes while snapping
    private bool _chromeReady;     // true once the constructor has wired up Terminal (OnPropertyChanged fires during the base ctor, before that)

    public TerminalWindow()
    {
        Background = Brushes.Black;
        Title = "Text Adventure";
        Width = 960;
        Height = 600;
        MinWidth = 480;
        MinHeight = 300;

        Terminal = new TerminalControl();
        Content = Terminal;

        Terminal.WindowControlInvoked += OnWindowControlInvoked;
        Terminal.WindowMoveRequested += OnWindowMoveRequested;
        Terminal.WindowResizeRequested += OnWindowResizeRequested;
        Terminal.CellMetricsChanged += SnapToCellGrid;

        ApplyWindowChromeMode();
        _chromeReady = true;
    }

    private void OnWindowControlInvoked(WindowControlKind kind)
    {
        switch (kind)
        {
            case WindowControlKind.Minimize:
                WindowState = WindowState.Minimized;
                break;
            case WindowControlKind.Maximize:
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                break;
            case WindowControlKind.Close:
                Close();
                break;
        }
    }

    private void OnWindowMoveRequested(PointerPressedEventArgs e) => BeginMoveDrag(e);

    private void OnWindowResizeRequested((WindowEdge Edge, PointerPressedEventArgs Args) request) =>
        BeginResizeDrag(request.Edge, request.Args);

    // ---- cell-snapped sizing (issue #116) -------------------------------------------------

    /// <summary>
    /// Reacts to client-size / window-state changes by snapping the window to a whole number of cells so
    /// the grid meets the window edges with no black border. Skipped when native chrome is used or the
    /// window is maximized/fullscreen (where a centred border is acceptable to preserve font fidelity).
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ClientSizeProperty || change.Property == WindowStateProperty)
            SnapToCellGrid();
        if (change.Property == TitleProperty && _chromeReady)
            Terminal.WindowTitle = Title;
    }

    private void SnapToCellGrid()
    {
        if (!_chromeReady || _adjustingSize || !_useTuiWindowChrome || WindowState != WindowState.Normal) return;

        double cellW = Terminal.CellPixelWidth, cellH = Terminal.CellPixelHeight;
        if (cellW <= 0 || cellH <= 0) return;

        (double w, double h) = WindowCellSnap.Snap(ClientSize.Width, ClientSize.Height, cellW, cellH, MinWidth, MinHeight);
        SetClientPixelSize(w, h);
    }

    private void SetClientPixelSize(double width, double height)
    {
        if (Math.Abs(Width - width) < 0.5 && Math.Abs(Height - height) < 0.5) return;

        _adjustingSize = true;
        try
        {
            Width = width;
            Height = height;
        }
        finally
        {
            _adjustingSize = false;
        }
    }

    public TerminalControl Terminal { get; }

    /// <summary>
    /// When true (the default), the native OS title bar and borders are removed and the window
    /// controls (close/minimize/maximize) are drawn inside the TUI on row 0, with platform-appropriate
    /// placement. Set to false to keep the standard native window chrome. Must be set before the
    /// window is opened.
    /// </summary>
    public bool UseTuiWindowChrome
    {
        get => _useTuiWindowChrome;
        set
        {
            if (_useTuiWindowChrome == value) return;
            _useTuiWindowChrome = value;
            ApplyWindowChromeMode();
        }
    }

    private void ApplyWindowChromeMode()
    {
        SystemDecorations = _useTuiWindowChrome ? SystemDecorations.None : SystemDecorations.Full;
        Terminal.ShowWindowControls = _useTuiWindowChrome;
        Terminal.WindowControlPlatform = HostPlatformDetector.Current;
        Terminal.WindowTitle = Title;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Terminal.Focus();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Terminal.Shutdown();
        base.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        bool altEnter = e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        if (e.Key == Key.F11 || altEnter)
        {
            WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}
