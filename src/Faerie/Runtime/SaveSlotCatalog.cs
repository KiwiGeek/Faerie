namespace Faerie.Runtime;

/// <summary>How save files are named from an optional slot label.</summary>
public enum SaveSlotNaming
{
    /// <summary><c>{baseName}{ext}</c> or <c>{baseName}-{label}{ext}</c>.</summary>
    SuffixDash,

    /// <summary>Sierra style: <c>SOFTP</c> + up to 3 label chars; empty label uses the catalog base name.</summary>
    SierraPrefix
}

/// <summary>One on-disk save slot discovered or addressed by label.</summary>
public sealed record SaveSlotEntry(string Label, string FileName, string FullPath, DateTime? LastWriteUtc);

/// <summary>Save vs restore when prompting for a slot.</summary>
public enum SavePickMode { Save, Restore }

/// <summary>Arguments for an optional host-provided save-slot picker (#40).</summary>
public sealed class SaveSlotPickRequest
{
    public SaveSlotPickRequest(SavePickMode mode, IReadOnlyList<SaveSlotEntry> slots)
    {
        Mode = mode;
        Slots = slots;
    }

    public SavePickMode Mode { get; }
    public IReadOnlyList<SaveSlotEntry> Slots { get; }
}

/// <summary>
/// Maps optional slot labels to files under a directory. Used by <see cref="GameEngine"/> and hosts.
/// </summary>
public sealed class SaveSlotCatalog
{
    public SaveSlotCatalog(
        string directory,
        string baseName,
        string extension = ".json",
        SaveSlotNaming naming = SaveSlotNaming.SuffixDash,
        int maxLabelLength = 3)
    {
        Directory = directory;
        BaseName = baseName;
        Extension = extension.StartsWith('.') ? extension : "." + extension;
        Naming = naming;
        MaxLabelLength = maxLabelLength;
    }

    public string Directory { get; }
    public string BaseName { get; }
    public string Extension { get; }
    public SaveSlotNaming Naming { get; }
    public int MaxLabelLength { get; }

    public string NormalizeLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label)) return "";
        string s = label.Trim().ToUpperInvariant();
        return s.Length <= MaxLabelLength ? s : s[..MaxLabelLength];
    }

    public string FileNameForLabel(string? label)
    {
        string norm = NormalizeLabel(label);
        return Naming switch
        {
            SaveSlotNaming.SierraPrefix => SierraFileName(norm),
            _ => SuffixDashFileName(norm)
        };
    }

    public string PathForLabel(string? label) => System.IO.Path.Combine(Directory, FileNameForLabel(label));

    public IReadOnlyList<SaveSlotEntry> ListSlots()
    {
        if (!System.IO.Directory.Exists(Directory))
            return [];

        var results = new List<SaveSlotEntry>();
        foreach (string path in System.IO.Directory.EnumerateFiles(Directory, "*" + Extension))
        {
            string file = System.IO.Path.GetFileName(path);
            if (!TryLabelFromFileName(file, out string? label))
                continue;

            DateTime? written = null;
            try { written = System.IO.File.GetLastWriteTimeUtc(path); }
            catch { /* ignore */ }

            results.Add(new SaveSlotEntry(label, file, path, written));
        }

        return results.OrderByDescending(s => s.LastWriteUtc ?? DateTime.MinValue).ToList();
    }

    public bool Exists(string? label) => System.IO.File.Exists(PathForLabel(label));

    public void Write(string? label, string json)
    {
        System.IO.Directory.CreateDirectory(Directory);
        System.IO.File.WriteAllText(PathForLabel(label), json);
    }

    public string? Read(string? label)
    {
        string path = PathForLabel(label);
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
    }

    public string DisplayName(string? label)
    {
        string norm = NormalizeLabel(label);
        return string.IsNullOrEmpty(norm) ? FileNameForLabel(null) : FileNameForLabel(norm);
    }

    private string SuffixDashFileName(string norm) =>
        string.IsNullOrEmpty(norm)
            ? BaseName + Extension
            : $"{BaseName}-{norm.ToLowerInvariant()}{Extension}";

    private string SierraFileName(string norm)
    {
        // SOFTP + up to 3 chars; bare SOFTP → SOFTPORN (Softporn default).
        const string sierraStem = "SOFTP";
        string stem = sierraStem + norm;
        stem = stem.Replace(" ", "");
        if (stem.Equals(sierraStem, StringComparison.OrdinalIgnoreCase))
            stem = BaseName.ToUpperInvariant();
        return stem + Extension.ToUpperInvariant();
    }

    private bool TryLabelFromFileName(string fileName, out string label)
    {
        label = "";
        if (!fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            return false;

        string stem = fileName[..^Extension.Length];

        if (Naming == SaveSlotNaming.SierraPrefix)
        {
            if (stem.Equals(BaseName, StringComparison.OrdinalIgnoreCase))
                return true;
            if (stem.StartsWith("SOFTP", StringComparison.OrdinalIgnoreCase) && stem.Length > 5)
            {
                label = stem[5..];
                return true;
            }
            return stem.StartsWith("SOFTP", StringComparison.OrdinalIgnoreCase);
        }

        if (stem.Equals(BaseName, StringComparison.OrdinalIgnoreCase))
            return true;

        string prefix = BaseName + "-";
        if (stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && stem.Length > prefix.Length)
        {
            label = stem[prefix.Length..].ToUpperInvariant();
            return true;
        }

        return false;
    }
}
