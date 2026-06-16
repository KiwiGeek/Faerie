using System.Text.Json;
using System.Text.Json.Serialization;
using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>
/// Serialises and restores a <see cref="GameState"/> to and from JSON. Captures the full mutable
/// state: thing locations, mutable attributes (open/locked/lit/worn/...), turn count, score, the
/// current room, global puzzle flags, one-shot daemon firing, and the win/lose outcome.
/// </summary>
public static class SaveSystem
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Capture(GameState state, IReadOnlyList<TurnDaemon> daemons)
    {
        Snapshot snap = new()
        {
            TurnCount = state.TurnCount,
            Score = state.Score,
            CurrentRoomId = state.CurrentRoom.Id,
            IsOver = state.IsOver,
            PlayerWon = state.PlayerWon,
            LastReferencedThingId = state.LastReferencedThing?.Id
        };

        foreach (Thing thing in state.World.Things)
        {
            snap.Locations[thing.Id] = PlacementDto.From(state.PlacementOf(thing));
            snap.ThingAttributes[thing.Id] = (long)thing.Attributes;
        }

        foreach (Room room in state.World.Rooms)
            snap.RoomAttributes[room.Id] = (long)room.Attributes;

        foreach ((string name, object keyObj) in state.GlobalKeysByName)
        {
            if (state.RawGlobals.TryGetValue(keyObj, out object? value) && value is not null)
                snap.Globals[name] = GlobalValue.From(value);
        }

        for (int i = 0; i < daemons.Count; i++)
            snap.FiredDaemons.Add(daemons[i].Fired);

        return JsonSerializer.Serialize(snap, Options);
    }

    public static void Restore(string json, GameState state, IReadOnlyList<TurnDaemon> daemons)
    {
        Snapshot snap = JsonSerializer.Deserialize<Snapshot>(json, Options)
            ?? throw new InvalidDataException("The save file could not be read.");

        state.TurnCount = snap.TurnCount;
        state.Score = snap.Score;
        state.RestoreOutcome(snap.IsOver, snap.PlayerWon);

        if (state.World.FindRoom(snap.CurrentRoomId) is { } room)
            state.CurrentRoom = room;

        state.LastReferencedThing = snap.LastReferencedThingId is { } id ? state.World.FindThing(id) : null;

        foreach ((string thingId, PlacementDto dto) in snap.Locations)
            if (state.World.FindThing(thingId) is { } thing)
                state.RestoreLocation(thing, dto.ToPlacement(state.World));

        foreach ((string thingId, long attrs) in snap.ThingAttributes)
            if (state.World.FindThing(thingId) is { } thing)
                thing.Attributes = (Attr)attrs;

        foreach ((string roomId, long attrs) in snap.RoomAttributes)
            if (state.World.FindRoom(roomId) is { } r)
                r.Attributes = (Attr)attrs;

        foreach ((string name, GlobalValue gv) in snap.Globals)
            if (gv.ToObject() is { } value)
                state.RestoreGlobal(name, value);

        for (int i = 0; i < daemons.Count && i < snap.FiredDaemons.Count; i++)
            daemons[i].Fired = snap.FiredDaemons[i];
    }

    // ---- DTOs -----------------------------------------------------------------------------

    private sealed class Snapshot
    {
        public int Version { get; set; } = 1;
        public int TurnCount { get; set; }
        public int Score { get; set; }
        public string CurrentRoomId { get; set; } = "";
        public bool IsOver { get; set; }
        public bool PlayerWon { get; set; }
        public string? LastReferencedThingId { get; set; }
        public Dictionary<string, PlacementDto> Locations { get; set; } = [];
        public Dictionary<string, long> ThingAttributes { get; set; } = [];
        public Dictionary<string, long> RoomAttributes { get; set; } = [];
        public Dictionary<string, GlobalValue> Globals { get; set; } = [];
        public List<bool> FiredDaemons { get; set; } = [];
    }

    private sealed class PlacementDto
    {
        public string Anchor { get; set; } = nameof(Runtime.Anchor.Offstage);
        public string? RoomId { get; set; }
        public string? ContainerId { get; set; }

        public static PlacementDto From(Placement p) => new()
        {
            Anchor = p.Anchor.ToString(),
            RoomId = p.Room?.Id,
            ContainerId = p.Container?.Id
        };

        public Placement ToPlacement(World world)
        {
            Anchor anchor = Enum.Parse<Anchor>(this.Anchor);
            return anchor switch
            {
                Runtime.Anchor.Room when RoomId is { } rid && world.FindRoom(rid) is { } r => Placement.InRoom(r),
                Runtime.Anchor.Inside when ContainerId is { } cid && world.FindThing(cid) is { } c => Placement.Inside(c),
                Runtime.Anchor.On when ContainerId is { } cid && world.FindThing(cid) is { } c => Placement.On(c),
                Runtime.Anchor.Carried => Placement.Carried,
                Runtime.Anchor.Worn => Placement.Worn,
                _ => Placement.Offstage
            };
        }
    }

    private sealed class GlobalValue
    {
        public string Type { get; set; } = "string";
        public string Value { get; set; } = "";

        public static GlobalValue From(object value) => value switch
        {
            bool b => new GlobalValue { Type = "bool", Value = b ? "true" : "false" },
            int i => new GlobalValue { Type = "int", Value = i.ToString() },
            long l => new GlobalValue { Type = "long", Value = l.ToString() },
            double d => new GlobalValue { Type = "double", Value = d.ToString("R") },
            _ => new GlobalValue { Type = "string", Value = value.ToString() ?? "" }
        };

        public object? ToObject() => Type switch
        {
            "bool" => Value == "true",
            "int" => int.TryParse(Value, out int i) ? i : 0,
            "long" => long.TryParse(Value, out long l) ? l : 0L,
            "double" => double.TryParse(Value, out double d) ? d : 0d,
            _ => Value
        };
    }
}
