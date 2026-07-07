using Avalonia.Controls;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// A window-management control drawn as a character cell inside the TUI, replacing the native OS
/// title-bar buttons that are removed when the window becomes borderless.
/// </summary>
public enum WindowControlKind
{
    /// <summary>Minimizes the window (iconify).</summary>
    Minimize,

    /// <summary>Toggles between the normal (snapped) size and maximized.</summary>
    Maximize,

    /// <summary>Closes the window (ends the game).</summary>
    Close,
}

/// <summary>
/// One window control placed at a specific column on the chrome (title) row.
/// </summary>
public readonly record struct WindowControlCell(WindowControlKind Kind, int Column);

/// <summary>
/// Pure, platform-aware layout for the in-TUI window controls. Contains no Avalonia types so it can be
/// unit-tested directly: given a platform and the grid width in columns, it returns which control sits
/// in which column of row 0, and hit-tests a column back to a control.
///
/// <para>Placement follows OS convention: macOS shows a close/minimize/maximize cluster at the
/// top-left (styled as red/yellow/green traffic lights by the renderer); Windows and Linux show
/// minimize/maximize/close at the top-right.</para>
/// </summary>
public static class WindowControlLayout
{
    /// <summary>Blank columns kept between the cluster and the nearest window edge.</summary>
    public const int EdgeMargin = 1;

    /// <summary>Blank columns kept between adjacent control buttons.</summary>
    public const int ButtonGap = 1;

    // Left-to-right visual order of the buttons on each platform.
    private static readonly WindowControlKind[] MacOrder =
        [WindowControlKind.Close, WindowControlKind.Minimize, WindowControlKind.Maximize];

    private static readonly WindowControlKind[] WinLinuxOrder =
        [WindowControlKind.Minimize, WindowControlKind.Maximize, WindowControlKind.Close];

    /// <summary>True when the control cluster is anchored to the left edge (macOS convention).</summary>
    public static bool IsLeftAligned(HostPlatform platform) => platform == HostPlatform.MacOS;

    /// <summary>Total columns the button cluster spans, including the gaps between buttons.</summary>
    public static int ClusterWidth(HostPlatform platform)
    {
        int count = OrderFor(platform).Length;
        return count + (count - 1) * ButtonGap;
    }

    private static WindowControlKind[] OrderFor(HostPlatform platform) =>
        IsLeftAligned(platform) ? MacOrder : WinLinuxOrder;

    /// <summary>
    /// Computes the control cells for a grid <paramref name="columns"/> wide on <paramref name="platform"/>,
    /// in left-to-right column order. Returns an empty list when the grid is too narrow to hold the cluster.
    /// </summary>
    public static IReadOnlyList<WindowControlCell> Compute(HostPlatform platform, int columns)
    {
        WindowControlKind[] order = OrderFor(platform);
        int span = ClusterWidth(platform);
        if (columns < span + EdgeMargin)
            return [];

        int start = IsLeftAligned(platform)
            ? EdgeMargin
            : columns - EdgeMargin - span;

        WindowControlCell[] cells = new WindowControlCell[order.Length];
        int col = start;
        for (int i = 0; i < order.Length; i++)
        {
            cells[i] = new WindowControlCell(order[i], col);
            col += 1 + ButtonGap;
        }

        return cells;
    }

    /// <summary>
    /// Hit-tests a column on the chrome row to a control. Returns false for gap/drag columns.
    /// </summary>
    public static bool TryHitTest(HostPlatform platform, int columns, int column, out WindowControlKind kind)
    {
        foreach (WindowControlCell cell in Compute(platform, columns))
        {
            if (cell.Column == column)
            {
                kind = cell.Kind;
                return true;
            }
        }

        kind = default;
        return false;
    }

    /// <summary>True when <paramref name="column"/> on the chrome row is occupied by a control button.</summary>
    public static bool IsButtonColumn(HostPlatform platform, int columns, int column) =>
        TryHitTest(platform, columns, column, out _);
}

/// <summary>
/// Pure hit-testing for the borderless window's resize border. Classifies a point relative to the
/// window bounds into one of the eight <see cref="WindowEdge"/> resize zones (or none). Uses Avalonia's
/// <see cref="WindowEdge"/> enum only as a value type, so it remains unit-testable without a window.
/// </summary>
public static class WindowResizeHitTest
{
    /// <summary>
    /// Returns the resize edge/corner for a point <paramref name="x"/>,<paramref name="y"/> within a
    /// window of <paramref name="width"/> x <paramref name="height"/>, using a border of
    /// <paramref name="thickness"/> px, or null when the point is in the interior.
    /// </summary>
    public static WindowEdge? Classify(double x, double y, double width, double height, double thickness)
    {
        bool left = x <= thickness;
        bool right = x >= width - thickness;
        bool top = y <= thickness;
        bool bottom = y >= height - thickness;

        if (top && left) return WindowEdge.NorthWest;
        if (top && right) return WindowEdge.NorthEast;
        if (bottom && left) return WindowEdge.SouthWest;
        if (bottom && right) return WindowEdge.SouthEast;
        if (left) return WindowEdge.West;
        if (right) return WindowEdge.East;
        if (top) return WindowEdge.North;
        if (bottom) return WindowEdge.South;
        return null;
    }
}

/// <summary>
/// Pure cell-snapping for the borderless window (issue #116): rounds a proposed client size to a whole
/// number of character cells so the grid meets the window edges with no black border. Enforces the
/// window's minimum size (rounded up to whole cells). Used only in the normal (non-maximized) state.
/// </summary>
public static class WindowCellSnap
{
    /// <summary>
    /// Snaps <paramref name="width"/> x <paramref name="height"/> to whole multiples of
    /// <paramref name="cellWidth"/> x <paramref name="cellHeight"/>, never smaller than
    /// <paramref name="minWidth"/> x <paramref name="minHeight"/>. Returns the input unchanged if the
    /// cell size is not yet known.
    /// </summary>
    public static (double Width, double Height) Snap(
        double width, double height, double cellWidth, double cellHeight, double minWidth, double minHeight)
    {
        if (cellWidth <= 0 || cellHeight <= 0)
            return (width, height);

        int cols = CellsFor(width, cellWidth, minWidth);
        int rows = CellsFor(height, cellHeight, minHeight);
        return (cols * cellWidth, rows * cellHeight);
    }

    private static int CellsFor(double extent, double cell, double min)
    {
        int minCells = Math.Max(1, (int)Math.Ceiling(min / cell));
        int cells = (int)Math.Round(extent / cell, MidpointRounding.AwayFromZero);
        return Math.Max(minCells, Math.Max(1, cells));
    }
}
