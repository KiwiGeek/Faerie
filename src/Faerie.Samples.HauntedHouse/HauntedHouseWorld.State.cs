using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.HauntedHouse;

internal sealed partial class HauntedHouseWorld
{
    // Puzzle flags (Usborne flag% array).
    internal StateKey<bool> CandleLit = null!;
    internal StateKey<bool> CoffinClosed = null!;
    internal StateKey<bool> DrawerClosed = null!;
    internal StateKey<bool> CoatUnsearched = null!;
    internal StateKey<bool> LobbyDoorOpen = null!;
    internal StateKey<bool> FrontDoorLocked = null!;
    internal StateKey<bool> BatsPresent = null!;
    internal StateKey<bool> GhostsBlocking = null!;
    internal StateKey<bool> VacuumOn = null!;
    internal StateKey<bool> TreeClimbReady = null!;
    internal StateKey<bool> TreeClimbing = null!;
    internal StateKey<bool> WeakWallIntact = null!;
    internal StateKey<bool> BarsDugOut = null!;
    internal StateKey<bool> SpellsBarrierDown = null!;
    internal StateKey<int> HintIndex = null!;

    private void DefineState()
    {
        CandleLit = _house.State("candle-lit", false);
        CoffinClosed = _house.State("coffin-closed", true);
        DrawerClosed = _house.State("drawer-closed", true);
        CoatUnsearched = _house.State("coat-unsearched", true);
        LobbyDoorOpen = _house.State("lobby-door-open", true);
        FrontDoorLocked = _house.State("front-door-locked", true);
        BatsPresent = _house.State("bats-present", true);
        GhostsBlocking = _house.State("ghosts-blocking", false);
        VacuumOn = _house.State("vacuum-on", false);
        TreeClimbReady = _house.State("tree-climb-ready", false);
        TreeClimbing = _house.State("tree-climbing", false);
        WeakWallIntact = _house.State("weak-wall-intact", true);
        BarsDugOut = _house.State("bars-dug-out", false);
        SpellsBarrierDown = _house.State("spells-barrier-down", false);
        HintIndex = _house.State("hint-index", 0);
    }

    internal bool HasLight(VerbContext ctx) =>
        ctx.Get(CandleLit) || ctx.ThingsHere(includePresent: true).Any(t => t.Has(Attr.Lit));

    internal IEnumerable<Thing> Treasures =>
    [
        Painting, Ring, MagicSpells, Goblet, OldScroll, OldCoins, SmallStatue, Candlestick,
        Matches, VacuumCleaner, Batteries, Shovel, Axe, Rope, SmallBoat, AerosolSpray, Candle, Key
    ];

    internal int CarriedTreasureCount(VerbContext ctx) =>
        Treasures.Count(t => ctx.Carrying(t));

    internal bool IsTreasure(Thing thing) => Treasures.Contains(thing);

    internal bool IsHere(VerbContext ctx, Thing thing) =>
        ctx.State.IsLocatedIn(thing, ctx.CurrentRoom);
}
