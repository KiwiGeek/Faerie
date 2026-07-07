using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Samples.HauntedHouse;

/// <summary>
/// Usborne <em>Haunted House</em> (Jenny Tyler &amp; Les Howarth) on the Faerie fluent engine.
/// Every room, object and puzzle is a named field wired by C# reference — no string tables or arrays.
/// </summary>
internal sealed partial class HauntedHouseWorld
{
    private readonly GameBuilder _house;

    // ---- Rooms: first-class fields, assigned in DefineRooms ----
    internal Room DarkCorner = null!, OvergrownGarden = null!, Woodpile = null!, RubbishYard = null!,
        Weedpatch = null!, Forest = null!, ThickForest = null!, BlastedTree = null!,
        HouseCorner = null!, KitchenEntrance = null!, Kitchen = null!, Scullery = null!,
        DustyRoom = null!, RearTurret = null!, Clearing = null!, Footpath = null!,
        SideOfHouse = null!, BackOfHallway = null!, DarkAlcove = null!, SmallDarkRoom = null!,
        SpiralStaircaseBottom = null!, WidePassage = null!, SlipperySteps = null!, CliffTop = null!,
        CrumblingWall = null!, GloomyPassage = null!, ShortCorridor = null!, ImpressiveHallway = null!,
        LockedDoorHall = null!, TrophyRoom = null!, BarredCellar = null!, CliffPath1 = null!,
        CoatCupboard = null!, FrontHall = null!, SittingRoom = null!, SecretRoom = null!,
        MarbleStairs = null!, DiningRoom = null!, DeepCellar = null!, CliffPath2 = null!,
        Closet = null!, FrontLobby = null!, Library = null!, Study = null!,
        CobwebbedRoom = null!, IceColdChamber = null!, SpookyRoom = null!, CliffPathMarsh = null!,
        Verandah = null!, FrontPorch = null!, FrontTower = null!, SlopingCorridor = null!,
        UpperGallery = null!, MarshByWall = null!, Marsh = null!, SoggyPath = null!,
        TwistedRailing = null!, IronGate = null!, OldRailings = null!, BeneathFrontTower = null!,
        CrumblingWallDebris = null!, FallenBrickwork = null!, RottingStoneArch = null!, CrumblingClifftop = null!;

    // ---- Things: first-class fields, assigned in DefineThings ----
    internal Thing Painting = null!, Ring = null!, MagicSpells = null!, Goblet = null!, OldScroll = null!,
        OldCoins = null!, SmallStatue = null!, Glove = null!, Matches = null!, VacuumCleaner = null!,
        Batteries = null!, Shovel = null!, Axe = null!, Rope = null!, SmallBoat = null!, AerosolSpray = null!,
        Candle = null!, Key = null!;
    internal Thing FrontDoor = null!, Vampires = null!, Ghosts = null!, Drawer = null!, Desk = null!,
        Coat = null!, Rubbish = null!, Coffin = null!, Books = null!, Jellybabies = null!, WeakWall = null!,
        GrimyCooker = null!;

    internal HauntedHouseWorld(GameBuilder house) => _house = house;

    internal void BuildAll()
    {
        DefineState();
        DefineRooms();
        DefineThings();
        DefineVerbs();
        DefineMovementHooks();
    }
}
