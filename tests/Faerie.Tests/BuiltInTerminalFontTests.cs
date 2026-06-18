using Faerie.Terminal.Avalonia;
using Xunit;

namespace Faerie.Tests;

public sealed class BuiltInTerminalFontTests
{
    [Fact]
    public void ToFontSpec_PointsAtBundledFontFiles()
    {
        foreach ((BuiltInTerminalFont font, string fileName) in ExpectedFiles())
        {
            string spec = BuiltInTerminalFonts.ToFontSpec(font);
            Assert.StartsWith(BuiltInTerminalFonts.ResourceRoot, spec, StringComparison.Ordinal);
            Assert.EndsWith(fileName, spec, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void All_IncludesEveryEnumValue()
    {
        Assert.Equal(Enum.GetValues<BuiltInTerminalFont>().Length, BuiltInTerminalFonts.All.Count);
    }

    [Fact]
    public void WithFontExtension_UsesBuiltInSpec()
    {
        var b = Faerie.Building.GameBuilder.Create("Font test")
            .WithFont(BuiltInTerminalFont.ZxSpectrum);
        Faerie.Model.Room start = b.Room("Start");
        b.StartIn(start);
        var game = b.Build();

        Assert.Equal(BuiltInTerminalFonts.ToFontSpec(BuiltInTerminalFont.ZxSpectrum), game.FontSpec);
    }

    [Fact]
    public void BundledFontFiles_ExistOnDisk()
    {
        string fontsDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src", "Faerie.Terminal.Avalonia", "Assets", "Fonts"));
        foreach ((_, string fileName) in ExpectedFiles())
        {
            Assert.True(File.Exists(Path.Combine(fontsDir, fileName)), $"Missing bundled font: {fileName}");
        }
    }

    private static IEnumerable<(BuiltInTerminalFont, string)> ExpectedFiles() =>
        Enum.GetValues<BuiltInTerminalFont>().Select(f => (f, f switch
        {
            BuiltInTerminalFont.IbmBios8x8 => "PxPlus_IBM_BIOS.ttf",
            BuiltInTerminalFont.IbmCga8x8 => "PxPlus_IBM_CGA.ttf",
            BuiltInTerminalFont.IbmMda9x14 => "PxPlus_IBM_MDA.ttf",
            BuiltInTerminalFont.IbmEga8x14 => "PxPlus_IBM_EGA_8x14.ttf",
            BuiltInTerminalFont.IbmVga8x14 => "PxPlus_IBM_VGA_8x14.ttf",
            BuiltInTerminalFont.IbmVga8x16 => "PxPlus_IBM_VGA_8x16.ttf",
            BuiltInTerminalFont.IbmVga9x16 => "PxPlus_IBM_VGA_9x16.ttf",
            BuiltInTerminalFont.Tandy10008x16 => "PxPlus_Tandy1K-II_200L.ttf",
            BuiltInTerminalFont.AmstradPc => "PxPlus_Amstrad_PC.ttf",
            BuiltInTerminalFont.DecRainbow80Col => "PxPlus_Rainbow100_re_80.ttf",
            BuiltInTerminalFont.AppleIIe => "PrintChar21.ttf",
            BuiltInTerminalFont.AppleII80Column => "PRNumber3.ttf",
            BuiltInTerminalFont.Commodore64 => "PetMe64.ttf",
            BuiltInTerminalFont.ZxSpectrum => "Speccy.ttf",
            BuiltInTerminalFont.Atari8Bit => "CandyAntics.ttf",
            BuiltInTerminalFont.AtariSt => "ProjectJasonSmall.ttf",
            BuiltInTerminalFont.Trs80CoCo => "HotCoCo.ttf",
            BuiltInTerminalFont.BbcMaster512 => "Px437_Master_512.ttf",
            BuiltInTerminalFont.BbcTeletext => "Teletext50.otf",
            BuiltInTerminalFont.Kaypro2k => "Px437_Kaypro2K_G.ttf",
            _ => throw new ArgumentOutOfRangeException(nameof(f), f, null),
        }));
}
