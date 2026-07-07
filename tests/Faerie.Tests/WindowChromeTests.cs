using System.Linq;
using Avalonia.Controls;
using Faerie.Presentation;
using Faerie.Terminal.Avalonia;
using Xunit;

namespace Faerie.Tests;

/// <summary>
/// Tests for the platform-aware in-TUI window controls (issue #116). These cover the pure layout and
/// hit-test logic only; the rendering and pointer wiring live in <see cref="TerminalControl"/> and are
/// exercised by manual GUI smoke tests.
/// </summary>
public sealed class WindowChromeTests
{
    [Fact]
    public void HostPlatform_Current_MatchesOperatingSystem_WhenNoOverrideSet()
    {
        // The FAERIE_WINDOW_CHROME override (if present) intentionally decouples Current from the OS.
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(HostPlatformDetector.OverrideEnvVar)))
            return;

        HostPlatform expected =
            OperatingSystem.IsMacOS() ? HostPlatform.MacOS
            : OperatingSystem.IsWindows() ? HostPlatform.Windows
            : HostPlatform.Linux;

        Assert.Equal(expected, HostPlatformDetector.Current);
    }

    [Fact]
    public void Compute_MacOs_PutsCloseMinimizeMaximize_TopLeft()
    {
        IReadOnlyList<WindowControlCell> cells = WindowControlLayout.Compute(HostPlatform.MacOS, 80);

        Assert.Equal(
            new[] { WindowControlKind.Close, WindowControlKind.Minimize, WindowControlKind.Maximize },
            cells.Select(c => c.Kind).ToArray());

        // Anchored to the left edge, ascending across the row.
        Assert.Equal(WindowControlLayout.EdgeMargin, cells[0].Column);
        Assert.True(cells[0].Column < cells[1].Column);
        Assert.True(cells[1].Column < cells[2].Column);
    }

    [Theory]
    [InlineData(HostPlatform.Windows)]
    [InlineData(HostPlatform.Linux)]
    public void Compute_WindowsLinux_PutMinimizeMaximizeClose_TopRight(HostPlatform platform)
    {
        const int columns = 80;
        IReadOnlyList<WindowControlCell> cells = WindowControlLayout.Compute(platform, columns);

        Assert.Equal(
            new[] { WindowControlKind.Minimize, WindowControlKind.Maximize, WindowControlKind.Close },
            cells.Select(c => c.Kind).ToArray());

        // Anchored to the right edge: the last (Close) button sits one margin in from the edge.
        Assert.Equal(columns - 1 - WindowControlLayout.EdgeMargin, cells[^1].Column);
        Assert.True(cells[0].Column > columns / 2);
    }

    [Theory]
    [InlineData(HostPlatform.MacOS)]
    [InlineData(HostPlatform.Windows)]
    [InlineData(HostPlatform.Linux)]
    public void Compute_ButtonsOccupyDistinctColumns_SoTitleCanBeClippedAroundThem(HostPlatform platform)
    {
        int[] columns = WindowControlLayout.Compute(platform, 80).Select(c => c.Column).ToArray();
        Assert.Equal(columns.Length, columns.Distinct().Count());
    }

    [Theory]
    [InlineData(HostPlatform.MacOS)]
    [InlineData(HostPlatform.Windows)]
    [InlineData(HostPlatform.Linux)]
    public void TryHitTest_RoundTripsEveryButtonColumn(HostPlatform platform)
    {
        const int columns = 80;
        foreach (WindowControlCell cell in WindowControlLayout.Compute(platform, columns))
        {
            Assert.True(WindowControlLayout.TryHitTest(platform, columns, cell.Column, out WindowControlKind kind));
            Assert.Equal(cell.Kind, kind);
        }

        // A column in the middle of the row is a drag region, not a button.
        Assert.False(WindowControlLayout.TryHitTest(platform, columns, columns / 2, out _));
    }

    [Fact]
    public void IsButtonColumn_GapBetweenButtons_IsNotAButton()
    {
        // macOS buttons land on columns 1, 3, 5 with a one-cell gap at 2 and 4.
        Assert.True(WindowControlLayout.IsButtonColumn(HostPlatform.MacOS, 80, 1));
        Assert.False(WindowControlLayout.IsButtonColumn(HostPlatform.MacOS, 80, 2));
        Assert.True(WindowControlLayout.IsButtonColumn(HostPlatform.MacOS, 80, 3));
    }

    [Fact]
    public void Compute_ReturnsEmpty_WhenGridTooNarrowForTheCluster()
    {
        Assert.Empty(WindowControlLayout.Compute(HostPlatform.Windows, 3));
    }

    // ---- resize border hit-testing --------------------------------------------------------

    [Theory]
    [InlineData(2, 2, WindowEdge.NorthWest)]
    [InlineData(998, 2, WindowEdge.NorthEast)]
    [InlineData(2, 598, WindowEdge.SouthWest)]
    [InlineData(998, 598, WindowEdge.SouthEast)]
    [InlineData(2, 300, WindowEdge.West)]
    [InlineData(998, 300, WindowEdge.East)]
    [InlineData(500, 2, WindowEdge.North)]
    [InlineData(500, 598, WindowEdge.South)]
    public void ResizeHitTest_ClassifiesEdgesAndCorners(double x, double y, WindowEdge expected)
    {
        WindowEdge? edge = WindowResizeHitTest.Classify(x, y, width: 1000, height: 600, thickness: 6);
        Assert.Equal(expected, edge);
    }

    [Fact]
    public void ResizeHitTest_InteriorIsNotAResizeZone()
    {
        Assert.Null(WindowResizeHitTest.Classify(500, 300, width: 1000, height: 600, thickness: 6));
    }

    [Fact]
    public void ResizeHitTest_CornerTakesPriorityOverEdge()
    {
        // A point inside the border on both axes is a corner, not a single edge.
        Assert.Equal(WindowEdge.SouthEast, WindowResizeHitTest.Classify(997, 597, 1000, 600, 6));
    }

    // ---- cell-snapped window sizing -------------------------------------------------------

    [Fact]
    public void CellSnap_ExactMultiples_AreUnchanged()
    {
        (double w, double h) = WindowCellSnap.Snap(1000, 600, cellWidth: 10, cellHeight: 20, minWidth: 0, minHeight: 0);
        Assert.Equal(1000, w);
        Assert.Equal(600, h);
    }

    [Theory]
    [InlineData(1004, 1000)]   // 100.4 cells -> 100
    [InlineData(1006, 1010)]   // 100.6 cells -> 101
    [InlineData(1005, 1010)]   // .5 rounds away from zero -> 101
    public void CellSnap_RoundsWidthToWholeCells(double width, double expected)
    {
        (double w, _) = WindowCellSnap.Snap(width, 600, cellWidth: 10, cellHeight: 20, minWidth: 0, minHeight: 0);
        Assert.Equal(expected, w);
    }

    [Fact]
    public void CellSnap_RespectsMinimums_RoundedUpToWholeCells()
    {
        (double w, double h) = WindowCellSnap.Snap(50, 50, cellWidth: 10, cellHeight: 20, minWidth: 200, minHeight: 100);
        Assert.Equal(200, w);   // ceil(200/10) = 20 cells
        Assert.Equal(100, h);   // ceil(100/20) = 5 cells
    }

    [Fact]
    public void CellSnap_ReturnsInputWhenCellSizeUnknown()
    {
        (double w, double h) = WindowCellSnap.Snap(1234, 567, cellWidth: 0, cellHeight: 0, minWidth: 0, minHeight: 0);
        Assert.Equal(1234, w);
        Assert.Equal(567, h);
    }

    [Fact]
    public void CellSnap_NeverProducesZeroCells()
    {
        (double w, double h) = WindowCellSnap.Snap(1, 1, cellWidth: 10, cellHeight: 20, minWidth: 0, minHeight: 0);
        Assert.Equal(10, w);
        Assert.Equal(20, h);
    }

    // ---- title-bar insets reserve room for the controls (no overlap) ----------------------

    [Fact]
    public void BarComposer_RightInset_KeepsRightContentClearOfReservedColumns()
    {
        BarContent content = new() { Right = "AB" };
        GlyphCell[] row = BarComposer.Compose(content, width: 10, leftInset: 0, rightInset: 3);

        // "AB" must end before the 3 reserved right columns (7,8,9), i.e. at columns 5-6.
        Assert.Equal('A', row[5].Glyph);
        Assert.Equal('B', row[6].Glyph);
        Assert.Equal(' ', row[7].Glyph);
        Assert.Equal(' ', row[8].Glyph);
        Assert.Equal(' ', row[9].Glyph);
    }

    [Fact]
    public void BarComposer_LeftInset_ShiftsLeftContentPastReservedColumns()
    {
        BarContent content = new() { Left = "AB" };
        GlyphCell[] row = BarComposer.Compose(content, width: 10, leftInset: 3, rightInset: 0);

        // Columns 0-2 are reserved; "AB" starts at column 3.
        Assert.Equal(' ', row[0].Glyph);
        Assert.Equal(' ', row[2].Glyph);
        Assert.Equal('A', row[3].Glyph);
        Assert.Equal('B', row[4].Glyph);
    }

    [Fact]
    public void BarComposer_NoInset_IsUnchangedBehavior()
    {
        BarContent content = new() { Left = "L", Right = "R" };
        GlyphCell[] row = BarComposer.Compose(content, width: 8);

        Assert.Equal('L', row[0].Glyph);
        Assert.Equal('R', row[7].Glyph);
    }
}
