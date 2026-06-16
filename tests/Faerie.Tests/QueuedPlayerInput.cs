using Faerie.Presentation;

namespace Faerie.Tests;

/// <summary>Deterministic <see cref="IPlayerInput"/> for tests.</summary>
public sealed class QueuedPlayerInput : IPlayerInput
{
    private readonly Queue<string> _lines;

    public QueuedPlayerInput(params string[] lines) => _lines = new Queue<string>(lines);

    public QueuedPlayerInput(IEnumerable<string> lines) => _lines = new Queue<string>(lines);

    public string ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : "";

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
