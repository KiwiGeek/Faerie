namespace Nixie.Model;

/// <summary>
/// The set of movement directions the engine understands out of the box.
/// Authors never need to reference directions by string; the enum is the source of truth.
/// </summary>
public enum Direction
{
    North,
    South,
    East,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest,
    Up,
    Down,
    In,
    Out
}

public static class DirectionExtensions
{
    /// <summary>Returns the reciprocal direction (North &lt;-&gt; South, Up &lt;-&gt; Down, In &lt;-&gt; Out, ...).</summary>
    public static Direction Opposite(this Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        Direction.NorthEast => Direction.SouthWest,
        Direction.SouthWest => Direction.NorthEast,
        Direction.NorthWest => Direction.SouthEast,
        Direction.SouthEast => Direction.NorthWest,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.In => Direction.Out,
        Direction.Out => Direction.In,
        _ => direction
    };

    /// <summary>Human-readable label, e.g. "north-east".</summary>
    public static string ToDisplayString(this Direction direction) => direction switch
    {
        Direction.NorthEast => "north-east",
        Direction.NorthWest => "north-west",
        Direction.SouthEast => "south-east",
        Direction.SouthWest => "south-west",
        _ => direction.ToString().ToLowerInvariant()
    };

    /// <summary>The words a player can type to mean this direction (including abbreviations).</summary>
    public static IReadOnlyList<string> Words(this Direction direction) => direction switch
    {
        Direction.North => ["north", "n"],
        Direction.South => ["south", "s"],
        Direction.East => ["east", "e"],
        Direction.West => ["west", "w"],
        Direction.NorthEast => ["northeast", "north-east", "ne"],
        Direction.NorthWest => ["northwest", "north-west", "nw"],
        Direction.SouthEast => ["southeast", "south-east", "se"],
        Direction.SouthWest => ["southwest", "south-west", "sw"],
        Direction.Up => ["up", "u"],
        Direction.Down => ["down", "d"],
        Direction.In => ["in", "inside", "enter"],
        Direction.Out => ["out", "outside", "exit"],
        _ => []
    };

    /// <summary>Attempts to parse a single token into a direction.</summary>
    public static bool TryParse(string token, out Direction direction)
    {
        token = token.Trim().ToLowerInvariant();
        foreach (Direction value in Enum.GetValues<Direction>())
        {
            if (value.Words().Contains(token))
            {
                direction = value;
                return true;
            }
        }

        direction = default;
        return false;
    }
}
