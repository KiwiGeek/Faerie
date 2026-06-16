namespace Faerie.Presentation;

/// <summary>
/// The shape of the input caret. Chosen by the game definition so a game can match the era it is
/// emulating (a solid block for DOS/Apple ][, a thin underline for some 8-bit machines, etc.).
/// </summary>
public enum TerminalCursor
{
    /// <summary>A filled block covering the whole cell (DOS / Apple ][ style).</summary>
    Block,

    /// <summary>A thin line along the bottom of the cell.</summary>
    Underline,

    /// <summary>A thin vertical bar at the left of the cell.</summary>
    Bar,

    /// <summary>A hollow outline of the cell.</summary>
    Outline
}
