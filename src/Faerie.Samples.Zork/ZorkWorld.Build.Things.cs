using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    // Things (registered by id)
    internal Thing Mailbox = null!;
    internal Thing Leaflet = null!;
    internal Thing KitchenWindow = null!;
    internal Thing Rug = null!;
    internal Thing TrapDoor = null!;
    internal Thing TrophyCase = null!;
    internal Thing Lantern = null!;
    internal Thing Sword = null!;
    internal Thing Sack = null!;
    internal Thing Lunch = null!;
    internal Thing Bottle = null!;
    internal Thing Garlic = null!;
    internal Thing Rope = null!;
    internal Thing DomeRailing = null!;
    internal Thing Leaves = null!;
    internal Thing Grating = null!;
    internal Thing Mirror1 = null!;
    internal Thing Mirror2 = null!;
    internal Thing SkeletonKey = null!;
    internal Thing Nest = null!;
    internal Thing Painting = null!;
    internal Thing JeweledEgg = null!;
    internal Thing GoldenCanary = null!;
    internal Thing BrassBauble = null!;
    internal Thing BagOfCoins = null!;
    internal Thing PlatinumBar = null!;
    internal Thing TrunkOfJewels = null!;
    internal Thing CrystalTrident = null!;
    internal Thing IvoryTorch = null!;
    internal Thing GoldCoffin = null!;
    internal Thing Sceptre = null!;
    internal Thing CrystalSkull = null!;
    internal Thing LargeEmerald = null!;
    internal Thing Scarab = null!;
    internal Thing PotOfGold = null!;
    internal Thing JadeFigurine = null!;
    internal Thing SapphireBracelet = null!;
    internal Thing HugeDiamond = null!;
    internal Thing SilverChalice = null!;
    internal Thing Coal = null!;
    internal Thing Wrench = null!;
    internal Thing Screwdriver = null!;
    internal Thing Bolt = null!;
    internal Thing Pump = null!;
    internal Thing PileOfPlastic = null!;
    internal Thing Boat = null!;
    internal Thing Machine = null!;
    internal Thing BlackBook = null!;
    internal Thing BrassBell = null!;
    internal Thing PairOfCandles = null!;
    internal Thing Matchbook = null!;
    internal Thing Shovel = null!;
    internal Thing RedBuoy = null!;
    internal Thing Skeleton = null!;
    internal Thing NastyKnife = null!;
    internal Thing RustyKnife = null!;
    internal Thing Troll = null!;
    internal Thing Cyclops = null!;
    internal Thing Thief = null!;
    internal Thing BloodyAxe = null!;
    internal Thing Sand = null!;
    internal Thing Basket = null!;

    private Thing Reg(string id, Thing thing)
    {
        _things[id] = thing;
        return thing;
    }

    private void DefineThings()
    {
        SkeletonKey = Reg("skeleton_key", _b.Item("skeleton key").Called("key").Adjectives("skeleton")
            .Describe("A skeleton key."));

        Mailbox = Reg("mailbox", _b.Scenery("small mailbox").Called("mailbox").Adjectives("small", "white")
            .Describe("The small mailbox is closed.").Container(open: false));

        Leaflet = Reg("leaflet", _b.Item("leaflet").Adjectives("small", "paper").Called("mail", "booklet")
            .Describe("A small leaflet.")
            .Readable("WELCOME TO ZORK!\n\nZORK is a game of adventure, danger, and low cunning. In it you " +
                      "will explore some of the most amazing territory ever seen by mortals. No computer should " +
                      "be without one!"));

        KitchenWindow = Reg("kitchen_window", _b.Scenery("kitchen window").Called("window").Adjectives("small")
            .Describe("The window is slightly ajar, but not enough to allow entry.").Openable(open: false));

        Leaves = Reg("leaves", _b.Scenery("pile of leaves").Called("leaves", "pile").Adjectives("leaf")
            .Describe("There is a large pile of leaves here."));

        Grating = Reg("grating", _b.Scenery("grating").Called("grate").Adjectives("rusty", "metal")
            .Describe("The grating is closed.").LockedWith(SkeletonKey).Openable(open: false));

        Mirror1 = Reg("mirror_1", _b.Scenery("enormous mirror").Called("mirror", "reflection").Adjectives("enormous"));
        Mirror2 = Reg("mirror_2", _b.Scenery("enormous mirror").Called("mirror", "reflection").Adjectives("enormous"));

        Rug = Reg("rug", _b.Scenery("oriental rug").Called("rug", "carpet").Adjectives("large", "oriental")
            .Describe("The rug is too heavy to lift, but you might be able to move it."));

        TrapDoor = Reg("trap_door", _b.Scenery("trap door").Called("door", "trapdoor").Adjectives("trap")
            .Describe("The door is closed.").Openable(open: false).Concealed());

        TrophyCase = Reg("trophy_case", _b.Scenery("trophy case").Called("case").Adjectives("trophy")
            .Describe("The trophy case is empty.").Container(open: false));

        Lantern = Reg("lantern", _b.Item("brass lantern").Called("lantern", "lamp").Adjectives("brass")
            .Describe("A battery-powered brass lantern.").LightSource(lit: false));

        // Proximity glow is driven by a per-turn daemon (see ZorkWorld.DefineSwordGlow).
        Sword = Reg("sword", _b.Item("elvish sword").Called("sword", "blade").Adjectives("elvish", "glowing")
            .Describe("A sword of elvish workmanship."));

        Sack = Reg("sack", _b.Item("brown sack").Called("sack", "bag").Adjectives("brown")
            .Describe("The brown sack is closed.").Container(open: false));

        Lunch = Reg("lunch", _b.Item("lunch").Called("sandwich", "food").Adjectives("delicious", "hot")
            .Describe("A hot pepper sandwich. It smells good.").Edible());

        Bottle = Reg("bottle", _b.Item("glass bottle").Called("bottle").Adjectives("glass")
            .Describe("A glass bottle.\nThe bottle of water is closed.").Container(open: false).Drinkable());

        Garlic = Reg("garlic", _b.Item("clove of garlic").Called("garlic", "clove").Adjectives("pungent")
            .Describe("A clove of garlic."));

        Rope = Reg("rope", _b.Item("rope").Adjectives("large", "coiled").Describe("A large coil of rope."));

        DomeRailing = Reg("dome_railing", _b.Scenery("railing").Called("railing", "ceiling").Adjectives("dome")
            .Describe("A railing around the edge of the dome overlooks a stomach-churning drop."));

        PileOfPlastic = Reg("plastic", _b.Item("pile of plastic").Called("plastic", "pile").Adjectives("deflated")
            .Describe("A small pile of plastic. It looks like it could be inflated into something useful."));

        Pump = Reg("pump", _b.Item("hand-held air pump").Called("pump", "air pump").Adjectives("hand-held")
            .Describe("A hand-held air pump suitable for inflating things."));

        Boat = Reg("boat", _b.Item("magic boat").Called("boat").Adjectives("magic", "white", "plastic")
            .Describe("The boat is deflated."));

        Wrench = Reg("wrench", _b.Item("wrench").Adjectives("adjustable")
            .Describe("An adjustable wrench. It would be useful for tightening bolts."));

        Screwdriver = Reg("screwdriver", _b.Item("screwdriver").Adjectives("flathead")
            .Describe("A flathead screwdriver."));

        Bolt = Reg("bolt", _b.Scenery("metal bolt").Called("bolt").Adjectives("large", "metal")
            .Describe("A large metal bolt is here. It is rusted and stuck."));

        Machine = Reg("machine", _b.Scenery("machine").Adjectives("large")
            .Describe("There is a large machine here with a small slot on the front."));

        BlackBook = Reg("black_book", _b.Item("black book").Called("book").Adjectives("large", "black")
            .Describe("A large black book. It is written in an unknown tongue.")
            .Readable("The book is written in an unknown tongue. You cannot read it."));

        BrassBell = Reg("bell", _b.Item("brass bell").Called("bell").Adjectives("brass")
            .Describe("A brass bell."));

        PairOfCandles = Reg("candles", _b.Item("pair of candles").Called("candles").Adjectives("pair")
            .Describe("A pair of candles.").OpenFlame(lit: false));

        Matchbook = Reg("matchbook", _b.Item("matchbook").Called("matches", "match").Adjectives("small")
            .Describe("A matchbook with a few matches remaining."));

        Shovel = Reg("shovel", _b.Item("shovel").Adjectives("rusty")
            .Describe("A rusty shovel."));

        Nest = Reg("nest", _b.Scenery("bird's nest").Called("nest").Adjectives("small")
            .Describe("A small bird's nest.").Supporter());

        JeweledEgg = Reg("egg", _b.Item("jewel-encrusted egg").Called("egg", "treasure").Adjectives("jeweled", "encrusted")
            .Describe("A large egg encrusted with precious jewels, extremely fragile.").Container(open: false));

        GoldenCanary = Reg("canary", _b.Item("golden clockwork canary").Called("canary", "bird").Adjectives("golden")
            .Describe("A golden clockwork canary with ruby eyes. It appears to have wound down."));

        Painting = Reg("painting", _b.Item("painting").Adjectives("priceless", "oil")
            .Describe("A painting by a forgotten master. It depicts a landscape of remarkable beauty."));

        BrassBauble = Reg("bauble", _b.Item("brass bauble").Called("bauble").Adjectives("brass")
            .Describe("A brass bauble. It is worthless."));

        BagOfCoins = Reg("coins", _b.Item("leather bag of coins").Called("bag", "coins").Adjectives("leather", "old")
            .Describe("An old leather bag, bulging with coins.").Container(open: true));

        PlatinumBar = Reg("platinum_bar", _b.Item("platinum bar").Called("bar").Adjectives("platinum", "large")
            .Describe("A large platinum bar. It is worth a fortune.").Concealed());

        TrunkOfJewels = Reg("trunk", _b.Item("trunk of jewels").Called("trunk", "jewels", "chest")
            .Adjectives("old").Describe("An old trunk, bulging with assorted jewels.").Concealed());

        CrystalTrident = Reg("trident", _b.Item("crystal trident").Called("trident").Adjectives("crystal")
            .Describe("A crystal trident. It catches the light in a thousand colors."));

        IvoryTorch = Reg("ivory_torch", _b.Item("ivory torch").Called("torch").Adjectives("ivory")
            .Describe("An ivory torch. It is worth a fortune in ivory alone."));

        GoldCoffin = Reg("coffin", _b.Item("gold coffin").Called("coffin").Adjectives("gold", "solid")
            .Describe("A solid gold coffin. It is worth a fortune in gold alone.").Container(open: false));

        Sceptre = Reg("sceptre", _b.Item("sceptre").Adjectives("ornate")
            .Describe("An ornate sceptre, encrusted with jewels."));

        CrystalSkull = Reg("skull", _b.Item("crystal skull").Called("skull").Adjectives("crystal")
            .Describe("A crystal skull. It is worth a fortune."));

        RedBuoy = Reg("buoy", _b.Item("red buoy").Called("buoy").Adjectives("red")
            .Describe("There is a red buoy here (probably a warning).").Container(open: false));

        LargeEmerald = Reg("emerald", _b.Item("large emerald").Called("emerald").Adjectives("large", "green")
            .Describe("A large emerald. It is worth a fortune."));

        Scarab = Reg("scarab", _b.Item("beautiful jeweled scarab").Called("scarab", "beetle")
            .Adjectives("jeweled", "beautiful").Describe("A beautiful jeweled scarab.").Concealed());

        Sand = Reg("sand", _b.Scenery("sand").Adjectives("ground").Describe("Sand covers the floor.").Fixed());
        Basket = Reg("basket", _b.Scenery("basket").Called("basket")
            .Describe("From the chain is suspended a basket.").Container(open: true));

        PotOfGold = Reg("pot", _b.Item("pot of gold").Called("pot").Adjectives("gold")
            .Describe("A pot of gold. What more need be said?").Concealed());

        JadeFigurine = Reg("figurine", _b.Item("jade figurine").Called("figurine").Adjectives("jade", "exquisitely")
            .Describe("An exquisitely carved jade figurine."));

        SapphireBracelet = Reg("bracelet", _b.Item("sapphire-encrusted bracelet").Called("bracelet")
            .Adjectives("sapphire", "jeweled").Describe("A bracelet encrusted with sapphires."));

        HugeDiamond = Reg("diamond", _b.Item("huge diamond").Called("diamond").Adjectives("huge", "perfect")
            .Describe("A perfectly cut diamond. It is worth a fortune.").Concealed());

        SilverChalice = Reg("chalice", _b.Item("silver chalice").Called("chalice").Adjectives("silver")
            .Describe("A silver chalice. It is worth a fortune in silver alone."));

        Coal = Reg("coal", _b.Item("small lump of coal").Called("coal", "lump").Adjectives("small", "black")
            .Describe("A small lump of coal."));

        Skeleton = Reg("skeleton", _b.Scenery("skeleton").Called("bones", "remains")
            .Describe("A skeleton, probably the remains of a luckless adventurer, lies here."));

        NastyKnife = Reg("nasty_knife", _b.Item("nasty knife").Called("knife").Adjectives("nasty", "sharp")
            .Describe("A nasty-looking knife."));

        RustyKnife = Reg("rusty_knife", _b.Item("rusty knife").Called("knife").Adjectives("rusty", "cursed")
            .Describe("A rusty knife."));

        Troll = Reg("troll", _b.Creature("troll").Called("creature", "monster").Adjectives("nasty", "axe-wielding")
            .Describe("A nasty-looking troll, brandishing a bloody axe, blocks all passages out of the room."));

        BloodyAxe = Reg("bloody_axe", _b.Item("bloody axe").Called("axe").Adjectives("bloody", "sharp")
            .Describe("A bloody axe. It is very sharp."));

        Cyclops = Reg("cyclops", _b.Creature("cyclops").Called("monster", "giant").Adjectives("hungry", "one-eyed")
            .Describe("A hungry cyclops is standing at the foot of the stairs."));

        Thief = Reg("thief", _b.Creature("thief").Called("robber", "figure").Adjectives("sneaky", "shadowy")
            .Describe("There is a suspicious-looking individual, holding a large bag, leaning against one wall."));

        ApplyThingSizes();
    }

    private void PlaceThings()
    {
        Mailbox.StartsIn(WestOfHouse);
        Leaflet.StartsInside(Mailbox);
        KitchenWindow.StartsIn(EastOfHouse);
        Leaves.StartsIn(Clearing1);
        Grating.StartsIn(Clearing1).Concealed();
        Mirror1.StartsIn(MirrorRoom1);
        Mirror2.StartsIn(MirrorRoom2);

        Rug.StartsIn(LivingRoom);
        TrapDoor.StartsIn(LivingRoom).Concealed();
        TrophyCase.StartsIn(LivingRoom);
        Lantern.StartsIn(LivingRoom).InitialText("A battery-powered brass lantern is on the trophy case.");
        Sword.StartsIn(LivingRoom).InitialText("Above the trophy case hangs an elvish sword.");

        Sack.StartsIn(Kitchen).InitialText("On the table is an elongated brown sack, smelling of hot peppers.");
        Lunch.StartsInside(Sack);
        Garlic.StartsInside(Sack);
        Bottle.StartsIn(Kitchen).InitialText("A bottle is sitting on the table.");

        Rope.StartsIn(Attic);
        DomeRailing.StartsIn(DomeRoom);
        NastyKnife.StartsIn(Attic).InitialText("On a table is a nasty-looking knife.");
        PileOfPlastic.StartsIn(R(ZorkIds.DamBase)).InitialText(
            "There is a folded pile of plastic here which has a small valve attached.");

        Nest.StartsIn(UpATree);
        JeweledEgg.StartsOn(Nest);
        GoldenCanary.StartsInside(JeweledEgg);

        Painting.StartsIn(Gallery).InitialText(
            "Fortunately, there is still one chance for you to be a vandal, for on the far wall is a painting of unparalleled beauty.");

        Wrench.StartsIn(MaintenanceRoom);
        Screwdriver.StartsIn(MaintenanceRoom);
        Bolt.StartsIn(Dam);
        Pump.StartsIn(R(ZorkIds.ReservoirNorth));
        Matchbook.StartsIn(R(ZorkIds.DamLobby));

        Coal.StartsIn(DeadEndMine);
        Machine.StartsIn(MachineRoom);

        PlatinumBar.StartsIn(LoudRoom);
        BagOfCoins.StartsIn(R(ZorkIds.Maze5));
        Skeleton.StartsIn(R(ZorkIds.Maze5));
        RustyKnife.StartsIn(R(ZorkIds.Maze5));
        SkeletonKey.StartsIn(R(ZorkIds.Maze5)).Concealed();

        TrunkOfJewels.StartsIn(Reservoir);

        CrystalTrident.StartsIn(AtlantisRoom);
        IvoryTorch.StartsIn(TorchRoom);
        GoldCoffin.StartsIn(EgyptianRoom);
        Sceptre.StartsInside(GoldCoffin);
        SilverChalice.StartsIn(TreasureRoom);
        JadeFigurine.StartsIn(BatRoom);
        SapphireBracelet.StartsIn(GasRoom);
        Scarab.StartsIn(SandyCave);
        Sand.StartsIn(SandyCave);
        Basket.StartsIn(R(ZorkIds.ShaftRoom));
        PotOfGold.StartsIn(EndOfRainbow);
        LargeEmerald.StartsInside(RedBuoy);
        RedBuoy.StartsIn(FrigidRiver);
        CrystalSkull.StartsIn(LandOfTheDead);

        BlackBook.StartsIn(Altar);
        BrassBell.StartsIn(Temple);
        PairOfCandles.StartsIn(Altar);
        Shovel.StartsIn(R(ZorkIds.SandyBeach));

        Troll.StartsIn(TrollRoom);
        Cyclops.StartsIn(CyclopsRoom);
        Thief.StartsIn(RoundRoom);
    }
}
