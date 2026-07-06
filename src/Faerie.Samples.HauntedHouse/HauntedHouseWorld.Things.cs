using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.HauntedHouse;

internal sealed partial class HauntedHouseWorld
{
    private void DefineThings()
    {
        // ---- treasures (objects 1–18 in the original) ----
        Painting = _house.Item("painting").Called("painting", "pai").StartsIn(SpookyRoom);
        Ring = _house.Item("ring").Called("ring", "rin").StartsIn(DeepCellar).Concealed();
        MagicSpells = _house.Item("magic spells").Plural().Called("spells", "mag", "spe").StartsIn(SecretRoom);
        Goblet = _house.Item("goblet").Called("goblet", "gob").StartsIn(FrontTower);
        OldScroll = _house.Item("old scroll").Called("scroll", "scr").StartsIn(RearTurret);
        OldCoins = _house.Item("old coins").Plural().Called("coins", "coi").StartsIn(DarkAlcove);
        SmallStatue = _house.Item("small statue").Called("statue", "sta").StartsIn(LockedDoorHall);
        Candlestick = _house.Item("candlestick").Called("candlestick", "candle stick", "can").StartsIn(Library);
        Matches = _house.Item("box of matches").Called("matches", "matchbox", "mat").StartsIn(Kitchen);
        VacuumCleaner = _house.Item("vacuum cleaner").Called("vacuum", "vac").StartsIn(GloomyPassage);
        Batteries = _house.Item("batteries").Plural().Called("batteries", "bat").StartsIn(PoolOfLight);
        Shovel = _house.Item("shovel").Called("shovel", "sho").StartsIn(Weedpatch);
        Axe = _house.Item("axe").Called("axe").StartsIn(Woodpile);
        Rope = _house.Item("rope").Called("rope", "rop").StartsIn(BlastedTree);
        SmallBoat = _house.Item("small boat").Called("boat", "boa").StartsIn(CliffPathMarsh);
        AerosolSpray = _house.Item("aerosol spray").Called("spray", "aerosol", "aer").StartsIn(CrumblingWallDebris);
        Candle = _house.Item("candle").Called("candle", "can").StartsIn(Study).Concealed();
        Key = _house.Item("key").Called("key").StartsIn(CoatCupboard).Concealed();

        // ---- scenery and creatures (objects 25–36 in the original) ----
        FrontDoor = _house.Scenery("door").Called("door", "doo").StartsIn(LockedDoorHall);
        Bats = _house.Creature("bats").Called("bats").StartsIn(RearTurret).Unlisted();
        Ghosts = _house.Creature("ghosts").Called("ghosts", "gho").StartsIn(UpperGallery).Unlisted();
        Drawer = _house.Scenery("drawer").Called("drawer", "dra").Openable().StartsIn(Study);
        Desk = _house.Scenery("desk").Called("desk", "des").StartsIn(Study);
        Coat = _house.Scenery("coat").Called("coat", "coa").StartsIn(CoatCupboard);
        Rubbish = _house.Scenery("rubbish").Called("rubbish", "rub").StartsIn(RubbishYard);
        Coffin = _house.Scenery("coffin").Called("coffin", "cof").Openable().StartsIn(DeepCellar);
        Books = _house.Scenery("books").Called("books", "boo").Plural().StartsIn(Library);
        Xzanfar = _house.Keyword("xzanfar").Called("xzanfar");
        WeakWall = _house.Scenery("wall").Called("wall", "wal").StartsIn(Study);
        GrimyCooker = _house.Scenery("grimy cooker").Called("cooker").StartsIn(Kitchen);
    }
}
