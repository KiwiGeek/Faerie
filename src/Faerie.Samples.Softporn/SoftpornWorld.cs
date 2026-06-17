using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private readonly GameBuilder _b;
    private readonly Dictionary<string, Room> _rooms = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Thing> _things = new(StringComparer.Ordinal);

    internal Room R(string id) => _rooms[id];
    internal Thing T(string id) => _things[id];

    internal Room Bar => R(SoftpornIds.Bar);
    internal StateKey<int> MoneyKey => _money;
    internal Room HookerBedroom => R(SoftpornIds.HookerBedroom);
    internal Room HookerBalcony => R(SoftpornIds.HookerBalcony);
    internal Room Backroom => R(SoftpornIds.Backroom);
    internal Room HotelHallway => R(SoftpornIds.HotelHallway);
    internal Room DiscoEntrance => R(SoftpornIds.DiscoEntrance);
    internal Room HoneymoonSuite => R(SoftpornIds.HoneymoonSuite);
    internal Room Jacuzzi => R(SoftpornIds.Jacuzzi);
    internal Room Garden => R(SoftpornIds.Garden);
    internal Room PenthousePorch => R(SoftpornIds.PenthousePorch);

    private StateKey<int> _money = null!;
    private StateKey<bool> _ropeInUse = null!;
    private StateKey<bool> _windowBroken = null!;
    private StateKey<bool> _hookerFucked = null!;
    private StateKey<bool> _doorWestOpen = null!;
    private StateKey<bool> _radioListened = null!;
    private StateKey<bool> _wineOrdered = null!;
    private StateKey<bool> _telephoneRinging = null!;
    private StateKey<bool> _telephoneAnswered = null!;
    private StateKey<bool> _holePeeped = null!;
    private StateKey<bool> _girl2Fucked = null!;
    private StateKey<bool> _tiedToBed = null!;
    private StateKey<bool> _drawerOpen = null!;
    private StateKey<bool> _closetOpen = null!;
    private StateKey<bool> _cabinetOpen = null!;
    private StateKey<bool> _dollInflated = null!;
    private StateKey<bool> _stoolClimbed = null!;
    private StateKey<bool> _waterOn = null!;
    private StateKey<bool> _pitcherFull = null!;
    private StateKey<bool> _appleGiven = null!;
    private StateKey<bool> _candyGiven = null!;
    private StateKey<bool> _flowersGiven = null!;
    private StateKey<bool> _ringGiven = null!;
    private StateKey<bool> _marriedToGirl = null!;
    private StateKey<bool> _curtainOpen = null!;
    private StateKey<bool> _called5556969 = null!;
    private StateKey<bool> _called5550439 = null!;
    private StateKey<bool> _called5550987 = null!;
    private StateKey<bool> _rubberWorn = null!;
    private StateKey<int> _tvChannel = null!;
    private StateKey<string> _girlName = null!;
    private StateKey<string> _girlPart = null!;
    private StateKey<string> _girlDo = null!;
    private StateKey<string> _yourPart = null!;
    private StateKey<string> _yourObject = null!;
    private StateKey<string> _rubberColor = null!;
    private StateKey<string> _rubberFlavor = null!;
    private StateKey<string> _rubberLubricated = null!;
    private StateKey<string> _rubberRibbed = null!;
    private StateKey<int> _carryCount = null!;

    internal SoftpornWorld(GameBuilder b) => _b = b;

    internal void BuildAll()
    {
        DefineState();
        DefineRooms();
        DefineThings();
        PlaceThings();
        ConnectMap();
        ConfigureDynamicExits();
        DefineCustomVerbs();
        DefinePuzzles();
        DefineDaemons();
    }

    private void DefineState()
    {
        _money = _b.State("money", 10);
        _ropeInUse = _b.State("rope-in-use", false);
        _windowBroken = _b.State("window-broken", false);
        _hookerFucked = _b.State("hooker-fucked", false);
        _doorWestOpen = _b.State("door-west-open", false);
        _radioListened = _b.State("radio-listened", false);
        _wineOrdered = _b.State("wine-ordered", false);
        _telephoneRinging = _b.State("telephone-ringing", false);
        _telephoneAnswered = _b.State("telephone-answered", false);
        _holePeeped = _b.State("hole-peeped", false);
        _girl2Fucked = _b.State("girl2-fucked", false);
        _tiedToBed = _b.State("tied-to-bed", false);
        _drawerOpen = _b.State("drawer-open", false);
        _closetOpen = _b.State("closet-open", false);
        _cabinetOpen = _b.State("cabinet-open", false);
        _dollInflated = _b.State("doll-inflated", false);
        _stoolClimbed = _b.State("stool-climbed", false);
        _waterOn = _b.State("water-on", false);
        _pitcherFull = _b.State("pitcher-full", false);
        _appleGiven = _b.State("apple-given", false);
        _candyGiven = _b.State("candy-given", false);
        _flowersGiven = _b.State("flowers-given", false);
        _ringGiven = _b.State("ring-given", false);
        _marriedToGirl = _b.State("married-to-girl", false);
        _curtainOpen = _b.State("curtain-open", false);
        _called5556969 = _b.State("called-5556969", false);
        _called5550439 = _b.State("called-5550439", false);
        _called5550987 = _b.State("called-5550987", false);
        _rubberWorn = _b.State("rubber-worn", false);
        _tvChannel = _b.State("tv-channel", 0);
        _girlName = _b.State("girl-name", "");
        _girlPart = _b.State("girl-part", "");
        _girlDo = _b.State("girl-do", "");
        _yourPart = _b.State("your-part", "");
        _yourObject = _b.State("your-object", "");
        _rubberColor = _b.State("rubber-color", "");
        _rubberFlavor = _b.State("rubber-flavor", "");
        _rubberLubricated = _b.State("rubber-lubricated", "non-lubricated");
        _rubberRibbed = _b.State("rubber-ribbed", "non-ribbed");
        _carryCount = _b.State("carry-count", 0);
    }
}
