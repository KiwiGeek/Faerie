using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Building;

internal sealed class RoomLinkRegistry
{
    private readonly Dictionary<string, Room> _rooms = new(StringComparer.Ordinal);
    private readonly List<PendingLink> _pending = [];

    private readonly record struct PendingLink(
        Room From,
        Direction Direction,
        string ToId,
        bool Reciprocal,
        Func<GameContext, bool>? When,
        string? Blocked,
        Func<GameContext, string?>? Gate,
        Func<GameContext, bool>? OnPass);

    public RoomRef Ref(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        return new RoomRef(id, this);
    }

    public void Bind(Room room)
    {
        if (_rooms.TryGetValue(room.Id, out Room? existing) && !ReferenceEquals(existing, room))
            throw new InvalidOperationException($"Duplicate room id '{room.Id}'.");

        _rooms[room.Id] = room;

        for (int i = _pending.Count - 1; i >= 0; i--)
        {
            PendingLink pending = _pending[i];
            if (!string.Equals(pending.ToId, room.Id, StringComparison.Ordinal)) continue;

            _pending.RemoveAt(i);
            ApplyLink(pending.From, pending.Direction, room, pending.Reciprocal,
                pending.When, pending.Blocked, pending.Gate, pending.OnPass);
        }
    }

    internal void QueueLink(
        Room from,
        Direction direction,
        RoomRef destination,
        bool reciprocal,
        Func<GameContext, bool>? when,
        string? blocked,
        Func<GameContext, string?>? gate,
        Func<GameContext, bool>? onPass)
    {
        if (destination.TryResolve(out Room dest))
            ApplyLink(from, direction, dest, reciprocal, when, blocked, gate, onPass);
        else
            _pending.Add(new PendingLink(from, direction, destination.Id, reciprocal, when, blocked, gate, onPass));
    }

    public void EnsureResolved()
    {
        if (_pending.Count == 0) return;

        string details = string.Join(", ",
            _pending.Select(p => $"{p.From.Id} {p.Direction} -> '{p.ToId}'"));
        throw new InvalidOperationException($"Unresolved room link(s): {details}");
    }

    public bool TryGetRoom(string id, out Room room) => _rooms.TryGetValue(id, out room!);

    private static void ApplyLink(
        Room from,
        Direction direction,
        Room to,
        bool reciprocal,
        Func<GameContext, bool>? when,
        string? blocked,
        Func<GameContext, string?>? gate,
        Func<GameContext, bool>? onPass)
    {
        Exit exit = from.SetExit(direction, to);
        ExitFluent.ApplyGate(exit, when, blocked, gate, onPass);
        ExitFluent.ApplyReciprocalExit(from, direction, to, reciprocal, when, blocked, gate, onPass);
    }
}
