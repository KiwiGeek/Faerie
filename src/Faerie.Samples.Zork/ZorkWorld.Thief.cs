using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Thief / robber — Infocom <c>I-THIEF</c>, <c>THIEF-VS-ADVENTURER</c> (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private const int ThiefDaemonStartTurn = 20;

    private static readonly HashSet<string> SacredRoomIds = new(StringComparer.Ordinal)
    {
        ZorkIds.WestOfHouse, ZorkIds.NorthOfHouse, ZorkIds.SouthOfHouse, ZorkIds.EastOfHouse,
        ZorkIds.Kitchen, ZorkIds.LivingRoom, ZorkIds.Attic, ZorkIds.StoneBarrow,
        ZorkIds.Forest1, ZorkIds.Forest2, ZorkIds.Forest3, ZorkIds.Forest4, ZorkIds.ForestPath,
        ZorkIds.Clearing1, ZorkIds.Clearing2, ZorkIds.UpATree, ZorkIds.CanyonView,
        ZorkIds.RockyLedge, ZorkIds.CanyonBottom,
    };

    private static readonly HashSet<string> WaterRoomIds = new(StringComparer.Ordinal)
    {
        ZorkIds.Reservoir, ZorkIds.FrigidRiver, ZorkIds.Stream, ZorkIds.River1, ZorkIds.River2,
        ZorkIds.River3, ZorkIds.River4, ZorkIds.OnTheRainbow, ZorkIds.Beach1, ZorkIds.Beach2,
        ZorkIds.SandyBeach, ZorkIds.Shore,
    };

    internal Thing LargeBag = null!;
    internal Thing Stiletto = null!;

    private List<Room> _thiefRoamRooms = null!;
    private HashSet<Room> _sacredRooms = null!;
    private int _thiefRoamIndex;

    private void DefineThiefThings()
    {
        LargeBag = Reg("large_bag", _b.Scenery("large bag").Called("bag").Adjectives("large", "thief's")
            .Describe("The bag is underneath the thief, so one can't say what, if anything, is inside.")
            .Container(open: true));
        Stiletto = Reg("stiletto", _b.Item("stiletto").Adjectives("vicious")
            .Describe("A vicious-looking stiletto."));
        Stiletto.StartsInside(Thief);
        LargeBag.StartsInside(Thief);
    }

    private void DefineThief()
    {
        DefineThiefThings();
        BuildThiefRoamRooms();

        Thief.Set(Attr.Concealed, true);
        Thief.Describe(ThiefDescription);

        WireThiefReactions();
        WireThiefTreasureRoom();
        WireThiefDaemon();
    }

    private void BuildThiefRoamRooms()
    {
        _sacredRooms = SacredRoomIds.Select(R).ToHashSet();

        _thiefRoamRooms = _rooms
            .Where(kv => !SacredRoomIds.Contains(kv.Key) && !WaterRoomIds.Contains(kv.Key))
            .Select(kv => kv.Value)
            .OrderBy(r => _rooms.First(kv => kv.Value == r).Key, StringComparer.Ordinal)
            .ToList();

        int start = _thiefRoamRooms.FindIndex(r => r == RoundRoom);
        _thiefRoamIndex = start >= 0 ? start : 0;
    }

    private bool IsSacredRoom(Room room) => _sacredRooms.Contains(room);

    private string ThiefDescription(GameContext ctx)
    {
        if (ctx.Get(_thiefKO) > 0)
            return "There is a suspicious-looking individual lying unconscious on the ground.";
        if (Thief.Has(Attr.Concealed))
            return "There is a suspicious-looking individual, holding a bag, leaning against one wall.";
        return "There is a suspicious-looking individual, holding a bag, leaning against one wall. " +
               "He is armed with a vicious-looking stiletto.";
    }

    private void WireThiefReactions()
    {
        _b.On(Thief).Before(_b.Verbs.Give!, ThiefGiveHandler);
        _b.On(LargeBag).Before(_b.Verbs.Take!, ctx =>
        {
            ctx.Say(ctx.Get(_thiefKO) > 0
                ? "Sadly for you, the robber collapsed on top of the bag. Trying to take it would wake him."
                : "The bag will be taken over his dead body.");
            return VerbResult.Done;
        });
        _b.On(LargeBag).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("Getting close enough would be a good trick.");
            return VerbResult.Done;
        });
    }

    private VerbResult ThiefGiveHandler(VerbContext ctx)
    {
        if (!ctx.Here(Thief) || ctx.Get(_thiefDead)) return VerbResult.Pass;
        if (ctx.Get(_thiefKO) > 0) return VerbResult.Pass;

        Thing? gift = ctx.DirectObject;
        if (gift is null || !ctx.Carrying(gift)) return VerbResult.Pass;

        if (IsTreasure(gift))
        {
            ctx.Remove(gift);
            gift.Set(Attr.Concealed, true);
            ctx.State.Move(gift, Placement.Inside(LargeBag));
            ctx.Set(_thiefEngrossed, true);
            ctx.Say($"The thief places the {gift.Name} in his bag and thanks you politely.");
            return VerbResult.Done;
        }

        ctx.Say("The thief is not interested in your gift.");
        return VerbResult.Done;
    }

    private void WireThiefTreasureRoom()
    {
        TreasureRoom.OnEnter = ctx =>
        {
            if (ctx.Get(_thiefDead) || ctx.State.TurnCount < ThiefDaemonStartTurn) return;
            if (ctx.Get(_thiefKO) > 0) return;

            if (!ctx.Here(Thief))
            {
                ctx.Say("You hear a scream of anguish as you violate the robber's hideaway. " +
                        "Using passages unknown to you, he rushes to its defense.");
                ctx.State.Move(Thief, Placement.InRoom(TreasureRoom));
            }

            RevealThief(ctx);
            ThiefVanishRoomTreasures(ctx);
        };
    }

    private void ThiefVanishRoomTreasures(GameContext ctx)
    {
        List<Thing> loot = ctx.State.ContentsOf(TreasureRoom)
            .Where(t => t != Thief && t != LargeBag && t != Stiletto && IsTreasure(t))
            .ToList();
        if (loot.Count == 0) return;

        ctx.Say("The thief gestures mysteriously, and the treasures in the room suddenly vanish.");
        foreach (Thing t in loot)
            MoveToThiefBooty(ctx, t);
    }

    private void WireThiefDaemon()
    {
        _b.EveryTurn(ThiefDaemonTick, when: ctx => !ctx.Get(_thiefDead));
    }

    private void ThiefDaemonTick(GameContext ctx)
    {
        if (ctx.State.TurnCount < ThiefDaemonStartTurn || ctx.Get(_thiefKO) > 0) return;

        Room thiefRoom = ThiefRoom(ctx);
        bool sameRoom = thiefRoom == ctx.CurrentRoom;
        bool visible = sameRoom && ctx.Here(Thief) && !Thief.Has(Attr.Concealed);
        bool lit = sameRoom && new Scope(ctx.State, ctx).IsCurrentRoomLit;

        if (thiefRoom == TreasureRoom && !sameRoom)
        {
            DepositBooty(ctx, TreasureRoom);
            return;
        }

        if (IsSacredRoom(ctx.CurrentRoom))
            return;

        if (sameRoom && lit && !ctx.Here(Troll) && ThiefVsAdventurer(ctx, visible))
            return;

        if (thiefRoom.Has(Attr.Visited) && !IsSacredRoom(thiefRoom))
        {
            RobRoomTreasures(ctx, thiefRoom, 75);
            if (IsMazeRoom(thiefRoom.Id))
                StealJunkFromRoom(ctx, thiefRoom);
        }

        if (!visible)
        {
            MoveThiefToNextRoom(ctx);
            DropJunkFromBag(ctx, ThiefRoom(ctx));
        }
    }

    private bool ThiefVsAdventurer(GameContext ctx, bool visible)
    {
        if (ctx.Get(_thiefEngrossed) && ctx.Random.Next(4) != 0)
            return false;

        if (!ctx.Get(_thiefHere) && !visible && ctx.Random.Next(10) < 3)
        {
            RevealThief(ctx);
            ctx.Say("Someone carrying a large bag is casually leaning against one of the walls here. " +
                    "He does not speak, but it is clear from his aspect that the bag will be taken only over his dead body.");
            ctx.Set(_thiefHere, true);
            return true;
        }

        if (!visible && ctx.Random.Next(10) < 3)
        {
            ctx.Say("The holder of the large bag just left, looking disgusted. Fortunately, he took nothing.");
            HideThief(ctx);
            ctx.Set(_thiefHere, false);
            return true;
        }

        if (ctx.Random.Next(10) < 7)
            return false;

        bool robbedRoom = RobRoomTreasures(ctx, ctx.CurrentRoom, 100);
        bool robbedPlayer = RobPlayerTreasures(ctx, 100);
        ctx.Set(_thiefHere, true);

        if (!visible)
        {
            ctx.Say(robbedRoom || robbedPlayer
                ? "A seedy-looking individual with a large bag just wandered through the room. " +
                  "On the way through, he quietly abstracted some valuables from " +
                  (robbedPlayer ? "your possession" : "the room") + ", mumbling something about \"Doing unto others before...\""
                : "A \"lean and hungry\" gentleman just wandered through, carrying a large bag. " +
                  "Finding nothing of value, he left disgruntled.");
        }
        else if (robbedPlayer)
        {
            ctx.Say("The thief just left, still carrying his large bag. You may not have noticed that he robbed you blind first.");
            MaybeStoleLight(ctx);
        }
        else if (robbedRoom)
        {
            ctx.Say("The thief just left, still carrying his large bag. You may not have noticed that he appropriated the valuables in the room.");
            MaybeStoleLight(ctx);
        }
        else
            ctx.Say("The thief, finding nothing of value, left disgusted.");

        HideThief(ctx);
        ctx.Set(_thiefHere, false);
        return true;
    }

    private bool RobRoomTreasures(GameContext ctx, Room room, int percent)
    {
        bool robbed = false;
        foreach (Thing t in ctx.State.ContentsOf(room).Where(IsTreasure).ToList())
        {
            if (t == Thief || t == LargeBag || t == Stiletto) continue;
            if (ctx.Random.Next(100) >= percent) continue;
            MoveToThiefBooty(ctx, t);
            robbed = true;
        }
        return robbed;
    }

    private bool RobPlayerTreasures(GameContext ctx, int percent)
    {
        if (IsSacredRoom(ctx.CurrentRoom))
            return false;

        bool robbed = false;
        foreach (Thing t in ctx.State.Inventory.Where(IsTreasure).Where(t => t != Sword && t != Lantern).ToList())
        {
            if (ctx.Random.Next(100) >= percent) continue;
            ctx.Remove(t);
            MoveToThiefBooty(ctx, t);
            robbed = true;
        }
        return robbed;
    }

    private void StealJunkFromRoom(GameContext ctx, Room room)
    {
        foreach (Thing t in ctx.State.ContentsOf(room).ToList())
        {
            if (IsTreasure(t) || t == Thief) continue;
            if (!t.Has(Attr.Takeable)) continue;
            if (ctx.Random.Next(10) != 0 && t != Rope) continue;
            MoveToThiefBooty(ctx, t);
            if (ctx.InRoom(room))
                ctx.Say($"You suddenly notice that the {t.Name} vanished.");
        }
    }

    private void MoveToThiefBooty(GameContext ctx, Thing item)
    {
        item.Set(Attr.Concealed, true);
        ctx.State.Move(item, Placement.Inside(LargeBag));
    }

    private void DepositBooty(GameContext ctx, Room room)
    {
        foreach (Thing t in ThiefBooty(ctx).ToList())
        {
            t.Set(Attr.Concealed, false);
            ctx.State.Move(t, Placement.InRoom(room));
        }
    }

    private IEnumerable<Thing> ThiefBooty(GameContext ctx)
    {
        foreach (Thing t in ctx.State.ContentsOf(LargeBag))
            yield return t;
        foreach (Thing t in ctx.State.ContentsOf(Thief))
        {
            if (t == LargeBag || t == Stiletto) continue;
            if (IsTreasure(t)) yield return t;
        }
    }

    private void DropJunkFromBag(GameContext ctx, Room room)
    {
        foreach (Thing t in ctx.State.ContentsOf(LargeBag).Where(t => !IsTreasure(t)).ToList())
        {
            if (ctx.Random.Next(10) >= 3) continue;
            t.Set(Attr.Concealed, false);
            ctx.State.Move(t, Placement.InRoom(room));
            if (ctx.InRoom(room))
                ctx.Say("The robber, rummaging through his bag, dropped a few items he found valueless.");
        }
    }

    private void MoveThiefToNextRoom(GameContext ctx)
    {
        if (_thiefRoamRooms.Count == 0) return;

        for (int i = 0; i < _thiefRoamRooms.Count; i++)
        {
            _thiefRoamIndex = (_thiefRoamIndex + 1) % _thiefRoamRooms.Count;
            Room next = _thiefRoamRooms[_thiefRoamIndex];
            if (IsSacredRoom(next)) continue;
            ctx.State.Move(Thief, Placement.InRoom(next));
            HideThief(ctx);
            ctx.Set(_thiefHere, false);
            return;
        }
    }

    private void RevealThief(GameContext ctx)
    {
        Thief.Set(Attr.Concealed, false);
        if (!ctx.Here(Thief))
            ctx.PlaceHere(Thief);
    }

    private void HideThief(GameContext ctx)
    {
        Thief.Set(Attr.Concealed, true);
    }

    private Room ThiefRoom(GameContext ctx) => ctx.RoomOf(Thief) ?? RoundRoom;

    private static bool IsMazeRoom(string roomId) => roomId.StartsWith("maze", StringComparison.Ordinal);

    private void MaybeStoleLight(GameContext ctx)
    {
        if (!new Scope(ctx.State, ctx).IsCurrentRoomLit && ctx.Carrying(Lantern) && Lantern.Has(Attr.Lit))
            ctx.Say("The thief seems to have left you in the dark.");
    }

    internal void DropThiefLoot(GameContext ctx)
    {
        Room here = ctx.CurrentRoom;
        foreach (Thing t in ThiefBooty(ctx).ToList())
        {
            t.Set(Attr.Concealed, false);
            ctx.State.Move(t, Placement.InRoom(here));
        }
    }
}
