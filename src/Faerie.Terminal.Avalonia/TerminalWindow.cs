using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// A ready-made window that hosts a <see cref="TerminalControl"/> filling the whole client area,
/// with F11 (or Alt+Enter) toggling fullscreen. Games can use this directly or host the control in
/// their own window.
/// </summary>
public class TerminalWindow : Window
{
    public TerminalWindow()
    {
        Background = Brushes.Black;
        Title = "Text Adventure";
        Width = 960;
        Height = 600;
        MinWidth = 480;
        MinHeight = 300;

        Terminal = new TerminalControl();
        Content = Terminal;
    }

    public TerminalControl Terminal { get; }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Terminal.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        bool altEnter = e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        if (e.Key == Key.F11 || altEnter)
        {
            WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}
