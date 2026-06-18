namespace Faerie.Terminal.Avalonia;

/// <summary>Maps <see cref="BuiltInTerminalFont"/> values to Avalonia resource font specs.</summary>
public static class BuiltInTerminalFonts
{
    /// <summary>Folder URI for fonts embedded in this assembly.</summary>
    public const string ResourceRoot = "avares://Faerie.Terminal.Avalonia/Assets/Fonts";

    private static readonly IReadOnlyDictionary<BuiltInTerminalFont, string> Files =
        new Dictionary<BuiltInTerminalFont, string>
        {
            [BuiltInTerminalFont.IbmBios8x8] = "PxPlus_IBM_BIOS.ttf",
            [BuiltInTerminalFont.IbmCga8x8] = "PxPlus_IBM_CGA.ttf",
            [BuiltInTerminalFont.IbmMda9x14] = "PxPlus_IBM_MDA.ttf",
            [BuiltInTerminalFont.IbmEga8x14] = "PxPlus_IBM_EGA_8x14.ttf",
            [BuiltInTerminalFont.IbmVga8x14] = "PxPlus_IBM_VGA_8x14.ttf",
            [BuiltInTerminalFont.IbmVga8x16] = "PxPlus_IBM_VGA_8x16.ttf",
            [BuiltInTerminalFont.IbmVga9x16] = "PxPlus_IBM_VGA_9x16.ttf",
            [BuiltInTerminalFont.Tandy10008x16] = "PxPlus_Tandy1K-II_200L.ttf",
            [BuiltInTerminalFont.AmstradPc] = "PxPlus_Amstrad_PC.ttf",
            [BuiltInTerminalFont.DecRainbow80Col] = "PxPlus_Rainbow100_re_80.ttf",
            [BuiltInTerminalFont.AppleIIe] = "PrintChar21.ttf",
            [BuiltInTerminalFont.AppleII80Column] = "PRNumber3.ttf",
            [BuiltInTerminalFont.Commodore64] = "PetMe64.ttf",
            [BuiltInTerminalFont.ZxSpectrum] = "Speccy.ttf",
            [BuiltInTerminalFont.Atari8Bit] = "CandyAntics.ttf",
            [BuiltInTerminalFont.AtariSt] = "ProjectJasonSmall.ttf",
            [BuiltInTerminalFont.Trs80CoCo] = "HotCoCo.ttf",
            [BuiltInTerminalFont.BbcMaster512] = "Px437_Master_512.ttf",
            [BuiltInTerminalFont.BbcTeletext] = "Teletext50.otf",
            [BuiltInTerminalFont.Kaypro2k] = "Px437_Kaypro2K_G.ttf",
        };

    /// <summary>All bundled fonts.</summary>
    public static IReadOnlyList<BuiltInTerminalFont> All => [.. Files.Keys];

    /// <summary>Returns a font spec string for a bundled font (stored on <see cref="Faerie.Runtime.Game.FontSpec"/>).</summary>
    public static string ToFontSpec(BuiltInTerminalFont font) =>
        $"{ResourceRoot}/{Files[font]}";
}
