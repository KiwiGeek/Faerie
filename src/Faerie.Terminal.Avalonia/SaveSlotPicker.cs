using Faerie.Presentation;
using Faerie.Runtime;

namespace Faerie.Terminal.Avalonia;

/// <summary>In-terminal save/restore slot browser (#40).</summary>
public static class SaveSlotPicker
{
    public static string? Pick(TerminalControl terminal, OutputWriter output, SaveSlotPickRequest request)
    {
        output.Blank();
        output.PrintLine(request.Mode == SavePickMode.Save
            ? "Save game — pick a slot (↑↓ Enter, Esc cancel):"
            : "Restore game — pick a slot (↑↓ Enter, Esc cancel):");

        var options = BuildOptions(request);
        if (options.Count == 0)
            return null;

        for (int i = 0; i < options.Count; i++)
            output.PrintLine($"  {(i + 1).ToString().PadRight(3)}{options[i].Display}");

        return terminal.RunSlotPicker(options, request.Mode == SavePickMode.Save);
    }

    private static List<SlotOption> BuildOptions(SaveSlotPickRequest request)
    {
        var options = new List<SlotOption>();

        if (request.Mode == SavePickMode.Save)
            options.Add(new SlotOption("", "(default slot)"));

        foreach (SaveSlotEntry entry in request.Slots)
        {
            string tag = entry.LastWriteUtc is { } t
                ? $"  {entry.FileName}  ({t.ToLocalTime():g})"
                : $"  {entry.FileName}";
            options.Add(new SlotOption(entry.Label, tag));
        }

        if (request.Mode == SavePickMode.Save)
            options.Add(new SlotOption(null, "(type a new name — letters, Enter)"));

        return options;
    }

    internal sealed record SlotOption(string? Label, string Display);
}
