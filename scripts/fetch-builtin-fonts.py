#!/usr/bin/env python3
"""Download the curated bundled terminal fonts and regenerate BuiltInTerminalFont*.cs."""

from __future__ import annotations

import io
import json
import urllib.request
import zipfile
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "src" / "Faerie.Terminal.Avalonia" / "Assets" / "Fonts"
SRC = ROOT / "src" / "Faerie.Terminal.Avalonia"
TESTS = ROOT / "tests" / "Faerie.Tests"
MANIFEST = Path(__file__).with_name("builtin-fonts.manifest.json")

INT10H_BASE = (
    "https://raw.githubusercontent.com/Izzy3110/oldschool_pc_font_pack_v2.2_FULL/main/ttf-Px"
)
KREATIVE_BASE = "https://www.kreativekorp.com/swdownload/fonts"
TELETEXT50_URL = "https://galax.xyz/Teletext50/Teletext50.otf"

UA = {"User-Agent": "Faerie-font-fetch/1.0 (+https://github.com/KiwiGeek/Faerie)"}


def fetch(url: str) -> bytes:
    req = urllib.request.Request(url, headers=UA)
    with urllib.request.urlopen(req, timeout=180) as resp:
        return resp.read()


def load_catalog() -> list[dict]:
    catalog = json.loads(MANIFEST.read_text(encoding="utf-8"))
    if not catalog:
        raise RuntimeError(f"{MANIFEST} is empty")
    enums = [entry["enum"] for entry in catalog]
    if len(enums) != len(set(enums)):
        raise RuntimeError("Duplicate enum names in manifest")
    files = [entry["file"] for entry in catalog]
    if len(files) != len(set(files)):
        raise RuntimeError("Duplicate file names in manifest")
    return catalog


def download_int10h(file_name: str) -> None:
    dest = OUT / file_name
    print(f"int10h {file_name}...")
    dest.write_bytes(fetch(f"{INT10H_BASE}/{file_name}"))


def download_kreative(zip_rel: str, member_name: str, dest_name: str) -> None:
    dest = OUT / dest_name
    zip_url = f"{KREATIVE_BASE}/{zip_rel}"
    print(f"kreative {zip_url} -> {dest_name}...")
    data = fetch(zip_url)
    with zipfile.ZipFile(io.BytesIO(data)) as zf:
        match = next((n for n in zf.namelist() if Path(n).name == member_name), None)
        if match is None:
            candidates = [n for n in zf.namelist() if n.lower().endswith(".ttf")]
            raise RuntimeError(
                f"Member {member_name!r} not found in {zip_url}. Candidates: {candidates}"
            )
        dest.write_bytes(zf.read(match))


def download_teletext() -> None:
    dest = OUT / "Teletext50.otf"
    print("teletext Teletext50.otf...")
    dest.write_bytes(fetch(TELETEXT50_URL))


def write_licenses() -> None:
    license_int10h = OUT / "LICENSE-int10h-CC-BY-SA-4.0.txt"
    if not license_int10h.exists():
        license_int10h.write_text(
            """Ultimate Oldschool PC Font Pack (int10h.org)
Licensed under Creative Commons Attribution-ShareAlike 4.0 International (CC BY-SA 4.0)
Source: https://int10h.org/oldschool-pc-fonts/
Copyright (c) 2016-2020 VileR (int10h.org)
Full license: https://creativecommons.org/licenses/by-sa/4.0/
""",
            encoding="utf-8",
        )

    kreative_license = OUT / "LICENSE-Kreative-FreeUse-1.2f.txt"
    if not kreative_license.exists():
        kreative_license.write_text(
            fetch("https://www.kreativekorp.com/software/fonts/FreeLicense.txt").decode(
                "utf-8", errors="replace"
            ),
            encoding="utf-8",
        )

    teletext_license = OUT / "LICENSE-Teletext50-PublicDomain.txt"
    if not teletext_license.exists():
        teletext_license.write_text(
            """Teletext50 / Bedstead (galax.xyz)
Released into the public domain.
Source: https://galax.xyz/Teletext50/
""",
            encoding="utf-8",
        )


def cleanup_unlisted(allowed: set[str]) -> None:
    for path in list(OUT.glob("*.ttf")) + list(OUT.glob("*.otf")):
        if path.name not in allowed:
            path.unlink()
            print(f"removed unlisted {path.name}")


def write_csharp(catalog: list[dict]) -> None:
    grouped: dict[str, list[dict]] = defaultdict(list)
    for entry in catalog:
        grouped[entry["group"]].append(entry)

    enum_lines = [
        "namespace Faerie.Terminal.Avalonia;",
        "",
        "/// <summary>Retro terminal fonts bundled with <c>Faerie.Terminal.Avalonia</c>.</summary>",
        "/// <remarks>Curated set — edit <c>scripts/builtin-fonts.manifest.json</c> and re-run <c>fetch-builtin-fonts.py</c>.</remarks>",
        "public enum BuiltInTerminalFont",
        "{",
    ]
    first = True
    for group_name in dict.fromkeys(entry["group"] for entry in catalog):
        if not first:
            enum_lines.append("")
        first = False
        enum_lines.append(f"    // {group_name}")
        for entry in catalog:
            if entry["group"] != group_name:
                continue
            summary = entry["summary"].replace("&", "&amp;").replace("<", "&lt;")
            enum_lines.append(f"    /// <summary>{summary}</summary>")
            enum_lines.append(f"    {entry['enum']},")
    enum_lines.append("}")
    enum_lines.append("")
    (SRC / "BuiltInTerminalFont.cs").write_text("\n".join(enum_lines), encoding="utf-8")

    dict_lines = [
        "namespace Faerie.Terminal.Avalonia;",
        "",
        "/// <summary>Maps <see cref=\"BuiltInTerminalFont\"/> values to Avalonia resource font specs.</summary>",
        "public static class BuiltInTerminalFonts",
        "{",
        "    /// <summary>Folder URI for fonts embedded in this assembly.</summary>",
        "    public const string ResourceRoot = \"avares://Faerie.Terminal.Avalonia/Assets/Fonts\";",
        "",
        "    private static readonly IReadOnlyDictionary<BuiltInTerminalFont, string> Files =",
        "        new Dictionary<BuiltInTerminalFont, string>",
        "        {",
    ]
    for entry in catalog:
        dict_lines.append(
            f"            [BuiltInTerminalFont.{entry['enum']}] = \"{entry['file']}\","
        )
    dict_lines.extend(
        [
            "        };",
            "",
            "    /// <summary>All bundled fonts.</summary>",
            "    public static IReadOnlyList<BuiltInTerminalFont> All => [.. Files.Keys];",
            "",
            "    /// <summary>Returns a font spec string for a bundled font (stored on <see cref=\"Faerie.Runtime.Game.FontSpec\"/>).</summary>",
            "    public static string ToFontSpec(BuiltInTerminalFont font) =>",
            "        $\"{ResourceRoot}/{Files[font]}\";",
            "}",
            "",
        ]
    )
    (SRC / "BuiltInTerminalFonts.cs").write_text("\n".join(dict_lines), encoding="utf-8")

    expected = "\n".join(
        f"            BuiltInTerminalFont.{entry['enum']} => \"{entry['file']}\","
        for entry in catalog
    )
    test_lines = [
        "using Faerie.Terminal.Avalonia;",
        "using Xunit;",
        "",
        "namespace Faerie.Tests;",
        "",
        "public sealed class BuiltInTerminalFontTests",
        "{",
        "    [Fact]",
        "    public void ToFontSpec_PointsAtBundledFontFiles()",
        "    {",
        "        foreach ((BuiltInTerminalFont font, string fileName) in ExpectedFiles())",
        "        {",
        "            string spec = BuiltInTerminalFonts.ToFontSpec(font);",
        "            Assert.StartsWith(BuiltInTerminalFonts.ResourceRoot, spec, StringComparison.Ordinal);",
        "            Assert.EndsWith(fileName, spec, StringComparison.Ordinal);",
        "        }",
        "    }",
        "",
        "    [Fact]",
        "    public void All_IncludesEveryEnumValue()",
        "    {",
        "        Assert.Equal(Enum.GetValues<BuiltInTerminalFont>().Length, BuiltInTerminalFonts.All.Count);",
        "    }",
        "",
        "    [Fact]",
        "    public void WithFontExtension_UsesBuiltInSpec()",
        "    {",
        "        var b = Faerie.Building.GameBuilder.Create(\"Font test\")",
        "            .WithFont(BuiltInTerminalFont.ZxSpectrum);",
        "        Faerie.Model.Room start = b.Room(\"Start\");",
        "        b.StartIn(start);",
        "        var game = b.Build();",
        "",
        "        Assert.Equal(BuiltInTerminalFonts.ToFontSpec(BuiltInTerminalFont.ZxSpectrum), game.FontSpec);",
        "    }",
        "",
        "    [Fact]",
        "    public void BundledFontFiles_ExistOnDisk()",
        "    {",
        "        string fontsDir = Path.GetFullPath(Path.Combine(",
        "            AppContext.BaseDirectory,",
        "            \"..\", \"..\", \"..\", \"..\", \"..\", \"src\", \"Faerie.Terminal.Avalonia\", \"Assets\", \"Fonts\"));",
        "        foreach ((_, string fileName) in ExpectedFiles())",
        "        {",
        "            Assert.True(File.Exists(Path.Combine(fontsDir, fileName)), $\"Missing bundled font: {fileName}\");",
        "        }",
        "    }",
        "",
        "    private static IEnumerable<(BuiltInTerminalFont, string)> ExpectedFiles() =>",
        "        Enum.GetValues<BuiltInTerminalFont>().Select(f => (f, f switch",
        "        {",
        expected,
        "            _ => throw new ArgumentOutOfRangeException(nameof(f), f, null),",
        "        }));",
        "}",
        "",
    ]
    (TESTS / "BuiltInTerminalFontTests.cs").write_text("\n".join(test_lines), encoding="utf-8")


def write_fonts_md(catalog: list[dict]) -> None:
    group_titles = {
        "IBM PC (int10h.org, CC BY-SA 4.0)": ("IBM PC", "int10h.org", "CC BY-SA 4.0"),
        "IBM PC compatibles (int10h.org, CC BY-SA 4.0)": (
            "IBM PC compatibles",
            "int10h.org",
            "CC BY-SA 4.0",
        ),
        "8-bit classics (Kreative Software, Free Use 1.2f)": (
            "8-bit classics",
            "Kreative Software",
            "Free Use License 1.2f",
        ),
        "BBC Micro (int10h.org, CC BY-SA 4.0)": ("BBC Micro",),
        "BBC Micro teletext (public domain)": ("BBC Micro",),
    }
    section_notes = {
        "BBC Micro": (
            "`BbcMaster512` — int10h.org, CC BY-SA 4.0. "
            "`BbcTeletext` — [Teletext50](https://galax.xyz/Teletext50/), public domain."
        ),
    }

    sections: dict[str, list[dict]] = defaultdict(list)
    section_order: list[str] = []
    for entry in catalog:
        title = group_titles.get(entry["group"], (entry["group"],))[0]
        if title not in sections:
            section_order.append(title)
        sections[title].append(entry)

    lines = [
        "# Bundled terminal fonts",
        "",
        f"{len(catalog)} retro fonts ship with `Faerie.Terminal.Avalonia`. Pick one in your game builder:",
        "",
        "```csharp",
        ".WithFont(BuiltInTerminalFont.IbmVga8x16)",
        "```",
        "",
        "Each enum value maps to a file in this folder. License texts are alongside the font files",
        "(`LICENSE-*.txt`).",
        "",
        "To change the bundled set, edit `scripts/builtin-fonts.manifest.json` and run",
        "`python scripts/fetch-builtin-fonts.py` (downloads fonts, regenerates the enum, and updates this file).",
        "",
        "---",
        "",
    ]

    for title in section_order:
        entries = sections[title]
        lines.append(f"## {title}")
        lines.append("")
        if title in section_notes:
            lines.append(section_notes[title])
        else:
            group = entries[0]["group"]
            mapped = group_titles.get(group)
            if mapped and len(mapped) == 3:
                lines.append(f"Source: {mapped[1]} — {mapped[2]}.")
        lines.append("")
        lines.append("| Enum | File | Description |")
        lines.append("|------|------|-------------|")
        for entry in entries:
            summary = entry["summary"]
            if entry["enum"] == "IbmVga8x16":
                summary += " **Default in sample games.**"
            lines.append(
                f"| `{entry['enum']}` | `{entry['file']}` | {summary} |"
            )
        lines.append("")
        lines.append("---")
        lines.append("")

    if lines[-1] == "":
        lines.pop()
    if lines[-1] == "---":
        lines.pop()

    (OUT / "FONTS.md").write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> None:
    catalog = load_catalog()
    OUT.mkdir(parents=True, exist_ok=True)
    write_licenses()

    allowed = {entry["file"] for entry in catalog}
    for entry in catalog:
        file_name = entry["file"]
        dest = OUT / file_name
        if "kreativeZip" in entry:
            if not dest.exists() or dest.stat().st_size < 500:
                download_kreative(
                    entry["kreativeZip"],
                    entry["kreativeMember"],
                    file_name,
                )
            else:
                print(f"skip existing {file_name}")
        elif file_name == "Teletext50.otf":
            if not dest.exists() or dest.stat().st_size < 500:
                download_teletext()
            else:
                print("skip existing Teletext50.otf")
        else:
            if not dest.exists() or dest.stat().st_size < 500:
                download_int10h(file_name)
            else:
                print(f"skip existing {file_name}")

    cleanup_unlisted(allowed)
    write_csharp(catalog)
    write_fonts_md(catalog)
    print(f"done. {len(catalog)} bundled fonts.")


if __name__ == "__main__":
    main()
