# Bundled terminal fonts

20 retro fonts ship with `Faerie.Terminal.Avalonia`. Pick one in your game builder:

```csharp
.WithFont(BuiltInTerminalFont.IbmVga8x16)
```

Each enum value maps to a file in this folder. License texts are alongside the font files
(`LICENSE-*.txt`).

To change the bundled set, edit `scripts/builtin-fonts.manifest.json` and run
`python scripts/fetch-builtin-fonts.py` (downloads fonts, regenerates the enum, and updates this file).

---

## IBM PC

Source: int10h.org — CC BY-SA 4.0.

| Enum | File | Description |
|------|------|-------------|
| `IbmBios8x8` | `PxPlus_IBM_BIOS.ttf` | PC BIOS 8×8 (shared by CGA graphics and early text modes). |
| `IbmCga8x8` | `PxPlus_IBM_CGA.ttf` | IBM CGA 8×8. |
| `IbmMda9x14` | `PxPlus_IBM_MDA.ttf` | IBM MDA 9×14 (80-column monochrome with 9th-dot box drawing). |
| `IbmEga8x14` | `PxPlus_IBM_EGA_8x14.ttf` | IBM EGA 8×14. |
| `IbmVga8x14` | `PxPlus_IBM_VGA_8x14.ttf` | IBM VGA 8×14. |
| `IbmVga8x16` | `PxPlus_IBM_VGA_8x16.ttf` | IBM VGA 8×16 (classic DOS 80×25). **Default in sample games.** |
| `IbmVga9x16` | `PxPlus_IBM_VGA_9x16.ttf` | IBM VGA 9×16 (9-dot-wide text mode). |

---

## IBM PC compatibles

Source: int10h.org — CC BY-SA 4.0.

| Enum | File | Description |
|------|------|-------------|
| `Tandy10008x16` | `PxPlus_Tandy1K-II_200L.ttf` | Tandy 1000 / PCjr 200-line 8×16. |
| `AmstradPc` | `PxPlus_Amstrad_PC.ttf` | Amstrad PC1512 system font. |
| `DecRainbow80Col` | `PxPlus_Rainbow100_re_80.ttf` | DEC Rainbow 100 80-column text. |
| `Kaypro2k` | `Px437_Kaypro2K_G.ttf` | Kaypro 2000 portable. |

---

## 8-bit classics

Source: Kreative Software — Free Use License 1.2f.

| Enum | File | Description |
|------|------|-------------|
| `AppleIIe` | `PrintChar21.ttf` | Apple II 40-column text (Print Char 21). |
| `AppleII80Column` | `PRNumber3.ttf` | Apple II 80-column text (PR Number 3). |
| `Commodore64` | `PetMe64.ttf` | Commodore 64 PETSCII (Pet Me 64). |
| `ZxSpectrum` | `Speccy.ttf` | ZX Spectrum. |
| `Atari8Bit` | `CandyAntics.ttf` | Atari 8-bit (Candy Antics). |
| `AtariSt` | `ProjectJasonSmall.ttf` | Atari ST low resolution (Project Jason Small). |
| `Trs80CoCo` | `HotCoCo.ttf` | TRS-80 Color Computer. |

---

## BBC Micro

`BbcMaster512` — int10h.org, CC BY-SA 4.0. `BbcTeletext` — [Teletext50](https://galax.xyz/Teletext50/), public domain.

| Enum | File | Description |
|------|------|-------------|
| `BbcMaster512` | `Px437_Master_512.ttf` | BBC Master 512 MOS 8×8 (Modes 0–6). |
| `BbcTeletext` | `Teletext50.otf` | BBC Mode 7 / teletext (SAA5050-style, Teletext50). |

