using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>
/// Zork I world builder. Simplified puzzle behavior is documented in <see cref="ZorkSimplifications"/>
/// and inline <c>ENGINE-LIMIT</c> comments; see <c>AGENTS.md</c> in this project for the full engine gap analysis.
/// </summary>
internal sealed partial class ZorkWorld
{
    private readonly GameBuilder _b;
    private readonly Dictionary<string, Room> _rooms = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Thing> _things = new(StringComparer.Ordinal);

    internal Room R(string id) => _rooms[id];
    internal Thing T(string id) => _things[id];

    // Convenience accessors used throughout puzzle code
    internal Room WestOfHouse => R(ZorkIds.WestOfHouse);
    internal Room LivingRoom => R(ZorkIds.LivingRoom);
    internal Room Kitchen => R(ZorkIds.Kitchen);
    internal Room Cellar => R(ZorkIds.Cellar);
    internal Room TrollRoom => R(ZorkIds.TrollRoom);
    internal Room TrophyCaseRoom => R(ZorkIds.LivingRoom);
    internal Room TreasureRoom => R(ZorkIds.TreasureRoom);
    internal Room LoudRoom => R(ZorkIds.LoudRoom);
    internal Room CyclopsRoom => R(ZorkIds.CyclopsRoom);
    internal Room Dam => R(ZorkIds.Dam);
    internal Room MaintenanceRoom => R(ZorkIds.MaintenanceRoom);
    internal Room Reservoir => R(ZorkIds.Reservoir);
    internal Room MachineRoom => R(ZorkIds.MachineRoom);
    internal Room StoneBarrow => R(ZorkIds.StoneBarrow);
    internal Room Clearing1 => R(ZorkIds.Clearing1);
    internal Room GratingRoom => R(ZorkIds.GratingRoom);
    internal Room RoundRoom => R(ZorkIds.RoundRoom);
    internal Room LandOfTheDead => R(ZorkIds.LandOfTheDead);
    internal Room EndOfRainbow => R(ZorkIds.EndOfRainbow);
    internal Room OnTheRainbow => R(ZorkIds.OnTheRainbow);
    internal Room SandyCave => R(ZorkIds.SandyCave);
    internal Room BatRoom => R(ZorkIds.BatRoom);
    internal Room GasRoom => R(ZorkIds.GasRoom);
    internal Room EgyptianRoom => R(ZorkIds.EgyptianRoom);
    internal Room AtlantisRoom => R(ZorkIds.AtlantisRoom);
    internal Room TorchRoom => R(ZorkIds.TorchRoom);
    internal Room Gallery => R(ZorkIds.Gallery);
    internal Room UpATree => R(ZorkIds.UpATree);
    internal Room ForestPath => R(ZorkIds.ForestPath);
    internal Room DeadEndMine => R(ZorkIds.DeadEnd);
    internal Room SlideRoom => R(ZorkIds.SlideRoom);
    internal Room EntranceToHades => R(ZorkIds.EntranceToHades);
    internal Room Altar => R(ZorkIds.Altar);
    internal Room Temple => R(ZorkIds.Temple);
    internal Room DomeRoom => R(ZorkIds.DomeRoom);
    internal Room StrangePassage => R(ZorkIds.StrangePassage);
    internal Room EastOfHouse => R(ZorkIds.EastOfHouse);
    internal Room Attic => R(ZorkIds.Attic);
    internal Room DraftyRoom => R(ZorkIds.DraftyRoom);
    internal Room EwPassage => R(ZorkIds.EwPassage);
    internal Room FrigidRiver => R(ZorkIds.FrigidRiver);

    // Puzzle flags
    private StateKey<bool> _rugMoved = null!;
    private StateKey<bool> _trapDoorOpen = null!;
    private StateKey<bool> _windowOpen = null!;
    private StateKey<bool> _trollDefeated = null!;
    private StateKey<bool> _cyclopsFlag = null!;
    private StateKey<bool> _cyclopsGone = null!;
    private StateKey<int> _cyclopsWrath = null!;
    private StateKey<bool> _cyclopsDaemon = null!;
    private StateKey<bool> _thiefDead = null!;
    private StateKey<bool> _thiefHere = null!;
    private StateKey<bool> _thiefEngrossed = null!;
    private StateKey<bool> _lowTide = null!;
    private StateKey<bool> _rainbowSolid = null!;
    private StateKey<bool> _boatInflated = null!;
    private StateKey<bool> _eggBroken = null!;
    private StateKey<bool> _coalProcessed = null!;
    private StateKey<bool> _magicFlag = null!;
    private StateKey<bool> _wonFlag = null!;
    private StateKey<bool> _allTreasuresInCase = null!;
    private StateKey<bool> _hadesOpen = null!;
    private StateKey<bool> _strangePassageOpen = null!;
    private StateKey<bool> _domeRopeTied = null!;
    private StateKey<bool> _chimneyFlag = null!;
    private StateKey<int> _lanternTurns = null!;
    private StateKey<int> _grueTurns = null!;
    private StateKey<int> _placeScoreMask = null!;
    // Death penalty and treasure scatter wired in ZorkWorld.Death.cs.
    private StateKey<int> _deathCount = null!;
    private StateKey<int> _sandDigs = null!;
    private StateKey<int> _thiefLocation = null!;
    private StateKey<int> _swordGlow = null!;   // 0 = none, 1 = faint, 2 = bright
    private StateKey<int> _playerHp = null!;    // player's fighting strength; 0 = dead
    private StateKey<int> _trollHp = null!;
    private StateKey<int> _thiefHp = null!;
    private StateKey<int> _trollKO = null!;      // turns the troll stays unconscious (0 = on his feet)
    private StateKey<int> _thiefKO = null!;
    private StateKey<bool> _loudQuieted = null!;  // the Loud Room has been silenced

    internal ZorkWorld(GameBuilder b) => _b = b;

    internal void BuildAll()
    {
        DefineState();
        DefineRooms();
        DefineThings();
        DefineScoringState();
        PlaceThings();
        ConnectMap();
        ConfigureConditionalExits();
        DefineDeath();
        DefineCustomVerbs();
        DefinePuzzles();
    }

    private void DefineState()
    {
        _rugMoved = _b.State("rug-moved", false);
        _trapDoorOpen = _b.State("trap-open", false);
        _windowOpen = _b.State("window-open", false);
        _trollDefeated = _b.State("troll-defeated", false);
        _cyclopsFlag = _b.State("cyclops-flag", false);
        _cyclopsGone = _b.State("cyclops-gone", false);
        _cyclopsWrath = _b.State("cyclops-wrath", 0);
        _cyclopsDaemon = _b.State("cyclops-daemon", false);
        _thiefDead = _b.State("thief-dead", false);
        _thiefHere = _b.State("thief-here", false);
        _thiefEngrossed = _b.State("thief-engrossed", false);
        _lowTide = _b.State("low-tide", false);
        _rainbowSolid = _b.State("rainbow-solid", false);
        _boatInflated = _b.State("boat-inflated", false);
        _eggBroken = _b.State("egg-broken", false);
        _coalProcessed = _b.State("coal-processed", false);
        _magicFlag = _b.State("magic-flag", false);
        _wonFlag = _b.State("won-flag", false);
        _allTreasuresInCase = _b.State("all-treasures", false);
        _hadesOpen = _b.State("hades-open", false);
        _strangePassageOpen = _b.State("strange-open", false);
        _domeRopeTied = _b.State("dome-rope", false);
        _chimneyFlag = _b.State("chimney-flag", false);
        _lanternTurns = _b.State("lantern-turns", 385);
        _grueTurns = _b.State("grue-turns", 0);
        _placeScoreMask = _b.State("place-score-mask", 0);
        _deathCount = _b.State("deaths", 0);
        _sandDigs = _b.State("sand-digs", 0);
        _thiefLocation = _b.State("thief-loc", 0);
        _swordGlow = _b.State("sword-glow", 0);
        _playerHp = _b.State("player-hp", 5);
        _trollHp = _b.State("troll-hp", 2);
        _thiefHp = _b.State("thief-hp", 4);
        _trollKO = _b.State("troll-ko", 0);
        _thiefKO = _b.State("thief-ko", 0);
        _loudQuieted = _b.State("loud-quieted", false);
    }

    private void DefineRooms()
    {
        foreach (RoomDef def in ZorkMapData.AllRooms)
        {
            Room room = _b.Room(def.Name).Describe(def.Description);
            if (def.Dark) room.Dark();
            _rooms[def.Id] = room;
        }
    }

    private void Link(string from, Direction dir, string to, bool reciprocal = true) =>
        R(from).Connect(dir, R(to), reciprocal);

    private void LinkOneWay(string from, Direction dir, string to) =>
        R(from).Connect(dir, R(to), reciprocal: false);

    private Exit Block(string from, Direction dir, string message)
    {
        Exit exit = R(from).Connect(dir, R(from), reciprocal: false);
        exit.Condition = _ => false;
        exit.BlockedMessage = message;
        return exit;
    }

    private bool InDarkWithoutLight(GameContext ctx) =>
        !new Faerie.Parsing.Scope(ctx.State, ctx).IsCurrentRoomLit;

    internal const int TreasureCount = 19;
}
