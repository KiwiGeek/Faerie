using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Building;

/// <summary>
/// A forward reference to a <see cref="Room"/> that may not exist yet. Use with
/// <see cref="RoomFluent.Connect(Room, Direction, RoomRef, bool, Func{GameContext, bool}?, string?)"/>
/// so exits can be declared from the source room before the destination is constructed.
/// Bind the ref by registering the room via <see cref="GameBuilder.Register(Room)"/> or
/// <see cref="GameBuilder.Room(string)"/> (matching <see cref="Element.Id"/>).
/// </summary>
public sealed class RoomRef
{
    internal RoomRef(string id, RoomLinkRegistry registry)
    {
        Id = id;
        Registry = registry;
    }

    /// <summary>Stable room id used when the destination room is created.</summary>
    public string Id { get; }

    internal RoomLinkRegistry Registry { get; }

    internal bool TryResolve(out Room room) => Registry.TryGetRoom(Id, out room);

    internal void QueueFrom(
        Room from,
        Direction direction,
        bool reciprocal,
        Func<GameContext, bool>? when,
        string? blocked,
        Func<GameContext, string?>? gate = null,
        Func<GameContext, bool>? onPass = null) =>
        Registry.QueueLink(from, direction, this, reciprocal, when, blocked, gate, onPass);
}
