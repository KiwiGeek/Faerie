using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Cyclops Room — Infocom <c>1actions.zil</c> CYCLOPS-FCN / V-ODYSSEUS (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private const int CyclopsWrathDeath = 6;

    private static readonly string[] CyclopsMadMessages =
    [
        "The cyclops seems somewhat agitated.",
        "The cyclops appears to be getting more agitated.",
        "The cyclops is moving about the room, looking for something.",
        "The cyclops was looking for salt and pepper. No doubt they are condiments for his upcoming snack.",
        "The cyclops is moving toward you in an unfriendly manner.",
        "You have two choices: 1. Leave  2. Become dinner."
    ];

    internal Thing Water = null!;

    private Verb _odysseus = null!;

    private void DefineCyclopsThings()
    {
        Water = Reg("water", _b.Item("water").Describe("A quantity of water."));
        Water.StartsInside(Bottle);
    }

    private void DefineCyclops()
    {
        DefineCyclopsThings();

        _odysseus = _b.DefineVerb("odysseus", ["odysseus", "ulysseus"], VerbForms.Intransitive, OdysseusHandler);

        ConfigureCyclopsRoom();
        WireCyclopsReactions();
        WireCyclopsDaemon();
    }

    private bool CyclopsPresent(GameContext ctx) =>
        !ctx.Get(_cyclopsGone) && ctx.Here(Cyclops);

    private void ConfigureCyclopsRoom()
    {
        CyclopsRoom.Describe(CyclopsRoomLook);

        R(ZorkIds.CyclopsRoom).ExitTo(Direction.Up)!.Condition = ctx => ctx.Get(_cyclopsFlag);
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.Up)!.BlockedMessage =
            "The cyclops doesn't look like he'll let you past.";
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.East)!.Condition = ctx => ctx.Get(_magicFlag);
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.East)!.BlockedMessage = "The east wall is solid rock.";

        CyclopsRoom.OnEnter = ctx =>
        {
            if (!ctx.InRoom(CyclopsRoom)) return;
            if (!ctx.Get(_cyclopsFlag) && CyclopsPresent(ctx))
            {
                ctx.Set(_cyclopsDaemon, true);
                ctx.Say("The cyclops blocks the staircase.");
            }
        };

        Cyclops.OnExamine = CyclopsExamine;
    }

    private void CyclopsExamine(GameContext ctx)
    {
        if (ctx.Get(_cyclopsGone)) return;

        if (ctx.Get(_cyclopsFlag))
        {
            ctx.Say("The cyclops is sleeping like a baby, albeit a very ugly one.");
            return;
        }

        if (ctx.Get(_cyclopsWrath) > 0)
        {
            ctx.Say("The cyclops is standing in the corner, eyeing you closely. I don't think he likes you very much. " +
                    "He looks extremely hungry, even for a cyclops.");
            return;
        }

        if (ctx.Get(_cyclopsWrath) < 0)
        {
            ctx.Say("The cyclops, having eaten the hot peppers, appears to be gasping. " +
                    "His enflamed tongue protrudes from his man-sized mouth.");
            return;
        }

        ctx.Say("A hungry cyclops is standing at the foot of the stairs.");
    }

    private string CyclopsRoomLook(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("This room has an exit on the northwest, and a staircase leading up.");

        if (ctx.Get(_cyclopsFlag) && !ctx.Get(_magicFlag) && CyclopsPresent(ctx))
            sb.Append("\nThe cyclops is sleeping blissfully at the foot of the stairs.");
        else if (ctx.Get(_magicFlag))
            sb.Append("\nThe east wall, previously solid, now has a cyclops-sized opening in it.");
        else if (CyclopsPresent(ctx))
        {
            if (ctx.Get(_cyclopsWrath) == 0)
                sb.Append("\nA cyclops, who looks prepared to eat horses (much less mere adventurers), blocks the staircase. " +
                          "From his state of health, and the bloodstains on the walls, you gather that he is not very friendly, though he likes people.");
            else if (ctx.Get(_cyclopsWrath) > 0)
                sb.Append("\nThe cyclops is standing in the corner, eyeing you closely. I don't think he likes you very much. " +
                          "He looks extremely hungry, even for a cyclops.");
            else
                sb.Append("\nThe cyclops, having eaten the hot peppers, appears to be gasping. " +
                          "His enflamed tongue protrudes from his man-sized mouth.");
        }

        return sb.ToString();
    }

    private void WireCyclopsReactions()
    {
        _b.On(Cyclops).Before(_b.Verbs.Give!, CyclopsGiveHandler);
        _b.On(Cyclops).Before(_attack, CyclopsAttackHandler);
        _b.On(Cyclops).Before(_b.Verbs.Take!, ctx =>
        {
            if (!CyclopsPresent(ctx)) return VerbResult.Pass;
            ctx.Say("The cyclops doesn't take kindly to being grabbed.");
            return VerbResult.Done;
        });
    }

    private VerbResult CyclopsGiveHandler(VerbContext ctx)
    {
        if (!CyclopsPresent(ctx)) return VerbResult.Pass;

        if (ctx.Get(_cyclopsFlag))
        {
            ctx.Say("No use talking to him. He's fast asleep.");
            return VerbResult.Done;
        }

        Thing? gift = ctx.DirectObject;
        if (gift == Lunch)
            return CyclopsGiveLunch(ctx);

        if (gift == Water || (gift == Bottle && ctx.State.ContentsOf(Bottle).Contains(Water)))
            return CyclopsGiveWater(ctx);

        if (gift == Garlic)
        {
            ctx.Say("The cyclops may be hungry, but there is a limit.");
            return VerbResult.Done;
        }

        ctx.Say("The cyclops is not so stupid as to eat THAT!");
        return VerbResult.Done;
    }

    private VerbResult CyclopsGiveLunch(VerbContext ctx)
    {
        if (ctx.Get(_cyclopsWrath) < 0)
            return VerbResult.Pass;

        ctx.Remove(Lunch);
        ctx.Say("The cyclops says \"Mmm Mmm. I love hot peppers! But oh, could I use a drink. Perhaps I could drink the blood of that thing.\"  " +
                "From the gleam in his eye, it could be surmised that you are \"that thing\".");
        int wrath = ctx.Get(_cyclopsWrath);
        ctx.Set(_cyclopsWrath, Math.Min(-1, -wrath));
        ctx.Set(_cyclopsDaemon, true);
        return VerbResult.Done;
    }

    private VerbResult CyclopsGiveWater(VerbContext ctx)
    {
        if (ctx.Get(_cyclopsWrath) >= 0)
        {
            ctx.Say("The cyclops apparently is not thirsty and refuses your generous offer.");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Bottle && !Bottle.Has(Attr.Open))
        {
            ctx.Say("The cyclops apparently is not thirsty and refuses your generous offer.");
            return VerbResult.Done;
        }

        if (ctx.State.ContentsOf(Bottle).Contains(Water))
            ctx.Remove(Water);
        else if (ctx.DirectObject == Water)
            ctx.Remove(Water);

        if (ctx.Carrying(Bottle))
        {
            ctx.Remove(Bottle);
            ctx.PlaceHere(Bottle);
        }

        Bottle.Set(Attr.Open, true);
        ctx.Set(_cyclopsFlag, true);
        ctx.Say("The cyclops takes the bottle, checks that it's open, and drinks the water. " +
                "A moment later, he lets out a yawn that nearly blows you over, and then falls fast asleep (what did you put in that drink, anyway?).");
        return VerbResult.Done;
    }

    private VerbResult CyclopsAttackHandler(VerbContext ctx)
    {
        if (!CyclopsPresent(ctx)) return VerbResult.Pass;

        if (ctx.Get(_cyclopsFlag))
        {
            WakeCyclops(ctx);
            return VerbResult.Done;
        }

        AggravateCyclops(ctx);
        return VerbResult.Done;
    }

    private void WakeCyclops(GameContext ctx)
    {
        ctx.Say("The cyclops yawns and stares at the thing that woke him up.");
        ctx.Set(_cyclopsFlag, false);
        int wrath = ctx.Get(_cyclopsWrath);
        if (wrath < 0)
            ctx.Set(_cyclopsWrath, -wrath);
        ctx.Set(_cyclopsDaemon, true);
    }

    private void AggravateCyclops(GameContext ctx)
    {
        ctx.Set(_cyclopsDaemon, true);
        ctx.Say("The cyclops shrugs but otherwise ignores your pitiful attempt.");
    }

    private void WireCyclopsDaemon()
    {
        _b.EveryTurn(CyclopsDaemonTick, when: ctx =>
            ctx.Get(_cyclopsDaemon) && ctx.InRoom(CyclopsRoom) && CyclopsPresent(ctx) && !ctx.Get(_cyclopsFlag));

        _b.EveryTurn(ctx =>
        {
            if (ctx.Get(_cyclopsDaemon) && !ctx.InRoom(CyclopsRoom))
                ctx.Set(_cyclopsDaemon, false);
        });
    }

    private void CyclopsDaemonTick(GameContext ctx)
    {
        int wrath = ctx.Get(_cyclopsWrath);
        if (Math.Abs(wrath) >= CyclopsWrathDeath)
        {
            ctx.Die("The cyclops, tired of all of your games and trickery, grabs you firmly. " +
                     "As he licks his chops, he says \"Mmm. Just like Mom used to make 'em.\" " +
                     "It's nice to be appreciated.");
            return;
        }

        wrath = wrath < 0 ? wrath - 1 : wrath + 1;
        ctx.Set(_cyclopsWrath, wrath);

        int index = Math.Clamp(Math.Abs(wrath) - 1, 0, CyclopsMadMessages.Length - 1);
        ctx.Say(CyclopsMadMessages[index]);
    }

    private VerbResult OdysseusHandler(VerbContext ctx)
    {
        if (ctx.InRoom(CyclopsRoom) && CyclopsPresent(ctx) && !ctx.Get(_cyclopsFlag))
        {
            ctx.Set(_cyclopsDaemon, false);
            ctx.Set(_cyclopsFlag, true);
            ctx.Set(_cyclopsGone, true);
            ctx.Set(_magicFlag, true);
            ctx.Remove(Cyclops);
            ctx.Say("The cyclops, hearing the name of his father's deadly nemesis, flees the room " +
                    "by knocking down the wall on the east of the room.");
            return VerbResult.Done;
        }

        ctx.Say("Wasn't he a sailor?");
        return VerbResult.Done;
    }
}
