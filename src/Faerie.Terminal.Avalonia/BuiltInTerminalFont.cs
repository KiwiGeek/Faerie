namespace Faerie.Terminal.Avalonia;

/// <summary>Retro terminal fonts bundled with <c>Faerie.Terminal.Avalonia</c>.</summary>
/// <remarks>Curated set — edit <c>scripts/builtin-fonts.manifest.json</c> and re-run <c>fetch-builtin-fonts.py</c>.</remarks>
public enum BuiltInTerminalFont
{
    // IBM PC (int10h.org, CC BY-SA 4.0)
    /// <summary>PC BIOS 8×8 (shared by CGA graphics and early text modes).</summary>
    IbmBios8x8,
    /// <summary>IBM CGA 8×8.</summary>
    IbmCga8x8,
    /// <summary>IBM MDA 9×14 (80-column monochrome with 9th-dot box drawing).</summary>
    IbmMda9x14,
    /// <summary>IBM EGA 8×14.</summary>
    IbmEga8x14,
    /// <summary>IBM VGA 8×14.</summary>
    IbmVga8x14,
    /// <summary>IBM VGA 8×16 (classic DOS 80×25).</summary>
    IbmVga8x16,
    /// <summary>IBM VGA 9×16 (9-dot-wide text mode).</summary>
    IbmVga9x16,

    // IBM PC compatibles (int10h.org, CC BY-SA 4.0)
    /// <summary>Tandy 1000 / PCjr 200-line 8×16.</summary>
    Tandy10008x16,
    /// <summary>Amstrad PC1512 system font.</summary>
    AmstradPc,
    /// <summary>DEC Rainbow 100 80-column text.</summary>
    DecRainbow80Col,
    /// <summary>Kaypro 2000 portable.</summary>
    Kaypro2k,

    // 8-bit classics (Kreative Software, Free Use 1.2f)
    /// <summary>Apple II 40-column text (Print Char 21).</summary>
    AppleIIe,
    /// <summary>Apple II 80-column text (PR Number 3).</summary>
    AppleII80Column,
    /// <summary>Commodore 64 PETSCII (Pet Me 64).</summary>
    Commodore64,
    /// <summary>ZX Spectrum.</summary>
    ZxSpectrum,
    /// <summary>Atari 8-bit (Candy Antics).</summary>
    Atari8Bit,
    /// <summary>Atari ST low resolution (Project Jason Small).</summary>
    AtariSt,
    /// <summary>TRS-80 Color Computer.</summary>
    Trs80CoCo,

    // BBC Micro (int10h.org, CC BY-SA 4.0)
    /// <summary>BBC Master 512 MOS 8×8 (Modes 0–6).</summary>
    BbcMaster512,

    // BBC Micro teletext (public domain)
    /// <summary>BBC Mode 7 / teletext (SAA5050-style, Teletext50).</summary>
    BbcTeletext,
}
