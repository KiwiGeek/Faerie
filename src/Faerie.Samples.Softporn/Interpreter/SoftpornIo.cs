namespace Faerie.Samples.Softporn.Interpreter;

public sealed class SoftpornIo
{
    private readonly TextReader _in;
    private readonly TextWriter _out;
    private Queue<string>? _scriptLines;
    private Queue<char>? _scriptChars;
    private string _lineFromKbd = "";

    public SoftpornIo(TextReader input, TextWriter output, IEnumerable<string>? scriptLines = null)
    {
        _in = input;
        _out = output;
        if (scriptLines != null)
        {
            _scriptLines = new Queue<string>(scriptLines);
            _scriptChars = new Queue<char>();
        }
    }

    private void EnsureScriptChars()
    {
        while (_scriptLines is { Count: > 0 } && (_scriptChars == null || _scriptChars.Count == 0))
        {
            var line = _scriptLines.Dequeue();
            foreach (var c in line)
                _scriptChars!.Enqueue(c);
        }
    }

    public string LineFromKbd
    {
        get => _lineFromKbd;
        set => _lineFromKbd = value;
    }

    public string ReadLine(string prompt)
    {
        _out.Write(prompt);
        _out.Flush();

        if (_scriptLines is { Count: > 0 })
            return _scriptLines.Dequeue();

        return _in.ReadLine() ?? "";
    }

    public char ReadKey(string prompt, ReadOnlySpan<char> valid)
    {
        _out.Write(prompt);
        _out.Flush();
        var cset = valid.ToArray().Select(char.ToUpperInvariant).ToHashSet();

        while (true)
        {
            char ch;
            if (_scriptLines != null)
            {
                EnsureScriptChars();
                if (_scriptChars is { Count: > 0 })
                    ch = _scriptChars.Dequeue();
                else
                    ch = '\n';
            }
            else
            {
                var read = _in.Read();
                if (read < 0)
                    return '\0';
                ch = (char)read;
            }

            var chUp = char.ToUpperInvariant(ch);
            if (cset.Contains(chUp))
            {
                _out.WriteLine(chUp);
                return chUp;
            }

            _out.Write('\a');
        }
    }

    public void WriteBell() => _out.Write('\a');

    public TextWriter Writer => _out;
}
