using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Faerie.Runtime;

namespace Faerie.Samples.Softporn;

/// <summary>Verbatim long messages from embedded <c>Data/softporn.txt</c> (original M1–M72).</summary>
internal static class SoftpornMessages
{
    private static readonly string[] Messages = Load();

    public static string Text(int messageNumber)
    {
        if (messageNumber < 1 || messageNumber > Messages.Length) return "";
        return Messages[messageNumber - 1];
    }

    public static void Say(GameContext ctx, int messageNumber) => ctx.Say(Text(messageNumber));

    public static void SayLong(GameContext ctx, int messageNumber)
    {
        ctx.Blank();
        ctx.Say(Text(messageNumber));
    }

    public static void SayBlock(GameContext ctx, int from, int to)
    {
        ctx.Blank();
        for (int n = from; n <= to; n++)
            ctx.Say(Text(n));
    }

    private static string[] Load()
    {
        Assembly asm = typeof(SoftpornMessages).Assembly;
        string? resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("softporn.txt", StringComparison.OrdinalIgnoreCase));

        string text;
        if (resourceName is not null)
        {
            using Stream stream = asm.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            text = reader.ReadToEnd();
        }
        else
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "softporn.txt");
            text = File.ReadAllText(path, Encoding.UTF8);
        }

        return Parse(text);
    }

    internal static string[] Parse(string text)
    {
        var messages = new string[72];
        var headerRx = new Regex(@"^###\s+M\s+(\d+)\s", RegexOptions.Multiline);
        var matches = headerRx.Matches(text).Cast<Match>().ToList();

        for (int i = 0; i < matches.Count; i++)
        {
            int num = int.Parse(matches[i].Groups[1].Value);
            if (num is < 1 or > 72) continue;

            int headerEnd = text.IndexOf('\n', matches[i].Index);
            headerEnd = headerEnd < 0 ? text.Length : headerEnd + 1;
            int end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            string block = text[headerEnd..end];

            List<string> lines = block.Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .SkipWhile(string.IsNullOrWhiteSpace)
                .ToList();

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
                lines.RemoveAt(lines.Count - 1);

            messages[num - 1] = string.Join('\n', lines);
        }

        return messages;
    }
}
