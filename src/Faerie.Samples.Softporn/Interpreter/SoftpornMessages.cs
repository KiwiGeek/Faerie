using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Faerie.Samples.Softporn.Interpreter;

public sealed class SoftpornMessages
{
    private readonly TextWriter _out;
    private readonly string[] _messages;

    private static readonly int[] DelayMessages = [39, 58, 69];

    public SoftpornMessages(TextWriter output)
    {
        _out = output;
        _messages = LoadFromEmbeddedResource();
    }

    public void WriteLongMessg(int messgNo)
    {
        if (messgNo < 1 || messgNo > _messages.Length || string.IsNullOrEmpty(_messages[messgNo - 1]))
            return;

        var text = _messages[messgNo - 1];
        var delay = DelayMessages.Contains(messgNo);

        foreach (var c in text)
        {
            if (c == '\r')
                continue;
            if (c == '\n')
                _out.WriteLine();
            else if (c >= ' ')
            {
                _out.Write(c);
                if (delay)
                    Thread.Sleep(150);
            }
        }
    }

    public void WriteLongMessage(int messgNo)
    {
        _out.WriteLine();
        WriteLongMessg(messgNo);
    }

    public void WriteMessage(string message)
    {
        _out.WriteLine();
        _out.WriteLine(message);
    }

    private static string[] LoadFromEmbeddedResource()
    {
        var asm = Assembly.GetExecutingAssembly();
        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("softporn.txt", StringComparison.OrdinalIgnoreCase));

        string text;
        if (resourceName != null)
        {
            using var stream = asm.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            text = reader.ReadToEnd();
        }
        else
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "softporn.txt");
            if (!File.Exists(path))
            {
                path = Path.Combine(
                    Path.GetDirectoryName(asm.Location) ?? ".",
                    "..", "..", "..", "Data", "softporn.txt");
            }
            text = File.ReadAllText(path, Encoding.UTF8);
        }

        return ParseSoftpornTxt(text);
    }

    internal static string[] ParseSoftpornTxt(string text)
    {
        var messages = new string[72];
        var headerRx = new Regex(@"^###\s+M\s+(\d+)\s", RegexOptions.Multiline);
        var matches = headerRx.Matches(text).Cast<Match>().ToList();

        for (var i = 0; i < matches.Count; i++)
        {
            var num = int.Parse(matches[i].Groups[1].Value);
            if (num < 1 || num > 72)
                continue;

            var headerEnd = text.IndexOf('\n', matches[i].Index);
            if (headerEnd < 0)
                headerEnd = text.Length;
            else
                headerEnd++;

            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var block = text[headerEnd..end];

            var lines = block.Split('\n')
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
