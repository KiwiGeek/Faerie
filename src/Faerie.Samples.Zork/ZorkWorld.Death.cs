using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    private readonly List<Room> _darkScatterRooms = [];
    private readonly List<Room> _surfaceScatterRooms = [];

    private const int MaxRevives = 2;

    private void DefineDeath()
    {
        foreach (RoomDef def in ZorkMapData.AllRooms)
        {
            Room room = R(def.Id);
            if (def.Dark && room != LandOfTheDead)
                _darkScatterRooms.Add(room);
            else if (!def.Dark)
                _surfaceScatterRooms.Add(room);
        }

        _b.OnDeath(HandlePlayerDeath);
    }

    private void HandlePlayerDeath(DeathContext death)
    {
        GameContext ctx = death.Context;
        int deaths = ctx.Get(_deathCount);
        if (deaths >= MaxRevives)
        {
            ctx.Say(
                "You clearly are a suicidal maniac. We don't allow psychotics in the cave, since they may " +
                "harm other adventurers. Your remains will be installed in the Land of the Living Dead, " +
                "where your fellow adventurers may gloat over them.");
            return;
        }

        ctx.Set(_deathCount, deaths + 1);
        ctx.State.Score = Math.Max(0, ctx.State.Score - 10);

        ctx.Say(@"
    ****  You have died  ****
");
        if (!ctx.Get(_lucky))
            ctx.Say("Bad luck, huh?");

        ScatterInventoryOnDeath(ctx);

        ctx.Set(_playerHp, PlayerMaxHp);
        ctx.Set(_grueTurns, 0);
        ctx.MovePlayerTo(R(ZorkIds.Forest1));

        ctx.Say(
            "Now, let's take a look here...\n" +
            "Well, you probably deserve another chance. I can't quite fix you up completely, " +
            "but you can't have everything.");

        death.Revived = true;
    }

    private void ScatterInventoryOnDeath(GameContext ctx)
    {
        Death.ScatterCarried(
            ctx,
            IsTreasure,
            _darkScatterRooms,
            _surfaceScatterRooms,
            RelocateCarriedOnDeath);
    }

    private void RelocateCarriedOnDeath(GameContext ctx, Thing thing)
    {
        if (thing == Lantern)
            ctx.Move(thing, Placement.InRoom(LivingRoom));
        else if (thing == GoldCoffin)
            ctx.Move(thing, Placement.InRoom(EgyptianRoom));
    }
}
