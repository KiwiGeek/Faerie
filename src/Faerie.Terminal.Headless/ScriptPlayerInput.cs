using Faerie.Presentation;

namespace Faerie.Terminal.Headless;

/// <summary>Feeds mid-turn prompts from the same script queue as top-level commands.</summary>
public sealed class ScriptPlayerInput : IPlayerInput
{
    private readonly Queue<string> _lines;
    private readonly Action<string>? _onRead;

    public ScriptPlayerInput(Queue<string> lines, Action<string>? onRead = null)
    {
        _lines = lines;
        _onRead = onRead;
    }

    public int Remaining => _lines.Count;

    public string ReadLine()
    {
        if (_lines.Count == 0) return "";
        string line = _lines.Dequeue();
        _onRead?.Invoke(line);
        return line;
    }

    public char ReadKey(ReadOnlySpan<char> validKeys)
    {
        string line = ReadLine().Trim();
        if (line.Length == 0) return '\0';

        char c = char.ToLowerInvariant(line[0]);
        foreach (char valid in validKeys)
        {
            if (char.ToLowerInvariant(valid) == c)
                return c;
        }

        return c;
    }
}
