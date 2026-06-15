namespace Nixie.Presentation;

/// <summary>Per-cell text attributes supported by the framebuffer.</summary>
[Flags]
public enum TextAttributes
{
    None = 0,
    Bold = 1 << 0,
    Underline = 1 << 1,
    Blink = 1 << 2,
    Inverse = 1 << 3
}

/// <summary>
/// A foreground/background colour pair plus attributes. This is the styling unit shared by the
/// engine's output and the framebuffer renderer.
/// </summary>
public readonly record struct TextStyle(
    TerminalColor Foreground,
    TerminalColor Background,
    TextAttributes Attributes = TextAttributes.None)
{
    /// <summary>The default amber-on-black DOS look.</summary>
    public static readonly TextStyle Default = new(TerminalColor.LightGray, TerminalColor.Black);

    public TextStyle With(TerminalColor? fg = null, TerminalColor? bg = null, TextAttributes? attr = null) =>
        new(fg ?? Foreground, bg ?? Background, attr ?? Attributes);

    public TextStyle WithForeground(TerminalColor fg) => this with { Foreground = fg };
    public TextStyle WithBackground(TerminalColor bg) => this with { Background = bg };
    public TextStyle Add(TextAttributes attr) => this with { Attributes = Attributes | attr };
}

/// <summary>A run of text rendered with a single style.</summary>
public readonly record struct StyledSpan(string Text, TextStyle Style);
