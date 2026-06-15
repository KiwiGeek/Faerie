using System.Text;

namespace Nixie.Presentation;

/// <summary>
/// A tiny, forgiving markup language for styled output. Tags are enclosed in braces and nest:
/// <list type="bullet">
/// <item><c>{fg:red}</c> / <c>{bg:blue}</c> — set foreground / background (palette name or #RRGGBB)</item>
/// <item><c>{bold}</c>, <c>{underline}</c>, <c>{blink}</c>, <c>{inverse}</c> — toggle attributes on</item>
/// <item><c>{/}</c> — pop the most recent style change</item>
/// </list>
/// A literal brace is written as <c>{{</c> or <c>}}</c>.
/// </summary>
public static class Markup
{
    public static IReadOnlyList<StyledSpan> Parse(string text, TextStyle baseStyle)
    {
        List<StyledSpan> spans = [];
        Stack<TextStyle> stack = new();
        stack.Push(baseStyle);

        StringBuilder current = new();

        void Flush()
        {
            if (current.Length == 0) return;
            spans.Add(new StyledSpan(current.ToString(), stack.Peek()));
            current.Clear();
        }

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '{' && i + 1 < text.Length && text[i + 1] == '{') { current.Append('{'); i++; continue; }
            if (c == '}' && i + 1 < text.Length && text[i + 1] == '}') { current.Append('}'); i++; continue; }

            if (c == '{')
            {
                int end = text.IndexOf('}', i + 1);
                if (end < 0) { current.Append(c); continue; }

                string tag = text.Substring(i + 1, end - i - 1).Trim();
                i = end;

                Flush();

                if (tag == "/")
                {
                    if (stack.Count > 1) stack.Pop();
                }
                else
                {
                    stack.Push(ApplyTag(stack.Peek(), tag));
                }

                continue;
            }

            current.Append(c);
        }

        Flush();
        return spans;
    }

    private static TextStyle ApplyTag(TextStyle style, string tag)
    {
        if (tag.StartsWith("fg:", StringComparison.OrdinalIgnoreCase))
            return TerminalColor.TryParse(tag[3..], out TerminalColor fg) ? style.WithForeground(fg) : style;

        if (tag.StartsWith("bg:", StringComparison.OrdinalIgnoreCase))
            return TerminalColor.TryParse(tag[3..], out TerminalColor bg) ? style.WithBackground(bg) : style;

        return tag.ToLowerInvariant() switch
        {
            "bold" or "b" => style.Add(TextAttributes.Bold),
            "underline" or "u" => style.Add(TextAttributes.Underline),
            "blink" => style.Add(TextAttributes.Blink),
            "inverse" or "reverse" => style.Add(TextAttributes.Inverse),
            _ => TerminalColor.TryParse(tag, out TerminalColor named) ? style.WithForeground(named) : style
        };
    }

    /// <summary>Strips all markup tags, returning plain text (useful for measuring / tests).</summary>
    public static string Strip(string text)
    {
        StringBuilder sb = new();
        foreach (StyledSpan span in Parse(text, TextStyle.Default))
            sb.Append(span.Text);
        return sb.ToString();
    }
}
