namespace Faerie.Presentation;

/// <summary>
/// A 24-bit RGB colour. The classic DOS/VGA palette is provided for convenience, but the
/// framebuffer is not limited to 16 colours — any RGB value is valid.
/// </summary>
public readonly record struct TerminalColor(byte R, byte G, byte B)
{
    public uint ToArgb() => 0xFF000000u | (uint)(R << 16) | (uint)(G << 8) | B;

    public static TerminalColor FromRgb(int r, int g, int b) => new((byte)r, (byte)g, (byte)b);

    // ---- the standard 16-colour VGA text palette ------------------------------------------
    public static readonly TerminalColor Black = new(0x00, 0x00, 0x00);
    public static readonly TerminalColor Blue = new(0x00, 0x00, 0xAA);
    public static readonly TerminalColor Green = new(0x00, 0xAA, 0x00);
    public static readonly TerminalColor Cyan = new(0x00, 0xAA, 0xAA);
    public static readonly TerminalColor Red = new(0xAA, 0x00, 0x00);
    public static readonly TerminalColor Magenta = new(0xAA, 0x00, 0xAA);
    public static readonly TerminalColor Brown = new(0xAA, 0x55, 0x00);
    public static readonly TerminalColor LightGray = new(0xAA, 0xAA, 0xAA);
    public static readonly TerminalColor DarkGray = new(0x55, 0x55, 0x55);
    public static readonly TerminalColor LightBlue = new(0x55, 0x55, 0xFF);
    public static readonly TerminalColor LightGreen = new(0x55, 0xFF, 0x55);
    public static readonly TerminalColor LightCyan = new(0x55, 0xFF, 0xFF);
    public static readonly TerminalColor LightRed = new(0xFF, 0x55, 0x55);
    public static readonly TerminalColor LightMagenta = new(0xFF, 0x55, 0xFF);
    public static readonly TerminalColor Yellow = new(0xFF, 0xFF, 0x55);
    public static readonly TerminalColor White = new(0xFF, 0xFF, 0xFF);

    // A couple of out-of-palette extras used by the sample game, proving the buffer is true-colour.
    public static readonly TerminalColor Amber = new(0xFF, 0xB0, 0x00);
    public static readonly TerminalColor Gold = new(0xD4, 0xAF, 0x37);
    public static readonly TerminalColor BloodRed = new(0x8A, 0x03, 0x03);

    /// <summary>Looks up a palette colour by name (case-insensitive). Used by the markup parser.</summary>
    public static bool TryParse(string name, out TerminalColor color)
    {
        switch (name.Trim().ToLowerInvariant())
        {
            case "black": color = Black; return true;
            case "blue": color = Blue; return true;
            case "green": color = Green; return true;
            case "cyan": color = Cyan; return true;
            case "red": color = Red; return true;
            case "magenta": color = Magenta; return true;
            case "brown": color = Brown; return true;
            case "lightgray" or "lightgrey" or "gray" or "grey": color = LightGray; return true;
            case "darkgray" or "darkgrey": color = DarkGray; return true;
            case "lightblue": color = LightBlue; return true;
            case "lightgreen": color = LightGreen; return true;
            case "lightcyan": color = LightCyan; return true;
            case "lightred": color = LightRed; return true;
            case "lightmagenta" or "pink": color = LightMagenta; return true;
            case "yellow": color = Yellow; return true;
            case "white": color = White; return true;
            case "amber": color = Amber; return true;
            case "gold": color = Gold; return true;
            case "bloodred" or "blood": color = BloodRed; return true;
        }

        // Allow #RRGGBB literals.
        if (name.StartsWith('#') && name.Length == 7
            && int.TryParse(name.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber, null, out int r)
            && int.TryParse(name.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber, null, out int g)
            && int.TryParse(name.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber, null, out int b))
        {
            color = new TerminalColor((byte)r, (byte)g, (byte)b);
            return true;
        }

        color = default;
        return false;
    }
}
