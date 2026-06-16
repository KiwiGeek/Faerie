using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>Where a thing sits relative to the rest of the world.</summary>
public enum Anchor
{
    /// <summary>Not present anywhere in the world (removed / not yet introduced).</summary>
    Offstage,

    /// <summary>Loose in a room.</summary>
    Room,

    /// <summary>Inside a container thing.</summary>
    Inside,

    /// <summary>On top of a supporter thing.</summary>
    On,

    /// <summary>Carried by the player.</summary>
    Carried,

    /// <summary>Worn by the player.</summary>
    Worn
}

/// <summary>An immutable description of a thing's position in the world.</summary>
public readonly record struct Placement(Anchor Anchor, Room? Room, Thing? Container)
{
    public static Placement InRoom(Room room) => new(Anchor.Room, room, null);
    public static Placement Inside(Thing container) => new(Anchor.Inside, null, container);
    public static Placement On(Thing supporter) => new(Anchor.On, null, supporter);
    public static readonly Placement Carried = new(Anchor.Carried, null, null);
    public static readonly Placement Worn = new(Anchor.Worn, null, null);
    public static readonly Placement Offstage = new(Anchor.Offstage, null, null);
}
