namespace Faerie.Presentation;

/// <summary>
/// The output surface the engine writes to. The Avalonia framebuffer implements this, but so can
/// a plain console or an in-memory buffer for tests — the engine has no idea which it is talking to.
/// </summary>
public interface ITerminal
{
    /// <summary>Number of character columns.</summary>
    int Columns { get; }

    /// <summary>Number of character rows.</summary>
    int Rows { get; }

    /// <summary>The default style used for plain text.</summary>
    TextStyle DefaultStyle { get; }

    /// <summary>Writes a run of text in the given style, advancing the cursor and scrolling as needed.</summary>
    void Write(string text, TextStyle style);

    /// <summary>Moves the cursor to the start of the next line.</summary>
    void NewLine();

    /// <summary>
    /// Replaces the current logical line (the one the cursor is on) without advancing to the next.
    /// Used for single-line animations such as slot reels. The next <see cref="NewLine"/> commits
    /// the final text to scrollback.
    /// </summary>
    void OverwriteLine(string text, TextStyle style);

    /// <summary>Clears the screen and resets the cursor to the top-left.</summary>
    void Clear();

    // ---- optional title / status bars -----------------------------------------------------
    // Implemented by the framebuffer terminal; no-ops elsewhere (console, tests).

    /// <summary>True if this terminal renders fixed title/status bars.</summary>
    bool SupportsBars => false;

    /// <summary>
    /// Columns to leave clear at the left of the title bar for host window chrome (e.g. in-TUI window
    /// controls). The engine composes title content into the region right of this inset. 0 by default.
    /// </summary>
    int TitleBarInsetLeft => 0;

    /// <summary>
    /// Columns to leave clear at the right of the title bar for host window chrome. The engine composes
    /// title content into the region left of this inset. 0 by default.
    /// </summary>
    int TitleBarInsetRight => 0;

    /// <summary>
    /// Reserves a non-scrolling top row (title bar) and/or bottom row (status bar). The scrolling
    /// text region shrinks to fit between whichever bars are enabled.
    /// </summary>
    void ConfigureBars(bool titleBar, bool statusBar) { }

    /// <summary>Sets the contents of the title bar row (length should equal <see cref="Columns"/>).</summary>
    void SetTitleBar(IReadOnlyList<GlyphCell> cells) { }

    /// <summary>Sets the contents of the status bar row, or null to leave it blank.</summary>
    void SetStatusBar(IReadOnlyList<GlyphCell>? cells) { }
}
