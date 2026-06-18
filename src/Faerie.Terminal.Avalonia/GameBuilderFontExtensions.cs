using Faerie.Building;

namespace Faerie.Terminal.Avalonia;

/// <summary>Chooses a terminal font bundled with <c>Faerie.Terminal.Avalonia</c>.</summary>
public static class GameBuilderFontExtensions
{
    /// <summary>Uses a built-in retro font. Custom fonts still use <see cref="GameBuilder.WithFont(string)"/>.</summary>
    public static GameBuilder WithFont(this GameBuilder builder, BuiltInTerminalFont font) =>
        builder.WithFont(BuiltInTerminalFonts.ToFontSpec(font));
}
