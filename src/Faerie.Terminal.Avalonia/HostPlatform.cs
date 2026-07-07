namespace Faerie.Terminal.Avalonia;

/// <summary>
/// The desktop platform hosting the Avalonia window. Used to choose the placement and styling of the
/// in-TUI window controls (macOS puts a traffic-light cluster top-left; Windows and Linux put
/// minimize/maximize/close top-right).
/// </summary>
public enum HostPlatform
{
    /// <summary>Microsoft Windows.</summary>
    Windows,

    /// <summary>Apple macOS.</summary>
    MacOS,

    /// <summary>Linux and any other X11/Wayland-style desktop (the default fallback).</summary>
    Linux,
}

/// <summary>
/// Resolves the current <see cref="HostPlatform"/>. Kept separate from the layout logic so tests can
/// drive layout for any platform without depending on the machine they run on.
/// </summary>
public static class HostPlatformDetector
{
    /// <summary>
    /// Environment variable that forces the window-control style regardless of the real OS, for
    /// previewing (e.g. set <c>FAERIE_WINDOW_CHROME=macos</c> on Windows to see the traffic-light
    /// cluster). Accepts <c>macos</c>/<c>mac</c>, <c>windows</c>/<c>win</c>, or <c>linux</c>.
    /// </summary>
    public const string OverrideEnvVar = "FAERIE_WINDOW_CHROME";

    /// <summary>
    /// The platform whose window-control convention should be used. Honors <see cref="OverrideEnvVar"/>
    /// if set; otherwise the real OS (anything that is neither macOS nor Windows is treated as Linux).
    /// </summary>
    public static HostPlatform Current
    {
        get
        {
            string? forced = Environment.GetEnvironmentVariable(OverrideEnvVar);
            if (!string.IsNullOrWhiteSpace(forced))
            {
                switch (forced.Trim().ToLowerInvariant())
                {
                    case "macos" or "mac" or "osx": return HostPlatform.MacOS;
                    case "windows" or "win": return HostPlatform.Windows;
                    case "linux" or "x11" or "wayland": return HostPlatform.Linux;
                }
            }

            return OperatingSystem.IsMacOS() ? HostPlatform.MacOS
                : OperatingSystem.IsWindows() ? HostPlatform.Windows
                : HostPlatform.Linux;
        }
    }
}
