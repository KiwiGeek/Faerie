using System.Text;
using Avalonia.Media;
using Avalonia.Platform;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// Resolves a game's font spec (a plain string from the game definition) into a usable
/// <see cref="Typeface"/> pair. Games may use a <see cref="BuiltInTerminalFont"/> (see
/// <see cref="GameBuilderFontExtensions"/>), embed their own font as an <c>AvaloniaResource</c>, or
/// name a system font family.
///
/// Accepted spec formats:
/// <list type="bullet">
/// <item>System family name(s): <c>"Consolas"</c> or <c>"Cascadia Mono,Consolas,monospace"</c>.</item>
/// <item>Resource font with explicit family: <c>"avares://MyGame/Assets/Fonts#PxPlus IBM VGA8"</c>.</item>
/// <item>Resource font <i>file</i> or <i>folder</i>: <c>"avares://MyGame/Assets/Fonts/My.ttf"</c> or
///       <c>"avares://MyGame/Assets/Fonts"</c> — the family name is read from the font's name table.</item>
/// </list>
/// If nothing resolves, a cross-platform monospace fallback is used so the app always runs.
/// </summary>
public static class TerminalFont
{
    public static readonly FontFamily MonospaceFallback =
        new("Cascadia Mono,Consolas,Menlo,DejaVu Sans Mono,monospace");

    public static (Typeface normal, Typeface bold) Resolve(string? spec)
    {
        try
        {
            foreach (FontFamily family in Candidates(spec))
            {
                Typeface candidate = new(family);
                if (FontManager.Current.TryGetGlyphTypeface(candidate, out _))
                    return (candidate, new Typeface(family, FontStyle.Normal, FontWeight.Bold));
            }
        }
        catch
        {
            // fall through to monospace
        }

        return (new Typeface(MonospaceFallback), new Typeface(MonospaceFallback, FontStyle.Normal, FontWeight.Bold));
    }

    private static IEnumerable<FontFamily> Candidates(string? spec)
    {
        if (!string.IsNullOrWhiteSpace(spec))
        {
            // Explicit family already provided (or a plain system family name / list).
            if (spec.Contains('#') || !spec.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            {
                yield return new FontFamily(spec);
            }
            else
            {
                // An avares URI without a family: a font file or a folder of fonts. Read the real
                // family name(s) from the file(s) so the author needn't know them.
                foreach (Uri fontUri in EnumerateFontFiles(spec))
                {
                    if (TryReadFamilyName(fontUri) is not { } familyName) continue;
                    string folder = FolderUri(spec);
                    yield return new FontFamily($"{folder}#{familyName}");
                }
            }
        }

        yield return MonospaceFallback;
    }

    private static string FolderUri(string avaresSpec)
    {
        // Strip a trailing /file.ttf to get the folder; leave folders as-is.
        if (avaresSpec.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
            avaresSpec.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
        {
            int slash = avaresSpec.LastIndexOf('/');
            return slash > "avares://".Length ? avaresSpec[..slash] : avaresSpec;
        }
        return avaresSpec.TrimEnd('/');
    }

    private static IEnumerable<Uri> EnumerateFontFiles(string avaresSpec)
    {
        // A direct font file.
        if (avaresSpec.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
            avaresSpec.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
        {
            Uri direct = new(avaresSpec);
            bool exists = false;
            try { exists = AssetLoader.Exists(direct); } catch { }
            if (exists) yield return direct;
            yield break;
        }

        // A folder: enumerate the fonts in it.
        IEnumerable<Uri> assets;
        try { assets = AssetLoader.GetAssets(new Uri(avaresSpec.TrimEnd('/')), null); }
        catch { yield break; }

        foreach (Uri uri in assets)
        {
            string path = uri.AbsolutePath;
            if (path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                yield return uri;
        }
    }

    /// <summary>Reads the family name from an OpenType/TrueType font's 'name' table.</summary>
    public static string? TryReadFamilyName(Uri uri)
    {
        try
        {
            byte[] data;
            using (Stream s = AssetLoader.Open(uri))
            using (MemoryStream ms = new())
            {
                s.CopyTo(ms);
                data = ms.ToArray();
            }
            return ParseFamilyName(data);
        }
        catch
        {
            return null;
        }
    }

    private static string? ParseFamilyName(byte[] d)
    {
        if (d.Length < 12) return null;

        ushort U16(int o) => (ushort)((d[o] << 8) | d[o + 1]);
        uint U32(int o) => ((uint)d[o] << 24) | ((uint)d[o + 1] << 16) | ((uint)d[o + 2] << 8) | d[o + 3];

        int numTables = U16(4);
        int nameTable = -1;
        for (int i = 0, p = 12; i < numTables && p + 16 <= d.Length; i++, p += 16)
        {
            string tag = $"{(char)d[p]}{(char)d[p + 1]}{(char)d[p + 2]}{(char)d[p + 3]}";
            if (tag == "name") { nameTable = (int)U32(p + 8); break; }
        }
        if (nameTable < 0 || nameTable + 6 > d.Length) return null;

        int count = U16(nameTable + 2);
        int storage = nameTable + U16(nameTable + 4);

        string? best = null;
        int bestScore = -1;
        for (int i = 0, rec = nameTable + 6; i < count && rec + 12 <= d.Length; i++, rec += 12)
        {
            ushort platform = U16(rec);
            ushort nameId = U16(rec + 6);
            ushort len = U16(rec + 8);
            ushort off = U16(rec + 10);
            if (nameId != 1 && nameId != 16) continue;   // 1 = Family, 16 = Typographic Family

            int start = storage + off;
            if (start + len > d.Length || len == 0) continue;

            string value = platform is 3 or 0
                ? Encoding.BigEndianUnicode.GetString(d, start, len)
                : Encoding.Latin1.GetString(d, start, len);

            int score = (nameId == 16 ? 20 : 10) + (platform == 3 ? 2 : platform == 0 ? 1 : 0);
            if (score > bestScore) { bestScore = score; best = value; }
        }

        return string.IsNullOrWhiteSpace(best) ? null : best;
    }
}
