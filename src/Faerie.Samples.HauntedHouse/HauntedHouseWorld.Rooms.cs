using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.HauntedHouse;

internal sealed partial class HauntedHouseWorld
{
    private void DefineRooms()
    {
        // ---- Garden and woods (map rows 0–2) ----
        DarkCorner = _house.Room("In a dark corner");
        OvergrownGarden = _house.Room("In an overgrown garden");
        Woodpile = _house.Room("By a large woodpile");
        RubbishYard = _house.Room("In a yard by a pile of rubbish");
        Weedpatch = _house.Room("In a weedpatch");
        Forest = _house.Room("In a forest");
        ThickForest = _house.Room("In a thicker part of the forest");
        BlastedTree = _house.Room("By a blasted tree");

        // ---- Kitchen wing (row 1, east) ----
        HouseCorner = _house.Room("By the corner of an old house");
        KitchenEntrance = _house.Room("At the entrance to the kitchen");
        Kitchen = _house.Room("In the kitchen. There is a grimy cooker here.");
        Scullery = _house.Room("In the scullery");
        DustyRoom = _house.Room("In a room thick with dust");
        RearTurret = _house.Room("In the rear turret room");
        Clearing = _house.Room("In a clearing by an old house");
        Footpath = _house.Room("On an old footpath");

        // ---- Hallway and spiral stair (rows 2–3) ----
        SideOfHouse = _house.Room("By the side of an old house");
        BackOfHallway = _house.Room("At the back of the hallway");
        DarkAlcove = _house.Room("In a dark alcove").Dark();
        SmallDarkRoom = _house.Room("In a small dark room");
        SpiralStaircaseBottom = _house.Room("At the bottom of a spiral staircase");
        WidePassage = _house.Room("In a wide passage");
        SlipperySteps = _house.Room("On a set of slippery steps");
        CliffTop = _house.Room("On a cliff top");

        // ---- Main hall and trophy wing (rows 3–4) ----
        CrumblingWall = _house.Room("Near a crumbling wall");
        GloomyPassage = _house.Room("In a gloomy passage");
        ShortCorridor = _house.Room("In a short corridor");
        ImpressiveHallway = _house.Room("In an impressive hallway").Dark();
        LockedDoorHall = _house.Room("In a hall by a thick wooden door. The door is locked.").Dark();
        TrophyRoom = _house.Room("In the trophy room").Dark();
        BarredCellar = _house.Room("In a cellar with a barred window");
        CliffPath1 = _house.Room("On a cliff path");

        // ---- Front rooms and dining (rows 4–5) ----
        CoatCupboard = _house.Room("In a cupboard with a coat hanging on the door");
        FrontHall = _house.Room("In the front hall");
        SittingRoom = _house.Room("In the sitting room");
        SecretRoom = _house.Room("In a secret room");
        MarbleStairs = _house.Room("On some steep marble stairs");
        DiningRoom = _house.Room("In the dining room");
        DeepCellar = _house.Room("In a deep cellar. There is a coffin here. It is closed.");
        CliffPath2 = _house.Room("On a cliff path");

        // ---- Library, study and upper gallery (rows 5–6) ----
        Closet = _house.Room("In a closet");
        FrontLobby = _house.Room("In the front lobby");
        Library = _house.Room("In a library full of evil books");
        Study = _house.Room("In a study. There is a desk here and a weak-looking wall.");
        CobwebbedRoom = _house.Room("In a weird cobwebbed room");
        IceColdChamber = _house.Room("In an ice-cold chamber");
        SpookyRoom = _house.Room("In a very spooky room");
        CliffPathMarsh = _house.Room("On a cliff path. There is a marsh close by.");

        // ---- Porch, tower and gate (rows 6–7) ----
        Verandah = _house.Room("On a rubble-strewn verandah");
        FrontPorch = _house.Room("On the front porch");
        FrontTower = _house.Room("In the front tower");
        SlopingCorridor = _house.Room("In a sloping corridor");
        UpperGallery = _house.Room("In the upper gallery");
        MarshByWall = _house.Room("In a marsh by a wall");
        Marsh = _house.Room("In a marsh");
        SoggyPath = _house.Room("On a soggy path");

        // ---- Marsh approach and clifftop (row 7) ----
        TwistedRailing = _house.Room("By a twisted railing");
        IronGate = _house.Room("On a path through an iron gate");
        OldRailings = _house.Room("By some old railings");
        BeneathFrontTower = _house.Room("Beneath the front tower of an old house");
        CrumblingWallDebris = _house.Room("By some debris from a crumbling wall");
        FallenBrickwork = _house.Room("By some large fallen brickwork");
        RottingStoneArch = _house.Room("By a rotting stone arch");
        CrumblingClifftop = _house.Room("On a crumbling clifftop");

        // ---- exits (second pass: all rooms exist before linking) ----
        DarkCorner.South(HouseCorner).East(OvergrownGarden);
        OvergrownGarden.East(Woodpile).West(DarkCorner);
        Woodpile.East(RubbishYard).West(OvergrownGarden);
        RubbishYard.South(Scullery).East(Weedpatch).West(Woodpile);
        Weedpatch.East(Forest).West(RubbishYard);
        Forest.East(ThickForest).West(Weedpatch);
        ThickForest.South(Clearing).East(BlastedTree).West(Forest);
        BlastedTree.South(Footpath).West(ThickForest);
        HouseCorner.North(DarkCorner).South(SideOfHouse);
        KitchenEntrance.South(BackOfHallway).East(Kitchen);
        Kitchen.East(Scullery).West(KitchenEntrance);
        Scullery.North(RubbishYard).West(Kitchen);
        DustyRoom.South(SpiralStaircaseBottom).East(RearTurret);
        RearTurret.West(DustyRoom);
        Clearing.North(ThickForest).East(Footpath);
        Footpath.North(BlastedTree).South(CliffTop).West(Clearing);
        SideOfHouse.North(HouseCorner).South(CrumblingWall);
        BackOfHallway.North(KitchenEntrance).South(GloomyPassage);
        DarkAlcove.South(ShortCorridor).East(SmallDarkRoom);
        SmallDarkRoom.East(SpiralStaircaseBottom).West(DarkAlcove);
        SpiralStaircaseBottom.North(DustyRoom).West(SmallDarkRoom).Up(DustyRoom, reciprocal: false).Down(SmallDarkRoom, reciprocal: false);
        WidePassage.South(TrophyRoom).East(SlipperySteps);
        SlipperySteps.South(BarredCellar).West(WidePassage).Up(WidePassage, reciprocal: false).Down(BarredCellar, reciprocal: false);
        CliffTop.North(Footpath).South(CliffPath1);
        CrumblingWall.North(SideOfHouse);
        GloomyPassage.North(BackOfHallway).South(FrontHall);
        ShortCorridor.North(DarkAlcove).South(SittingRoom).East(ImpressiveHallway);
        ImpressiveHallway.East(LockedDoorHall).West(ShortCorridor);
        LockedDoorHall.East(TrophyRoom).West(ImpressiveHallway);
        TrophyRoom.North(WidePassage).South(DiningRoom).West(LockedDoorHall);
        BarredCellar.North(SlipperySteps).South(DeepCellar);
        CliffPath1.North(CliffTop).South(CliffPath2);
        CoatCupboard.South(Closet);
        FrontHall.North(GloomyPassage).South(FrontLobby).East(SittingRoom);
        SittingRoom.North(ShortCorridor).South(Library).West(FrontHall);
        SecretRoom.South(Study, reciprocal: false);
        MarbleStairs.North(LockedDoorHall, reciprocal: false).South(CobwebbedRoom).Up(CobwebbedRoom, reciprocal: false).Down(LockedDoorHall, reciprocal: false);
        DiningRoom.North(TrophyRoom);
        DeepCellar.North(BarredCellar);
        CliffPath2.North(CliffPath1).South(CliffPathMarsh);
        Closet.North(CoatCupboard).East(FrontLobby);
        FrontLobby.North(FrontHall).West(Closet);
        Library.North(SittingRoom).East(Study);
        Study.West(Library);
        CobwebbedRoom.North(MarbleStairs).South(UpperGallery).East(IceColdChamber);
        IceColdChamber.East(SpookyRoom).West(CobwebbedRoom);
        SpookyRoom.West(IceColdChamber);
        CliffPathMarsh.North(CliffPath2).South(SoggyPath, reciprocal: false).West(SpookyRoom, reciprocal: false);
        Verandah.South(TwistedRailing);
        FrontPorch.South(IronGate).West(Verandah, reciprocal: false);
        FrontTower.North(Library, reciprocal: false).West(FrontPorch, reciprocal: false);
        SlopingCorridor.North(Study, reciprocal: false).East(UpperGallery);
        UpperGallery.North(CobwebbedRoom).East(MarshByWall).West(SlopingCorridor);
        MarshByWall.East(Marsh).West(UpperGallery);
        Marsh.East(SoggyPath).West(MarshByWall);
        SoggyPath.East(TwistedRailing).West(Marsh);
        TwistedRailing.North(Verandah).East(IronGate).West(SoggyPath);
        IronGate.North(FrontPorch).East(OldRailings).West(TwistedRailing);
        OldRailings.East(BeneathFrontTower, reciprocal: false).West(IronGate);
        BeneathFrontTower.North(SlopingCorridor, reciprocal: false).East(CrumblingWallDebris);
        CrumblingWallDebris.North(UpperGallery, reciprocal: false).East(FallenBrickwork).West(BeneathFrontTower);
        FallenBrickwork.North(MarshByWall, reciprocal: false).East(RottingStoneArch).West(CrumblingWallDebris);
        RottingStoneArch.West(FallenBrickwork);
        CrumblingClifftop.West(RottingStoneArch, reciprocal: false);
    }
}
