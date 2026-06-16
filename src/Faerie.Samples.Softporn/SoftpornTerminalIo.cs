using System.Collections.Concurrent;
using System.Text;
using Avalonia.Threading;
using Faerie.Presentation;
using Faerie.Terminal;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Softporn;

/// <summary>Feeds Avalonia terminal input to the Softporn interpreter and writes output to a <see cref="TerminalBuffer"/>.</summary>
internal sealed class SoftpornTerminalSession
{
    private readonly TerminalControl _control;
    private readonly TerminalBuffer _buffer;
    private readonly OutputWriter _output;
    private readonly InteractiveTerminalReader _reader;
    private readonly OutputWriterTextWriter _writer;
    private readonly Action _onQuit;

    public SoftpornTerminalSession(TerminalControl control, Action onQuit)
    {
        _control = control;
        _onQuit = onQuit;
        _buffer = new TerminalBuffer(80, 25, new TextStyle(TerminalColor.LightGray, TerminalColor.Black));
        _output = new OutputWriter(_buffer);
        _writer = new OutputWriterTextWriter(_output, PostToUi);
        _reader = new InteractiveTerminalReader();
        _control.Buffer = _buffer;
        _control.CommandEntered += OnCommandEntered;
        _control.Ready += StartGame;
    }

    private void StartGame()
    {
        Task.Run(() =>
        {
            try
            {
                new Interpreter.SoftpornHost(_reader, _writer).Run();
            }
            finally
            {
                PostToUi(() =>
                {
                    _control.EndInput();
                    _onQuit();
                });
            }
        });
    }

    private void OnCommandEntered(string line)
    {
        _output.SyncColumn(0);
        _reader.PushLine(line);
    }

    private static void PostToUi(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            Dispatcher.UIThread.Post(action);
    }
}

internal sealed class OutputWriterTextWriter : TextWriter
{
    private readonly OutputWriter _out;
    private readonly Action<Action> _dispatch;
    private readonly StringBuilder _pending = new();

    public OutputWriterTextWriter(OutputWriter output, Action<Action> dispatch)
    {
        _out = output;
        _dispatch = dispatch;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        if (value == '\n')
            FlushPending(newLine: true);
        else if (value != '\r')
            _pending.Append(value);
    }

    public override void Write(string? value)
    {
        if (value is null) return;
        foreach (char c in value)
            Write(c);
    }

    public override void WriteLine(string? value)
    {
        if (!string.IsNullOrEmpty(value))
            Write(value);
        Write('\n');
    }

    public override void Flush() => FlushPending();

    private void FlushPending(bool newLine = false)
    {
        if (_pending.Length == 0 && !newLine) return;
        string text = _pending.ToString().Replace("{", "{{").Replace("}", "}}");
        _pending.Clear();
        _dispatch(() =>
        {
            if (text.Length > 0)
                _out.Print(text);
            if (newLine)
                _out.NewLine();
        });
    }
}

/// <summary>Line-oriented input from the UI; supports single-character <see cref="TextReader.Read"/> for ReadKey prompts.</summary>
internal sealed class InteractiveTerminalReader : TextReader
{
    private readonly BlockingCollection<string> _lines = new();
    private readonly Queue<char> _chars = new();

    public void PushLine(string line)
    {
        _lines.Add(line);
    }

    public override string? ReadLine()
    {
        DrainChars();
        return _lines.Take();
    }

    public override int Read()
    {
        if (_chars.Count == 0)
        {
            string line = _lines.Take();
            foreach (char c in line)
                _chars.Enqueue(c);
            _chars.Enqueue('\n');
        }

        return _chars.Dequeue();
    }

    private void DrainChars()
    {
        while (_chars.Count > 0 && _chars.Peek() != '\n')
            _chars.Dequeue();
        if (_chars.Count > 0 && _chars.Peek() == '\n')
            _chars.Dequeue();
    }
}
