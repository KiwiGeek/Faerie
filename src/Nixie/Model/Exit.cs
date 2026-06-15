using Nixie.Runtime;

namespace Nixie.Model;

/// <summary>
/// A connection leading out of a room in a given direction. Exits may be gated by an optional
/// condition and/or by a door <see cref="Thing"/> that must be open to pass.
/// </summary>
public sealed class Exit
{
    public Exit(Direction direction, Room destination)
    {
        Direction = direction;
        Destination = destination;
    }

    public Direction Direction { get; }

    /// <summary>The room reached by travelling this way.</summary>
    public Room Destination { get; }

    /// <summary>Optional door governing this exit; if set and not open, passage is blocked.</summary>
    public Thing? Door { get; set; }

    /// <summary>Optional predicate; when it returns false the player cannot pass.</summary>
    public Func<GameContext, bool>? Condition { get; set; }

    /// <summary>Message shown when <see cref="Condition"/> blocks passage.</summary>
    public string? BlockedMessage { get; set; }

    /// <summary>Determines whether the player may currently travel through this exit.</summary>
    public bool CanPass(GameContext context, out string? reason)
    {
        if (Door is not null && Door.Has(Attr.Openable) && !Door.Has(Attr.Open))
        {
            reason = $"The {Door.Name} is closed.";
            return false;
        }

        if (Condition is not null && !Condition(context))
        {
            reason = BlockedMessage ?? "You can't go that way.";
            return false;
        }

        reason = null;
        return true;
    }
}
