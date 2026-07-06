using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.HauntedHouse;

internal sealed partial class HauntedHouseWorld
{
    private void DefineRooms()
    {
        // ---- Garden and woods (map rows 0–2) ----
        DarkCorner = _house.Room("In a dark corner")
            .Describe("Ivy chokes the garden wall here, and shadows pool thickly even by daylight.");
        OvergrownGarden = _house.Room("In an overgrown garden")
            .Describe("Rose bushes have run wild, their thorns tangled into a hedge on every side but the path.");
        Woodpile = _house.Room("By a large woodpile")
            .Describe("Someone stacked these logs for a fire that was never lit; the wood is silvered with age.");
        RubbishYard = _house.Room("In a yard by a pile of rubbish")
            .Describe("Broken crates and rusted tins are heaped against the wall, leavings of a household long since fled.");
        Weedpatch = _house.Room("In a weedpatch")
            .Describe("Chest-high weeds hiss in the wind, hiding whatever the garden once grew here.");
        Forest = _house.Room("In a forest")
            .Describe("Thin, twisted trees crowd together, their branches laced overhead into a roof of shadow.");
        ThickForest = _house.Room("In a thicker part of the forest")
            .Describe("The trees close ranks here, and what little daylight remains fails almost entirely.");
        BlastedTree = _house.Room("By a blasted tree")
            .Describe("A single great tree stands scorched and hollow, split clean down the middle by some old fire.");

        // ---- Kitchen wing (row 1, east) ----
        HouseCorner = _house.Room("By the corner of an old house")
            .Describe("The house's stonework is cracked and streaked with damp where it meets the earth.");
        KitchenEntrance = _house.Room("At the entrance to the kitchen")
            .Describe("A sagging door hangs from a single hinge, leading into the gloom of the kitchen.");
        Kitchen = _house.Room("In the kitchen. There is a grimy cooker here.")
            .Describe("Cobwebs drape the cooker, and the smell of long-dead cooking still clings to the air.");
        Scullery = _house.Room("In the scullery")
            .Describe("Cracked sinks and mildewed shelves line the walls of this forgotten washroom.");
        DustyRoom = _house.Room("In a room thick with dust")
            .Describe("Dust lies undisturbed in thick drifts, as though no one has set foot here in decades.");
        RearTurret = _house.Room("In the rear turret room")
            .Describe("Narrow slit windows look out over the grounds from this crumbling turret room.");
        Clearing = _house.Room("In a clearing by an old house")
            .Describe("A patch of open ground breaks the trees, close by the old house's silhouette.");
        Footpath = _house.Room("On an old footpath")
            .Describe("A weed-cracked footpath winds between the forest and the cliffs.");

        // ---- Hallway and spiral stair (rows 2–3) ----
        SideOfHouse = _house.Room("By the side of an old house")
            .Describe("Ivy and damp have eaten deep into the mortar along this side of the house.");
        BackOfHallway = _house.Room("At the back of the hallway")
            .Describe("A dim passage runs from here toward the deeper reaches of the house.");
        DarkAlcove = _house.Room("In a dark alcove").Dark()
            .Describe("The darkness here is total, thick enough to feel against your skin.");
        SmallDarkRoom = _house.Room("In a small dark room")
            .Describe("Barely more than a landing, this cramped room is lit only by whatever light you carry.");
        SpiralStaircaseBottom = _house.Room("At the bottom of a spiral staircase")
            .Describe("A tight spiral staircase corkscrews upward into the gloom above.");
        WidePassage = _house.Room("In a wide passage")
            .Describe("A broad, echoing passage stretches away, its ceiling lost in shadow.");
        SlipperySteps = _house.Room("On a set of slippery steps")
            .Describe("Damp and moss make these worn steps treacherous underfoot.");
        CliffTop = _house.Room("On a cliff top")
            .Describe("Wind whips across the cliff top, salt-laced and cold.");

        // ---- Main hall and trophy wing (rows 3–4) ----
        CrumblingWall = _house.Room("Near a crumbling wall")
            .Describe("Age has crumbled the mortar here to little more than gravel.");
        GloomyPassage = _house.Room("In a gloomy passage")
            .Describe("Faded portraits watch from the walls of this dim passage, their painted eyes seeming to follow you.");
        PoolOfLight = _house.Room("In a pool of light")
            .Describe("A shaft of daylight falls here from somewhere above, the one bright spot in an otherwise gloomy passage.");
        ImpressiveHallway = _house.Room("In an impressive hallway").Dark()
            .Describe("Once grand, this hallway's chandeliers now hang dark and thick with dust.");
        LockedDoorHall = _house.Room("In a hall by a thick wooden door. The door is locked.").Dark()
            .Describe("A heavy, iron-bound door bars the way, its lock rusted but unyielding.");
        TrophyRoom = _house.Room("In the trophy room").Dark()
            .Describe("Mounted heads and tarnished trophies line the walls, watching with glass eyes.");
        BarredCellar = _house.Room("In a cellar with a barred window")
            .Describe("Iron bars cross the single window, letting in a thin blade of daylight.");
        CliffPath1 = _house.Room("On a cliff path")
            .Describe("The path clings to the cliff edge, loose stones skittering away underfoot.");

        // ---- Front rooms and dining (rows 4–5) ----
        CoatCupboard = _house.Room("In a cupboard with a coat hanging on the door")
            .Describe("A single moth-eaten coat hangs from a hook on the back of the door.");
        FrontHall = _house.Room("In the front hall")
            .Describe("A grand staircase must once have graced this hall; only splinters remain.");
        SittingRoom = _house.Room("In the sitting room")
            .Describe("Rotted armchairs slump around a cold, ash-choked fireplace.");
        SecretRoom = _house.Room("In a secret room")
            .Describe("A hidden chamber, reachable only through the broken wall behind you.");
        MarbleStairs = _house.Room("On some steep marble stairs")
            .Describe("Once-polished marble steps spiral steeply upward, cold underfoot.");
        DiningRoom = _house.Room("In the dining room")
            .Describe("A long table, thick with dust, is still set for a dinner no one attended.");
        DeepCellar = _house.Room("In a deep cellar. There is a coffin here. It is closed.")
            .Describe("The air is cold and still down here, heavy with the smell of old earth.");
        CliffPath2 = _house.Room("On a cliff path")
            .Describe("The cliff path narrows further, marsh grass showing on the ground below.");

        // ---- Library, study and upper gallery (rows 5–6) ----
        Closet = _house.Room("In a closet")
            .Describe("A cramped closet, empty but for cobwebs and the smell of mothballs.");
        FrontLobby = _house.Room("In the front lobby")
            .Describe("Faded wallpaper peels from the walls of this once-elegant lobby.");
        Library = _house.Room("In a library full of evil books")
            .Describe("Shelf upon shelf of leather-bound books rise to a ceiling lost in shadow.");
        Study = _house.Room("In a study. There is a desk here and a weak-looking wall.")
            .Describe("A heavy desk dominates the room, and one wall sounds oddly hollow when struck.");
        CobwebbedRoom = _house.Room("In a weird cobwebbed room")
            .Describe("Thick cobwebs sway from every corner, brushing at your face as you pass.");
        IceColdChamber = _house.Room("In an ice-cold chamber")
            .Describe("Your breath mists in air that is unnaturally, bone-achingly cold.");
        SpookyRoom = _house.Room("In a very spooky room")
            .Describe("An oppressive stillness fills this room, as though something is watching from the walls.");
        CliffPathMarsh = _house.Room("On a cliff path. There is a marsh close by.")
            .Describe("The cliff path drops away toward a stretch of marshland below.");

        // ---- Porch, tower and gate (rows 6–7) ----
        Verandah = _house.Room("On a rubble-strewn verandah")
            .Describe("Rubble from a collapsed roof litters this once-elegant verandah.");
        FrontPorch = _house.Room("On the front porch")
            .Describe("Weathered boards creak underfoot on the sagging front porch.");
        FrontTower = _house.Room("In the front tower")
            .Describe("A narrow stair winds up into the front tower, open to the sky in places.");
        SlopingCorridor = _house.Room("In a sloping corridor")
            .Describe("The floor here tilts alarmingly, as though the house itself is subsiding.");
        UpperGallery = _house.Room("In the upper gallery")
            .Describe("Portraits with sunken, staring eyes line this long upper gallery.");
        MarshByWall = _house.Room("In a marsh by a wall")
            .Describe("Reeds and stagnant water lap at a crumbling boundary wall.");
        Marsh = _house.Room("In a marsh")
            .Describe("Black, sucking mud stretches in every direction, dotted with tussocks of reed.");
        SoggyPath = _house.Room("On a soggy path")
            .Describe("The ground squelches underfoot on this waterlogged path.");

        // ---- Marsh approach and clifftop (row 7) ----
        TwistedRailing = _house.Room("By a twisted railing")
            .Describe("A section of ornamental railing has been wrenched and twisted out of shape.");
        IronGate = _house.Room("On a path through an iron gate")
            .Describe("A tall iron gate, spotted with rust, marks the boundary of the grounds.");
        OldRailings = _house.Room("By some old railings")
            .Describe("Rusted railings lean at odd angles along the crumbling boundary.");
        BeneathFrontTower = _house.Room("Beneath the front tower of an old house")
            .Describe("Fallen masonry from the tower above litters the ground here.");
        CrumblingWallDebris = _house.Room("By some debris from a crumbling wall")
            .Describe("Broken stone and mortar are scattered where the old wall gave way.");
        FallenBrickwork = _house.Room("By some large fallen brickwork")
            .Describe("A great section of brickwork has come down, blocking half the path.");
        RottingStoneArch = _house.Room("By a rotting stone arch")
            .Describe("A stone archway stands half-collapsed, its keystone long since given way.");
        CrumblingClifftop = _house.Room("On a crumbling clifftop")
            .Describe("The cliff edge here looks ready to give way at the next strong wind.");

        // ---- exits (second pass: all rooms exist before linking) ----
        // This graph is transcribed directly from the original game's authoritative room/route
        // table (cross-checked against a faithful C reimplementation of the BASIC listing and
        // against the book's own master-plan map) and validated by replaying the full published
        // walkthrough move-by-move against it. Each call below sets up a reciprocal exit by
        // default; `reciprocal: false` marks the one-way passages the original game itself has
        // (e.g. the marsh perimeter, and the door that slams shut behind you).
        DarkCorner.South(HouseCorner).East(OvergrownGarden);
        OvergrownGarden.East(Woodpile);
        Woodpile.East(RubbishYard);
        RubbishYard.South(Scullery).East(Weedpatch);
        Weedpatch.East(Forest);
        Forest.East(ThickForest);
        ThickForest.South(Clearing).East(BlastedTree);
        BlastedTree.South(Footpath);
        HouseCorner.South(SideOfHouse);
        KitchenEntrance.South(BackOfHallway).East(Kitchen);
        Kitchen.East(Scullery);
        DustyRoom.East(RearTurret);
        Clearing.East(Footpath);
        Footpath.South(CliffTop);
        SideOfHouse.South(CrumblingWall);
        BackOfHallway.South(GloomyPassage);
        DarkAlcove.South(PoolOfLight).East(SmallDarkRoom);
        WidePassage.South(TrophyRoom);
        CliffTop.South(CliffPath1);
        GloomyPassage.South(FrontHall);
        PoolOfLight.South(SittingRoom).East(ImpressiveHallway);
        ImpressiveHallway.East(LockedDoorHall);
        LockedDoorHall.East(TrophyRoom);
        TrophyRoom.South(DiningRoom);
        BarredCellar.South(DeepCellar);
        CliffPath1.South(CliffPath2);
        CoatCupboard.South(Closet);
        FrontHall.South(FrontLobby).East(SittingRoom);
        SittingRoom.South(Library);
        SecretRoom.South(Study, reciprocal: false);
        CliffPath2.South(CliffPathMarsh);
        Closet.East(FrontLobby);
        Library.East(Study);
        CobwebbedRoom.South(UpperGallery).East(IceColdChamber);
        IceColdChamber.East(SpookyRoom);
        CliffPathMarsh.South(SoggyPath);
        Verandah.South(TwistedRailing).East(FrontPorch);
        FrontPorch.North(FrontLobby, reciprocal: false).South(IronGate);
        FrontTower.East(SlopingCorridor);
        SlopingCorridor.East(UpperGallery);
        MarshByWall.South(FallenBrickwork);
        Marsh.South(RottingStoneArch).West(MarshByWall, reciprocal: false);
        SoggyPath.West(Marsh, reciprocal: false);
        TwistedRailing.East(IronGate);
        IronGate.East(OldRailings);
        OldRailings.East(BeneathFrontTower);
        BeneathFrontTower.East(CrumblingWallDebris);
        CrumblingWallDebris.East(FallenBrickwork);
        FallenBrickwork.East(RottingStoneArch);
        RottingStoneArch.East(CrumblingClifftop);

        // The three staircases show only Up/Down exits on their own side (replacing the
        // original's redundant compass wording used to fake vertical movement), but a plain
        // compass direction is used when approaching them from an adjacent room - approaching
        // the foot or head of a staircase is ordinary navigation, not the "faked" mechanic.
        SmallDarkRoom.East(SpiralStaircaseBottom, reciprocal: false);
        SpiralStaircaseBottom.Down(SmallDarkRoom, reciprocal: false);
        DustyRoom.South(SpiralStaircaseBottom, reciprocal: false);
        SpiralStaircaseBottom.Up(DustyRoom, reciprocal: false);

        WidePassage.East(SlipperySteps, reciprocal: false);
        SlipperySteps.Up(WidePassage, reciprocal: false);
        BarredCellar.North(SlipperySteps, reciprocal: false);
        SlipperySteps.Down(BarredCellar, reciprocal: false);

        CobwebbedRoom.North(MarbleStairs, reciprocal: false);
        MarbleStairs.Down(CobwebbedRoom, reciprocal: false);
        MarbleStairs.Up(LockedDoorHall, reciprocal: false);
        // (LockedDoorHall's matching South exit to MarbleStairs is added dynamically once the
        // front door is unlocked - see UnlockHandler.)
    }
}
