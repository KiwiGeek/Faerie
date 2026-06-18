using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    private const int NarrowPassageMaxItemSize = 4;
    private const string NarrowPassageMessage = "You cannot fit through this passage with that load.";
    private const string ChimneyEmptyHandsMessage = "Going up empty-handed is a bad idea.";
    private const string ChimneyOverloadMessage = "You can't get up there with what you're carrying.";
    private const string AltarCoffinDownMessage = "You haven't a prayer of getting the coffin down there.";

    private void ConfigureEncumbranceExits()
    {
        ConfigureNarrowPassage(ZorkIds.TimberRoom, Direction.West);
        ConfigureNarrowPassage(ZorkIds.DraftyRoom, Direction.East);
        R(ZorkIds.DraftyRoom).ExitTo(Direction.Out)
            ?.When(ctx => FitsNarrowPassage(ctx), NarrowPassageMessage);

        Exit altarDown = R(ZorkIds.Altar).ExitTo(Direction.Down)!;
        altarDown.Condition = ctx => !ctx.Carrying(GoldCoffin);
        altarDown.BlockedMessage = AltarCoffinDownMessage;

        Exit studioUp = R(ZorkIds.Studio).Connect(Direction.Up, Kitchen, reciprocal: false);
        studioUp.Gate = ChimneyUpGate;
    }

    private void ConfigureNarrowPassage(string roomId, Direction direction) =>
        R(roomId).ExitTo(direction)?.When(ctx => FitsNarrowPassage(ctx), NarrowPassageMessage);

    private static bool FitsNarrowPassage(GameContext ctx) =>
        !ctx.State.Inventory.Concat(ctx.State.Worn).Any(t => t.Size > NarrowPassageMaxItemSize);

    private ExitGate ChimneyUpGate(GameContext ctx)
    {
        if (!ctx.Get(_chimneyFlag))
            return ExitGate.Block("You can't go that way.");

        IReadOnlyList<Thing> carried = ctx.State.Inventory.Concat(ctx.State.Worn).ToList();
        if (carried.Count == 0)
            return ExitGate.Block(ChimneyEmptyHandsMessage);
        if (!ctx.Carrying(Lantern) && !ctx.Wearing(Lantern))
            return ExitGate.Block(ChimneyOverloadMessage);
        if (carried.Count > 2)
            return ExitGate.Block(ChimneyOverloadMessage);

        return ExitGate.Open;
    }

    private void ApplyThingSizes()
    {
        Leaflet.Size = 2;
        Lantern.Size = 15;
        Sword.Size = 30;
        Sack.Size = 9;
        Garlic.Size = 4;
        Rope.Size = 10;
        PileOfPlastic.Size = 20;
        Boat.Size = 20;
        Wrench.Size = 10;
        BlackBook.Size = 10;
        PairOfCandles.Size = 10;
        Matchbook.Size = 2;
        Shovel.Size = 15;
        Painting.Size = 15;
        BagOfCoins.Size = 15;
        PlatinumBar.Size = 20;
        TrunkOfJewels.Size = 35;
        CrystalTrident.Size = 20;
        IvoryTorch.Size = 20;
        GoldCoffin.Size = 55;
        Sceptre.Size = 3;
        RedBuoy.Size = 10;
        Scarab.Size = 8;
        PotOfGold.Size = 15;
        JadeFigurine.Size = 10;
        SapphireBracelet.Size = 10;
        SilverChalice.Size = 10;
        Coal.Size = 20;
        RustyKnife.Size = 20;
        BloodyAxe.Size = 25;
        SkeletonKey.Size = 10;
    }
}
